// Copyright (c) 2020 Markus Hofer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FaderX {
	/// <summary>
	/// Fade from one value to the other over a set amount of time.
	/// If you want to continually assign a new target value, use a "Lerper" instead!
	/// </summary>
	public abstract class BaseFader<T> where T : struct {
		[SerializeField] private T m_CurrentValue;
		private T m_StartValue;
		private T m_TargetValue;

		private bool m_SetStartTime = false;
		private float m_StartTime = 0f;
		private float m_CurrentTime = 0f;
		private float m_Duration = 1f; //duration of the fade
		private float m_Sec = 1f; //user-definde duration
		private float m_TimeFactor = 1f; //apply to speed up or slow down the fade to reach the user-defined duration

		[SerializeField] private AnimationCurve m_InCurve = new AnimationCurve( new Keyframe[] {
			new Keyframe( 0f, 0f, 0f, 0f ), new Keyframe( 0.1301245f, 0.1734855f, 2.954247f, 2.954247f ),
			new Keyframe( 0.4551825f, 0.8159907f, 0.8537326f, 0.8537326f ), new Keyframe( 1f, 1f, 0f, 0f )
		} );
		//new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.1301245f, 0.1734855f, 2.954247f, 2.954247f), new Keyframe(0.553239f, 0.9039673f, 0.5954359f, 0.5954359f), new Keyframe(1f, 1f, 0f, 0f)  });
		//public AnimationCurve outCurve;

		private bool m_Done = false;
		public FadeDone Callback = null;

		public bool Done => m_Done;

		public delegate void FadeDone();

		public BaseFader() {
			//Setup(default(T), default(T), sec);
		}

		public BaseFader( T startVal, T targetVal ) {
			Setup( startVal, targetVal, m_Sec );
			m_CurrentValue = startVal;
		}

		public BaseFader( T startVal, T targetVal, float sec ) {
			Setup( startVal, targetVal, sec );
			m_CurrentValue = startVal;
		}

		public float Sec {
			get => m_Sec;
			set => Setup( m_StartValue, m_TargetValue, value );
		}

		/// <summary>
		/// Get or set the AnimationCurve used by this Fader
		/// </summary>
		/// <value>The curve.</value>
		public AnimationCurve Curve {
			get => m_InCurve;
			set {
				m_InCurve = value;
				m_Duration = m_InCurve.keys[m_InCurve.keys.Length - 1].time; //get time of last key
				m_TimeFactor = m_Duration / m_Sec;
			}
		}

		protected void Setup( T startVal, T endVal, float sec, FadeDone callback = null ) {
#if VERBOSE
		Debug.Log ("Fader Setup: " + startVal + " >> " + endVal + " sec: " + sec);
#endif
			if( sec <= 0f ) { //jump
				m_StartValue = m_TargetValue = m_CurrentValue = endVal;
				m_StartTime = -999f; //done, stay done.
				return;
			}

			m_Done = false;
			m_StartValue = startVal;
			m_TargetValue = endVal;
			m_Duration = m_InCurve.keys[m_InCurve.keys.Length - 1].time; //get time of last key
			m_Sec = sec;
			m_TimeFactor = m_Duration / sec;
			m_SetStartTime = true;

			Callback?.Invoke(); //make sure we don't lose previous Callback
			Callback = callback;
		}

		
		public T CurrentValue {
			get {
				if( m_SetStartTime ) {
					m_StartTime = Time.time;
					m_SetStartTime = false;
				}

				if( m_Done || m_CurrentTime == Time.time ) return m_CurrentValue;
				m_CurrentTime = Time.time;
				if( ( Time.time - m_StartTime ) * m_TimeFactor > m_Duration ) {
					if( !EqualityComparer<T>.Default.Equals( m_CurrentValue, m_TargetValue ) || !m_Done ) {
						m_CurrentValue = m_TargetValue;
						m_Done = true;
						Callback?.Invoke();
						Callback = null;
					}

					return m_CurrentValue;
				}

				return m_CurrentValue = Lerp( m_StartValue, m_TargetValue, m_InCurve.Evaluate( ( Time.time - m_StartTime ) * m_TimeFactor ) );
			}
		}

		protected abstract T Lerp( T a, T b, float t );
		
		
		public T FadeTo( T val, FadeDone callback = null ) {
			if( !EqualityComparer<T>.Default.Equals( val, m_TargetValue ) ) StartFade( val, callback );
			return CurrentValue;
		}

		public T FadeTo( T val, float sec, FadeDone callback = null ) {
			if( !EqualityComparer<T>.Default.Equals( val, m_TargetValue ) ) StartFade( val, sec, callback );
			return CurrentValue;
		}

		public T FadeTo( T startVal, T targetVal, float sec, FadeDone callback = null, bool ignorePrevTargetValueCheck = false ) {
#if VERBOSE
		Debug.Log ("FadeTo: " + startVal + " >> " + targetVal + ", sec: " + sec + " (targetValue " + targetValue + " ok? " + (targetVal != targetValue || ignorePrevTargetValueCheck) + ")");
#endif
			if( !EqualityComparer<T>.Default.Equals( targetVal, m_TargetValue ) || ignorePrevTargetValueCheck )
				StartFade( startVal, targetVal, sec, callback );
			return CurrentValue;
		}

		public T JumpTo( T targetValue ) {
			m_CurrentValue = targetValue;
			Callback?.Invoke();
			Callback = null;
			m_Done = true;
			return CurrentValue;
		}

		protected void StartFade( T val, FadeDone callback = null ) {
			Setup( m_CurrentValue, val, m_Sec, callback );
		}

		protected void StartFade( T val, float sec, FadeDone callback = null ) {
			Setup( m_CurrentValue, val, sec, callback );
		}

		protected void StartFade( T startVal, T targetVal, float sec, FadeDone callback = null ) {
			Setup( startVal, targetVal, sec, callback );
		}


		public bool Changed => Time.time == m_CurrentTime;
	}

	[Serializable]
	public class FloatFader : BaseFader<float> {
		public FloatFader() { }
		public FloatFader( float startVal, float targetVal ) : base( startVal, targetVal ) { }
		public FloatFader( float startVal, float targetVal, float sec ) : base( startVal, targetVal, sec ) { }

		protected override float Lerp( float a, float b, float t ) {
			return Mathf.Lerp( a, b, t );
		}
	}

	public class Vector2Fader : BaseFader<Vector2> {
		public Vector2Fader() { }
		public Vector2Fader( Vector2 startVal, Vector2 targetVal ) : base( startVal, targetVal ) { }
		public Vector2Fader( Vector2 startVal, Vector2 targetVal, float sec ) : base( startVal, targetVal, sec ) { }

		protected override Vector2 Lerp( Vector2 a, Vector2 b, float t ) {
			return Vector2.Lerp( a, b, t );
		}
	}
	
	public class Vector3Fader : BaseFader<Vector3> {
		public Vector3Fader() { }
		public Vector3Fader( Vector3 startVal, Vector3 targetVal ) : base( startVal, targetVal ) { }
		public Vector3Fader( Vector3 startVal, Vector3 targetVal, float sec ) : base( startVal, targetVal, sec ) { }

		protected override Vector3 Lerp( Vector3 a, Vector3 b, float t ) {
			return Vector3.Lerp( a, b, t );
		}   
	}	
	
	public class Vector4Fader : BaseFader<Vector4> {
		public Vector4Fader() { }
		public Vector4Fader( Vector4 startVal, Vector4 targetVal ) : base( startVal, targetVal ) { }
		public Vector4Fader( Vector4 startVal, Vector4 targetVal, float sec ) : base( startVal, targetVal, sec ) { }

		protected override Vector4 Lerp( Vector4 a, Vector4 b, float t ) {
			return Vector4.Lerp( a, b, t );
		}   
	}
	
	public class ColorFader : BaseFader<Color> {
		public ColorFader() { }
		public ColorFader( Color startVal, Color targetVal ) : base( startVal, targetVal ) { }
		public ColorFader( Color startVal, Color targetVal, float sec ) : base( startVal, targetVal, sec ) { }

		protected override Color Lerp( Color a, Color b, float t ) {
			return Color.Lerp( a, b, t );
		}   
	}
	
}