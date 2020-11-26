



Shader "UnityKorea/StandardRoughnessAlphaTest" {
	
	
	Properties {
	
	  [Header(Simple StandardShader)]
   	  [Space(20)]
	    
	    _Color ("Color", Color) = (1,1,1,1)
		
		
	  [Space(20)]
		
		_MainTex ("Albedo (RGB), Alpha(A)", 2D) = "white" {}
		
		
		[Space(10)]
		
		[Normal][NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Intensity", Float) = 1.0
		
		[NoScaleOffset]_MetallicGlossMap("Meatllic(R), Occlusion(G), Roughness(A)", 2D) = "white" {}
	
	
	
	   [Space(20)]
		
		_Metallic ("Metallic", Range(0,1)) = 0.5
		_Glossiness ("Roughness", Range(0,1)) = 0.5
		
		[MaterialToggle(_Ocu_ON)] _Ocu("Occlusion Toggle", float) = 0
		_OccInten("Occlusion", Range(0,1)) = 1
		
   	    [Space(30)]
		[Header(Emissive Parameter)]
		[MaterialToggle(_Emi_ON)] _Emi("Emissive Toggle", float) = 0
		_EmissiveColor("Emissive Color", Color) = (1,1,1,1)
		_EmissiveTex ("Emissive Texture(RGB)", 2D) = "white" {}
		_EmissiveInten("Emissive Inten", Range(0, 5)) = 1
		
		[Space(30)]
	    [Header(AlphaTest Setup)]
		_Cutoff     ("Cut Off", Range(0,1)) = 0.5
	    [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Off : 2side", Float) = 2
		
		
	}
	
	
	
	SubShader {
		
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull [_Cull]
		

		CGPROGRAM
		
		#pragma surface surf Standard alphatest:_Cutoff keepalpha fullforwardshadows
		#pragma target 3.0
		#pragma shader_feature _Emi_ON
		#pragma shader_feature _Ocu_ON
		

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _MetallicGlossMap;
		
		
		

		struct Input {
			
			
			float2 uv_MainTex;
	
	};

	
	    half _Glossiness;
		half _Metallic;
		half _BumpScale;
		half _OccInten;
		
		fixed4 _Color;
		
		#if _Emi_ON
		
		sampler2D _EmissiveTex;
		fixed4 _EmissiveColor;
		half _EmissiveInten;
		
		#endif

		
		UNITY_INSTANCING_BUFFER_START(Props)
		
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			fixed4 c  = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed3 nm = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex)); 
			fixed4 sg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
		
		    o.Albedo = c.rgb;
		    
		    o.Normal = nm * fixed3(1, _BumpScale, 1);
		
			o.Metallic   = sg.r * _Metallic;
			o.Smoothness = sg.a * (1.0f - _Glossiness);
			
			#if _Ocu_ON

			o.Occlusion = sg.g * _OccInten;

			#endif
			
			#if _Emi_ON
			
			fixed3 em = tex2D(_EmissiveTex, IN.uv_MainTex);
			
			o.Emission = em * _EmissiveColor * _EmissiveInten;
			
			#endif
			
			
			
			o.Alpha = c.a;
		}
		
		ENDCG
	}
	
	FallBack "Diffuse"
	
}
