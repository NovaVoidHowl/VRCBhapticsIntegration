using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VRCBhapticsAvatarIntegration
{
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("VRCBhapticsAvatarIntegration/Camera Parser")]
	public class VRCBhapticsAvatarIntegrationCameraParser : MonoBehaviour
	{
		public enum PositionType
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
		public PositionType Position = PositionType.Head;
		public int Intensity = 50;
		public byte[] Value = new byte[20];
		private Color[] OldColors = null;
		private Texture2D TempTexture = null;
		private Rect TempTextureRect = Rect.zero;
		
		private void Start() => Initialise(Application.identifier, Application.productName);
		
		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			Graphics.Blit(src, dest);
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
			SubmitByteArray(string.Concat("vrchat_", Position.ToString()), Position, Value, Value.Length, 100);
		}
		
		[DllImport("haptic_library")]
		extern private static void Initialise(string appId, string appName);
		[DllImport("haptic_library", CallingConvention = CallingConvention.Cdecl)]
		extern private static void SubmitByteArray(string key, PositionType pos, byte[] charPtr, int length, int durationMillis);
	}
}