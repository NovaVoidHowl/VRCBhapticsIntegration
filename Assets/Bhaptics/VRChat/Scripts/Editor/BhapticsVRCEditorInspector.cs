using System;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(BhapticsVRCEditor))]
public class BhapticsVRCEditorInspector : Editor
{
    public BhapticsVRCEditor script
    {
        get
        {
            return target as BhapticsVRCEditor;
        }
    }

	public void FixCameraScaling(Camera cam)
	{
		if (cam.name.StartsWith("Dummy"))
		{
			cam.orthographicSize = ((script.selectedDevice.lossyScale.y > script.selectedDevice.lossyScale.x) ? script.selectedDevice.lossyScale.y : script.selectedDevice.lossyScale.x) 
			* BhapticsVRCEditor.cameraDummySizeOffsets[(int)script.selectedDeviceType];
			cam.nearClipPlane = script.selectedDevice.lossyScale.z * BhapticsVRCEditor.cameraDummyNearOffsets[(int)script.selectedDeviceType];
			cam.farClipPlane = script.selectedDevice.lossyScale.z * BhapticsVRCEditor.cameraDummyFarOffsets[(int)script.selectedDeviceType];
		}
		else
		{
			cam.orthographicSize = ((script.selectedDevice.lossyScale.y > script.selectedDevice.lossyScale.x) ? script.selectedDevice.lossyScale.y : script.selectedDevice.lossyScale.x) * BhapticsVRCEditor.cameraSizeOffsets[(int)script.selectedDeviceType];
			cam.nearClipPlane = script.selectedDevice.lossyScale.z * BhapticsVRCEditor.cameraNearOffsets[(int)script.selectedDeviceType];
			cam.farClipPlane = script.selectedDevice.lossyScale.z * BhapticsVRCEditor.cameraFarOffsets[(int)script.selectedDeviceType];
		}
	}

    public override void OnInspectorGUI()
    {
		if (script == null)
            return;
	
        if (script.anim == null)
        {
            EditorGUILayout.HelpBox("BhapticsVRCEditor / Required Animator component & avatar.", MessageType.Error);
            return;
        }

        if (script.anim.avatar == null)
        {
            EditorGUILayout.HelpBox("BhapticsVRCEditor / Required Animator component & avatar.", MessageType.Error);
            return;
        }

        if (script.avatarDescriptor == null)
        {
            EditorGUILayout.HelpBox("BhapticsVRCEditor / Required VRC AvatarDescriptor component.", MessageType.Error);
            return;
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIStyle deviceIconStyle = new GUIStyle(GUI.skin.label);
        deviceIconStyle.margin = new RectOffset(20, 20, 12, 5);

        GUIStyle deviceOnIconStyle = new GUIStyle(GUI.skin.label);
        deviceOnIconStyle.margin = new RectOffset(20, 20, 12, 5);

        for (int i = 0; i < script.deviceIcons.Length; ++i)
        {
            if (script.deviceIcons[i] != null)
            {
                var deviceTypeIndex = script.ConvertIconIndexToDeviceTypeIndex(i);
                if (deviceTypeIndex == -1)
                {
                    continue;
                }
                if (script.deviceGameObjects[deviceTypeIndex] == null)
                {
                    if (GUILayout.Button(new GUIContent(script.deviceIcons[i], "CLICK: Add object"), deviceIconStyle, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        script.selectedDeviceType = (BhapticsDeviceType)deviceTypeIndex;
                        script.AddDevicePrefab(script.selectedDeviceType.ToString());
                        Debug.Log("BhapticsVRCEditor / <color=green>Add </color>" + script.selectedDeviceType.ToString() + " Object");
                    }
                }
                else
                {
                    if (GUILayout.Button(script.deviceOnIcons[i], deviceOnIconStyle, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        script.selectedDeviceType = (BhapticsDeviceType)deviceTypeIndex;
                        OnSceneGUI();
                    }
                }
                if (Screen.width < 580 && i == script.deviceIcons.Length / 2 - 1)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button("Finish Editor", GUILayout.Height(30)))
        {
            Undo.DestroyObjectImmediate(script);
        }

        EditorGUILayout.HelpBox("If you finished setup, press [Finish Editor] button.", MessageType.Info);
        GUILayout.Space(2);

        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
        {
            script.DestroyDevicePrefab(script.selectedDeviceType.ToString());
            Debug.Log("BhapticsVRCEditor / <color=red>Delete </color>" + script.selectedDeviceType.ToString() + " Object");
            Event.current.Use();
            return;
        }
    }

	private Transform previousGameObject;
	private bool IsVisualized;
    void OnSceneGUI()
    {
        if (script == null)
        {
            return;
        }
		
		if (!script.enabled)
			return;

        if (script.anim == null)
        {
            return;
        }

        if (script.anim.avatar == null)
        {
            return;
        }

        if (script.avatarDescriptor == null)
        {
            return;
        }

        if (Event.current != null && Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(0);
        }

        Handles.BeginGUI();

        GUIStyle windowStyle = new GUIStyle("Window");
        windowStyle.margin.left = 15;
        GUILayout.BeginVertical("Bhaptics VRC Editor", windowStyle, GUILayout.Width(300), GUILayout.MaxHeight(10), GUILayout.ExpandHeight(true));

        GUILayout.BeginHorizontal();
        GUIStyle topMarginStyle = new GUIStyle(GUI.skin.label);
        topMarginStyle.margin.top = 0;
        GUILayout.Label("Device Type", topMarginStyle, GUILayout.Width(80));

        GUILayout.BeginVertical();
        script.selectedDeviceType = (BhapticsDeviceType)EditorGUILayout.EnumPopup(string.Empty, script.selectedDeviceType, GUILayout.ExpandWidth(true), GUILayout.MinWidth(10));
        GUILayout.EndVertical();

        script.selectedDevice = script.deviceGameObjects[(int)script.selectedDeviceType] == null ? null : script.deviceGameObjects[(int)script.selectedDeviceType].transform;
        if (script.selectedDevice != null)
        {
            GUIStyle selectButtonStyle = new GUIStyle(GUI.skin.button);
            selectButtonStyle.margin.top = 0;
            if (GUILayout.Button("Select", selectButtonStyle, GUILayout.Width(50)))
            {
                Selection.activeGameObject = script.selectedDevice.gameObject;
            }
        }
        GUILayout.EndHorizontal();

        if (script.selectedDevice == null)
        {
            GUILayout.Space(10);
            script.symmetry = false;
            GUIStyle addButtonStyle = new GUIStyle(GUI.skin.button);
            addButtonStyle.richText = true;
            if (GUILayout.Button("<color=green>Add</color> " + script.selectedDeviceType + " Object", addButtonStyle, GUILayout.Height(30)))
            {
                script.AddDevicePrefab(script.selectedDeviceType.ToString());
                Debug.Log("BhapticsVRCEditor / <color=green>Add </color>" + script.selectedDeviceType.ToString() + " Object");
                return;
            }
        }
        else
        {
            GUILayout.Space(2);

			if (previousGameObject != script.selectedDevice)
			{
				previousGameObject = script.selectedDevice;
				MeshRenderer[] render = script.selectedDevice.GetComponentsInChildren<MeshRenderer>();
				IsVisualized = render[0].enabled;
			}
			
            GUILayout.BeginHorizontal();
            GUILayout.Label("Visible Mode", GUILayout.Width(80));
            GUIStyle visualButtonStyle = new GUIStyle(GUI.skin.button);
            visualButtonStyle.margin.top = 1;
            if (GUILayout.Button(IsVisualized ? "Visualized" : "Hidden", visualButtonStyle, GUILayout.Width(100), GUILayout.Height(18)))
            {
				IsVisualized = !IsVisualized;
				MeshRenderer[] render = script.selectedDevice.GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer renderer in render)
					renderer.enabled = IsVisualized;
				SkinnedMeshRenderer[] render2 = script.selectedDevice.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer renderer in render2)
					renderer.enabled = IsVisualized;
                string visual = "<color=green>Visualized</color>";
                string hide = "<color=red>Hidden</color>";
                Debug.Log("BhapticsVRCEditor / Change Visible Mode " + (IsVisualized ? visual : hide) + " -> " + (IsVisualized ? hide : visual));
                return;
            }
            GUILayout.EndHorizontal();

            var isLeft = script.selectedDevice.name.Contains("Left");
            var isRight = script.selectedDevice.name.Contains("Right");
            if (isLeft || isRight)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Symmetry", GUILayout.Width(80));
                script.symmetry = EditorGUILayout.Toggle(script.symmetry);
                GUILayout.EndHorizontal();

                if (script.symmetry)
                {
                    if (isLeft)
                    {
                        script.symmetryDevice = script.FindDeviceObject(script.selectedDevice.name.Replace("Left", "Right"));
                    }
                    else if (isRight)
                    {
                        script.symmetryDevice = script.FindDeviceObject(script.selectedDevice.name.Replace("Right", "Left"));
                    }

                    if (script.symmetryDevice == null)
                    {
                        EditorGUILayout.HelpBox("Symmetry object is not found! Create one.", MessageType.Error);
                    }
                }
                else
                {
                    script.symmetryDevice = null;
                }
            }
            else
            {
                script.symmetryDevice = null;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Edit Mode", GUILayout.Width(80));

            GUILayout.BeginVertical();

            GUIStyle boldStyle = new GUIStyle(GUI.skin.label);
            boldStyle.fontStyle = FontStyle.Bold;

            GUIStyle tooltipStyle = new GUIStyle(GUI.skin.label);
            tooltipStyle.richText = true;
            tooltipStyle.padding.top = -3;

            if (Tools.current == Tool.Move)
            {
                GUILayout.Label("Position", boldStyle, GUILayout.Width(60));
                GUILayout.Label("<size=9>*shortcut = </size><b>[W]</b><size=9> E R</size>", tooltipStyle);
            }
            else if (Tools.current == Tool.Rotate)
            {
                GUILayout.Label("Rotation", boldStyle, GUILayout.Width(60));
                GUILayout.Label("<size=9>*shortcut = W </size><b>[E]</b><size=9> R</size>", tooltipStyle);
            }
            else if (Tools.current == Tool.Scale)
            {
                GUILayout.Label("Scale", boldStyle, GUILayout.Width(60));
                GUILayout.Label("<size=9>*shortcut = W E </size><b>[R]</b>", tooltipStyle);
            }
            else
            {
                GUILayout.Label("Etc");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Position", GUILayout.Width(80));
            script.selectedDevice.localPosition = EditorGUILayout.Vector3Field("", script.RoundVector3(script.selectedDevice.localPosition), GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Rotation", GUILayout.Width(80));
            script.selectedDevice.localEulerAngles = EditorGUILayout.Vector3Field("", script.RoundVector3(script.selectedDevice.localEulerAngles), GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scale", GUILayout.Width(80));
            script.selectedDevice.localScale = EditorGUILayout.Vector3Field("", script.RoundVector3(script.selectedDevice.localScale), GUILayout.Width(200));
            var cameras = script.selectedDevice.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cameras.Length; ++i)
				FixCameraScaling(cameras[i]);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUIStyle deleteButtonStyle = new GUIStyle(GUI.skin.button);
            deleteButtonStyle.richText = true;
            if (GUILayout.Button("<color=red>Delete</color> " + script.selectedDeviceType + " Object [Del]", deleteButtonStyle, GUILayout.Height(30)))
            {
				Undo.DestroyObjectImmediate(script.selectedDevice.gameObject);
                Debug.Log("BhapticsVRCEditor / <color=red>Delete </color>" + script.selectedDeviceType.ToString() + " Object");
                return;
            }
        }
        GUILayout.Space(2);

        GUILayout.EndVertical();
        Handles.EndGUI();




        Color buttonColor = new Color(0.9f, 0.9f, 0.3f);
        Color defaultColor = new Color(0.6f, 0.6f, 0.6f);
        Handles.color = buttonColor;

        for (int i = 0; i < script.deviceGameObjects.Length; ++i)
        {
            if (script.deviceGameObjects[i] == null)
            {
                continue;
            }
            if (script.selectedDevice != null && script.selectedDevice == script.deviceGameObjects[i].transform)
            {
                continue;
            }
            var buttonSize = HandleUtility.GetHandleSize(script.deviceGameObjects[i].transform.position) / 10f;
            if (Handles.Button(script.deviceGameObjects[i].transform.position, Quaternion.identity, buttonSize, buttonSize, Handles.DotHandleCap))
            {
                script.selectedDeviceType = (BhapticsDeviceType)i;
                return;
            }
        }
        Handles.color = defaultColor;

        if (script.selectedDevice != null)
        {
            try
            {
                if (Tools.current == Tool.Move)
                {
                    Handles.CubeHandleCap(0, script.selectedDevice.position, Quaternion.identity, HandleUtility.GetHandleSize(script.selectedDevice.position) / 7f, EventType.Repaint);
                    script.selectedDevice.position = Handles.PositionHandle(script.selectedDevice.position, script.selectedDevice.rotation);
                }
                else if (Tools.current == Tool.Rotate)
                {
                    script.selectedDevice.rotation = Handles.RotationHandle(script.selectedDevice.rotation, script.selectedDevice.position);
                }
                else if (Tools.current == Tool.Scale)
                {
                    script.selectedDevice.localScale = Handles.ScaleHandle(script.selectedDevice.localScale, script.selectedDevice.position, script.selectedDevice.rotation,
                                                                           HandleUtility.GetHandleSize(script.selectedDevice.position));
                    var cameras = script.selectedDevice.GetComponentsInChildren<Camera>(true);
                    for (int i = 0; i < cameras.Length; ++i)
						FixCameraScaling(cameras[i]);
                }
            }
            catch (Exception e)
            {
            }

            if (script.symmetry && script.symmetryDevice != null)
            {
                script.symmetryDevice.position = Vector3.Reflect(script.selectedDevice.position - script.transform.position, script.transform.right);
                script.symmetryDevice.eulerAngles = Vector3.Reflect(script.selectedDevice.eulerAngles, script.transform.up);
                script.symmetryDevice.localScale = script.selectedDevice.localScale;
                var symmetryCameras = script.symmetryDevice.GetComponentsInChildren<Camera>(true);
                for (int i = 0; i < symmetryCameras.Length; ++i)
					FixCameraScaling(symmetryCameras[i]);
                Undo.RecordObjects(new Transform[] { script.selectedDevice, script.symmetryDevice }, "Change TargetObject and SymmetryTargetObject");
            }
            else
            {
                Undo.RecordObject(script.selectedDevice, "Change TargetObject");
            }

            if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {
                script.DestroyDevicePrefab(script.selectedDeviceType.ToString());
                Debug.Log("BhapticsVRCEditor / <color=red>Delete </color>" + script.selectedDeviceType.ToString() + " Object");
                Event.current.Use();
                return;
            }
        }
    }
}