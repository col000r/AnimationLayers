using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
	public static class OtherExtensions {
		public static bool Contains( this LayerMask layerMask, int layer ) {
			return layerMask == ( layerMask | ( 1 << layer ) );
		}

		public static T GetOrAddComponent<T>( this Component component ) where T : Component {
			return component.gameObject.GetOrAddComponent<T>();
		}

		public static T GetOrAddComponent<T>( this GameObject go ) where T : Component {
			T result = go.GetComponent<T>();
			return result ? result : go.AddComponent<T>();
		}

		
		public static void Log( this AnimationCurve animationCurve ) {
			LogAnimationCurve( animationCurve );
		}

		public static void LogAnimationCurve( AnimationCurve a, bool square = false ) {
			string str = "new AnimationCurve(new Keyframe[] { ";
			int count = 1;
			foreach( Keyframe k in a.keys ) {
				str += "new Keyframe(" + ( k.time * ( square ? k.time : 1f ) ) + "f, " + k.value + "f, " + k.inTangent +
				       "f, " + k.outTangent + "f)";
				if( count < a.keys.Length ) str += ", ";
				else str += " ";
			}

			str += " });";
			Debug.Log( str );
		}
		
			
		public static Rect Adjust(this Rect rect, float x, float y, float w, float h) {
			return new Rect(rect.x + x, rect.y + y, rect.width + w, rect.height + h);
		}
		
		public static Rect AddRect(Rect r, float x, float y, float width, float height) {
			r.x += x;
			r.y += y;
			r.width += width;
			r.height += height;
			return r;
		}

		public static Rect Add( this Rect r, float x, float y, float width, float height ) {
			r.x += x;
			r.y += y;
			r.width += width;
			r.height += height;
			return r;
		}
		
	}
}