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

[ExecuteInEditMode]
[RequireComponent(typeof(VRC.SDKBase.VRC_AvatarDescriptor))]
[AddComponentMenu("VRCBhapticsIntegrationEditor")]
public class VRCBhapticsIntegrationEditor : MonoBehaviour
{
	//public static readonly Vector3[] objectPositionOffsets = new Vector3[8];
	public static readonly Vector3[] objectRotationOffsets = new Vector3[8];
	public static readonly Vector3[] objectScaleOffsets = new Vector3[8];
	public static readonly float[] cameraSizeOffsets = new float[8];
	public static readonly float[] cameraNearOffsets = new float[8];
	public static readonly float[] cameraFarOffsets = new float[8];
    public static readonly float[] cameraDummySizeOffsets = new float[8];
	public static readonly float[] cameraDummyNearOffsets = new float[8];
	public static readonly float[] cameraDummyFarOffsets = new float[8];
	
	public static void SetupOffsets()
	{
		GetOffsetsFromPrefab("Vest", 0);
		GetOffsetsFromPrefab("LeftArm", 1);
		GetOffsetsFromPrefab("RightArm", 2);
		GetOffsetsFromPrefab("Head", 3);
		GetOffsetsFromPrefab("LeftHand", 4);
		GetOffsetsFromPrefab("RightHand", 5);
		GetOffsetsFromPrefab("LeftFoot", 6);
		GetOffsetsFromPrefab("RightFoot", 7);
	}
	public static void GetOffsetsFromPrefab(string deviceTypeStr, int offset_index)
	{
#if UNITY_EDITOR
		string prefabName = GetDevicePrefabName(deviceTypeStr);
        string path = FindAssetPath(prefabName);
        if (path == null)
        {
            Debug.LogErrorFormat("BhapticsVRCEditor / Cannot find asset {0}", deviceTypeStr);
            return;
        }
        GameObject device =  AssetDatabase.LoadAssetAtPath<GameObject>(path);
		//objectPositionOffsets[offset_index] = device.transform.position;
		objectRotationOffsets[offset_index] = device.transform.eulerAngles;
		objectScaleOffsets[offset_index] = device.transform.lossyScale;
		Camera[] cameras = device.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; ++i)
		{
			Camera cam = cameras[i];
			if (cam.name.StartsWith("Dummy"))
			{
				cameraDummySizeOffsets[offset_index] =  cam.orthographicSize;
				cameraDummyNearOffsets[offset_index] =  cam.nearClipPlane;
				cameraDummyFarOffsets[offset_index] =  cam.farClipPlane;
			}
			else
			{
				cameraSizeOffsets[offset_index] =  cam.orthographicSize;
				cameraNearOffsets[offset_index] =  cam.nearClipPlane;
				cameraFarOffsets[offset_index] =  cam.farClipPlane;
			}
		}
#endif
	}
	
	public static void SetObjectScaleFromPrefab(GameObject device, int offset_index) => 
		device.transform.localScale = objectScaleOffsets[offset_index];
	
	public static void SetObjectOffetsFromPrefab(GameObject device, int offset_index)
	{
		//device.transform.localPosition = objectPositionOffsets[offset_index];
		/*
		Vector3 currentRotation = device.transform.localEulerAngles;
		Transform currentParent = device.transform.parent;
		device.transform.SetParent(null, true);
		device.transform.eulerAngles = objectRotationOffsets[offset_index];
		device.transform.SetParent(currentParent);
		device.transform.localEulerAngles -= currentRotation;
		*/
		device.transform.localPosition = Vector3.zero;
		
		Camera[] cameras = device.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; ++i)
		{
			Camera cam = cameras[i];
			if (cam.name.StartsWith("Dummy"))
			{
				cam.orthographicSize = cameraDummySizeOffsets[offset_index];
				cam.nearClipPlane = cameraDummyNearOffsets[offset_index];
				cam.farClipPlane = cameraDummyFarOffsets[offset_index];
			}
			else
			{
				cam.orthographicSize = cameraSizeOffsets[offset_index];
				cam.nearClipPlane = cameraNearOffsets[offset_index];
				cam.farClipPlane = cameraFarOffsets[offset_index];
			}
		}
	}
	
   
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
    








    void OnEnable()
    {
#if UNITY_EDITOR
        Reset();
		
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
		
		SetupOffsets();
#endif
    }

    private void Reset()
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
		if (!enabled)
			return;

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

    public void AddDevicePrefab(string deviceTypeStr)
    {
#if UNITY_EDITOR
        if (!AvatarCheck())
        {
            return;
        }

        DestroyDevicePrefab(deviceTypeStr);

        string prefabName = GetDevicePrefabName(deviceTypeStr);
        string path = FindAssetPath(prefabName);
        if (path == null)
        {
            Debug.LogErrorFormat("BhapticsVRCEditor / Cannot find asset {0}", deviceTypeStr);
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
				SetObjectScaleFromPrefab(ins, i);
				deviceGameObjects[i] = ins;
                if (deviceTypeNames[i] == "LeftHand" || deviceTypeNames[i] == "RightHand")
                {
					ins.transform.SetParent(anim.GetBoneTransform(handBoneDic[i * 10]));
					SetObjectOffetsFromPrefab(ins, i);
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
                }
				else
				{
					ins.transform.SetParent(anim.GetBoneTransform(humanBodyBoneDic[i]));
					SetObjectOffetsFromPrefab(ins, i);
				}
                break;
            }
#endif
    }

    public void DestroyDevicePrefab(string prefabName)
    {
#if UNITY_EDITOR
        if (!AvatarCheck())
            return;
        Transform target = FindDeviceObject(GetDevicePrefabName(prefabName));
        if (target != null)
            Undo.DestroyObjectImmediate(target.gameObject); 
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

    private static string FindAssetPath(string assetName)
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
			var devicePrefabName = GetDevicePrefabName(deviceTypeNames[i]);
			var device = FindDeviceObject(devicePrefabName);
			if (device != null)
			{
				deviceGameObjects[i] = device.gameObject;
				++deviceCount;
			}
        }


        Debug.Log("BhapticsVRCEditor / Init Setup: <color=green>" + deviceCount + " device</color> detected.");
    }

    private static string GetDevicePrefabName(string deviceType)
    {
        switch (deviceType)
        {
            case "Vest":
                return "BhapticsVRC_Vest";
            case "LeftArm":
                return "BhapticsVRC_LeftArm";
            case "RightArm":
                return "BhapticsVRC_RightArm";
            case "Head":
                return "BhapticsVRC_Head";
            case "LeftHand":
                return "BhapticsVRC_LeftHand";
            case "RightHand":
                return "BhapticsVRC_RightHand";
            case "LeftFoot":
                return "BhapticsVRC_LeftFoot";
            case "RightFoot":
                return "BhapticsVRC_RightFoot";
        }
        return null;
    }
}

[ExecuteInEditMode]
[RequireComponent(typeof(VRC.SDKBase.VRC_AvatarDescriptor))]
[AddComponentMenu("BhapticsVRCEditor")]
public class BhapticsVRCEditor : VRCBhapticsIntegrationEditor {};