using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FootStepFromTexture),true)]
public class FootStepEditor : Editor 
{
    SerializedObject footStep;

    void OnEnable()
    {
        footStep = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        if (footStep == null) return;
		CheckColliders();
		EditorGUILayout.Space();
		GUILayout.BeginVertical ("box");
		GUILayout.Label ("Foot Triggers");
		EditorGUILayout.Separator ();
		footStep.FindProperty ("debugTextureName").boolValue = EditorGUILayout.Toggle("Debug Texture Name", footStep.FindProperty("debugTextureName").boolValue);
		EditorGUILayout.Separator ();
		footStep.FindProperty ("colliderRadius").floatValue = EditorGUILayout.Slider ("Radius",footStep.FindProperty ("colliderRadius").floatValue, 0.00f, 1f);
		GUILayout.BeginHorizontal ("box");
		EditorGUILayout.PropertyField (footStep.FindProperty ("leftFootTrigger"),new GUIContent("",null,"leftFootTrigger"));
		EditorGUILayout.Separator ();
		EditorGUILayout.PropertyField (footStep.FindProperty ("rightFootTrigger"),new GUIContent("",null,"rightFootTrigger"));
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		EditorGUILayout.Space();
        GUILayout.BeginVertical("Default Surface", "window");
        DrawSingleSurface(footStep.FindProperty("defaultSurface"), false);
        GUILayout.EndVertical();
		EditorGUILayout.Space();
        GUILayout.BeginVertical("Custom Surface", "window");
        DrawMultipleSurface(footStep.FindProperty("customSurfaces"));
        GUILayout.EndVertical();
        if (GUI.changed)
        {
            footStep.ApplyModifiedProperties();
        }
		EditorGUILayout.Space();
    }

	void CheckColliders()
	{
		var _footStep = (FootStepFromTexture) target;
		if (_footStep.leftFootTrigger == null) 
		{
			var animator = _footStep.transform.GetComponent<Animator>();
			var leftFT = animator.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<SphereCollider>();
			
			if(leftFT != null)
				_footStep.leftFootTrigger = leftFT;
			else
				animator.GetBoneTransform(HumanBodyBones.LeftFoot).gameObject.AddComponent<SphereCollider>();
		}
		
		if (_footStep.rightFootTrigger == null) 
		{
			var animator = _footStep.transform.GetComponent<Animator>();
			var rightFT = animator.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<SphereCollider>();
			
			if(rightFT != null)
				_footStep.rightFootTrigger = rightFT;
			else
				animator.GetBoneTransform(HumanBodyBones.RightFoot).gameObject.AddComponent<SphereCollider>();
		}
		
		if (_footStep.leftFootTrigger != null && _footStep.leftFootTrigger.gameObject.GetComponent<FootStepTrigger> () == null)
			_footStep.leftFootTrigger.gameObject.AddComponent<FootStepTrigger>();
		
		if (_footStep.rightFootTrigger != null && _footStep.rightFootTrigger.gameObject.GetComponent<FootStepTrigger> () == null) 		
			_footStep.rightFootTrigger.gameObject.AddComponent<FootStepTrigger>();
		
		if(_footStep.leftFootTrigger != null && _footStep.rightFootTrigger != null)
		{
			_footStep.leftFootTrigger.isTrigger = true;
			_footStep.rightFootTrigger.isTrigger = true;
			_footStep.leftFootTrigger.radius = _footStep.colliderRadius;
			_footStep.rightFootTrigger.radius = _footStep.colliderRadius;
		}
	}

    void DrawSingleSurface(SerializedProperty surface,bool showListNames)
    {
        GUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(surface.FindPropertyRelative("source"), false);
		EditorGUILayout.PropertyField(surface.FindPropertyRelative("name"), new GUIContent("Surface Name"), false);

        if (showListNames)
            DrawSimpleList(surface.FindPropertyRelative("TextureNames"),false);

        DrawSimpleList(surface.FindPropertyRelative("audioClips"),true);
        GUILayout.EndVertical();
    }

    void DrawMultipleSurface(SerializedProperty surfaceList)
    {
        GUILayout.BeginVertical();
        EditorGUILayout.PropertyField(surfaceList);
        if (surfaceList.isExpanded)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                surfaceList.arraySize++;
            }
            if (GUILayout.Button("Clear"))
            {
                surfaceList.arraySize = 0;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            for (int i = 0; i < surfaceList.arraySize; i++)
            {
				GUILayout.BeginHorizontal();
                GUILayout.BeginHorizontal("box");
               
                EditorGUILayout.Space();
                if (i < surfaceList.arraySize && i >= 0)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.PropertyField(surfaceList.GetArrayElementAtIndex(i));
                    if (surfaceList.GetArrayElementAtIndex(i).isExpanded) 
                    DrawSingleSurface(surfaceList.GetArrayElementAtIndex(i), true);
					EditorGUILayout.Space();
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

				if (GUILayout.Button("-"))
				{
					surfaceList.DeleteArrayElementAtIndex(i);
				}
				GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();
    }

    void DrawTextureNames(SerializedProperty textureNames)
    {
        for (int i = 0; i < textureNames.arraySize; i++)        
            EditorGUILayout.PropertyField(textureNames.GetArrayElementAtIndex(i), true);      
    }

    void DrawSimpleList(SerializedProperty list,bool useDraBox)
    {
        EditorGUILayout.PropertyField(list);

        if (list.isExpanded)
        {
            if (useDraBox)
                DrawDragBox(list);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                list.arraySize++;
            }
            if (GUILayout.Button("Clear"))
            {
                list.arraySize=0;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            for (int i = 0; i < list.arraySize; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                {
                    list.DeleteArrayElementAtIndex(i);
					if(useDraBox)
                    	list.DeleteArrayElementAtIndex(i);
                }

                if (i < list.arraySize && i >= 0)
                    EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent("", null, ""));
                
                GUILayout.EndHorizontal();
            }
        }       
    }

    void DrawDragBox(SerializedProperty list)
    {
        //var dragAreaGroup = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        GUI.skin.box.normal.textColor = Color.red;
        GUILayout.Box("Drag your audio clips here!", "box", GUILayout.MinHeight(50), GUILayout.ExpandWidth(true));
        var dragAreaGroup = GUILayoutUtility.GetLastRect();

        switch (Event.current.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dragAreaGroup.Contains(Event.current.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (var dragged in DragAndDrop.objectReferences)
                    {
                        var clip = dragged as AudioClip;                       
                        if (clip == null)
                            continue;
                        list.arraySize++;
                        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = clip;                                             
                    }
                }
			footStep.ApplyModifiedProperties();
                Event.current.Use();
                break;
        }       
    }
}
