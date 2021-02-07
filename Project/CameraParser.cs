using System;
using UnityEngine;
using MelonLoader;
using UnhollowerBaseLib.Attributes;

namespace VRCBhapticsIntegration
{
	internal class VRCBhaptics_CameraParser : MonoBehaviour
	{
		private bool Enabled = true;
		private bHaptics.PositionType Position = VRCBhaptics_Config.PositionArr[0];
		private int Intensity = VRCBhaptics_Config.DefaultIntensity;
		private byte[] Value = new byte[20];
		private Color[] OldColors = null;

		public VRCBhaptics_CameraParser(IntPtr ptr) : base(ptr) { }

		[HideFromIl2Cpp]
		internal void SetupFromConfig(int index, bool clearoldcolors = false)
        {
			Enabled = VRCBhaptics_Config.Allow_bHapticsPlayer_Communication.Value && VRCBhaptics_Config.Entries_Enable[index].Value;
			Position = VRCBhaptics_Config.PositionArr[index];
			Intensity = VRCBhaptics_Config.Entries_Intensity[index].Value;
			if (clearoldcolors)
				OldColors = null;
		}

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
			if (Position == bHaptics.PositionType.VestFront)
			{
				Array.Reverse(Value, 0, 4);
				Array.Reverse(Value, 4, 4);
				Array.Reverse(Value, 8, 4);
				Array.Reverse(Value, 12, 4);
				Array.Reverse(Value, 16, 4);
			}
			else if (Position == bHaptics.PositionType.Head)
				Array.Reverse(Value, 0, 6);
			else if (Position == bHaptics.PositionType.FootR)
				Array.Reverse(Value, 0, 3);
			bHaptics.Submit(string.Concat("vrchat_", Position.ToString()), Position, Value, 100);
		}
	}
}