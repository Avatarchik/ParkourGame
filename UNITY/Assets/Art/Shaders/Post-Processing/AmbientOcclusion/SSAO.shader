Shader "Custom/SSAO" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SSAO ("SSAO (R)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass
		{
		
		CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
		#pragma glsl
		#pragma target 3.0
		#include "UnityCG.cginc"
		#include "AmbientOcclusion.cginc"

		uniform sampler2D _MainTex;
		uniform sampler2D _CameraDepthTexture;
		uniform sampler2D _CameraNormalsTexture;
		uniform sampler2D _Jitter;
		uniform float4x4 _InverseProj;
		
		half4 frag(v2f_img i) : COLOR
		{
			half2 coords = i.uv;
			half3 color = tex2D(_MainTex, coords);
			half3 viewNormal = tex2D(_CameraNormalsTexture, coords);
			//viewNormal.z = 1.0 - viewNormal.z;
			half ao = SSAO(coords, viewNormal - 0.1, _CameraDepthTexture, _Jitter, _InverseProj);
			half3 skyLighting = ao * ao * ao * ao * ao;
			skyLighting = skyLighting + pow(UNITY_LIGHTMODEL_AMBIENT, 5.0f);
			return half4(color * skyLighting, 1.0f);
		}

		ENDCG
		
		} 
	}
}