using System;
using System.Collections;
using VRC.SDKBase;
using UnityEngine;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;


[Serializable]
public enum BhapticsDeviceType
{
    Vest,
    LeftArm,
    RightArm,
    Head,
    LeftHand,
    RightHand,
    LeftFoot,
    RightFoot
}

[Serializable]
public enum BhapticsPrefabMode
{
    Visualized, Hidden
}

[ExecuteInEditMode]
[RequireComponent(typeof(VRC.SDKBase.VRC_AvatarDescriptor))]
[AddComponentMenu("BhapticsVRCEditor")]
public class BhapticsVRCEditor : MonoBehaviour
{
    public static readonly float[] cameraSizeOffest = new float[] { 0.19f, 0.05f, 0.05f, 0.012f, 0.022f, 0.022f, 0.015f, 0.015f };
    public static readonly float[] cameraFarOffest = new float[] { 0.14f, 0.175f, 0.175f, 0.225f, 0.15f, 0.15f, 0.175f, 0.175f };
    public static readonly Dictionary<int, HumanBodyBones> humanBodyBoneDic =
        new Dictionary<int, HumanBodyBones>()
        {
            { 0, HumanBodyBones.Chest },
            { 1, HumanBodyBones.LeftLowerArm },
            { 2, HumanBodyBones.RightLowerArm },
            { 3, HumanBodyBones.Head },
            { 4, HumanBodyBones.LeftLowerArm },
            { 5, HumanBodyBones.RightLowerArm },
            { 6, HumanBodyBones.LeftFoot },
            { 7, HumanBodyBones.RightFoot },
        };
    public static readonly Dictionary<int, HumanBodyBones> handBoneDic =
        new Dictionary<int, HumanBodyBones>()
        {
            { 40, HumanBodyBones.LeftHand },
            { 41, HumanBodyBones.LeftIndexProximal},
            { 42, HumanBodyBones.LeftIndexIntermediate },
            { 50, HumanBodyBones.RightHand },
            { 51, HumanBodyBones.RightIndexProximal },
            { 52, HumanBodyBones.RightIndexIntermediate },
        };
    public static readonly string[] deviceIconFileNames =
        new string[]
        {
            "tal_standard", "tot_standard", "tosy_left_standard", "tosy_right_standard",
            "tosyh_left_standard", "tosyh_right_standard", "tosyf_left_standard", "tosyf_right_standard"
        };
    public static readonly string[] deviceOnIconFileNames =
        new string[]
        {
            "tal_on", "tot_on", "tosy_left_on", "tosy_right_on",
            "tosyh_left_on", "tosyh_right_on", "tosyf_left_on", "tosyf_right_on"
        };


    public int DeviceCounts
    {
        get
        {
            int count = 0;
            for (int i = 0; i < deviceGameObjects.Length; ++i)
            {
                if (deviceGameObjects[i] != null)
                {
                    ++count;
                }
            }
            return count;
        }
    }
    [HideInInspector] public GameObject[] deviceGameObjects = new GameObject[Enum.GetNames(typeof(BhapticsDeviceType)).Length];
    [HideInInspector] public Transform selectedDevice;
    [HideInInspector] public Transform symmetryDevice;
    [HideInInspector] public BhapticsDeviceType selectedDeviceType;
    [HideInInspector] public bool symmetry;
    [HideInInspector] public Texture[] deviceIcons = new Texture[Enum.GetNames(typeof(BhapticsDeviceType)).Length];
    [HideInInspector] public Texture[] deviceOnIcons = new Texture[Enum.GetNames(typeof(BhapticsDeviceType)).Length];
    [HideInInspector] public Animator anim;
    [HideInInspector] public VRC_AvatarDescriptor avatarDescriptor;
    








    void Awake()
    {
#if UNITY_EDITOR
        if (GetComponents<BhapticsVRCEditor>().Length > 1)
        {
            Debug.LogError("BhapticsVRCEditor / Only one component can be added");
            DestroyImmediate(this);
            return;
        }

        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("BhapticsVRCEditor / Required Animator component & avatar.");
            return;
        }

        if (anim.avatar == null)
        {
            Debug.LogError("BhapticsVRCEditor / Required Animator component & avatar.");
            return;
        }

        avatarDescriptor = GetComponent<VRC_AvatarDescriptor>();
        if (avatarDescriptor == null)
        {
            Debug.LogError("BhapticsVRCEditor / Required VRC AvatarDescriptor component.");
            return;
        }

        for (int i = 0; i < deviceIcons.Length; ++i)
        {
            var path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(deviceIconFileNames[i], null)[0]);
            deviceIcons[i] = AssetDatabase.LoadAssetAtPath<Texture>(path);
        }
        for (int i = 0; i < deviceOnIcons.Length; ++i)
        {
            var path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(deviceOnIconFileNames[i], null)[0]);
            deviceOnIcons[i] = AssetDatabase.LoadAssetAtPath<Texture>(path);
        }
#endif
    }

    private void Reset()
    {
        Awake();
        InitSetupDeviceGameObjects();
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        InitSetupDeviceGameObjects();
        BhapticsVRCHelper.ToggleCameraGizmos(false);
#endif
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        BhapticsVRCHelper.ToggleCameraGizmos(true);
#endif
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (anim == null)
        {
            return;
        }
        if (anim.avatar == null)
        {
            return;
        }
        if (avatarDescriptor == null)
        {
            return;
        }

        if (Selection.activeGameObject != null && Selection.activeGameObject == gameObject)
        {
            if (BhapticsVRCHelper.cameraGizmosActive)
            {
                BhapticsVRCHelper.ToggleCameraGizmos(false);
            }
            if (selectedDevice != null)
            {
                if(selectedDeviceType == BhapticsDeviceType.LeftHand || selectedDeviceType == BhapticsDeviceType.RightHand)
                {
                    var joints = selectedDevice.GetComponentsInChildren<ParentConstraint>();
                    for (int i = 0; i < joints.Length - 1; ++i)
                    {
                        Gizmos.color = new Color(1f, 1f, 0f, 1f);
                        Gizmos.DrawSphere(joints[i].transform.position, 0.01f);

                        var targetBone = anim.GetBoneTransform(handBoneDic[i + (int)selectedDeviceType * 10]);
                        if (targetBone != null)
                        {
                            Gizmos.color = new Color(0f, 1f, 0f, 1f);
                            Gizmos.DrawSphere(targetBone.position, 0.01f);
                        }
                    }
                }
                else
                {
                    Gizmos.color = new Color(1f, 1f, 0.5f, 0.1f);
                    var skin = selectedDevice.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (skin != null)
                    {
                        Gizmos.DrawWireMesh(skin.sharedMesh, skin.transform.position, skin.transform.rotation, skin.transform.lossyScale);
                    }
                    else
                    {
                        var meshFilter = selectedDevice.GetComponentInChildren<MeshFilter>();
                        if (meshFilter != null)
                        {
                            Gizmos.DrawWireMesh(meshFilter.sharedMesh, meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
                        }
                    }
                }
            }
        }
        else
        {
            if (!BhapticsVRCHelper.cameraGizmosActive)
            {
                BhapticsVRCHelper.ToggleCameraGizmos(true);
            }
        }
#endif
    }













    public void AddDevicePrefab(string deviceTypeStr, BhapticsPrefabMode prefabMode)
    {
        AddDevicePrefab(deviceTypeStr, prefabMode, false, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    public void AddDevicePrefab(string deviceTypeStr, BhapticsPrefabMode prefabMode, bool activeOffset, Vector3 posOffset, Quaternion rotOffset, Vector3 scaleOffset)
    {
#if UNITY_EDITOR
        if (!AvatarCheck())
        {
            return;
        }

        DestroyDevicePrefab(deviceTypeStr);

        string prefabName = GetDevicePrefabName(deviceTypeStr, prefabMode);
        string path = FindAssetPath(prefabName);
        if (path == null)
        {
            Debug.LogErrorFormat("BhapticsVRCEditor / Cannot find asset {0}, {1}", deviceTypeStr, prefabMode);
            return;
        }
        var ins = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), transform);
        ins.name = prefabName;
        Undo.RegisterCreatedObjectUndo(ins, "Create New GameObject");
        Undo.RecordObjects(ins.GetComponentsInChildren<Transform>(), "Change Transform Position");

            var deviceTypeNames = Enum.GetNames(typeof(BhapticsDeviceType));
            for (int i = 0; i < deviceTypeNames.Length; ++i)
            {
                if (deviceTypeStr != deviceTypeNames[i])
                {
                    continue;
                }
                if (deviceTypeNames[i] == "LeftHand" || deviceTypeNames[i] == "RightHand")
                {
                    ins.transform.parent = anim.GetBoneTransform(humanBodyBoneDic[i]);
                    if (activeOffset)
                    {
                        ins.transform.localPosition = posOffset;
                        ins.transform.localRotation = rotOffset;
                        ins.transform.localScale = scaleOffset;
                    }
                    var joints = ins.GetComponentsInChildren<ParentConstraint>();
                    for (int jointIndex = 0; jointIndex < joints.Length; ++jointIndex)
                    {
                        var constraintSource = new ConstraintSource();
                        constraintSource.weight = 1f;
                        constraintSource.sourceTransform = anim.GetBoneTransform(handBoneDic[jointIndex + i * 10]);
                        if (constraintSource.sourceTransform != null)
                        {
                            joints[jointIndex].SetSource(0, constraintSource);
                        }
                        else
                        {
                            Debug.LogError("BhapticsVRCEditor / " + deviceTypeNames[i] + " device's parentConstraint setting is wrong!"
                                + "\n" + "<color=red>" + handBoneDic[jointIndex + i * 10] + " is null.</color>");
                        }
                    }
                    deviceGameObjects[i] = ins;
                }
                else
                {
                    ins.transform.parent = anim.GetBoneTransform(humanBodyBoneDic[i]);
                    if (activeOffset)
                    {
                        ins.transform.localPosition = posOffset;
                        ins.transform.localRotation = rotOffset;
                        ins.transform.localScale = scaleOffset;
                    }
                    deviceGameObjects[i] = ins;
                }
                break;
            }
#endif
    }

    public void DestroyDevicePrefab(string prefabName)
    {
#if UNITY_EDITOR
        if (!AvatarCheck())
        {
            return;
        }
        Transform target = null;
        switch (prefabName)
        {
            case "Vest":
                if (target == null) target = FindDeviceObject("BhapticsVRC_Vest_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_Vest_Hidden");
                break;                                                    
            case "LeftArm":                                               
                if (target == null) target = FindDeviceObject("BhapticsVRC_LeftArm_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_LeftArm_Hidden");
                break;                                                    
            case "RightArm":                                              
                if (target == null) target = FindDeviceObject("BhapticsVRC_RightArm_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_RightArm_Hidden");
                break;                                                    
            case "Head":                                                  
                if (target == null) target = FindDeviceObject("BhapticsVRC_Head_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_Head_Hidden");
                break;                                                    
            case "LeftHand":                                              
                if (target == null) target = FindDeviceObject("BhapticsVRC_LeftHand_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_LeftHand_Hidden");
                break;                                                    
            case "RightHand":                                             
                if (target == null) target = FindDeviceObject("BhapticsVRC_RightHand_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_RightHand_Hidden");
                break;                                                    
            case "LeftFoot":                                              
                if (target == null) target = FindDeviceObject("BhapticsVRC_LeftFoot_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_LeftFoot_Hidden");
                break;                                                    
            case "RightFoot":                                             
                if (target == null) target = FindDeviceObject("BhapticsVRC_RightFoot_Visualized");
                if (target == null) target = FindDeviceObject("BhapticsVRC_RightFoot_Hidden");
                break;
        }
        if (target != null)
        {
            Undo.DestroyObjectImmediate(target.gameObject);
        }

        return;
#endif
    }

    public Transform FindDeviceObject(string targetName)
    {
        return FindDeviceObject(transform, targetName);
    }

    public Vector3 RoundVector3(Vector3 value, int pointIndex = 4)
    {
        float x = value.x;
        float y = value.y;
        float z = value.z;

        if (pointIndex < 0)
        {
            return value;
        }
        else if (pointIndex == 0)
        {
            return new Vector3((int)x, (int)y, (int)z);
        }
        else
        {
            x = Mathf.Round(x * Mathf.Pow(10f, pointIndex)) / Mathf.Pow(10f, pointIndex);
            y = Mathf.Round(y * Mathf.Pow(10f, pointIndex)) / Mathf.Pow(10f, pointIndex);
            z = Mathf.Round(z * Mathf.Pow(10f, pointIndex)) / Mathf.Pow(10f, pointIndex);
            return new Vector3(x, y, z);
        }
    }

    public int ConvertIconIndexToDeviceTypeIndex(int iconIndex)
    {
        switch (iconIndex)
        {
            case 0: return 3;
            case 1: return 0;
            case 2: return 1;
            case 3: return 2;
            case 4: 
            case 5: 
            case 6: 
            case 7: return iconIndex;
            default: return -1;
        }
    }
    




















    private bool AvatarCheck()
    {
        if (anim == null)
        {
            Debug.LogError("BhapticsVRCEditor / Animator component is null.");
            return false;
        }

        if (anim.avatar.isHuman == false)
        {
            Debug.LogError("BhapticsVRCEditor / Set avatar's Animation Type to Humanoid.");
            return false;
        }

        return true;
    }

    private string FindAssetPath(string assetName)
    {
#if UNITY_EDITOR
        return AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(assetName, null)[0]);
#else
        return null;
#endif
    }

    private Transform FindDeviceObject(Transform findParent, string deviceObjectName)
    {
        if (findParent == null)
        {
            return null;
        }
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(findParent);
        while (queue.Count > 0)
        {
            var pick = queue.Dequeue();
            if (pick.name == deviceObjectName)
            {
                return pick;
            }
            for (int i = 0; i < pick.childCount; ++i)
            {
                queue.Enqueue(pick.GetChild(i));
            }
        }
        return null;
    }

    private void InitSetupDeviceGameObjects()
    {
        int deviceCount = 0;
        var deviceTypeNames = Enum.GetNames(typeof(BhapticsDeviceType));
        for (int i = 0; i < deviceTypeNames.Length; ++i)
        {
            for (int j = 0; j < Enum.GetNames(typeof(BhapticsPrefabMode)).Length; ++j)
            {
                var devicePrefabName = GetDevicePrefabName(deviceTypeNames[i], (BhapticsPrefabMode)j);
                var device = FindDeviceObject(devicePrefabName);
                if (device != null)
                {
                    deviceGameObjects[i] = device.gameObject;
                    ++deviceCount;
                }
            }
        }


        Debug.Log("BhapticsVRCEditor / Init Setup: <color=green>" + deviceCount + " device</color> detected.");
    }

    private string GetDevicePrefabName(string deviceType, BhapticsPrefabMode prefabMode)
    {
        switch (deviceType)
        {
            case "Vest":
                return "BhapticsVRC_Vest_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
            case "LeftArm":
                return "BhapticsVRC_LeftArm_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
            case "RightArm":
                return "BhapticsVRC_RightArm_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
            case "Head":
                return "BhapticsVRC_Head_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
            case "LeftHand":
                return "BhapticsVRC_LeftHand_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
            case "RightHand":
                return "BhapticsVRC_RightHand_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
            case "LeftFoot":
                return "BhapticsVRC_LeftFoot_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
            case "RightFoot":
                return "BhapticsVRC_RightFoot_" + Enum.GetName(typeof(BhapticsPrefabMode), prefabMode);
        }
        return null;
    }
}
