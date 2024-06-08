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
	/// Lerp towards a target value
	/// </summary>
	public abstract class BaseLerper<T> where T : struct {
		[SerializeField] private T m_CurrentValue;
		[SerializeField] private T m_TargetValue;
		private float m_Speed;
		
		private long m_CurrentFrame;
		private bool m_Done = false;
		public LerpDone Callback = null;

		public bool Done => m_Done;

		public float Speed {
			get => m_Speed;
			set => m_Speed = value;
		}

		public delegate void LerpDone();

		public BaseLerper() {
			m_CurrentValue = default(T);
			m_TargetValue = default(T);
			m_Speed = 5f;
		}

		public BaseLerper( T startValue, T endValue ) {
			m_CurrentValue = startValue;
			m_TargetValue = endValue;
			m_Speed = 5f;
		}
		
		public BaseLerper( T startValue, T endValue, float speed ) {
			m_CurrentValue = startValue;
			m_TargetValue = endValue;
			m_Speed = speed;
		}

		public T CurrentValue {
			get {
				if( m_Done || m_CurrentFrame == Time.frameCount ) return m_CurrentValue;
				m_CurrentFrame = Time.frameCount;
				
				if( Equals( m_CurrentValue, m_TargetValue ) && !m_Done ) {
					m_CurrentValue = m_TargetValue;
					m_Done = true;
					Callback?.Invoke();
					return m_CurrentValue;
				}
				
				return m_CurrentValue = Lerp( m_CurrentValue, m_TargetValue, Time.deltaTime * m_Speed );
			}
		}
		
		public T TargetValue => m_TargetValue;

		protected virtual bool Equals( T a, T b ) {
			return EqualityComparer<T>.Default.Equals( a, b );
		}
		
		protected abstract T Lerp(T a, T b, float t);

		public T LerpTo( T targetValue ) {
			if( !Equals( m_TargetValue, targetValue ) ) {
				m_TargetValue = targetValue;
				m_Done = false;	
			}
			return CurrentValue;
		}

		public T LerpTo( T targetValue, float speed ) {
			Speed = speed;
			return LerpTo( targetValue );
		}

		public T JumpTo( T targetValue ) {
			m_Done = false;
			m_TargetValue = m_CurrentValue = targetValue;
			return m_CurrentValue;
		}
	}


	[Serializable]
	public class FloatLerper : BaseLerper<float> {

		private float m_EqualityLimit;

		public float EqualityLimit {
			get => m_EqualityLimit;
			set => m_EqualityLimit = value;
		}

		public FloatLerper() { m_EqualityLimit = 0.0001f; }
		public FloatLerper( float startValue, float endValue ) : base( startValue, endValue ) { m_EqualityLimit = 0.0001f; }
		public FloatLerper( float startValue, float endValue, float speed ) : base( startValue, endValue, speed ) { m_EqualityLimit = 0.0001f; }

		protected override float Lerp( float a, float b, float t ) {
			return Mathf.Lerp( a, b, t );
		}

		protected override bool Equals( float a, float b ) {
			return Mathf.Abs( a - b ) < m_EqualityLimit;
		}
	}
	
	public class Vector2Lerper : BaseLerper<Vector2> {
		
		private float m_EqualityLimit;

		public float EqualityLimit {
			get => m_EqualityLimit;
			set => m_EqualityLimit = value;
		}

		public Vector2Lerper() { m_EqualityLimit = 0.01f; }
		public Vector2Lerper( Vector2 startValue, Vector2 endValue ) : base( startValue, endValue ) { m_EqualityLimit = 0.01f; }
		public Vector2Lerper( Vector2 startValue, Vector2 endValue, float speed ) : base( startValue, endValue, speed ) { m_EqualityLimit = 0.01f; }

		protected override Vector2 Lerp( Vector2 a, Vector2 b, float t ) {
			return Vector2.Lerp( a, b, t );
		}

		protected override bool Equals( Vector2 a, Vector2 b ) {
			return ( a - b ).sqrMagnitude < m_EqualityLimit * m_EqualityLimit;
		}
	}
	
	public class Vector3Lerper : BaseLerper<Vector3> {
		
		private float m_EqualityLimit;
		
		public float EqualityLimit {
			get => m_EqualityLimit;
			set => m_EqualityLimit = value;
		}
		
		public Vector3Lerper() { m_EqualityLimit = 0.01f; }
		public Vector3Lerper( Vector3 startValue, Vector3 endValue ) : base( startValue, endValue ) { m_EqualityLimit = 0.01f; }
		public Vector3Lerper( Vector3 startValue, Vector3 endValue, float speed ) : base( startValue, endValue, speed ) { m_EqualityLimit = 0.01f; }

		protected override Vector3 Lerp( Vector3 a, Vector3 b, float t ) {
			return Vector3.Lerp( a, b, t );
		}

		protected override bool Equals( Vector3 a, Vector3 b ) {
			return ( a - b ).sqrMagnitude < m_EqualityLimit * m_EqualityLimit;
		}
	}
	
	public class QuaternionLerper : BaseLerper<Quaternion> {
		
		private float m_EqualityLimit;

		public float EqualityLimit {
			get => m_EqualityLimit;
			set => m_EqualityLimit = value;
		}

		public QuaternionLerper() { m_EqualityLimit = 0.01f; }
		public QuaternionLerper( Quaternion startValue, Quaternion endValue ) : base( startValue, endValue ) { m_EqualityLimit = 0.01f; }
		public QuaternionLerper( Quaternion startValue, Quaternion endValue, float speed ) : base( startValue, endValue, speed ) { m_EqualityLimit = 0.01f; }

		protected override Quaternion Lerp( Quaternion a, Quaternion b, float t ) {
			return Quaternion.Lerp( a, b, t );
		}

		protected override bool Equals( Quaternion a, Quaternion b ) {
			return Quaternion.Angle( a, b ) < m_EqualityLimit;
		}
	}

	public class ColorLerper : BaseLerper<Color> {
		
		private float m_EqualityLimit;

		public float EqualityLimit {
			get => m_EqualityLimit;
			set => m_EqualityLimit = value;
		}

		public ColorLerper() { m_EqualityLimit = 0.001f; }
		public ColorLerper( Color startValue, Color endValue ) : base( startValue, endValue ) { m_EqualityLimit = 0.001f; }
		public ColorLerper( Color startValue, Color endValue, float speed ) : base( startValue, endValue, speed ) { m_EqualityLimit = 0.001f; }

		protected override Color Lerp( Color a, Color b, float t ) {
			return Color.Lerp( a, b, t );
		}

		protected override bool Equals( Color a, Color b ) {
			return ( Math.Abs( a.r - b.r ) < m_EqualityLimit && Math.Abs( a.g - b.g ) < m_EqualityLimit &&
			         Math.Abs( a.b - b.b ) < m_EqualityLimit && Math.Abs( a.a - b.a ) < m_EqualityLimit );
		}
	}
	
}