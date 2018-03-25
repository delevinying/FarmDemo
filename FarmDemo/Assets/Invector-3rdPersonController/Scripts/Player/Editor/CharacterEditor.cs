using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Character),true)]
public class CharacterEditor : Editor
{
    public override void OnInspectorGUI()
    {
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical();
		GUILayout.BeginVertical("Third Person Controller by Invector", "window");
        base.OnInspectorGUI();
		GUILayout.EndVertical();
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();
		EditorGUILayout.Space();
    }
}
