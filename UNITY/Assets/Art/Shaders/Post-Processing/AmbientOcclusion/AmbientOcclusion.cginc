#ifndef AMBIENTOCCLUSION_INCLUDED
#define AMBIENTOCCLUSION_INCLUDED

// Can either be 1 or 2 but nothing else.. yet
#define QUALITYBOOST 2

#define Radius 0.585
#define Intensity 3.775
#define Distance 0.7
#define Bias 0.575

float3 ViewSpacePosition(float2 coords, sampler2D depthTex, float4x4 InverseProj)
{
	float depth = LinearEyeDepth(tex2D(depthTex, coords).x);
	float4 pos = float4((coords.x - 0.5) * 2.0, (0.5 - coords.y) * -2.0, 1.0, 1.0);
	float4 ray = mul(pos, InverseProj);
	return ray.xyz * depth;
}

float RangeCheck(float a, float b)
{
	float rangeCheck = smoothstep(0.0f, 1.0f, Radius / abs(a - b));
	return rangeCheck;
}

float Noise(float2 uv)
{
	float2 noise = (frac(sin(dot(uv, float2(12.9898, 78.233)*2.0)) * 43758.5453));
	return abs(noise.x + noise.y) * 0.5;
}

float AO(float2 coords, float2 uv, float3 p, float3 n, sampler2D depth, float4x4 ip)
{
	float3 diff = ViewSpacePosition(coords + uv, depth, ip) - p;
	float3 v = normalize(diff);
	float d = length(diff) * Distance;
	float f = 1.0 / ((1.0 + d) * (1.0 + d));
	float dep = (dot(n, v) - Bias);
	float ao = f * Intensity * max(0.0, dep / (1.0 - Bias));
	return ao * RangeCheck(dep - (Bias * 2.0f), diff.z);
}

float SSAO(float2 uv, float3 N, sampler2D depthTex, sampler2D jitter, float4x4 inverseProj)
{
	#if QUALITYBOOST == 1
	const float2 Kernel[4] = { float2(1.0, 0.0), float2(-1.0, 0.0), float2(0.0, 1.0), float2(0.0, -1.0) };
	#endif
	
	#if QUALITYBOOST == 2
	const float2 Kernel[8] = {
	float2(1.0, 0.0), float2(-1.0, 0.0),
	float2(0.0, 1.0), float2(0.0, -1.0),
	float2(1.0, 1.0), float2(-1.0, 1.0),
	float2(-1.0, -1.0), float2(1.0, -1.0)
	};
	#endif
	
	//float depth = tex2D(depthTex, uv).x;
	float ao = 0.0;
	
	float3 position = ViewSpacePosition(uv, depthTex, inverseProj);
	float3 normal = N;

	float radius = Radius / position.z;
	float2 random = normalize(tex2D(jitter, _ScreenParams.xy * uv / 4.0).rg * 2.0 - 1.0);
	//float2 random = Noise(uv * 7.5);
	
	for (int j = 0; j < 4 * QUALITYBOOST; j++)
	{
		float2 coord1;

		coord1 = reflect(Kernel[j], random) * radius;
		float2 coord2 = coord1 * 0.707;
		coord2 = float2(coord2.x - coord2.y, coord2.x + coord2.y);

		ao += AO(uv, coord1 * 0.25, position, normal, depthTex, inverseProj);
		ao += AO(uv, coord2 * 0.50, position, normal, depthTex, inverseProj);
		ao += AO(uv, coord1 * 0.75, position, normal, depthTex, inverseProj);
		ao += AO(uv, coord2 * 1.00, position, normal, depthTex, inverseProj);
	}
	
	ao /= (16.0 * QUALITYBOOST);
	ao = 1.0f - ao;
	
	return ao;
}

#endif