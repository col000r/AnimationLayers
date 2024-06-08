using System;
using System.Collections;
using System.Collections.Generic;
using AnimLayers;
using UnityEngine;

public class AnimationLayersTest : MonoBehaviour {
	public AnimationLayers AnimLayers;

	public bool Scrub;
	public float TargetVal = 1f;
	
	void OnGUI() {
		if( GUILayout.Button( "ScrubTo 0" ) ) {
			AnimLayers.ScrubTo( 0, 0f, 1f );
		}

		if( GUILayout.Button( "ScrubTo 0.5" ) ) {
			AnimLayers.ScrubTo( 0, 0.5f, 2f );
		}

		if( GUILayout.Button( "ScrubTo 1" ) ) {
			AnimLayers.ScrubTo( 0, 1f, 2f );
		}
	}

	private void Update() {
		if( Scrub ) {
			AnimLayers.Scrub( 0, TargetVal );
		}
		
	}
}