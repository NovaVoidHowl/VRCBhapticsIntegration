Shader "Bhaptics/TouchView"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_TouchColor("TouchColor", Color) = (0,0,0,1)
		[MaterialToggle] _IsFlip("IsFlip", Float) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

		CGPROGRAM

		#pragma surface surf Standard alpha
		#pragma target 3.0

		uniform sampler2D _MainTex;
		uniform float4 _TouchColor;
		uniform fixed _IsFlip;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float2 _IsFlip_var = lerp(((IN.uv_MainTex*float2(-1, 1)) + float2(1, 0)), IN.uv_MainTex, _IsFlip);
			float4 _MainTex_var = tex2D(_MainTex, _IsFlip_var) * _TouchColor;
			o.Albedo = _MainTex_var.rgb;
			o.Alpha = _MainTex_var.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}