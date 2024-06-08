using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimLayers {
	public class BlendTree : MonoBehaviour {
		[SerializeField] private AnimationLayers m_AnimationLayers;
		public List<BlendPoint> BlendPoints = new List<BlendPoint>();
	
		public float XAxis = 0.5f;
		public float YAxis = 0.5f;

		public AnimationLayers AnimationLayers => m_AnimationLayers;
		
		private void Reset() {
			OnValidate();
		}

		private void OnValidate() {
			if (m_AnimationLayers == null) {
				m_AnimationLayers = GetComponent<AnimationLayers>();
			}
			if (BlendPoints.Count == 0) {
				BlendPoints.Add(new BlendPoint(new Vector2(0f, 0f), 0));
				BlendPoints.Add(new BlendPoint(new Vector2(1f, 0f), 1));
				BlendPoints.Add(new BlendPoint(new Vector2(0f, 1f), 2));
				BlendPoints.Add(new BlendPoint(new Vector2(1f, 1f), 3));
			}
		}

		public void Update() {
			SampleWeightsCartesian(new Vector2(XAxis, YAxis), BlendPoints);
			foreach (BlendPoint blendPoint in BlendPoints) {
				m_AnimationLayers.SetInputWeight(blendPoint.LayerId, blendPoint.CurrentWeight);
			}
		}

		#region BLENDING

		public static void SampleWeightsCartesian( Vector2 samplePoint, List<BlendPoint> blendInfoList ) {
			int pointCount = blendInfoList.Count;

			float totalWeight = 0.0f;

			for( int i = 0; i < pointCount; ++i ) {
				// Calc vec i -> sample
				Vector2 pointI = blendInfoList[i].Position;
				Vector2 vecIS = samplePoint - pointI;

				float weight = 1.0f;

				for( int j = 0; j < pointCount; ++j ) {
					if( j == i )
						continue;

					// Calc vec i -> j
					Vector2 pointJ = blendInfoList[j].Position;
					Vector2 vecIJ = pointJ - pointI;

					// Calc Weight
					float lensqIJ = Vector2.Dot( vecIJ, vecIJ );
					float newWeight = Vector2.Dot( vecIS, vecIJ ) / lensqIJ;
					newWeight = 1.0f - newWeight;
					newWeight = Mathf.Clamp( newWeight, 0.0f, 1.0f );

					weight = Mathf.Min( weight, newWeight );
				}

				blendInfoList[i].CurrentWeight = weight;
				totalWeight += weight;
			}

			float inverseTotalWeight = 1.0f / totalWeight;
			for( int i = 0; i < pointCount; ++i ) {
				BlendPoint blendPoint = blendInfoList[i];
				blendPoint.CurrentWeight = blendPoint.CurrentWeight * inverseTotalWeight;
			}
		}

		public static float[] SampleWeightsCartesian( Vector2 samplePoint, Vector2[] points ) {
			int pointCount = points.Length;
			float[] weights = new float[pointCount];

			float totalWeight = 0.0f;

			for( int i = 0; i < pointCount; ++i ) {
				// Calc vec i -> sample
				Vector2 pointI = points[i];
				Vector2 vecIS = samplePoint - pointI;

				float weight = 1.0f;

				for( int j = 0; j < pointCount; ++j ) {
					if( j == i )
						continue;

					// Calc vec i -> j
					Vector2 pointJ = points[j];
					Vector2 vecIJ = pointJ - pointI;

					// Calc Weight
					float lensqIJ = Vector2.Dot( vecIJ, vecIJ );
					float newWeight = Vector2.Dot( vecIS, vecIJ ) / lensqIJ;
					newWeight = 1.0f - newWeight;
					newWeight = Mathf.Clamp( newWeight, 0.0f, 1.0f );

					weight = Mathf.Min( weight, newWeight );
				}

				weights[i] = weight;
				totalWeight += weight;
			}

			for( int i = 0; i < pointCount; ++i ) {
				weights[i] = weights[i] / totalWeight;
			}

			return weights;
		}

		public static float SignedAngle( Vector2 a, Vector2 b ) {
			return Mathf.Atan2( a.x * b.y - a.y * b.x, a.x * b.x + a.y * b.y );
		}

		#endregion
	}
	
	[System.Serializable]
	public class BlendPoint {
		public Vector2 Position;
		public int LayerId;
		public float CurrentWeight;

		public BlendPoint( Vector2 position, int layerId ) {
			Position = position;
			LayerId = layerId;
		}
	}
	
}