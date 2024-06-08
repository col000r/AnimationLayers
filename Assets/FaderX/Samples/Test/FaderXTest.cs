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

using UnityEngine;

public class FaderXTest : MonoBehaviour {

	[SerializeField] private FaderX.FloatFader m_FloatFader = new FaderX.FloatFader();
	[SerializeField] private FaderX.Vector2Fader m_Vector2Fader = new FaderX.Vector2Fader();
	[SerializeField] private AnimationCurve m_AnimCurve;
	
	[SerializeField] private FaderX.Vector2Lerper m_Vector2Lerper = new FaderX.Vector2Lerper();
	
	void OnGUI() {
		GUILayout.BeginHorizontal(  );
		if( GUILayout.Button( "Float to 0" ) ) {
			m_FloatFader.FadeTo( 0f );
		}
		if( GUILayout.Button( "Float to 100" ) ) {
			m_FloatFader.FadeTo( 100f );
		}
		GUILayout.EndHorizontal();
		GUILayout.Label( m_FloatFader.CurrentValue.ToString("0.000") );
		
		
		if(GUILayout.Button("Fade to 0")) {
			m_Vector2Fader.FadeTo( new Vector2( 0f, 200f ) );
			m_Vector2Lerper.LerpTo( new Vector2( 0f, 200f ) );
		}
		if(GUILayout.Button("Fade to 200")) {
			m_Vector2Fader.FadeTo(new Vector2(200f, 300f));
			m_Vector2Lerper.LerpTo( new Vector2( 200f, 300f ) );
		}
		
		GUILayout.BeginHorizontal(  );
		{
			if( GUILayout.Button( "Speed 1" ) ) {
				m_Vector2Lerper.Speed = 1;
			}

			if( GUILayout.Button( "Speed 5" ) ) {
				m_Vector2Lerper.Speed = 5;
			}

			if( GUILayout.Button( "Speed 15" ) ) {
				m_Vector2Lerper.Speed = 15;
			}
		}
		GUILayout.EndHorizontal();
		
		if(GUILayout.Button("Set AnimCurve")) {
			m_Vector2Fader.Curve = m_AnimCurve;
		}

		if( Event.current.type == EventType.MouseDown && Event.current.mousePosition.y > 150f ) {
			m_Vector2Fader.FadeTo( Event.current.mousePosition );
		}
		if( Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag && Event.current.mousePosition.y > 150f ) {
			m_Vector2Lerper.LerpTo( Event.current.mousePosition );
		}


		GUILayout.Label( m_Vector2Fader.CurrentValue.ToString() + " " + ( m_Vector2Fader.Done ? "DONE" : "" ) );
		GUILayout.Label( m_Vector2Lerper.CurrentValue.ToString() + " " + ( m_Vector2Lerper.Done ? "DONE" : "" ) );
		
		GUI.Box(new Rect(m_Vector2Fader.CurrentValue.x, m_Vector2Fader.CurrentValue.y, 50f, 50f), new GUIContent("F"));
		GUI.color = Color.blue;
		GUI.Box(new Rect(m_Vector2Lerper.CurrentValue.x, m_Vector2Lerper.CurrentValue.y, 50f, 50f),new GUIContent("L"));

	}
}
