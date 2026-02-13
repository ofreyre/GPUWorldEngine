
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Lighting.hlsl"
#include "RaymarchingParams.hlsl"

float sphereDistance(float r, float3 p)
{
    return length(p) - r;
}

float3 sphereNormal(float r, float3 p)
{
    return normalize(float3(
        sphereDistance(r, p + float3(DELTA, 0, 0)) - sphereDistance(r, p + float3(-DELTA, 0, 0)),
        sphereDistance(r, p + float3(0, DELTA, 0)) - sphereDistance(r, p + float3(0, -DELTA, 0)),
        sphereDistance(r, p + float3(0, 0, DELTA)) - sphereDistance(r, p + float3(0, 0, 0 - DELTA))
    ));

}

float4 sphereMarching(float r, float3 p, float3 d)
{
    for (uint i = 0; i < STEPS; i++)
    {
        float dist = sphereDistance(0.5, p);
        if (dist < EPSILON)
        {
            return float4(sphereNormal(r, p), 1);
        }
        p += d * dist;
    }
    return 0;
}

float4 sphereColor(float3 p, float3 d, float3 color)
{   
    float4 result = sphereMarching(0.5, p, normalize(p - _WorldSpaceCameraPos));
    float3 c = color * simpleLanbert(result.xyz, _MainLightPosition.xyz) * _MainLightColor;
    return float4(c.r, c.g, c.b, result.a);
}

