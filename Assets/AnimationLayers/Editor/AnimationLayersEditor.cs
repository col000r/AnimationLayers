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
using UnityEngine;
using UnityEditor;
using Core;
using UnityEngine.Playables;

namespace AnimLayers {

	[CanEditMultipleObjects]
	[CustomEditor( typeof( AnimationLayers ), true )]
	public class AnimationLayersEditor : Editor {
		private void OnEnable() {
			EditorApplication.update += Update;
		}

		private void OnDisable() {
			EditorApplication.update -= Update;
		}

		private void Update() {
			Repaint();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawScriptField(); //Draw the script field
			DrawContent(); //Draw your custom content
			serializedObject.ApplyModifiedProperties();
		}

		public virtual void DrawScriptField() {
			GUI.enabled = false;
			SerializedProperty prop = serializedObject.FindProperty( "m_Script" );
			EditorGUILayout.PropertyField( prop, true, new GUILayoutOption[0] );
			GUI.enabled = true;
		}
		
		public virtual void DrawContent() {
			AnimationLayers animLayers = (AnimationLayers) target;
			float rememberLabelWidth = EditorGUIUtility.labelWidth;

			EditorGUIUtility.labelWidth = 60f;

			GUILayout.BeginHorizontal();
			if( serializedObject.FindProperty( "Animator" ).objectReferenceValue == null ) {
				GUI.color = Color.red;
			}
			
			EditorGUILayout.PropertyField( serializedObject.FindProperty( "Animator" ) );
			if( animLayers.Animator == null ) {
				if( GUILayout.Button( "Add", GUILayout.Width( 60f ) ) ) {
					Undo.RecordObject( animLayers.gameObject, "Add Animator" );
					serializedObject.FindProperty( "Animator" ).objectReferenceValue =
						Undo.AddComponent<Animator>( animLayers.gameObject );
				}
			}

			GUI.color = Color.white;
			GUILayout.EndHorizontal();
			
			if (animLayers.Animator != null && animLayers.Animator.runtimeAnimatorController != null) {
				EditorGUILayout.HelpBox("The Animator has an AnimatorController assigned! Will automatically unassigned to avoid conflicts!", MessageType.Info);
			}

			GUILayout.Space( 4f );
			
			for( int i = 0; i < animLayers.Layers.Count; i++ ) {
				Rect r = GUILayoutUtility.GetRect( new GUIContent( "Layer" ), GUI.skin.button, GUILayout.Height( 45f ) );
				Layer l = animLayers.Layers[i];
				DrawAnimationLayerUI( r, animLayers, i );
				GUILayout.Space( 4f );
			}
			
			if( GUILayout.Button( "Add Layer" ) ) {
				Undo.RecordObject( animLayers, "Add new Layer" );
				animLayers.Layers.Add( new Layer( null, 0f, 1f ) );
			}

			EditorGUIUtility.labelWidth = rememberLabelWidth;

		}
		
		private void DrawAnimationLayerUI( Rect r, AnimationLayers animLayers, int layerId ) {
			Layer layer = animLayers.Layers[layerId];

			Color c = GUI.color;
			Color bgC = GUI.backgroundColor;

			float labelFieldWidth = 59f;
			float valueFieldWidth = 34f;

			if( r.Contains( Event.current.mousePosition ) ) {
				Repaint();
			}

			// BACKGROUND
			GUI.backgroundColor = Color.white.WithAlpha( 0.1f );
			IMGUIToolkit.DrawRect( r, IMGUIToolkit.Corners.All );

			GUI.backgroundColor = Color.black.WithAlpha( 0.45f );
			Rect idR = new Rect( r.x + 1f, r.y + 1f, 19f, r.height -2f );
			IMGUIToolkit.DrawRect( idR, IMGUIToolkit.Corners.Left );
			GUI.Label( idR, new GUIContent( layerId.ToString() ), IMGUIToolkit.Styles.LabelCentered.Style );
			if (Event.current.type == EventType.MouseDown) {
				if (idR.Contains(Event.current.mousePosition)) {
					if (Event.current.button == 1) { //Right-click option to delete
						GenericMenu menu = new GenericMenu();
						menu.AddItem( new GUIContent("Remove"), false, () => {
							if( EditorUtility.DisplayDialog( "AnimationLayers", "Remove Layer " + layerId + "?", "Remove", "Keep" ) ) {
								Undo.RecordObject( animLayers, "Remove Layer" );
								animLayers.Layers.RemoveAt( layerId );
							}
						} );
						menu.ShowAsContext();
					}
				}
			}
			
			r.x += 20f;
			r.width -= 20f;
			
			
			Rect tr = new Rect( r.x + 1f, r.y + 1f, r.width - 2f, r.height * 0.5f - 1.5f );
			Rect br = new Rect( tr.x, tr.yMax + 1f, tr.width, tr.height );

			// PLAY BUTTON
			GUI.backgroundColor = Color.black.WithAlpha( 0.45f );
			Rect playRect = new Rect( tr.x, tr.y, 15f, tr.height );
			IMGUIToolkit.DrawRect( playRect, IMGUIToolkit.Corners.None );

			if( EditorApplication.isPlaying ) {
				if( layer.Clip != null ) {
					if( GUI.Button( playRect, GUIContent.none, IMGUIToolkit.Styles.LabelCentered.Style ) ) {
						if( layer.ClipPlayable.GetPlayState() == PlayState.Paused || layer.CurrentMode == Layer.Mode.Scrubbing ) {
							if( Event.current.alt ) {
								animLayers.SetSpeed( layerId, layer.Speed);
								animLayers.PlayFrom( layerId, 0f );
							}
							else {
								animLayers.SetSpeed( layerId, layer.Speed);
								animLayers.Play( layerId );
							}
						}
						else {
							animLayers.Pause( layerId );
						}
					}
					GUI.color = Color.white.WithAlpha( 0.6f );
					GUI.DrawTexture( playRect, layer.ClipPlayable.GetPlayState() == PlayState.Paused || layer.CurrentMode == Layer.Mode.Scrubbing ? IMGUIToolkit.Styles.Play : IMGUIToolkit.Styles.Pause );
					GUI.color = c;
				}
			}
			else {
				if( GUI.Button( playRect, new GUIContent("", layer.AutoPlay ? "Auto-Play on Start" : "Don't start automatically" ), IMGUIToolkit.Styles.LabelCentered.Style ) ) {
					layer.AutoPlay = !layer.AutoPlay;
				}
				GUI.color = Color.white.WithAlpha( 0.6f );
				GUI.DrawTexture( playRect, layer.AutoPlay ? IMGUIToolkit.Styles.Play : IMGUIToolkit.Styles.Pause );
				GUI.color = c;
			}

			// WEIGHT / SPEED
			Rect weightValueRect = new Rect( tr.xMax - valueFieldWidth, tr.y, valueFieldWidth, tr.height );
			IMGUIToolkit.DrawRect( weightValueRect, IMGUIToolkit.Corners.TopRight );
			Rect weightLabelRect = new Rect( weightValueRect.xMin - 1f - 51f, tr.y, 51f, tr.height );
			IMGUIToolkit.DrawRect( weightLabelRect, IMGUIToolkit.Corners.None );
			Rect speedValueRect = new Rect( weightLabelRect.xMin - 1f - valueFieldWidth, tr.y, valueFieldWidth,
				tr.height );
			IMGUIToolkit.DrawRect( speedValueRect, IMGUIToolkit.Corners.None );
			Rect speedLabelRect = new Rect( speedValueRect.xMin - 1f - 51f, tr.y, 51f, tr.height );
			IMGUIToolkit.DrawRect( speedLabelRect, IMGUIToolkit.Corners.None );

			Rect nameRect = new Rect( playRect.xMax + 1f, tr.y,
				tr.width - playRect.width - weightValueRect.width - weightLabelRect.width - speedValueRect.width -
				speedLabelRect.width - 5f, tr.height );
			IMGUIToolkit.DrawRect( nameRect, IMGUIToolkit.Corners.None );

			
			// NAME
			GUI.Label( nameRect.Adjust( 0f, 0f, -20f, 0f ), new GUIContent( layer.Clip == null ? "[Drop AnimationClip here]" : layer.Clip.name ), IMGUIToolkit.Styles.LabelLeft.Style );
			
			// Clear Button X
			var clearButtonRect = nameRect.Adjust( nameRect.width - 21f, 0f, -nameRect.width + 21f, 0f );
			if( GUI.Button( nameRect.Adjust( nameRect.width - 21f, 0f, -nameRect.width + 21f, 0f ), GUIContent.none, IMGUIToolkit.Styles.LabelCentered.Style ) ) {
				Undo.RecordObject( animLayers, "Change Clip " + layerId );
				layer.Clip = null;
				if( EditorApplication.isPlaying ) {
					animLayers.ReplaceClip( layerId, null );
				}
			}
			GUI.color = Color.white.WithAlpha( 0.2f );
			if( clearButtonRect.Contains( Event.current.mousePosition ) ) {
				GUI.color = Color.white.WithAlpha( 0.6f );
			}
			GUI.DrawTexture( clearButtonRect.Adjust( 4f, 4f, -8f, -8f ), IMGUIToolkit.Styles.CloseX );
			GUI.color = c;
			
			
			// Invisible drop box
			GUI.color = Color.clear;
			AnimationClip newClip = EditorGUI.ObjectField( nameRect, GUIContent.none, layer.Clip, typeof( AnimationClip ), true ) as AnimationClip;
			if( newClip != layer.Clip ) {
				Undo.RecordObject( animLayers, "Change Clip " + layerId );
				layer.Clip = newClip;
				if( EditorApplication.isPlaying ) {
					animLayers.ReplaceClip( layerId, newClip );
				}
			}
			GUI.color = c;
			
			
			// Speed slider box thing
			GUI.backgroundColor = new Color( 0.22f, 0.21f, 0.11f );
			float clampedSpeed = Mathf.Clamp01( layer.Speed );
			Rect speedRect = new Rect( speedLabelRect.x + 1f, speedLabelRect.y + 1f,
				( speedLabelRect.width - 2f ) * clampedSpeed, speedLabelRect.height - 2f );
			//GUI.DrawTexture( weightRect, IMGUIToolkit.WhiteTexture );
			IMGUIToolkit.DrawRect( speedRect, IMGUIToolkit.Corners.None );

			float speed1to2 = Mathf.Clamp01( layer.Speed - 1f );
			GUI.backgroundColor = new Color( 0.31f, 0.3f, 0.15f );
			speedRect = new Rect( speedLabelRect.x + 1f, speedLabelRect.y + 1f,
				( speedLabelRect.width - 2f ) * speed1to2 * 1f, speedLabelRect.height - 2f );
			IMGUIToolkit.DrawRect( speedRect, IMGUIToolkit.Corners.None );

			// < 0f
			GUI.backgroundColor = new Color( 0.38f, 0.05f, 0.07f );
			float clampedNegSpeed = Mathf.Clamp01( layer.Speed * -1f );
			float w = ( speedLabelRect.width - 2f ) * clampedNegSpeed;
			speedRect = new Rect( speedLabelRect.x + 1f + speedLabelRect.width - 2f - w, speedLabelRect.y + 1f,
				w, speedLabelRect.height - 2f );
			IMGUIToolkit.DrawRect( speedRect, IMGUIToolkit.Corners.None );

			float speedNeg1to2 = Mathf.Clamp01( ( layer.Speed + 1f ) * -1f );
			w = ( speedLabelRect.width - 2f ) * speedNeg1to2;
			GUI.backgroundColor = new Color( 0.48f, 0.19f, 0.2f );
			speedRect = new Rect( speedLabelRect.x + 1f + speedLabelRect.width - 2f - w, speedLabelRect.y + 1f,
				w, speedLabelRect.height - 2f );
			IMGUIToolkit.DrawRect( speedRect, IMGUIToolkit.Corners.None );


			GUI.Label( speedLabelRect, new GUIContent( "Speed" ), IMGUIToolkit.Styles.LabelCentered.Style );
			var speed = EditorGUI.FloatField( speedValueRect, GUIContent.none, layer.Speed,
				IMGUIToolkit.Styles.LabelCentered.Style );

			// and now the actual field (but invisible)...
			Rect speedField = new Rect( speedLabelRect.x, speedLabelRect.y, speedLabelRect.width + speedValueRect.width,
				speedLabelRect.height );
			EditorGUIUtility.labelWidth = labelFieldWidth;
			GUI.color = Color.clear;
			speed = EditorGUI.FloatField( speedField, new GUIContent( "Speed" ), speed );
			GUI.color = c;

			// weight slider box thing
			GUI.backgroundColor = new Color( 0.21f, 0.25f, 0.3f );
			float clampedWeight = Mathf.Clamp01( layer.InputWeight );
			Rect weightRect = new Rect( weightLabelRect.x + 1f, weightLabelRect.y + 1f,
				( weightLabelRect.width - 2f ) * clampedWeight * 1f, weightLabelRect.height - 2f );
			IMGUIToolkit.DrawRect( weightRect, IMGUIToolkit.Corners.None );
			float weight1to2 = Mathf.Clamp01( layer.InputWeight - 1f );
			GUI.backgroundColor = new Color( 0.31f, 0.37f, 0.45f );
			weightRect = new Rect( weightLabelRect.x + 1f, weightLabelRect.y + 1f,
				( weightLabelRect.width - 2f ) * weight1to2 * 1f, weightLabelRect.height - 2f );
			IMGUIToolkit.DrawRect( weightRect, IMGUIToolkit.Corners.None );

			GUI.Label( weightLabelRect, new GUIContent( "Weight" ), IMGUIToolkit.Styles.LabelCentered.Style );

			var weight = EditorGUI.FloatField( weightValueRect, GUIContent.none, layer.InputWeight,
				IMGUIToolkit.Styles.LabelCentered.Style );

			// and now the actual field (but invisible)...
			Rect weightField = new Rect( weightLabelRect.x, weightLabelRect.y,
				weightLabelRect.width + weightValueRect.width, weightLabelRect.height );
			EditorGUIUtility.labelWidth = labelFieldWidth;
			GUI.color = Color.clear;
			weight = Mathf.Clamp( EditorGUI.FloatField( weightField, new GUIContent( "Weight" ), weight ), 0f, 10f );
			EditorGUIUtility.labelWidth = 0;

			GUI.color = c;


			// BOTTOM

			GUI.backgroundColor = Color.black.WithAlpha( 0.45f );

			Rect valueSliderRect = new Rect( br.x, br.y, br.width - valueFieldWidth * 2f - 1f, br.height );
			IMGUIToolkit.DrawRect( valueSliderRect, IMGUIToolkit.Corners.None ); //BottomLeft
			Rect valueRect = new Rect( valueSliderRect.xMax + 1f, br.y, valueFieldWidth * 2f, br.height );
			IMGUIToolkit.DrawRect( valueRect, IMGUIToolkit.Corners.BottomRight );

			var value = EditorGUI.FloatField( valueRect, GUIContent.none, layer.CurrentValue, IMGUIToolkit.Styles.LabelCentered.Style );

			// VALUE slider box
			Rect valueSlider = new Rect( valueSliderRect.x + 2f, valueSliderRect.y + 2f, ( valueSliderRect.width - 4f ) * Mathf.Repeat( value, 1f ), valueSliderRect.height - 4f );
			GUI.backgroundColor = Color.white.WithAlpha( 0.05f );
			IMGUIToolkit.DrawRect( valueSlider, IMGUIToolkit.Corners.None );
			GUI.backgroundColor = new Color( 0.54f, 0.54f, 0.54f );
			IMGUIToolkit.DrawRect( new Rect( Mathf.Clamp( valueSlider.xMax - 2f, valueSlider.x + 1f, 9999f ), valueSlider.y - 1f, 2f, valueSlider.height + 2f ), IMGUIToolkit.Corners.None );

			if( Event.current.type == EventType.MouseDown || ( Event.current.type == EventType.MouseDrag ) ) {
				if( valueSliderRect.Contains( Event.current.mousePosition ) ) {
					float v = Mathf.Round( Mathf.InverseLerp( valueSliderRect.xMin, valueSliderRect.xMax,
						Event.current.mousePosition.x ) * 100f ) * 0.01f;
					Repaint();
					value = v;
					Event.current.Use();
				}
			}

			GUI.color = c;
			GUI.backgroundColor = bgC;


			if( Math.Abs( speed - layer.Speed ) > 0.001f  ) {
				if( EditorApplication.isPlaying ) {
					animLayers.SetSpeed( layerId, speed );
				}
				else {
					layer.Speed = speed;
				}
			}

			if( Math.Abs( weight - layer.InputWeight ) > 0.001f ) {
				if( EditorApplication.isPlaying ) {
					animLayers.SetInputWeight( layerId, weight );
				}
				else {
					layer.InputWeight = weight;
				}
			}

			if( Math.Abs( value - layer.CurrentValue ) > 0.001f ) {
				if( EditorApplication.isPlaying ) {
					animLayers.Scrub( layerId, value );
				}
				else {
					layer.Value = value;
				}
			}

		}

	}
}