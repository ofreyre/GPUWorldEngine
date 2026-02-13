
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Lighting.hlsl"
#include "RaymarchingParams.hlsl"

float cuberDistance(float3 r, float3 r1, float3 p)
{
    float3 q = abs(p) + r1 - r;
    return length(max(q, 0)) - r1;
}

float3 cuberNormal(float r, float3 r1, float3 p)
{
    return normalize(float3(
        cuberDistance(r, r1, p + float3(DELTA, 0, 0)) - cuberDistance(r, r1, p + float3(-DELTA, 0, 0)),
        cuberDistance(r, r1, p + float3(0, DELTA, 0)) - cuberDistance(r, r1, p + float3(0, -DELTA, 0)),
        cuberDistance(r, r1, p + float3(0, 0, DELTA)) - cuberDistance(r, r1, p + float3(0, 0, 0 - DELTA))
    ));
}

float4 cuberMarching(float r, float3 r1, float3 p, float3 d)
{
    for (uint i = 0; i < STEPS; i++)
    {
        float dist = cuberDistance(r, r1, p);
        if (dist < EPSILON)
        {
            return float4(cuberNormal(r, r1, p), 1);
        }
        p += d * dist;
    }
    return 0;
}

float4 cuberColor(float3 p, float3 d, float3 color)
{   
    float4 result = cuberMarching(0.5, 0.2, p, normalize(p - _WorldSpaceCameraPos));
    float3 c = color * simpleLanbert(result.xyz, _MainLightPosition.xyz) * _MainLightColor;
    return float4(c.r, c.g, c.b, result.a);
}

