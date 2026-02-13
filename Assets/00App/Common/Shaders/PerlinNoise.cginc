#ifndef dbg_perlinnoise
  #define dbg_perlinnoise

#ifndef dbg_math
  #include "Math.cginc"
#endif

#ifndef dbg_ease
  #include "Ease.cginc"
#endif

#ifndef dbg_whitenoise
  #include "WhiteNoise.cginc"
#endif

float PerlinNoise(float p, float cellLength)
{
    float v = p / cellLength;
    float f = frac(v);
    float i = easeInOut(f);

    float cm0 = rand1dTo1d(floor(v)) * 2 - 1;
    float cm1 = rand1dTo1d(ceil(v)) * 2 - 1;

    float y0 = cm0 * f;
    float y1 = cm1 * (f - 1);
    return lerp(y0, y1, i) + 0.5;
}

float PerlinNoiseTiled(float p, float cellLength, float period)
{
    float pe = period / cellLength;
    float v = p / cellLength;
    float f = frac(v);
    float interpolation = easeInOut(f);

    float cm0 = rand1dTo1d(modulo(floor(v), pe)) * 2 - 1;
    float cm1 = rand1dTo1d(modulo(ceil(v), pe)) * 2 - 1;

    float y0 = cm0 * f;
    float y1 = cm1 * (f - 1);
    return lerp(y0, y1, interpolation) + 0.5;
}

float PerlinNoise2D(float2 p, float cellLength)
{
    float2 v = p / cellLength;
    float2 f = frac(v);
    float2 i = float2(easeInOut(f.x), easeInOut(f.y));

    float cv00 = rand2dTo1d(float2(floor(v.x), floor(v.y))) * 2 - 1;
    float cv10 = rand2dTo1d(float2(ceil(v.x), floor(v.y))) * 2 - 1;
    float cv01 = rand2dTo1d(float2(floor(v.x), ceil(v.y))) * 2 - 1;
    float cv11 = rand2dTo1d(float2(ceil(v.x), ceil(v.y))) * 2 - 1;

    float2 d00 = f - float2(0, 0);
    float2 d10 = f - float2(1, 0);
    float2 d01 = f - float2(0, 1);
    float2 d11 = f - float2(1, 1);

    float dot00 = dot(cv00, d00);
    float dot10 = dot(cv10, d10);
    float dot01 = dot(cv01, d01);
    float dot11 = dot(cv11, d11);

    float dot_0 = lerp(dot00, dot10, i.x);
    float dot_1 = lerp(dot01, dot11, i.x);

    return lerp(dot_0, dot_1, i.y) + 0.5;
}

float PerlinNoise2DTiled(float2 p, float cellLength, float period)
{
    float pe = period / cellLength;
    float2 v = p / cellLength;
    float2 f = frac(v);
    float2 i = float2(easeInOut(f.x), easeInOut(f.y));

    float2 cMin = modulo2(floor(v), pe);
    float2 cMax = modulo2(ceil(v), pe);

    float cv00 = rand2dTo1d(float2(cMin.x, cMin.y)) * 2 - 1;
    float cv10 = rand2dTo1d(float2(cMax.x, cMin.y)) * 2 - 1;
    float cv01 = rand2dTo1d(float2(cMin.x, cMax.y)) * 2 - 1;
    float cv11 = rand2dTo1d(float2(cMax.x, cMax.y)) * 2 - 1;

    float2 d00 = f - float2(0, 0);
    float2 d10 = f - float2(1, 0);
    float2 d01 = f - float2(0, 1);
    float2 d11 = f - float2(1, 1);

    float dot00 = dot(cv00, d00);
    float dot10 = dot(cv10, d10);
    float dot01 = dot(cv01, d01);
    float dot11 = dot(cv11, d11);

    float dot_0 = lerp(dot00, dot10, i.x);
    float dot_1 = lerp(dot01, dot11, i.x);

    return lerp(dot_0, dot_1, i.y) + 0.5;
}

float PerlinNoise3(float3 p, float cellLength)
{
    float3 v = p / cellLength;
    float3 f = frac(p / cellLength);
    float3 i = float3(easeInOut(f.x), easeInOut(f.y), easeInOut(f.z));

    float nZ[2];
    float nY[2];
    float nX[2];
    [unroll(2)]
    for (int z = 0; z <= 1; z++)
    {
        [unroll(2)]
        for (int y = 0; y <= 1; y++)
        {
            [unroll(2)]
            for (int x = 0; x <= 1; x++)
            {
                float3 cd = rand3dTo1d(floor(v) + float3(x, y, z)) * 2 - 1;
                float3 d = f - float3(x, y, z);
                nX[x] = dot(cd, d);
            }
            nY[y] = lerp(nX[0], nX[1], i.x);
        }
        nZ[z] = lerp(nY[0], nY[1], i.y);
    }
    return lerp(nZ[0], nZ[1], i.z) + 0.5;
}

float PerlinNoise3Tiled(float3 p, float cellLength, float period)
{
    float pe = period / cellLength;
    float3 v = p / cellLength;
    float3 f = frac(v);
    float3 i = float3(easeInOut(f.x), easeInOut(f.y), easeInOut(f.z));

    float nZ[2];
    float nY[2];
    float nX[2];
    [unroll(2)]
    for (int z = 0; z <= 1; z++)
    {
        [unroll(2)]
        for (int y = 0; y <= 1; y++)
        {
            [unroll(2)]
            for (int x = 0; x <= 1; x++)
            {
                float3 cd = rand3dTo1d(modulo3(floor(v) + float3(x, y, z), pe)) * 2 - 1;
                float3 d = f - float3(x, y, z);
                nX[x] = dot(cd, d);
            }
            nY[y] = lerp(nX[0], nX[1], i.x);
        }
        nZ[z] = lerp(nY[0], nY[1], i.y);
    }
    return lerp(nZ[0], nZ[1], i.z) + 0.5;
}

float PerlinNoise3Tiledlayered(float3 p, float scale, float cellLength, float period, uint layers, float persistance, float roughness)
{
    float noise = 0;
    float frequency = 1;
    float factor = 1;
    layers = min(16, layers);
    
    [unroll(16)]
    for (uint i = 0; i < layers; i++)
    {
        noise += PerlinNoise3Tiled(p * frequency, cellLength, period) * factor;
        frequency *= persistance;
        factor *= roughness;
    }

    return noise;
}


#endif




