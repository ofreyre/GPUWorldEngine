
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Lighting.hlsl"
#include "RaymarchingParams.hlsl"

float torusDistance(float r, float r1, float3 p)
{
    return length(float2(length(p.xy) - r, p.z)) - r1;
}

float3 torusNormal(float r, float3 r1, float3 p)
{
    return normalize(float3(
        torusDistance(r, r1, p + float3(DELTA, 0, 0)) - torusDistance(r, r1, p + float3(-DELTA, 0, 0)),
        torusDistance(r, r1, p + float3(0, DELTA, 0)) - torusDistance(r, r1, p + float3(0, -DELTA, 0)),
        torusDistance(r, r1, p + float3(0, 0, DELTA)) - torusDistance(r, r1, p + float3(0, 0, 0 - DELTA))
    ));
}

float4 torusMarching(float r, float3 r1, float3 p, float3 d)
{
    for (uint i = 0; i < STEPS; i++)
    {
        float dist = torusDistance(r, r1, p);
        if (dist < EPSILON)
        {
            return float4(torusNormal(r, r1, p), 1);
        }
        p += d * dist;
    }
    return 0;
}

float4 torusColor(float3 p, float3 d, float3 color)
{   
    float4 result = torusMarching(0.5, 0.2, p, normalize(p - _WorldSpaceCameraPos));
    float3 c = color * simpleLanbert(result.xyz, _MainLightPosition.xyz) * _MainLightColor;
    return float4(c.r, c.g, c.b, result.a);
}

