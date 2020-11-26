using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public const string Version = "1.0.3";
        public const string DownloadLink = null;
    }

    public class VRCBhapticsIntegration : MelonMod
	{
		private static string PrefCategory = "vrcbhi";
		private static int DefaultIntensity = 50;
		private static VRCBhapticsIntegrationCameraParser[] CameraParsers;
		private static IntPtr HapticLibraryModule = IntPtr.Zero;

		private static string[] ProperNames = new string[]
		{
			"Head",
			"Vest Front",
			"Vest Back",
			"Right Arm",
			"Left Arm",
			"Right Hand",
			"Left Hand",
			"Right Foot",
			"Left Foot"
		};

		private static string[] RenderTextureNames = new string[]
		{
			"tactal_head",
			"tactot_front",
			"tactot_back",
			"tactosy_arm_right",
			"tactosy_arm_left",
			"tactosy_hand_right",
			"tactosy_hand_left",
			"tactosy_foot_right",
			"tactosy_foot_left"
		};

		private static VRCBhapticsIntegrationCameraParser.PositionType[] PositionArr = new VRCBhapticsIntegrationCameraParser.PositionType[]
		{
			VRCBhapticsIntegrationCameraParser.PositionType.Head,
			VRCBhapticsIntegrationCameraParser.PositionType.VestFront,
			VRCBhapticsIntegrationCameraParser.PositionType.VestBack,
			VRCBhapticsIntegrationCameraParser.PositionType.ForearmR,
			VRCBhapticsIntegrationCameraParser.PositionType.ForearmL,
			VRCBhapticsIntegrationCameraParser.PositionType.HandR,
			VRCBhapticsIntegrationCameraParser.PositionType.HandL,
			VRCBhapticsIntegrationCameraParser.PositionType.FootR,
			VRCBhapticsIntegrationCameraParser.PositionType.FootL
		};

		public override void OnApplicationStart()
        {
			string filepath = Path.Combine(Path.GetTempPath(), "haptic_library.dll");
			try
			{
				if (!File.Exists(filepath))
					File.WriteAllBytes(filepath, Properties.Resources.haptic_library);
				else
				{
					byte[] oldfile = File.ReadAllBytes(filepath);
					if (oldfile != Properties.Resources.haptic_library)
					{
						File.Delete(filepath);
						File.WriteAllBytes(filepath, Properties.Resources.haptic_library);
					}
				}
			}
			catch (Exception ex) { }
			HapticLibraryModule = LoadLibrary(filepath);
			if (HapticLibraryModule == IntPtr.Zero)
				throw new Exception("Unable to Load Native Haptic Library!");
			IntPtr InitialisePtr = GetProcAddress(HapticLibraryModule, "Initialise");
			if (InitialisePtr == IntPtr.Zero)
				throw new Exception("Unable to Find Initialise Export!");
			Initialise = Marshal.GetDelegateForFunctionPointer(InitialisePtr, typeof(dInitialise)) as dInitialise;
			IntPtr SubmitByteArrayPtr = GetProcAddress(HapticLibraryModule, "SubmitByteArray");
			if (SubmitByteArrayPtr == IntPtr.Zero)
				throw new Exception("Unable to Find SubmitByteArray Export!");
			VRCBhapticsIntegrationCameraParser.SubmitByteArray = Marshal.GetDelegateForFunctionPointer(SubmitByteArrayPtr, typeof(VRCBhapticsIntegrationCameraParser.dSubmitByteArray)) as VRCBhapticsIntegrationCameraParser.dSubmitByteArray;
			IntPtr TurnOffPtr = GetProcAddress(HapticLibraryModule, "TurnOff");
			if (TurnOffPtr == IntPtr.Zero)
				throw new Exception("Unable to Find TurnOff Export!");
			TurnOff = Marshal.GetDelegateForFunctionPointer(TurnOffPtr, typeof(dTurnOff)) as dTurnOff;

			CameraParsers = new VRCBhapticsIntegrationCameraParser[PositionArr.Length];

			MelonPrefs.RegisterCategory(PrefCategory, BuildInfo.Name);
			MelonPrefs.RegisterBool(PrefCategory, "allow_player_communication", true, "Allow Player Communication");
			foreach (string name in ProperNames)
			{
				MelonPrefs.RegisterBool(PrefCategory,("enable_" + name.ToLower().Replace(" ", "_")), true, ("Enable " + name));
				MelonPrefs.RegisterInt(PrefCategory, (name.ToLower().Replace(" ", "_") + "_intensity"), DefaultIntensity, (name + " Intensity"));
			}
			Initialise?.Invoke(Application.identifier, "VRChat");
			ClassInjector.RegisterTypeInIl2Cpp<VRCBhapticsIntegrationCameraParser>();

			HarmonyInstance harmonyInstance = HarmonyInstance.Create("VRCBhapticsIntegration");
			harmonyInstance.Patch(typeof(VRCAvatarManager).GetMethod("Awake", BindingFlags.Public | BindingFlags.Instance), null, new HarmonyMethod(typeof(VRCBhapticsIntegration).GetMethod("AwakePatch", BindingFlags.NonPublic | BindingFlags.Static)));
		}

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

		public override void OnApplicationQuit()
		{
			for (int i = 0; i < CameraParsers.Length; i++)
				CameraParsers[i] = null;
			Initialise = null;
			VRCBhapticsIntegrationCameraParser.SubmitByteArray = null;
            if (TurnOff != null)
            {
				TurnOff();
				TurnOff = null;
            }
			HapticLibraryModule = IntPtr.Zero;
		}

		public override void OnModSettingsApplied()
		{
			for (int i = 0; i < CameraParsers.Length; i++)
			{
				if (CameraParsers[i] == null)
					continue;
				CameraParsers[i].Enabled = (MelonPrefs.GetBool(PrefCategory, "allow_player_communication") && MelonPrefs.GetBool(PrefCategory, ("enable_" + ProperNames[i].ToLower().Replace(" ", "_"))));
				CameraParsers[i].Position = PositionArr[i];
				CameraParsers[i].Intensity = MelonPrefs.GetInt(PrefCategory, (ProperNames[i].ToLower().Replace(" ", "_") + "_intensity"));
				CameraParsers[i].OldColors = null;
			}
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
				for (int i = 0; i < RenderTextureNames.Length; i++)
                {
					if (!tex.name.Equals(RenderTextureNames[i]))
						continue;
					TempCameraArray[i] = cam;
					break;
                }
			}
			for (int i = 0; i < TempCameraArray.Length; i++)
            {
				if (TempCameraArray[i] == null)
					continue;
				CameraParsers[i] = TempCameraArray[i].gameObject.AddComponent(Il2CppType.Of<VRCBhapticsIntegrationCameraParser>()).TryCast<VRCBhapticsIntegrationCameraParser>();
				CameraParsers[i].Enabled = (MelonPrefs.GetBool(PrefCategory, "allow_player_communication") && MelonPrefs.GetBool(PrefCategory, ("enable_" + ProperNames[i].ToLower().Replace(" ", "_"))));
				CameraParsers[i].Position = PositionArr[i];
				CameraParsers[i].Intensity = MelonPrefs.GetInt(PrefCategory, (ProperNames[i].ToLower().Replace(" ", "_") + "_intensity"));
				MelonLogger.Log(ProperNames[i].ToString() + " Linked!");
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void dInitialise(string appId, string appName);
		internal static dInitialise Initialise;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void dTurnOff();
		internal static dTurnOff TurnOff;

		[DllImport("kernel32")]
		public static extern IntPtr LoadLibrary(string lpLibFileName);
		[DllImport("kernel32")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
	}

	internal class VRCBhapticsIntegrationCameraParser : MonoBehaviour
	{
		internal enum PositionType
		{
			All = 0,
			Left = 1,
			Right = 2,
			Vest = 3,
			Head = 4,
			Racket = 5,
			HandL = 6,
			HandR = 7,
			FootL = 8,
			FootR = 9,
			ForearmL = 10,
			ForearmR = 11,
			VestFront = 201,
			VestBack = 202,
			GloveLeft = 203,
			GloveRight = 204,
			Custom1 = 251,
			Custom2 = 252,
			Custom3 = 253,
			Custom4 = 254
		}
		internal bool Enabled = true;
		internal PositionType Position = PositionType.Head;
		internal int Intensity = 50;
		private byte[] Value = new byte[20];
		internal Color[] OldColors = null;

		public VRCBhapticsIntegrationCameraParser(IntPtr ptr) : base(ptr) { }

		private Texture2D TempTexture = null;
		private Rect TempTextureRect = Rect.zero;
		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			Graphics.Blit(src, dest);
			if (!Enabled)
				return;
			for (int i = 0; i < Value.Length; i++)
				Value[i] = 0;
			int width = dest.width;
			int height = dest.height;
			if (TempTexture == null)
				TempTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
			if (TempTextureRect == Rect.zero)
				TempTextureRect = new Rect(0, 0, width, height);
			TempTexture.ReadPixels(TempTextureRect, 0, 0);
			TempTexture.Apply();
			Color[] pixelcolors = TempTexture.GetPixels(0, 0, width, height);
			if (pixelcolors.Length <= 0)
				return;
			if (OldColors == null)
				OldColors = pixelcolors;
			for (int col = 0; col < height; col++)
				for (int row = 0; row < width; row++)
				{
					int bytepos = row * height + col;
					int colorpos = bytepos - 1;
					if (colorpos < 0)
						colorpos = 0;
					else if (colorpos >= 0)
						colorpos += 1;
					Color pixel = pixelcolors[colorpos];
					Color oldpixel = OldColors[colorpos];
					if (pixel != oldpixel)
						Value[colorpos] = (byte)Intensity;
				}
			Array.Reverse(Value, 0, pixelcolors.Length);
			if (Position == PositionType.VestFront)
			{
				Array.Reverse(Value, 0, 4);
				Array.Reverse(Value, 4, 4);
				Array.Reverse(Value, 8, 4);
				Array.Reverse(Value, 12, 4);
				Array.Reverse(Value, 16, 4);
			}
			else if (Position == PositionType.Head)
				Array.Reverse(Value, 0, 6);
			SubmitByteArray?.Invoke(string.Concat("vrchat_", Position.ToString()), Position, Value, Value.Length, 100);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void dSubmitByteArray(string key, PositionType pos, byte[] charPtr, int length, int durationMillis);
		internal static dSubmitByteArray SubmitByteArray;
	}
}