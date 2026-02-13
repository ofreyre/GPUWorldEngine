//#ifndef TERRAINALBEDO_INCLUDE
//#define TERRAINALBEDO_INCLUDE

#ifndef dbg_math
  #include "../../Common/Shaders/Math.cginc"
#endif

/*
** biomes:
** r = temperature
** g = biome value
** b = biome blend
** a = biome order
*/

const static float epsilon = 1E-4;
StructuredBuffer<uint> _biomes;

struct BiomeData {
	int albedoIndex;
	float biomeOrder;
};

float3 triplanarUV(UnityTexture2DArray tex, UnitySamplerState samplerState, float3 worldPos, float scale, float3 blendAxes, int textureIndex)
{
	float3 scaledWorldPos = worldPos / scale;

	float3 xProjection = SAMPLE_TEXTURE2D_ARRAY(tex, samplerState.samplerstate, float2(scaledWorldPos.y, scaledWorldPos.z), textureIndex) * blendAxes.x;
	float3 yProjection = SAMPLE_TEXTURE2D_ARRAY(tex, samplerState.samplerstate, float2(scaledWorldPos.x, scaledWorldPos.z), textureIndex) * blendAxes.y;
	float3 zProjection = SAMPLE_TEXTURE2D_ARRAY(tex, samplerState.samplerstate, float2(scaledWorldPos.x, scaledWorldPos.y), textureIndex) * blendAxes.z;	

	return xProjection + yProjection + zProjection;
}

float3 triplanarUV_grad(UnityTexture2DArray tex, UnitySamplerState samplerState, float3 worldPos, float scale, float3 blendAxes, int textureIndex, float dpx, float dpy)
{
	float3 scaledWorldPos = worldPos / scale;

	float3 xProjection = SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samplerState.samplerstate, float2(scaledWorldPos.y, scaledWorldPos.z), textureIndex, dpx, dpy) * blendAxes.x;
	float3 yProjection = SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samplerState.samplerstate, float2(scaledWorldPos.x, scaledWorldPos.z), textureIndex, dpx, dpy) * blendAxes.y;
	float3 zProjection = SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samplerState.samplerstate, float2(scaledWorldPos.x, scaledWorldPos.y), textureIndex, dpx, dpy) * blendAxes.z;

	return xProjection + yProjection + zProjection;
}

float3 triplanarNormal(UnityTexture2DArray baseNormals, UnitySamplerState samplerState, float3 worldPos, float scale, float3 worldNormal, float3 blendAxes, int textureIndex)
{
	float3 scaledWorldPos = worldPos / scale;

	// Triplanar uvs
	float2 uvX = scaledWorldPos.zy; // x facing plane
	float2 uvY = scaledWorldPos.xz; // y facing plane
	float2 uvZ = scaledWorldPos.xy; // z facing plane


	// Tangent space normal maps
	half3 tnormalX = SAMPLE_TEXTURE2D_ARRAY(baseNormals, samplerState.samplerstate, uvX, textureIndex);
	half3 tnormalY = SAMPLE_TEXTURE2D_ARRAY(baseNormals, samplerState.samplerstate, uvY, textureIndex);
	half3 tnormalZ = SAMPLE_TEXTURE2D_ARRAY(baseNormals, samplerState.samplerstate, uvZ, textureIndex);


	// Swizzle world normals into tangent space and apply Whiteout blend
	tnormalX = half3(
		tnormalX.xy + worldNormal.zy,
		abs(tnormalX.z) * worldNormal.x
		);
	tnormalY = half3(
		tnormalY.xy + worldNormal.xz,
		abs(tnormalY.z) * worldNormal.y
		);
	tnormalZ = half3(
		tnormalZ.xy + worldNormal.xy,
		abs(tnormalZ.z) * worldNormal.z
		);


	// Swizzle tangent normals to match world orientation and triblend
	return normalize(
		tnormalX.zyx * blendAxes.x +
		tnormalY.xzy * blendAxes.y +
		tnormalZ.xyz * blendAxes.z
		);
}

float3 triplanarNormal_grad(UnityTexture2DArray baseNormals, UnitySamplerState samplerState, float3 worldPos, float scale, float3 worldNormal, float3 blendAxes, int textureIndex, float dpx, float dpy)
{
	float3 scaledWorldPos = worldPos / scale;

	// Triplanar uvs
	float2 uvX = scaledWorldPos.zy; // x facing plane
	float2 uvY = scaledWorldPos.xz; // y facing plane
	float2 uvZ = scaledWorldPos.xy; // z facing plane


	// Tangent space normal maps
	half3 tnormalX = SAMPLE_TEXTURE2D_ARRAY_GRAD(baseNormals, samplerState.samplerstate, uvX, textureIndex, dpx, dpy);
	half3 tnormalY = SAMPLE_TEXTURE2D_ARRAY_GRAD(baseNormals, samplerState.samplerstate, uvY, textureIndex, dpx, dpy);
	half3 tnormalZ = SAMPLE_TEXTURE2D_ARRAY_GRAD(baseNormals, samplerState.samplerstate, uvZ, textureIndex, dpx, dpy);


	// Swizzle world normals into tangent space and apply Whiteout blend
	tnormalX = half3(
		tnormalX.xy + worldNormal.zy,
		abs(tnormalX.z) * worldNormal.x
		);
	tnormalY = half3(
		tnormalY.xy + worldNormal.xz,
		abs(tnormalY.z) * worldNormal.y
		);
	tnormalZ = half3(
		tnormalZ.xy + worldNormal.xy,
		abs(tnormalZ.z) * worldNormal.z
		);


	// Swizzle tangent normals to match world orientation and triblend
	return normalize(
		tnormalX.zyx * blendAxes.x +
		tnormalY.xzy * blendAxes.y +
		tnormalZ.xyz * blendAxes.z
		);
}

/*
BiomeData GetAlbedoUV(UnityTexture2D biomes, float2 uv)
{
	BiomeData data;
	float2 biome = tex2D(biomes, uv).ga;
	data.albedoIndex = int(round(biome.x * 9));
	data.biomeOrder = biome.g;
	return data;
}
*/

BiomeData GetAlbedoUV(UnityTexture2D biomes, UnitySamplerState samplerState, float2 uv)
{	
    BiomeData data;
    //float2 biome = tex2D(biomes, uv).ga;
    float2 biome = SAMPLE_TEXTURE2D(biomes, samplerState.samplerstate, uv).yz;
	
	
    if (biome.x > 0.8)
        data.albedoIndex = 8;
    else if (biome.x > 0.7)
        data.albedoIndex = 7;
    else if (biome.x > 0.6)
        data.albedoIndex = 6;
    else if (biome.x > 0.5)
        data.albedoIndex = 5;
    else if (biome.x > 0.4)
        data.albedoIndex = 4;
    else if (biome.x > 0.3)
        data.albedoIndex = 3;
    else if (biome.x > 0.2)
        data.albedoIndex = 2;
    else if (biome.x > 0.1)
        data.albedoIndex = 1;
	else
        data.albedoIndex = 0;
    //data.albedoIndex = int(round(biome.x * 9.0));
    //data.albedoIndex = int(ceil(biome.x * 9.0));
    data.biomeOrder = biome.y;
    return data;
}

uint GetBiomeIndex(uint2 biomeCellCoords, uint biomeWidth)
{
    return _biomes[biomeCellCoords.x + biomeCellCoords.y * biomeWidth];

}

float3 mix(float3 x, float3 y, float a)
{
	return x * (1 - a) + y * a;
}

float4 RND2_4(float2 p) {
	return frac(sin(float4(
		1.0 + dot(p, float2(37.0, 17.0)),
		2.0 + dot(p, float2(11.0, 47.0)),
		3.0 + dot(p, float2(41.0, 29.0)),
		4.0 + dot(p, float2(23.0, 31.0)))
	) * 103.0);
}

void TerrainAlbedo_float(
		float3 worldPos,
		float3 worldNormal,
		float2 biomeChunkPos,
		float biomeChunkLength,
		float2 biomeCelluvLength,
		float chunkCellLength,
		UnitySamplerState samplerState,
		UnityTexture2DArray albedo,
		UnityTexture2DArray normals,
		UnityTexture2D biomes, //first row: baseColours[maxLayerCount], second row: baseStartHeights, baseBlends, baseColourStrength, baseTextureScales
		float biomeWidth,
		out float3x3 result)
{	
	float3 blendAxes = abs(worldNormal);
	blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
	float uvScale = chunkCellLength;
	
    float2 v = (worldPos.xz - biomeChunkPos) / chunkCellLength;
    float2 biomeCellCoords = floor(v);
    float2 pCell = frac(v);
	float dpx = ddx(pCell.x);
	float dpy = ddy(pCell.y);

	float3 color = float3(0, 0, 0);
	float3 normal = float3(0, 0, 0);
	float w1 = 0.0;
	float w2 = 0.0;

	for (int i = -1; i < 2; i++)
	{
		for (int j = -1; j < 2; j++)
		{
            float2 cell = biomeCellCoords + float2(i, j);
            float4 rnd = RND2_4(cell);
			float2 r = cell + rnd.xy - v; // cell + rnd.xy voronoi cell point
			float d = dot(r, r);
			float w = exp(-5 * d);
			
			float2 wp2 = worldPos.xz + rnd.zw * chunkCellLength;
			//float2 biomeuv = biomeuv0 + float2(i, j) * biomeCelluvLength;
            //BiomeData data = GetAlbedoUV(biomes, albedosamplerState, biomeuv);
            float3 p = float3(wp2.x, worldPos.y, wp2.y);
            uint biomeIndex = GetBiomeIndex(cell, uint(biomeWidth));
            float3 c = triplanarUV_grad(albedo, samplerState, p, uvScale, blendAxes, biomeIndex, dpx, dpy);
            float3 n = triplanarNormal_grad(normals, samplerState, p, uvScale, worldNormal, blendAxes, biomeIndex, dpx, dpy);
			color += w * c;
			normal += w * n;
			w1 += w;
			w2 += w * w;
		}
	}
	
	float mean = 0.3;
	float3 res = mean + (color - w1 * mean) / sqrt(w2);
	float3 biomeColor = mix(color / w1, res, 0.3);
	res = mean + (normal - w1 * mean) / sqrt(w2);
	float3 biomeNormal = mix(normal / w1, res, 0.3);

	result = float3x3(biomeColor, biomeNormal, float3(0.1,0,0));
}
