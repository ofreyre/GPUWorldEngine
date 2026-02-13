#ifndef dbg_cellularnoise
  #define dbg_cellularnoise

#ifndef dbg_math
#include "Ease.cginc"
#endif

#ifndef dbg_whitenoise
#include "WhiteNoise.cginc"
#endif
            
float Cellular2d(float2 p, float cellLength)
{
    float2 v = p / cellLength;
    float2 cp = floor(p / cellLength);
    float d = 9999999;
    [unroll(3)]
    for (int y = -1; y < 2; y++)
    {
        [unroll(3)]
        for (int x = -1; x < 2; x++)
        {
            float2 cell = cp + float2(x, y);
            float2 cellValue = cell + rand2dTo2d(cell);
            float dist = distance(cellValue, v);
            d = min(d, dist);
        }
    }
    return d;
}
            
float Cellular2dtiled(float2 p, float cellLength, float period)
{
    float pe = period / cellLength;
    float2 v = p / cellLength;
    float2 cp = floor(p / cellLength);
    float d = 9999999;
    [unroll(3)]
    for (int y = -1; y < 2; y++)
    {
        [unroll(3)]
        for (int x = -1; x < 2; x++)
        {
            float2 cell = cp + float2(x, y);
            float2 cellValue = cell + rand2dTo2d(modulo2(cell, period));
            float dist = distance(cellValue, v);
            d = min(d, dist);
        }
    }
    return d;
}

float Cellular3d(float3 p, float cellLength)
{
    float3 v = p / cellLength;
    float3 cp = floor(p / cellLength);
    float d = 9999999;
    [unroll(3)]
    for (int z = -1; z < 2; z++)
    {
    [unroll(3)]
        for (int y = -1; y < 2; y++)
        {
        [unroll(3)]
            for (int x = -1; x < 2; x++)
            {
                float3 cell = cp + float3(x, y, z);
                float3 cellValue = cell + rand3dTo3d(cell);
                float dist = distance(cellValue, v);
                d = min(d, dist);
            }
        }
    }
    return d;
}

float Cellular3dtiled(float3 p, float cellLength, float period)
{
    float pe = period / cellLength;
    float3 v = p / cellLength;
    float3 cp = floor(p / cellLength);
    float d = 9999999;
    [unroll(3)]
    for (int z = -1; z < 2; z++)
    {
    [unroll(3)]
        for (int y = -1; y < 2; y++)
        {
        [unroll(3)]
            for (int x = -1; x < 2; x++)
            {
                float3 cell = cp + float3(x, y, z);
                float3 tiledCell = modulo3(cell, pe);
                float3 cellValue = cell + rand3dTo3d(modulo3(tiledCell, pe));
                float dist = distance(cellValue, v);
                d = min(d, dist);
            }
        }
    }
    return d;
}

float Cellular3dtiledlayered(float3 p, float scale, float cellLength, float period, uint layers, float persistance, float roughness)
{
    float noise = 0;
    float frequency = 1;
    float factor = 1;
    layers = min(16, layers);
    
    [unroll(16)]
    for (uint i = 0; i < layers; i++)
    {
        noise += Cellular3dtiled(p * frequency, cellLength, period) * factor;
        frequency *= persistance;
        factor *= roughness;
    }

    return noise;
}

#endif




