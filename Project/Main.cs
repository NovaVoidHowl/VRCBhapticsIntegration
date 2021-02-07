using System;
using System.Reflection;
using UnityEngine;
using MelonLoader;
using UnhollowerRuntimeLib;
using Harmony;

namespace VRCBhapticsIntegration
{
    internal static class BuildInfo
    {
        public const string Name = "VRCBhapticsIntegration";
        public const string Author = "Herp Derpinstine";
        public const string Company = "Lava Gang";
        public const string Version = "1.0.4";
        public const string DownloadLink = "https://github.com/HerpDerpinstine/VRCBhapticsIntegration";
    }

    public class VRCBhapticsIntegration : MelonMod
	{
		private static VRCBhaptics_CameraParser[] CameraParsers;

		public override void OnApplicationStart()
        {
			CameraParsers = new VRCBhaptics_CameraParser[VRCBhaptics_Config.PositionArr.Length];
			VRCBhaptics_Config.Initialize();
			ClassInjector.RegisterTypeInIl2Cpp<VRCBhaptics_CameraParser>();
			HarmonyInstance harmonyInstance = HarmonyInstance.Create("VRCBhapticsIntegration");
			harmonyInstance.Patch(typeof(VRCAvatarManager).GetMethod("Awake", BindingFlags.Public | BindingFlags.Instance), null, new HarmonyMethod(typeof(VRCBhapticsIntegration).GetMethod("AwakePatch", BindingFlags.NonPublic | BindingFlags.Static)));
		}

		public override void OnPreferencesSaved() { for (int i = 0; i < CameraParsers.Length; i++) { if (CameraParsers[i] == null) continue; CameraParsers[i].SetupFromConfig(i, true); } }

		private static void AwakePatch(VRCAvatarManager __instance)
		{
			VRCPlayer vrcPlayer = VRCPlayer.field_Internal_Static_VRCPlayer_0;
			if (vrcPlayer == null)
				return;
			VRCAvatarManager vrcAvatarManager = vrcPlayer.prop_VRCAvatarManager_0;
			if ((vrcAvatarManager == null) || (vrcAvatarManager != __instance))
				return;
			__instance.field_Internal_MulticastDelegateNPublicSealedVoGaVRBoUnique_0 = (
				(__instance.field_Internal_MulticastDelegateNPublicSealedVoGaVRBoUnique_0 == null)
				? new Action<GameObject, VRC.SDKBase.VRC_AvatarDescriptor, bool>(OnAvatarInstantiated)
				: Il2CppSystem.Delegate.Combine(__instance.field_Internal_MulticastDelegateNPublicSealedVoGaVRBoUnique_0, (VRCAvatarManager.MulticastDelegateNPublicSealedVoGaVRBoUnique)new Action<GameObject, VRC.SDKBase.VRC_AvatarDescriptor, bool>(OnAvatarInstantiated)).Cast<VRCAvatarManager.MulticastDelegateNPublicSealedVoGaVRBoUnique>());
			__instance.field_Internal_MulticastDelegateNPublicSealedVoGaVRBoUnique_1 = (
				(__instance.field_Internal_MulticastDelegateNPublicSealedVoGaVRBoUnique_1 == null)
				? new Action<GameObject, VRC.SDKBase.VRC_AvatarDescriptor, bool>(OnAvatarInstantiated)
				: Il2CppSystem.Delegate.Combine(__instance.field_Internal_MulticastDelegateNPublicSealedVoGaVRBoUnique_1, (VRCAvatarManager.MulticastDelegateNPublicSealedVoGaVRBoUnique)new Action<GameObject, VRC.SDKBase.VRC_AvatarDescriptor, bool>(OnAvatarInstantiated)).Cast<VRCAvatarManager.MulticastDelegateNPublicSealedVoGaVRBoUnique>());
		}

		private static Camera[] TempCameraArray = null;
        private static void OnAvatarInstantiated(GameObject __0, VRC.SDKBase.VRC_AvatarDescriptor __1, bool __2)
        {
			if ((__0 == null) || (__1 == null))
                return;
			for (int i = 0; i < CameraParsers.Length; i++)
				CameraParsers[i] = null;
			Camera[] foundcameras = __0.GetComponentsInChildren<Camera>(true);
			if (foundcameras.Length <= 0)
				return;
			if (TempCameraArray == null)
				TempCameraArray = new Camera[CameraParsers.Length];
			else
				for (int i = 0; i < TempCameraArray.Length; i++)
					TempCameraArray[i] = null;
			foreach (Camera cam in foundcameras)
			{
				RenderTexture tex = cam.targetTexture;
				if (tex == null)
					continue;
				for (int i = 0; i < VRCBhaptics_Config.RenderTextureNames.Length; i++)
                {
					if (!tex.name.Equals(VRCBhaptics_Config.RenderTextureNames[i]))
						continue;
					TempCameraArray[i] = cam;
					break;
                }
			}
			for (int i = 0; i < TempCameraArray.Length; i++)
            {
				if (TempCameraArray[i] == null)
					continue;
				CameraParsers[i] = TempCameraArray[i].gameObject.AddComponent(Il2CppType.Of<VRCBhaptics_CameraParser>()).TryCast<VRCBhaptics_CameraParser>();
				CameraParsers[i].SetupFromConfig(i);
				MelonLogger.Msg(VRCBhaptics_Config.ProperNames[i].ToString() + " Linked!");
			}
		}
	}
}