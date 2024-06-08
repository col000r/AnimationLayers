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

using System.Collections.Generic;
using FaderX;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace AnimLayers {
	public class AnimationLayers : MonoBehaviour {

		public Animator Animator;
		private PlayableGraph m_Graph;
		private AnimationMixerPlayable m_MixerPlayable;
		public List<Layer> Layers = new List<Layer>();

		void OnValidate() {
			if( Animator == null ) {
				Animator = GetComponent<Animator>();
			}
		}

		void Awake() {
			if( Animator.runtimeAnimatorController != null ) {
				Animator.runtimeAnimatorController = null;
				Debug.Log( "AnimationLayers: Removed RuntimeAnimatorController from Animator.", Animator.gameObject );
			}

			// Create the PlayableGraph.
			m_Graph = PlayableGraph.Create();
			m_Graph.SetTimeUpdateMode( DirectorUpdateMode.GameTime );

			// Add an AnimationPlayableOutput to the graph.
			AnimationPlayableOutput animOutput = AnimationPlayableOutput.Create( m_Graph, "AnimationOutput", Animator );

			// Add an AnimationMixerPlayable to the graph.
			m_MixerPlayable = AnimationMixerPlayable.Create( m_Graph, Layers.Count, false );

			// Add AnimationClipPlayables to the graph.
			int layersCount = Layers.Count;
			for( int i = 0; i < layersCount; ++i ) {
				if( Layers[i].Clip != null ) {
					Layers[i].ClipPlayable = AnimationClipPlayable.Create( m_Graph, Layers[i].Clip );
					m_Graph.Connect( Layers[i].ClipPlayable, 0, m_MixerPlayable, i );
					m_MixerPlayable.SetInputWeight( i, Layers[i].InputWeight );
					if( Layers[i].ClipDuration <= 0f ) { //also make sure length is cached
						Layers[i].ClipDuration = Layers[i].Clip.length;
					}
				}
			}

			// Use the AnimationMixerPlayable as the source for the AnimationPlayableOutput.
			animOutput.SetSourcePlayable( m_MixerPlayable );

			// Play the graph.
			m_Graph.Play();
		}

		private void Start() {
			for( int index = 0; index < Layers.Count; index++ ) {
				if( Layers[index].AutoPlay ) {
					Play( index );
				}
			}
		}

		void Update() {
			for( int i = 0; i < Layers.Count; i++ ) {
				if( Layers[i] != null ) {
					if( Layers[i].Clip != null ) {
						if( Layers[i].CurrentMode == Layer.Mode.Scrubbing ) {
							if( !Layers[i].Fader.Done )	Layers[i].Value = Layers[i].Fader.CurrentValue;
							Scrub( i, Layers[i].Value ); //TODO: Temp, remove? Or check if it changed before applying.
						}
					}
				}
			}
		}


		public void Scrub( int layerId, float t, float inputWeight ) {
			SetInputWeight( layerId, inputWeight );
			Scrub( layerId, t );
		}

		public void Scrub( int layerId, float t ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer " +layerId+ " doesn't exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				Layer layer = Layers[layerId];
				if( layer != null ) {
					layer.Value = t;
					layer.CurrentMode = Layer.Mode.Scrubbing;
					layer.ClipPlayable.SetTime( layer.ClipDuration * t );
				}
				else {
					Debug.Log( layerId + " layer is null :(" );
				}
			}
		}

		public void ScrubTo( int layerId, float t, float sec ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( sec <= 0 ) {
				Scrub( layerId, t );
				return; //*******************
			}

			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].CurrentMode = Layer.Mode.Scrubbing;
					Layers[layerId].Fader.LerpTo( t, 3f / sec );
				}
			}
		}

		public void SetInputWeight( int layerId, float inputWeight ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].InputWeight = inputWeight;
					m_MixerPlayable.SetInputWeight( layerId, Layers[layerId].InputWeight );
				}
			}
		}

		public void SetSpeed( int layerId, float speed ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].Speed = speed;
					Layers[layerId].ClipPlayable.SetSpeed( speed );
				}
			}
		}

		public void Play( int layerId, float inputWeight ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].InputWeight = inputWeight;
					Play( layerId );
				}
			}
		}

		public void Play( int layerId ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].CurrentMode = Layer.Mode.Playing;
					SetSpeed( layerId, Layers[layerId].Speed );
					m_MixerPlayable.SetInputWeight( layerId, Layers[layerId].InputWeight );
					PlayableExtensions.Play( Layers[layerId].ClipPlayable );
				}
				else {
					Debug.Log( layerId + " clipPlayeable doesn't work?!" );
				}
			}
		}

		public void PlayFrom( int layerId, float t ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].ClipPlayable.SetTime( (double) t );
					Play( layerId );
				}
				else {
					Debug.Log( layerId + " clipPlayeable doesn't work?!" );
				}
			}
		}


		public void PlayAllLayers() {
			for( int i = 0; i < Layers.Count; i++ ) {
				Play( i );
			}
		}


		public void Pause( int layerId, float inputWeight ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].InputWeight = inputWeight;
					Pause( layerId );
				}
			}
		}

		public void Pause( int layerId ) {
			Debug.Assert( layerId >= 0 && layerId < Layers.Count, "Layer does not exist!" );
			if( layerId >= 0 && layerId < Layers.Count ) {
				if( Layers[layerId] != null ) {
					Layers[layerId].CurrentMode = Layer.Mode.Playing;
					m_MixerPlayable.SetInputWeight( layerId, Layers[layerId].InputWeight );
					PlayableExtensions.Pause( Layers[layerId].ClipPlayable );
				}
				else {
					Debug.Log( layerId + " clipPlayeable doesn't work?!" );
				}
			}
		}

		public void PauseAllLayers() {
			for( int i = 0; i < Layers.Count; i++ ) {
				Pause( i );
			}
		}

		public void ReplaceClip( int layerId, AnimationClip newClip ) { //TODO
			Layers[layerId].Clip = newClip;
			Layers[layerId].ClipDuration = newClip == null ? 0f : newClip.length;
			Layers[layerId].ClipPlayable = AnimationClipPlayable.Create( m_Graph, newClip );
			m_MixerPlayable.SetInputWeight( layerId, Layers[layerId].InputWeight );
		}

		private void OnDestroy() {
			m_Graph.Destroy();
		}
	}


	[System.Serializable]
	public class Layer {
		public AnimationClip Clip;
		public AnimationClipPlayable ClipPlayable;
		public float ClipDuration;
		public float Value;
		public float InputWeight;
		public float Speed;
		public bool AutoPlay;
		private FloatLerper m_Fader;

		public FloatLerper Fader {
			get {
				if( m_Fader == null ) m_Fader = new FloatLerper( Value, Value, 1f );
				return m_Fader;
			}
		}

		public enum Mode {
			Scrubbing = 0,
			Playing = 1
		}

		public Mode CurrentMode = Mode.Scrubbing;

		public Layer( AnimationClip clip, float value, float inputWeight ) {
			Clip = clip;
			if( clip != null ) {
				this.ClipDuration = clip.length;
			}
			else {
				this.ClipDuration = 0f;
			}

			Value = value;
			InputWeight = inputWeight;
			Speed = 1f;
		}

		public float GetDeltaTime() {
			return (float) ( ClipPlayable.GetPreviousTime() - ClipPlayable.GetTime() );
		}

		public float CurrentValue {
			get {
				if( Application.isPlaying ) {
					return (float) ClipPlayable.GetTime();
				}
				return Value;
			}
		}
	}

}