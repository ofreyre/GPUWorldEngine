#ifndef dbg_perlinHeightMap
  #define dbg_perlinHeightMap

#include "../../Common/Shaders/PerlinNoise.cginc"


float perlinScale;
int octaves; //max = 16
float persistance;
float lacunarity;
float heightNormalK;
StructuredBuffer<float2> octaveOffsets; //Max size = 16

float PerlinHeightMap (float2 p)
{
	float amplitude = 1;
	float frequency = 1;
    float noiseHeight = 0;

    [unroll(16)]
    for(int i=0;i<octaves;i++)
    {        
		float2 sample = (p + octaveOffsets[i]) * perlinScale * frequency;
        //float perlinValue = PerlinNoise2D(sample) * 2 - 1;
        float perlinValue = PerlinNoise2D(sample, 1);
        noiseHeight += perlinValue * amplitude;
        amplitude *= persistance;
		frequency *= lacunarity;
    }

    noiseHeight *= heightNormalK;
    noiseHeight *= CubicHermiteSplines(noiseHeight);

    return noiseHeight;

    //return noiseHeight * heightNormalK;

    //return (PerlinNoise2D(p * perlinScale));
}


#endif




