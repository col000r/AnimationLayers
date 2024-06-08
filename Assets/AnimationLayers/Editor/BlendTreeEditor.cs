#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEditor;
using UnityEngine;

namespace AnimLayers {
	[CanEditMultipleObjects, CustomEditor(typeof(BlendTree), true)]
	public class BlendTreeEditor : Editor {
		
		[SerializeField] private SingleGUIStyle TitleHeader;
		
		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawScriptField(); //Draw the script field
			DrawContent(); //Draw your custom content
			serializedObject.ApplyModifiedProperties();
		}

		public virtual void DrawScriptField() {
			GUI.enabled = false;
			SerializedProperty prop = serializedObject.FindProperty("m_Script");
			EditorGUILayout.PropertyField(prop, true);
			GUI.enabled = true;
		}

		private int dragId = -1;
		

		public virtual void DrawContent() {
			BlendTree blendTree = (BlendTree) target;

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AnimationLayers"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("BlendPoints"), true);
		
			Undo.RecordObject(blendTree, "BlendTree changed");
			if (blendTree.AnimationLayers != null) {
				for (int i = 0; i < blendTree.BlendPoints.Count; i++) {
					blendTree.BlendPoints[i].LayerId = EditorGUILayout.Popup("Node " + i, blendTree.BlendPoints[i].LayerId,
						blendTree.AnimationLayers.Layers
							.Select((l, id) => id.ToString() + ": " + (l.Clip == null ? "EMPTY" : l.Clip.name)).ToArray());
				}
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("XAxis"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("YAxis"));
			serializedObject.ApplyModifiedProperties();

			Rect prevRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.numberField);
			GUILayout.Space(230f);

			//GUI.Box(new Rect(prevRect.width * 0.5f - 100f, prevRect.y + prevRect.height, 200f, 200f), GUIContent.none, StateMachineEditor.ButtonBoxSimpleStyle);
			Vector2 samplePos = DrawGraph(new Rect(prevRect.width*0.5f - 100f, prevRect.y + prevRect.height, 200f, 200f),
				new Vector2(blendTree.XAxis, blendTree.YAxis), blendTree.BlendPoints, ref dragId, Repaint);

			Undo.RecordObject(blendTree.AnimationLayers, "Change MLAP");
			BlendTree.SampleWeightsCartesian(samplePos, blendTree.BlendPoints);
			foreach (BlendPoint blendInfo in blendTree.BlendPoints) {
				if(blendInfo.LayerId >= 0 && blendInfo.LayerId < blendTree.AnimationLayers.Layers.Count) blendTree.AnimationLayers.Layers[blendInfo.LayerId].InputWeight = blendInfo.CurrentWeight;
			}

			serializedObject.Update();
			serializedObject.FindProperty("XAxis").floatValue = samplePos.x;
			serializedObject.FindProperty("YAxis").floatValue = samplePos.y;

			serializedObject.ApplyModifiedProperties();
		}


		public Vector2 DrawGraph(Rect rect, Vector2 samplePos, List<BlendPoint> blendInfoList, ref int dragId, Action onChangedAction,
			float dotSize = 8f) {
			Color c = GUI.color;
			GUI.color = new Color(0.2f, 0.2f, 0.2f);
			GUI.color = new Color(0.3490566f, 0.3490566f, 0.3490566f, 1f);
			GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
			GUI.color = new Color(0.1603774f, 0.1603774f, 0.1603774f, 1f);
			GUI.DrawTexture(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f), EditorGUIUtility.whiteTexture);
			GUI.color = c;

			if (Event.current.type == EventType.MouseDown) {
				dragId = -1;
			}

			for (int i = 0; i < blendInfoList.Count; i++) {
				Rect dotRect =
					DrawDot(
						rect.position + new Vector2(blendInfoList[i].Position.x*rect.size.x, blendInfoList[i].Position.y*rect.size.y),
						blendInfoList[i].CurrentWeight, dotSize);
				DrawLabel(
					rect.position + new Vector2(blendInfoList[i].Position.x*rect.size.x, blendInfoList[i].Position.y*rect.size.y),
					i + ": " + blendInfoList[i].CurrentWeight.ToString("0.00"));

				if (Event.current.type == EventType.MouseDown) {
					if (dotRect.Contains(Event.current.mousePosition)) {
						dragId = i;
						if (Event.current.button == 1) { //Right-click option to delete
							GenericMenu menu = new GenericMenu();
							menu.AddItem(new GUIContent("Remove Node"), false, RemoveNode,
								new Dictionary<int, List<BlendPoint>>() {{i, blendInfoList}});
							menu.ShowAsContext();
						}
					}
				}
			}

			if (dragId < 0 && Event.current.type == EventType.MouseDown && Event.current.button == 1) { //Right-click menu to add
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Add Node"), false, AddNode,
					new Dictionary<Vector2, List<BlendPoint>>() { { new Vector2((Event.current.mousePosition.x - rect.position.x) / rect.size.x, (Event.current.mousePosition.y - rect.position.y) / rect.size.y), blendInfoList } });
				menu.ShowAsContext();
				dragId = -1;
			}

			DrawDot(rect.position + new Vector2(samplePos.x*rect.size.x, samplePos.y*rect.size.y), 1f, 12);

			if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag ||
			     Event.current.type == EventType.MouseUp)) {
				if (dragId < 0) {
					samplePos.x = (Mathf.Clamp(Event.current.mousePosition.x, rect.xMin, rect.xMax) - rect.position.x)/rect.size.x;
					samplePos.y = (Mathf.Clamp(Event.current.mousePosition.y, rect.yMin, rect.yMax) - rect.position.y)/rect.size.y;
				} else {
					blendInfoList[dragId].Position = new Vector2((Mathf.Clamp(Event.current.mousePosition.x, rect.xMin, rect.xMax) - rect.position.x)/rect.size.x,
						(Mathf.Clamp(Event.current.mousePosition.y, rect.yMin, rect.yMax) - rect.position.y)/rect.size.y);
				}
				onChangedAction();
			}

			if (Event.current.type == EventType.MouseUp) {
				dragId = -1;
			}
			return samplePos;
		}

		private void AddNode(object data) {
			Dictionary<Vector2, List<BlendPoint>> dict = (Dictionary<Vector2, List<BlendPoint>>)data;
			if (dict != null) {
				foreach (KeyValuePair<Vector2, List<BlendPoint>> keyValuePair in dict) {
					keyValuePair.Value.Add(new BlendPoint(keyValuePair.Key, -1));
				}
			}
		}

		private void RemoveNode(object data) {
			Dictionary<int, List<BlendPoint>> dict = (Dictionary<int, List<BlendPoint>>)data;
			if (dict != null) {
				foreach (KeyValuePair<int, List<BlendPoint>> keyValuePair in dict) {
					keyValuePair.Value.RemoveAt(keyValuePair.Key);
				}
			}
		}

		private Rect DrawDot(Vector2 pos, float alpha, float dotSize = 8f) {
			GUI.color = new Color(0.7f, 0.7f, 0.7f);
			GUI.DrawTexture(new Rect(pos.x - dotSize*0.5f - 1f, pos.y - dotSize*0.5f - 1f, dotSize + 2f, dotSize + 2f),
				EditorGUIUtility.whiteTexture);
			Rect rect = new Rect(pos.x - dotSize*0.5f, pos.y - dotSize*0.5f, dotSize, dotSize);
			GUI.color = Color.black;
			GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
			GUI.color = new Color(1f, 1f, 1f, alpha); //new Color(0.73f, 0.78f, 0.93f, alpha);
			GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
			GUI.color = Color.white;
			return rect;
		}

		private void DrawLabel(Vector2 pos, string text) {
			GUI.Label(new Rect(pos.x - 100f, pos.y + 6f, 200f, 20f), text, TitleHeader.Style);
		}
	}
}