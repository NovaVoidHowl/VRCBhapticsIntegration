using MelonLoader;

namespace VRCBhapticsIntegration
{
	internal static class VRCBhaptics_Config
	{
		internal static void Initialize()
		{
			Category = MelonPreferences.CreateCategory(BuildInfo.Name, BuildInfo.Name);
			Allow_bHapticsPlayer_Communication = (MelonPreferences_Entry<bool>)Category.CreateEntry("Allow_bHapticsPlayer_Communication", true, "Allow bHapticsPlayer Communication");
			Entries_Enable = new MelonPreferences_Entry<bool>[PositionArr.Length];
			Entries_Intensity = new MelonPreferences_Entry<int>[PositionArr.Length];
			for (int i = 0; i < PositionArr.Length; i++)
            {
				string name = ProperNames[i];
				string name_underscore = name.Replace(" ", "_");
				Entries_Enable[i] = (MelonPreferences_Entry<bool>)Category.CreateEntry(("Enable_" + name_underscore), true, ("Enable " + name));
				Entries_Intensity[i] = (MelonPreferences_Entry<int>)Category.CreateEntry((name_underscore + "_Intensity"), DefaultIntensity, (name + " Intensity"));
			}
		}

		internal static string[] ProperNames = new string[]
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

		internal static string[] RenderTextureNames = new string[]
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

		internal static bHaptics.PositionType[] PositionArr = new bHaptics.PositionType[]
		{
			bHaptics.PositionType.Head,
			bHaptics.PositionType.VestFront,
			bHaptics.PositionType.VestBack,
			bHaptics.PositionType.ForearmR,
			bHaptics.PositionType.ForearmL,
			bHaptics.PositionType.HandR,
			bHaptics.PositionType.HandL,
			bHaptics.PositionType.FootR,
			bHaptics.PositionType.FootL
		};

		internal static int DefaultIntensity = 50;
		private static MelonPreferences_Category Category = null;
		internal static MelonPreferences_Entry<bool> Allow_bHapticsPlayer_Communication = null;
		internal static MelonPreferences_Entry<bool>[] Entries_Enable = null;
		internal static MelonPreferences_Entry<int>[] Entries_Intensity = null;
	}
}