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

struct BiomeData {
	int albedoIndex;
	float biomeOrder;
};

float inverseLerp(float a, float b, float value) {
	return saturate((value-a)/(b-a));
}

float RND_2_1(float2 p, float2 dotDir = float2(12.9898, 78.233))
{
	float r = dot((p), dotDir);
	return frac(sin(r) * 143758.5453);
}

float RND_3_1(float3 p, float3 dotDir = float3(12.9898, 78.233, 113.3675))
{
	float r = dot((p), dotDir);
	return frac(sin(r) * 143758.5453);
}

float RND_2_2(float2 p)
{
	return float2(RND_2_1(p), RND_2_1(p, float2(5.9898, 57.233)));
}

float3 RND_3_3(float3 p)
{
	return float3(RND_3_1(p), 0, RND_3_1(p, float3(83.39217, 17.5791, 43.6935)) );
}

float4 RND2_4(float2 p) {
	return frac(sin(float4(1.0 + dot(p, float2(37.0, 17.0)),
		2.0 + dot(p, float2(11.0, 47.0)),
		3.0 + dot(p, float2(41.0, 29.0)),
		4.0 + dot(p, float2(23.0, 31.0)))) * 103.0);
}


float3 TransformWP(float3 p, float3 cellCenter, float cellLength)
{
	float3 offset = RND_3_3(cellCenter) * cellLength * 0.5;
	//float3 offset = RND_3_3(cellCenter) * 0.5;
	float2 transforms = RND_2_2(cellCenter.xz);
	float3 p1 = (p - cellCenter + offset) * transforms.x;
	float c = cos(transforms.y), s = sin(transforms.y);
	float3x3 rotation = float3x3(
		 c, 0, s, // column 1
		 0, 1, 0,
		-s, 0, c // column 2		
	);
	return mul(rotation, p1) + cellCenter;
}

float3 triplanarUV(UnityTexture2DArray tex, UnitySamplerState samplerState, float3 worldPos, float scale, float3 blendAxes, int textureIndex)
{
	float3 scaledWorldPos = worldPos / scale;

	float3 xProjection = SAMPLE_TEXTURE2D_ARRAY(tex, samplerState.samplerstate, float2(scaledWorldPos.y, scaledWorldPos.z), textureIndex) * blendAxes.x;
	float3 yProjection = SAMPLE_TEXTURE2D_ARRAY(tex, samplerState.samplerstate, float2(scaledWorldPos.x, scaledWorldPos.z), textureIndex) * blendAxes.y;
	float3 zProjection = SAMPLE_TEXTURE2D_ARRAY(tex, samplerState.samplerstate, float2(scaledWorldPos.x, scaledWorldPos.y), textureIndex) * blendAxes.z;	

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

BiomeData GetAlbedoUV1(UnityTexture2D biomes, float2 worldPos, float2 biomeChunkPos, float biomeChunkLength)
{
	BiomeData data;
	float2 biomeuv = (worldPos - biomeChunkPos) / biomeChunkLength;
	float2 biome = tex2D(biomes, biomeuv).ga;
	data.albedoIndex = int(biome.x * 9);
	data.biomeOrder = biome.y;
	return data;
}

BiomeData GetAlbedoUV(UnityTexture2D biomes, float2 uv)
{
	BiomeData data;
	float2 biome = tex2D(biomes, uv).ga;
	data.albedoIndex = int(round(biome.x * 9));
	data.biomeOrder = biome.g;
	return data;
}

float3 mix(float3 x, float3 y, float a)
{
	return x * (1 - a) + y * a;
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
		out float3x3 result)
{

	float2 biomeuv = (worldPos.xz - biomeChunkPos) / biomeChunkLength + biomeCelluvLength;
	float2 biomeuvCoord = fmod(biomeuv, biomeCelluvLength);
	float3 biome = tex2D(biomes, biomeuv).gba;
	int albedoIndex = int(round(biome.x * 9));
	float biomeOrder = biome.z;

	//map blend to [-1,1] x [-1,1]
	//If blend < 0 blend from tile before
	//If blend = 0 don't blend
	//If blend > 0 blend from tile after
	float blendLimit = biome.g * biomeCelluvLength / 2;
	float2 blend = biomeuvCoord - biomeCelluvLength/2;
	blend = sign(blend) * clamp(abs(blend) - biomeCelluvLength/2 + blendLimit, float2(0,0), blendLimit) / blendLimit / 2;
	
	float3 blendAxes = abs(worldNormal);
	blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
	float uvScale = chunkCellLength;

	
	//**************** Color
	float3 biomeColor = triplanarUV(albedo, samplerState, worldPos, uvScale, blendAxes, albedoIndex);

	//BiomeData dataL = GetAlbedoUV(biomes, worldPos.xz + float2(-chunkCellLength, 0), biomeChunkPos, biomeChunkLength);
	BiomeData dataL = GetAlbedoUV(biomes, biomeuv + float2(-biomeCelluvLength.x, 0));
	float3 biomeLColor = triplanarUV(albedo, samplerState, worldPos + float3(-chunkCellLength, 0, 0), uvScale, blendAxes, dataL.albedoIndex);

	//BiomeData dataR = GetAlbedoUV(biomes, worldPos.xz + float2(chunkCellLength, 0), biomeChunkPos, biomeChunkLength);
	BiomeData dataR = GetAlbedoUV(biomes, biomeuv + float2(biomeCelluvLength.x, 0));
	float3 biomeRColor = triplanarUV(albedo, samplerState, worldPos + float3(chunkCellLength, 0, 0), uvScale, blendAxes, dataR.albedoIndex);

	//BiomeData dataT = GetAlbedoUV(biomes, worldPos.xz + float2(0, chunkCellLength), biomeChunkPos, biomeChunkLength);
	BiomeData dataT = GetAlbedoUV(biomes, biomeuv + float2(0, biomeCelluvLength.y));
	float3 biomeTColor = triplanarUV(albedo, samplerState, worldPos + float3(0, 0, chunkCellLength), uvScale, blendAxes, dataT.albedoIndex);

	//BiomeData dataB = GetAlbedoUV(biomes, worldPos.xz + float2(0, -chunkCellLength), biomeChunkPos, biomeChunkLength);
	BiomeData dataB = GetAlbedoUV(biomes, biomeuv + float2(0, -biomeCelluvLength.y));
	float3 biomeBColor = triplanarUV(albedo, samplerState, worldPos + float3(0, 0, -chunkCellLength), uvScale, blendAxes, dataB.albedoIndex);
		
	BiomeData dataLT = GetAlbedoUV(biomes, biomeuv + float2(-biomeCelluvLength.x, biomeCelluvLength.y));
	float3 biomeLTColor = triplanarUV(albedo, samplerState, worldPos + float3(-chunkCellLength, 0, chunkCellLength), uvScale, blendAxes, dataLT.albedoIndex);

	BiomeData dataRT = GetAlbedoUV(biomes, biomeuv + float2(biomeCelluvLength.x, biomeCelluvLength.y));
	float3 biomeRTColor = triplanarUV(albedo, samplerState, worldPos + float3(chunkCellLength, 0, chunkCellLength), uvScale, blendAxes, dataRT.albedoIndex);

	BiomeData dataLB = GetAlbedoUV(biomes, biomeuv + float2(-biomeCelluvLength.x, -biomeCelluvLength.y));
	float3 biomeLBColor = triplanarUV(albedo, samplerState, worldPos + float3(-chunkCellLength, 0, -chunkCellLength), uvScale, blendAxes, dataLB.albedoIndex);

	BiomeData dataRB = GetAlbedoUV(biomes, biomeuv + float2(biomeCelluvLength.x, -biomeCelluvLength.y));
	float3 biomeRBColor = triplanarUV(albedo, samplerState, worldPos + float3(chunkCellLength, 0, -chunkCellLength), uvScale, blendAxes, dataRB.albedoIndex);
	

	float blendL = saturate(-blend.x); //*step(biomeOrder + 0.001, dataL.biomeOrder);
	float blendR = saturate(blend.x); //*step(biomeOrder + 0.001, dataR.biomeOrder);
	float blendB = saturate(-blend.y); //*step(biomeOrder + 0.001, dataB.biomeOrder);
	float blendT = saturate(blend.y); //*step(biomeOrder + 0.001, dataT.biomeOrder);

	uint maskL = ceil(blendL);
	uint maskR = ceil(blendR);
	uint maskB = ceil(blendB);
	uint maskT = ceil(blendT);

	/*
	if (maskL != 0 && maskB != 0)
	{
		float blendLB = sqrt(blendL + blendB) / 2;
		//float3 color = mix(biomeLColor, biomeBColor, blendB / (blendL + blendB));
		float3 color = (biomeLColor * blendL + biomeBColor * blendB + biomeLBColor * blendLB) / (blendL + blendB + blendLB);
		//color = mix(color, biomeLBColor, max(blendL, blendB));
		float m = max(blendLB, max(blendL, blendB));
		biomeColor = mix(biomeColor, color, m);
	}
	else if (maskR != 0 && maskB != 0)
	{
		float3 color = mix(biomeRColor, biomeBColor, blendB / (blendR + blendB));
		biomeColor = mix(biomeColor, color, max(blendR, blendB));
	}
	else if (maskL != 0 && maskT != 0)
	{
		float3 color = mix(biomeLColor, biomeTColor, blendT / (blendL + blendT));
		biomeColor = mix(biomeColor, color, max(blendL, blendT));
	}
	else if (maskR != 0 && maskT != 0)
	{
		float3 color = mix(biomeRColor, biomeTColor, blendT / (blendR + blendT));
		biomeColor = mix(biomeColor, color, max(blendR, blendT));
	}
	else
	{
		float mask = max(1, maskL + maskR + maskB + maskT);
		biomeColor = (biomeColor * (1 - blendL - blendR - blendB - blendT)
			+ biomeLColor * blendL + biomeRColor * blendR + biomeBColor * blendB + biomeTColor * blendT) / mask;
	}
	*/

	blendLimit = biome.g * biomeCelluvLength / 2;
	float len = biomeCelluvLength;
	float2 T = float2(0, len);
	float2 R = float2(len, 0);
	float2 p = biomeuvCoord - len / 2;
	float b = saturate((len - length(p)) / len);
	float bL = saturate((len - distance(p, -R)) / len);
	float bR = saturate((len - distance(p, R)) / len);
	float bB = saturate((len - distance(p, -T)) / len);
	float bT = saturate((len - distance(p, T)) / len);
	float bLT = saturate((len - distance(p, float2(-len, len))) / len);
	float bRT = saturate((len - distance(p, float2(len, len))) / len);
	float bLB = saturate((len - distance(p, float2(-len, -len))) / len);
	float bRB = saturate((len - distance(p, float2(len, -len))) / len);
	biomeColor = (biomeColor * b + biomeLColor * bL + biomeRColor * bR + biomeBColor * bB + biomeTColor * bT
		+ biomeLTColor * bLT + biomeRTColor * bRT + biomeLBColor * bLB + biomeRBColor * bRB);
	

	//******** Normal
	float3 biomeNormal = triplanarNormal(normals, samplerState, worldPos, uvScale, worldNormal, blendAxes, albedoIndex);

	biomeLColor = triplanarNormal(normals, samplerState, worldPos + float3(-chunkCellLength, 0, 0), uvScale, worldNormal, blendAxes, dataL.albedoIndex);

	biomeRColor = triplanarNormal(normals, samplerState, worldPos + float3(chunkCellLength, 0, 0), uvScale, worldNormal, blendAxes, dataR.albedoIndex);

	biomeTColor = triplanarNormal(normals, samplerState, worldPos + float3(0, 0, chunkCellLength), uvScale, worldNormal, blendAxes, dataT.albedoIndex);

	biomeBColor = triplanarNormal(normals, samplerState, worldPos + float3(0, 0, -chunkCellLength), uvScale, worldNormal, blendAxes, dataB.albedoIndex);

	biomeLTColor = triplanarNormal(normals, samplerState, worldPos + float3(-chunkCellLength, 0, chunkCellLength), uvScale, worldNormal, blendAxes, dataLT.albedoIndex);

	biomeRTColor = triplanarNormal(normals, samplerState, worldPos + float3(chunkCellLength, 0, chunkCellLength), uvScale, worldNormal, blendAxes, dataRT.albedoIndex);

	biomeLBColor = triplanarNormal(normals, samplerState, worldPos + float3(-chunkCellLength, 0, -chunkCellLength), uvScale, worldNormal, blendAxes, dataLB.albedoIndex);

	biomeRBColor = triplanarNormal(normals, samplerState, worldPos + float3(chunkCellLength, 0, -chunkCellLength), uvScale, worldNormal, blendAxes, dataRB.albedoIndex);
	
	biomeNormal = (biomeNormal * b + biomeLColor * bL + biomeRColor * bR + biomeBColor * bB + biomeTColor * bT
		+ biomeLTColor * bLT + biomeRTColor * bRT + biomeLBColor * bLB + biomeRBColor * bRB) * 3;

	
	

	float2 c = floor((worldPos.xz - biomeChunkPos) / chunkCellLength) * chunkCellLength;
	float3 cc = float3(c.x, 0, c.y);
	biomeuv = (worldPos.xz - biomeChunkPos) / biomeChunkLength + biomeCelluvLength;
	float2 wp2 = worldPos.xz - biomeChunkPos;


	float3 va = float3(0, 0, 0);
	float w1 = 0.0;
	float w2 = 0.0;

	for (int i = -1; i < 2; i++)
	{
		for (int j = -1; j < 2; j++)
		{
			//BiomeData dat = GetAlbedoUV(biomes, biomeuv + float2(j, i) * biomeCelluvLength);
			float3 cell = cc + float3(j, 0, i) * chunkCellLength;
			float3 pp = TransformWP(worldPos, cell, chunkCellLength);
			//biomeuv = (worldPos.xz - biomeChunkPos) / biomeChunkLength + biomeCelluvLength;
			biomeuv = (pp.xz - biomeChunkPos) / biomeChunkLength + biomeCelluvLength;
			BiomeData dat = GetAlbedoUV(biomes, biomeuv);
			//BiomeData dat = GetAlbedoUV(biomes, biomeuv + float2(j, i) * biomeCelluvLength);
			float2 r = (wp2 - pp.xz) / chunkCellLength;
			float d = dot(r, r);
			float w = exp(-5.0 * d);
			float3 color = triplanarUV(albedo, samplerState, pp, uvScale, blendAxes, dat.albedoIndex);
			va += w * color;
			w1 += w;
			w2 += w * w;
			if (i == 0 && j == 0)
			{
				va = color;
			}
		}
	}

	float mean = 0.3;// textureGrad( samp, uv, ddx*16.0, ddy*16.0 ).x;
	float3 res = mean + (va *50 - w1 * mean) / sqrt(w2);
	biomeColor = mix(va / w1, res, 0.3);
	biomeColor = va;

	/*biomeuv = (worldPos.xz - biomeChunkPos) / biomeChunkLength + biomeCelluvLength;
	BiomeData dat = GetAlbedoUV(biomes, biomeuv + float2(0, 0) * biomeCelluvLength);
	worldPos.xz -= biomeChunkPos;
	biomeColor = triplanarUV(albedo, samplerState, worldPos , uvScale, blendAxes, dat.albedoIndex);
	*/

	result = float3x3(biomeColor, biomeNormal, float3(0.1,0,0));
}
