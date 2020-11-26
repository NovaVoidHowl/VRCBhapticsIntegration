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
    public static void ToggleCameraGizmos(bool gizmosOn)
    {
        cameraGizmosActive = gizmosOn;
        int val = gizmosOn ? 1 : 0;
        Assembly asm = Assembly.GetAssembly(typeof(Editor));
        Type type = asm.GetType("UnityEditor.AnnotationUtility");
        if (type != null)
        {
            MethodInfo getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            //MethodInfo setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            var annotations = getAnnotations.Invoke(null, null);
            foreach (object annotation in (IEnumerable)annotations)
            {
                Type annotationType = annotation.GetType();
                FieldInfo classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                if (classIdField != null && scriptClassField != null)
                {
                    int classId = (int)classIdField.GetValue(annotation);
                    string scriptClass = (string)scriptClassField.GetValue(annotation);

                    //camera classId 20. ref YAML Class ID Reference
                    if (classId == 20)
                    {
                        setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, val });
                    }
                    //setIconEnabled.Invoke(null, new object[] { classId, scriptClass, val });
                }
            }
        }
    }
#endif














#if UNITY_EDITOR
    [MenuItem("VRChat SDK/Bhaptics/Visit Bhaptics homepage")]
    private static void OpenBhapticsHomepage()
    {
        Application.OpenURL("https://www.bhaptics.com");
    }

    [MenuItem("VRChat SDK/Bhaptics/Show Tutorial Video")]

    private static void ShowTutorial()
    {
        Debug.Log("BhapticsVRCHelper / VRChat tutorial coming soon...");
    }

    [MenuItem("VRChat SDK/Bhaptics/Set Layers for VRChat")]
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