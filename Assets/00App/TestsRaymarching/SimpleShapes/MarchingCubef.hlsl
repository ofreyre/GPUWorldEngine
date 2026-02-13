
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Lighting.hlsl"
#include "RaymarchingParams.hlsl"

float min3(float a, float b, float c)
{
    return min(a, min(b, c));

}

float cubefDistance(float3 r, float3 r1, float3 p)
{
    p = abs(p) - r;
    float3 q = abs(p + r1) - r1;
    return min3(
        length(max(float3(p.x, q.y, q.z), 0)),
        length(max(float3(q.x, p.y, q.z), 0)),
        length(max(float3(q.x, q.y, p.z), 0))
    );
    
}

float3 cubefNormal(float r, float3 r1, float3 p)
{
    return normalize(float3(
        cubefDistance(r, r1, p + float3(DELTA, 0, 0)) - cubefDistance(r, r1, p + float3(-DELTA, 0, 0)),
        cubefDistance(r, r1, p + float3(0, DELTA, 0)) - cubefDistance(r, r1, p + float3(0, -DELTA, 0)),
        cubefDistance(r, r1, p + float3(0, 0, DELTA)) - cubefDistance(r, r1, p + float3(0, 0, 0 - DELTA))
    ));
}

float4 cubefMarching(float r, float3 r1, float3 p, float3 d)
{
    for (uint i = 0; i < STEPS; i++)
    {
        float dist = cubefDistance(r, r1, p);
        if (dist < EPSILON)
        {
            return float4(cubefNormal(r, r1, p), 1);
        }
        p += d * dist;
    }
    return 0;
}

float4 cubefColor(float3 p, float3 d, float3 color)
{   
    float4 result = cubefMarching(0.5, 0.1, p, normalize(p - _WorldSpaceCameraPos));
    float3 c = color * simpleLanbert(result.xyz, _MainLightPosition.xyz) * _MainLightColor;
    return float4(c.r, c.g, c.b, result.a);
}

