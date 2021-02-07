using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using System.Reflection;
using System.Collections;



public class BhapticsVRCHelper
{
    private enum VRChatLayerPreset
    {
        Interactive = 8,
        Player = 9,
        PlayerLocal = 10,
        Environment = 11,
        UiMenu = 12,
        Pickup = 13,
        PickupNoEnvironment = 14,
        StereoLeft = 15,
        StereoRight = 16,
        Walkthrough = 17,
        MirrorReflection = 18,
        reserved2 = 19,
        reserved3 = 20,
        reserved4 = 21
    }

    public static bool cameraGizmosActive;








#if UNITY_EDITOR
	/*
	static MethodInfo SetIconEnabledMethod = Assembly.GetAssembly(typeof(Editor))?.GetType("UnityEditor.AnnotationUtility")?.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
	static MethodInfo SetGizmoEnabledMethod = Assembly.GetAssembly(typeof(Editor))?.GetType("UnityEditor.AnnotationUtility")?.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
	static MethodInfo GetAnnotationsMethod = Assembly.GetAssembly(typeof(Editor))?.GetType("UnityEditor.AnnotationUtility")?.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
	static FieldInfo classIDField = Assembly.GetAssembly(typeof(Editor))?.GetType("UnityEditor.Annotation")?.GetField("classID", BindingFlags.Instance | BindingFlags.Public);
	static FieldInfo scriptClassField = Assembly.GetAssembly(typeof(Editor))?.GetType("UnityEditor.Annotation")?.GetField("scriptClass", BindingFlags.Instance | BindingFlags.Public);
	*/
    public static void ToggleCameraGizmos(bool gizmosOn)
    {
        cameraGizmosActive = gizmosOn;
		// CAUSES LAG - NEED TO FIX
		/*
        int val = gizmosOn ? 1 : 0;
		var annotations = GetAnnotationsMethod?.Invoke(null, null);
		if (annotations == null)
			return;
		foreach (object annotation in (IEnumerable)annotations)
		{
			int classId = (int)classIDField.GetValue(annotation);
			string scriptClass = (string)scriptClassField.GetValue(annotation);
			if (classId != 20) //camera classId 20. ref YAML Class ID Reference
				continue;
			SetGizmoEnabledMethod?.Invoke(null, new object[] { classId, scriptClass, val }); 
            SetIconEnabledMethod?.Invoke(null, new object[] { classId, scriptClass, val });
		}
		*/
    }
#endif














#if UNITY_EDITOR
    [MenuItem("bHaptics/Visit bHaptics homepage")]
    private static void OpenBhapticsHomepage()
    {
        Application.OpenURL("https://www.bhaptics.com");
    }

    [MenuItem("bHaptics/Show Tutorial Video")]

    private static void ShowTutorial()
    {
        Application.OpenURL("https://youtu.be/duaOPZsA5A0");
    }

    [MenuItem("bHaptics/Set Layers for VRChat")]
    private static void SetLayerForVrChat()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layerProperty = tagManager.FindProperty("layers");
        var vrChatLayers = Enum.GetValues(typeof(VRChatLayerPreset)).Cast<VRChatLayerPreset>();
        SerializedProperty layerElement;
        for (int i = 0; i < vrChatLayers.Count(); ++i)
        {
            layerElement = layerProperty.GetArrayElementAtIndex((int)vrChatLayers.ElementAt(i));
            layerElement.stringValue = vrChatLayers.ElementAt(i).ToString();
        }
        tagManager.ApplyModifiedProperties();
        Debug.Log("BhapticsVRCHelper / Apply Layer for VRChat");
    }
#endif
}