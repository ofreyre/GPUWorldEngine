struct BoneWeight1
{
    float weight;
    uint boneIndex;
};

StructuredBuffer<uint> bonesWeightsPerVertexStart;
StructuredBuffer<BoneWeight1> bonesWeightsPerVertex;
StructuredBuffer<float4x4> boneKey;

static float4x4 ZERO4 = float4x4(
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0
);

static float4x4 I4 = float4x4(
    1, 0, 0, 0,
    0, 1, 0, 0,
    0, 0, 1, 0,
    0, 0, 0, 1
);

void GetVertexData_float(uint vertexCount, uint vertexID, float3 p, float3 n, 
                        uint clipStart, uint clipFramesCount, float clipFrameLength,
                        uint bonesCount, 
                        float speed, float startTime, float time,
                        out float4x4 result)
{
    float t = time * speed - startTime;
    float k = t / clipFrameLength;
    uint frame = uint(k) % clipFramesCount;
    uint nextFrame = (frame + 1) % clipFramesCount;
    uint bonesStart = bonesWeightsPerVertexStart[vertexID];
    //uint bonesStartNext = bonesWeightsPerVertexStart[(vertexID + 1) % vertexCount];
    uint bonesC = min((vertexID < vertexCount - 1 ? bonesWeightsPerVertexStart[vertexID + 1] : vertexCount) - bonesStart, 10);
    float4x4 bones4 = ZERO4;
    float4x4 bones4next = ZERO4;
    uint boneKeyIndexStart = clipStart + frame * bonesCount;
    uint boneKeyIndexStartNext = clipStart + nextFrame * bonesCount;
    
    float fw = frac(k);
    
    [unroll(10)]
    for (uint i = 0; i < bonesC; i++)
    {
        BoneWeight1 boneWeight = bonesWeightsPerVertex[bonesStart + i];
        uint boneKeyIndex = boneKeyIndexStart + boneWeight.boneIndex;
        bones4 += boneKey[boneKeyIndex] * boneWeight.weight;
        
        boneKeyIndex = boneKeyIndexStartNext + boneWeight.boneIndex;
        bones4next += boneKey[boneKeyIndex] * boneWeight.weight;
    }
    
    float4x4 b = bones4 * (1 - fw) + bones4next * fw;    
    float4 pp = mul(b, float4(p, 1.0));
    float4 nn = float4(normalize(mul((float3x3) b, n)), 0);
    
    result = float4x4(
        pp,
        nn,
        float4(0, 0, 0, 0),
        float4(0, 0, 0, 0)
    );
    
    /*
    result = float4x4(
        float4(p, 0),
        float4(n, 0),
        float4(0, 0, 0, 0),
        float4(0, 0, 0, 0)
    );
*/
}