
struct AnimData
{
    float3 p, n;
};

StructuredBuffer<uint2> clipsData; //(startIndex, frames)
StructuredBuffer<AnimData> geometry;

void GetVertexData_float(float clip, float vertexID, float vertexCount, float fps, float time, out float3x3 result)
{
    uint2 clipData = clipsData[uint(clip)];
    uint frame = uint(time * fps) % clipData.y;
    uint i = clipData.x + frame * vertexCount + vertexID;
    AnimData spacialData = geometry[i];
    result = float3x3(spacialData.p, spacialData.n, float3(0.1, 0, 0));
}