struct MeshCollisionData
{

    float3 p;
    float3 n;
    float d;
};

float2 meshCollisionOrigin;
float cellCollisionWidth;
int collisionGridWidthInCells;
StructuredBuffer<MeshCollisionData> meshCollisionData;

uint2 WorldPosToCollisionCellCoords(float2 worldPos)
{
    float2 localPos = worldPos - meshCollisionOrigin;
    //return uint2(localPos.x / cellCollisionWidth, int(localPos.y / cellCollisionWidth) * 2) + int(sign((localPos.y % cellCollisionWidth) - (localPos.x % cellCollisionWidth)) * 0.5);
    return uint2(localPos.x / cellCollisionWidth, int(localPos.y / cellCollisionWidth) * 2 + int(sign((localPos.y % cellCollisionWidth) - (localPos.x % cellCollisionWidth)) * 0.5));
}

uint WorldPosToCollisionCellIndex(float2 worldPos)
{
    uint2 cellCoord = WorldPosToCollisionCellCoords(worldPos);
    return cellCoord.y * collisionGridWidthInCells + cellCoord.x;
}

MeshCollisionData WorldPosToCollisionCellData(float2 worldPos)
{
    return meshCollisionData[WorldPosToCollisionCellIndex(worldPos)];
}

float GetMeshY(float2 worldPos)
{
    uint cellIndex = WorldPosToCollisionCellIndex(worldPos);
    MeshCollisionData collisionData = meshCollisionData[cellIndex];
    return (collisionData.d - collisionData.n.x * worldPos.x - collisionData.n.z * worldPos.y) / collisionData.n.y;
}

float GetMeshY(float2 worldPos, MeshCollisionData collisionData)
{
    return (collisionData.d - collisionData.n.x * worldPos.x - collisionData.n.z * worldPos.y) / collisionData.n.y;
}

float2 GetTangentXZ(float2 worldPos)
{
    uint cellIndex = WorldPosToCollisionCellIndex(worldPos);
    MeshCollisionData collisionData = meshCollisionData[cellIndex];
    return collisionData.n.xz;
}

