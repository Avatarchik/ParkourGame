Shader "Custom/Filmic Tonemap" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_A ("Shoulder Strength", Range(0.0,1.0)) = 0.15
		_B  ("Linear Strength", Range(0.0,1.0)) = 0.55
		_C  ("Linear Angle", Range(0.0,1.0)) = 0.1
		_D ("Toe Strength", Range(0.0,1.0)) = 0.2
		_E ("Toe Numerator", Range(0.0,1.0)) = 0.02
		_F ("Toe Denominator", Range(0.0,2.0)) = 0.7
		_W ("Weight", Range(0.0,20.0)) = 10.2
	}
	SubShader 
	{
		Pass
		{
		
		CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
		#pragma target 3.0
		#include "UnityCG.cginc"
		
		#define E 2.718281828459045f

		uniform sampler2D _MainTex;
		uniform sampler2D _CameraDepthTexture;
		uniform half _A;
		uniform half _B;
		uniform half _C;
		uniform half _D;
		uniform half _E;
		uniform half _F;
		uniform half _W;
		uniform half _V;
		uniform half _S;
		uniform half _X;
		uniform half _T;
		uniform half _I;
		uniform half _CA;
		uniform half _CT;
		uniform half _O;
		uniform half _U;
		uniform half _Z;
		uniform half4 _DF;
		
		half3 Uncharted2Tonemap(half3 x)
		{
		    return ((x*(_A*x+_C*_B)+_D*_E)/(x*(_A*x+_B)+_D*_F))-_E/_F;
		}
		
		half3 CalculateVibrance(half3 color)
		{
			half3 v = half3(_I, _I, _I);
			v = clamp(v, -0.699, 1.0);
			half3 lum = half3(0.212656f, 0.712158f, 0.072186f);
			half maxColor = max(color.r, max(color.g, color.b));
			half minColor = min(color.r, min(color.g, color.b));
			half colorSat = maxColor - minColor;
			half3 luma = dot(lum, color.rgb);
			half inter = 1.0 + (v * (1.0 - (sign(v) * colorSat)));
			return lerp(luma, color.rgb, inter);
		}
		
		half2 CalculateOptics(half2 coords, half s)
		{
		    half r2 = (coords.x - 0.5f) * (coords.x - 0.5f) + (coords.y - 0.5f) * (coords.y - 0.5f) * _ScreenParams.z  * _ScreenParams.z;
		    half f = 1 + r2 * s;
		    half2 uv = f * (coords - 0.5f) + 0.5f;	
			return uv;
		}
		
		half3 CalculateChroma(sampler2D col, half3 c, half2 coords, half coc)
		{	
			half3 constant = half3(1.005, 1.000, 1.000);
		
			half d = distance(coords, half2(0.5, 0.5));
			half f = smoothstep(0.8, 1.8, d);
			half3 chroma = pow(f + constant, _CT * coc * _CA);
			
			half2 tr = ((2.0 * coords - 1.0) * chroma.r) * 0.5 + 0.5;
			half2 tg = ((2.0 * coords - 1.0) * chroma.g) * 0.5 + 0.5;
			half2 tb = ((2.0 * coords - 1.0) * chroma.b) * 0.5 + 0.5;
			
			half r = tex2D(col, tr).r;
			half g = tex2D(col, tg).g;
			half b = tex2D(col, tb).b;
			
			half3 color = half3(r, g, b) * (1.0 - f);
			
			return color;
		}
		
		half CalculateVignette(half2 coords)
		{
			half v = half2(1.0f, 1.0f) - length( half2(0.5f, 0.5f) - coords ) / length( half2(0.5f, 0.5f) );
			return pow(v, _V);
		}
		
		half3 LetterBox(half3 c, half x)
		{
			return c.xyz = (x > _U && x < _Z) ? c.xyz : 0.0;
		}
		
		half3 CalculateSharpening(sampler2D col, half3 temp, half2 coords, half coc)
		{
			half x = (1.0h / _ScreenParams.x);
		    half y = (1.0h / _ScreenParams.y);
			
			temp = CalculateChroma(col, temp, coords, coc);
			
			half3 blur = temp * 9.0f;
		    
		    blur -= tex2Dlod(col, half4(coords + half2(x, y) * _S, 0, 0)).rgb;
		    blur -= tex2Dlod(col, half4(coords + half2(-x, -y) * _S, 0, 0)).rgb;
		    blur -= tex2Dlod(col, half4(coords + half2(-x, y) * _S, 0, 0)).rgb;
		    blur -= tex2Dlod(col, half4(coords + half2(x, -y) * _S, 0, 0)).rgb;
		    blur -= tex2Dlod(col, half4(coords + half2(-x, 0) * _S, 0, 0)).rgb;
		    blur -= tex2Dlod(col, half4(coords + half2(0, -y) * _S, 0, 0)).rgb;
		    blur -= tex2Dlod(col, half4(coords + half2(0, y) * _S, 0, 0)).rgb;
		    blur -= tex2Dlod(col, half4(coords + half2(x, 0) * _S, 0, 0)).rgb;
			
			return blur;
		}
		
		half CalculateWeight(half3 col)
		{
			half3 lumC = half3(0.299f, 0.587f, 0.114f);
			half luma = dot(col, lumC);
			half weight = 1.0f / (1.0f + luma);
			
			return weight;
		}
		
		half3 Technicolor2Strip(half3 c)
		{
			half3 temp = 1.0f - c;half3 t1 = temp.grg;half3 t2 = temp.bbr;half3 temp2 = c * t1;
			temp2 *= t2;temp = temp2;temp2 *= _X;t1 = temp.grg;
			t2 = temp.bbr;temp = c - t1;temp += temp2;
			temp2 = temp - t2;
			return lerp(c, temp2, clamp(_T, 0.0, 0.85));
		}
		
		half4 frag(v2f_img i) : COLOR
		{
			half2 coords = i.uv;
			half2 p = half2(1.0f / _ScreenParams.x, 1.0f / _ScreenParams.y);
			//half de = Linear01Depth(tex2Dlod(_CameraDepthTexture, half4(coords, 0, 0))).x;
			half CoC = _O;
			half3 colour = CalculateSharpening( _MainTex, colour, i.uv, CoC);
			half3 d = (_DF.rgb / 5.0f);
			colour = clamp(colour - d, 0.0f, 1.0f);
			colour = Technicolor2Strip(colour);
			colour = CalculateVibrance(colour);
			colour *= 16.0f;
			half e = 0.2f;
			half3 c = Uncharted2Tonemap(e * colour);
			half3 w = 1.0f / Uncharted2Tonemap(_W);
			half weight = CalculateWeight(colour);
			half3 col = c * w * (1.0f - weight);
			half3 color = pow(col, 1.0f / 2.2f);
			color = max(0.0f, color);
			half v = CalculateVignette(i.uv);
			color *= v;
			color *= _X;
			color = LetterBox(color, coords.y);
			
			return half4(color, 1.0f);
		}

		ENDCG
		
		} 
	}
}