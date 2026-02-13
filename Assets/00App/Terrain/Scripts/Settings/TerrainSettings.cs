using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainSettings : ScriptableObject
{
    public HeightMapSettings m_heightMapSettings;
    public ClimateSettings m_climateSettings;
    public BiomesSettings m_biomesSettings;
    public MeshSettings m_meshSettings;
    public BoidsSettings m_boidsSettings;
    public RaysSettings m_raysSettings;
    public int m_lengthInChunks = 1;
    public int m_visibleRadiusInChunks = 1;
    public int m_cacheRadiusInChunks = 1;
    public Material m_material;
    public int EMPTY;

    public void UpdateMaterialGlobalProperties()
    {
        if (m_material)
        {
            m_material.SetFloat("BiomeChunkLength", m_heightMapSettings.mapSize * m_meshSettings.meshScale);
            m_material.SetFloat("ChunkCellLength", m_meshSettings.meshScale);
            float biomeCelluvLength = 1.0f / m_heightMapSettings.mapSize;
            m_material.SetVector("BiomeCelluvLength", new Vector4(biomeCelluvLength, biomeCelluvLength, 0, 0));
            m_material.SetFloat("BiomeWidth", m_heightMapSettings.mapSize);

        }
    }

    public int MeshLineVertexCount
    {
        get { return m_heightMapSettings.mapSize - 2; }
    }

    public int MeshVertexCount
    {
        get { return MeshLineVertexCount * MeshLineVertexCount; }
    }

    public int MeshTriangleCount
    {
        get
        {
            return (MeshLineVertexCount - 1) * (MeshLineVertexCount - 1) * 6;
        }
    }

    public float MeshSideLength
    {
        get { return (MeshLineVertexCount - 1) * m_meshSettings.meshScale; }
    }

    public int MeshLineCollisionCount
    {
        get { return (MeshLineVertexCount - 1); }
    }

    public int MeshCollisionCount
    {
        get { return MeshLineCollisionCount * MeshLineCollisionCount * 2; }
    }

    public float LatitudeRange
    {
        get { return m_lengthInChunks * MeshSideLength * m_meshSettings.meshScale; }
    }

    public int ChunkSideCellsCount
    {
        get { return MeshLineVertexCount - 1; }
    }

    public Vector3 MeshSize
    {
        get { return new Vector3(MeshSideLength, m_meshSettings.heightScale, MeshSideLength); }
    }

    public Vector2Int WorldToCells(Vector2 position)
    {
        float cellLength = m_meshSettings.meshScale;
        return new Vector2Int((int)(position.x / cellLength), (int)(position.y / cellLength));
    }

    public RectInt GetBoidRectInCells(Vector2 centerWS)
    {
        int boidsR = m_boidsSettings.boidsPerceptionInBoidsCells;
        Vector2Int centerInCells = WorldToCells(centerWS);
        return new RectInt(centerInCells.x - boidsR, centerInCells.y - boidsR, boidsR * 2 + 1, boidsR * 2 + 1);
    }

    public RectInt GetCollisionRectInCells(Vector2 centerWS)
    {
        int boidsR = m_boidsSettings.boidsPerceptionInBoidsCells;
        Vector2Int centerInCells = WorldToCells(centerWS);
        return new RectInt(centerInCells.x - boidsR - 1, centerInCells.y - boidsR - 1, (boidsR + 1) * 2 + 1, (boidsR + 1) * 2 + 1);
    }

    public Vector2Int WorldToChunks(Vector2 position)
    {
        float chunkLength = MeshSideLength;
        return new Vector2Int((int)(position.x / chunkLength), (int)(position.y / chunkLength)); ;
    }

    public RectInt GetViewRectInChunks(Vector2 centerWS)
    {
        int chunksR = m_visibleRadiusInChunks;
        Vector2Int centerInCells = WorldToChunks(centerWS);
        return new RectInt(centerInCells.x - chunksR, centerInCells.y - chunksR, chunksR * 2 + 1, chunksR * 2 + 1);
    }

    public RectInt GetCacheRectInChunks(Vector2 centerWS)
    {
        int chunksR = m_cacheRadiusInChunks;
        Vector2Int centerInCells = WorldToChunks(centerWS);
        return new RectInt(centerInCells.x - chunksR, centerInCells.y - chunksR, chunksR * 2 + 1, chunksR * 2 + 1);
    }

#if UNITY_EDITOR

    public const int minPowerOf2 = 6;
    public const int maxPowerOf2 = 10;

    int prev_mapSize;
    int prev_meshSize;
    float prev_meshWorldSize;
    float prev_meshScale;

    public void OnValidate()
    {
        m_heightMapSettings.OnValidate();


        if (prev_mapSize != m_heightMapSettings.mapSize)
        {
            m_heightMapSettings.mapSize = IntToPowerOf2(m_heightMapSettings.mapSize);
            UpdateSizes();
            UpdateMaterialGlobalProperties();
        }
        else if(prev_meshSize != m_meshSettings.meshSize)
        {
            m_heightMapSettings.mapSize = IntToPowerOf2(m_meshSettings.meshSize);
            UpdateSizes();
            UpdateMaterialGlobalProperties();
        }
        else if(prev_meshWorldSize != m_meshSettings.meshWorldSize)
        {
            m_heightMapSettings.mapSize = IntToPowerOf2((int)(m_meshSettings.meshWorldSize / m_meshSettings.meshScale));
            UpdateSizes();
            UpdateMaterialGlobalProperties();
        }
        else if(prev_meshScale != m_meshSettings.meshScale)
        {
            m_meshSettings.meshWorldSize = m_heightMapSettings.mapSize * m_meshSettings.meshScale;
            prev_meshScale = m_meshSettings.meshScale;
            prev_meshWorldSize = m_meshSettings.meshWorldSize;
            UpdateMaterialGlobalProperties();
        }
    }

    int IntToPowerOf2(int value)
    {
        int n = Mathf.Min(10, Mathf.Max(4, (int)Mathf.Round(Mathf.Log10(value) / Mathf.Log10(2))));
        n = Mathf.Clamp(n, minPowerOf2, maxPowerOf2);
        return (int)(Mathf.Pow(2, n));
    }

    void UpdateSizes()
    {
        m_meshSettings.meshSize = MeshLineVertexCount;//Number of side vertes
        m_meshSettings.meshWorldSize = MeshSideLength;
        prev_mapSize = m_heightMapSettings.mapSize;
        prev_meshSize = m_meshSettings.meshSize;
        prev_meshWorldSize = m_meshSettings.meshWorldSize;
    }
#endif
}
