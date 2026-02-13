
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Lighting.hlsl"
#include "RaymarchingParams.hlsl"

float cubeDistance(float3 r, float3 p)
{
    float3 q = abs(p) - r;
    return length(max(q, 0.0));
}

float3 cubeNormal(float r, float3 p)
{
    return normalize(float3(
        cubeDistance(r, p + float3(DELTA, 0, 0)) - cubeDistance(r, p + float3(-DELTA, 0, 0)),
        cubeDistance(r, p + float3(0, DELTA, 0)) - cubeDistance(r, p + float3(0, -DELTA, 0)),
        cubeDistance(r, p + float3(0, 0, DELTA)) - cubeDistance(r, p + float3(0, 0, 0 - DELTA))
    ));

}

float4 cubeMarching(float r, float3 p, float3 d)
{
    for (uint i = 0; i < STEPS; i++)
    {
        float dist = cubeDistance(0.5, p);
        if (dist < EPSILON)
        {
            return float4(cubeNormal(r, p), 1);
        }
        p += d * dist;
    }
    return 0;
}

float4 cubeColor(float3 p, float3 d, float3 color)
{   
    float4 result = cubeMarching(0.5, p, normalize(p - _WorldSpaceCameraPos));
    float3 c = color * simpleLanbert(result.xyz, _MainLightPosition.xyz) * _MainLightColor;
    return float4(c.r, c.g, c.b, result.a);
}

