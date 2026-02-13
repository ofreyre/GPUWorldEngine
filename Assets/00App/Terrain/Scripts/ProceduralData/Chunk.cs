using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEngine.UI;
using System.Threading;
using AnimatedModel;

[Serializable]
public class Chunk
{
    public enum STATE { 
        initializing,
        unloading,
        destroying,
        ready
    }

    TerrainSettings m_terrainSettings;

    public HeightMapComputer m_heightMapComputer;
    BiomesComputer m_biomesComputer;
    public MeshComputer m_meshComputer;
    TreesInitComputer m_treesInitComputer;
    BoidsInitComputer m_boidsInitComputer;
    public Vector2 m_position;
    GameObject m_gameObject;
    Mesh m_mesh;
    Action<Vector2Int> m_callbackChunkInitialized;
    Action<Vector2Int> m_callbackChunkUnloaded;
    MaterialPropertyBlock m_propertyBlock;
    Bounds m_bounds;
    MeshRenderer m_renderer;
    public Vector2Int m_gridCoords;
    public Vector2Int m_chunkCoords;
    public RectInt m_gridRect;
    int chunkSideCellsCount;
    public STATE m_state = STATE.ready;
    STATE m_nextState = STATE.ready;
    Vector2 m_nextPosition;
    Vector2 m_savedPosition = new Vector2(-1,-1);


    static int m_trianglesCount;
    static int m_biomesPropID;
    public static Transform m_exclusionTransformCenter;

    public List<Boid> m_boids = new List<Boid>();
    public List<Cylinder> m_trees = new List<Cylinder>();

    public Chunk(Vector2 position, TerrainSettings terrainSettings, 
        Action<Vector2Int> callbackChunkInitialized,
        Action<Vector2Int> callbackChunkUnloaded)
    {
        m_callbackChunkInitialized = callbackChunkInitialized;
        m_callbackChunkUnloaded = callbackChunkUnloaded;
        m_terrainSettings = terrainSettings;
        m_gameObject = new GameObject();
        chunkSideCellsCount = terrainSettings.ChunkSideCellsCount;

        m_propertyBlock = new MaterialPropertyBlock();
        m_renderer = m_gameObject.AddComponent<MeshRenderer>();
        m_renderer.sharedMaterial = m_terrainSettings.m_material;

        MeshFilter meshFilter = m_gameObject.AddComponent<MeshFilter>();
        m_mesh = new Mesh();
        meshFilter.mesh = m_mesh;

        //Static fields
        m_biomesPropID = Shader.PropertyToID("Biomes");
        m_trianglesCount = m_terrainSettings.MeshTriangleCount;

        InitMesh(position);
    }

    public void SetPosition(Vector2 position)
    {
        m_position = position;
        m_gameObject.transform.position = new Vector3(m_position.x, 0, m_position.y);

        m_gridCoords = new Vector2Int(
            (int)(m_position.x / m_terrainSettings.m_meshSettings.meshScale),
            (int)(m_position.y / m_terrainSettings.m_meshSettings.meshScale)
        );

        m_chunkCoords = new Vector2Int(
            (int)(m_position.x / m_terrainSettings.MeshSideLength),
            (int)(m_position.y / m_terrainSettings.MeshSideLength)
        );

        m_gameObject.name = m_chunkCoords.ToString();

        m_gridRect = new RectInt(
            (int)(m_position.x / m_terrainSettings.m_meshSettings.meshScale),
            (int)(m_position.y / m_terrainSettings.m_meshSettings.meshScale),
            chunkSideCellsCount,
            chunkSideCellsCount
        );

        m_renderer.GetPropertyBlock(m_propertyBlock);
        m_propertyBlock.SetVector("BiomeChunkPos", new Vector4(m_position.x - m_terrainSettings.m_meshSettings.meshScale,
            m_position.y - m_terrainSettings.m_meshSettings.meshScale,
            0, 0)
        );
        m_renderer.SetPropertyBlock(m_propertyBlock);

        m_bounds = new Bounds(new Vector3(m_position.x, 0, m_position.y) +
            new Vector3(m_terrainSettings.MeshSideLength * 0.5f, 0, m_terrainSettings.MeshSideLength * 0.5f),
            m_terrainSettings.MeshSize);
    }

    public void InitMesh(Vector2 position)
    {
        if(m_state != STATE.ready)
        {
            m_nextPosition = position;
            m_nextState = STATE.initializing;
            return;
        }

        m_nextState = STATE.ready;
        if (m_position == position)
        {
            //m_gameObject.SetActive(true);
            m_callbackChunkInitialized(m_chunkCoords);
            return;
        }

        m_state = STATE.initializing;
        Clear();
        SetPosition(position);

        ComputeHeightMap();
        //ComputeBiomes();
        //ComputeMesh();

        //m_heightMapData.GetData();
        //m_heightMapComputer.ReleaseAll();
        //m_biomesComputer.ReleaseAll();
        //m_meshComputer.GetData();

        //UpdateMesh();
        //m_terrainSettings.m_material.SetTexture("Biomes", m_biomesRenderTex);
    }

    void ComputeHeightMap()
    {
        m_heightMapComputer = new HeightMapComputer(m_terrainSettings);
        m_heightMapComputer.Compute(m_position, HeightMapReady);
    }

    void HeightMapReady()
    {
        ComputeBiomes();
    }

    void ComputeBiomes()
    {
        m_biomesComputer = new BiomesComputer(m_terrainSettings);
        m_biomesComputer.Compute(m_position, m_heightMapComputer.heightMapBuffer, BiomesReady);
    }

    void BiomesReady()
    {
        ComputeMesh();
    }

    public void ComputeMesh()
    {
        m_meshComputer = new MeshComputer(m_terrainSettings);
        m_meshComputer.Compute(m_heightMapComputer.heightMapBuffer, new Vector3(m_position.x, 0, m_position.y), MeshDataReady);
    }

    void MeshDataReady()
    {
        m_heightMapComputer.ReleaseHeightmapBuffer();
        m_renderer.GetPropertyBlock(m_propertyBlock);
        //m_propertyBlock.SetTexture(m_biomesPropID, m_biomesComputer.biomes);
        m_propertyBlock.SetBuffer("_biomes", m_biomesComputer.biomes);
        m_renderer.SetPropertyBlock(m_propertyBlock);
        UpdateMesh();

        //m_biomesComputer.LogBiomes();
        InitTrees();
        //InitBoids();
    }

    void UpdateMesh()
    {
        m_mesh.Clear();
        m_mesh.vertices = m_meshComputer.vertices;        
        m_mesh.normals = m_meshComputer.normals;
        m_mesh.triangles = m_meshComputer.triangles;
        m_mesh.RecalculateBounds();
        m_gameObject.SetActive(true);
    }

    void InitTrees()
    {
        m_treesInitComputer = new TreesInitComputer(m_terrainSettings, m_position, m_meshComputer.normalsBuffer, m_meshComputer.meshCollisionData);
        m_treesInitComputer.Compute(m_biomesComputer.biomes, Time.time, TreesInitDataReady,
            new Vector2(m_exclusionTransformCenter.position.x, m_exclusionTransformCenter.position.z),
            20);
    }

    void TreesInitDataReady()
    {
        SpacialData[] spacialResult = m_treesInitComputer.m_spacialResult;
        BoidsInitComputer.Cyllinder[] colliders = m_treesInitComputer.m_collidersResult;
        int[] hasResult = m_treesInitComputer.m_hasResult;
        GameObject[] trees = m_terrainSettings.m_biomesSettings.Trees;

        for (int i = 0; i < hasResult.Length; i++)
        {
            int treeIndex = hasResult[i];
            if (treeIndex > -1)
            {
                GameObject gobj = GameObject.Instantiate(trees[treeIndex]);
                gobj.transform.SetParent(m_gameObject.transform);
                Cylinder cylinder = gobj.AddComponent<Cylinder>();
                //Debug.Log(spacialResult[i].position + "  "+ spacialResult[i].eulerAngles + "  "+ spacialResult[i].scale);
                cylinder.spacialData = spacialResult[i];
                cylinder.m_collider = colliders[i];
                AddTree(cylinder);
            }
        }
        InitBoids();
    }

    void InitBoids()
    {
       if(PersistentBoids.BoidsExist(m_chunkCoords))
        {
            LoadBoids();
        }
       else
        {
            ComputeBoids();
        }
    }

    void LoadBoids()
    {
        ThreadStart threadStart = delegate {
            PersistentBoid[] boids = PersistentBoids.Load(m_chunkCoords);
            BoidsInitDataReady(boids);
        };
        new Thread(threadStart).Start();
    }

    void ComputeBoids()
    {
        m_boidsInitComputer = new BoidsInitComputer(m_terrainSettings, m_position, m_meshComputer.normalsBuffer, m_meshComputer.meshCollisionData, m_treesInitComputer.m_hasResultBuffer);
        m_boidsInitComputer.Compute(Time.time, BoidsInitDataReady,
            new Vector2(m_exclusionTransformCenter.position.x, m_exclusionTransformCenter.position.z),
            20);
    }

    void BoidsInitDataReady()
    {
        m_meshComputer.ReleaseNormalBuffer();
        m_treesInitComputer.ReleaseHasResultBuffer();
        BoidsInitComputer.BoidSettingsResult[] boidsSettingsResult = m_boidsInitComputer.boidsSettingsResult;
        BoidsInitComputer.Cyllinder[] colliders = m_boidsInitComputer.boidCollidersResult;
        int[] hasBoidResult = m_boidsInitComputer.hasBoidResult;
        BoidSettings[] boids = m_terrainSettings.m_boidsSettings.boids;

        for (int i = 0; i < hasBoidResult.Length; i++)
        {
            int boidIndex = hasBoidResult[i] - 1;
            if (boidIndex > -1)
            {
                /*
                if (boidIndex > boids.Length)
                {
                    Debug.Log(boidIndex + "  " + i);
                }
                */
                BoidSettings boidSettings = boids[boidIndex];

                /*
                if(m_position == new Vector2(183f, 183f))
                {
                    Debug.Log(m_boidsInitComputer.meshPos+"   "+ m_boidsInitComputer.chunkCellsCount + "   "+ m_boidsInitComputer.cellLength + "   " + boidSettingsResult.chunkPos);
                }
                */
                GameObject gobj = GameObject.Instantiate(boidSettings.prefab);
                gobj.transform.SetParent(m_gameObject.transform);
                Boid boid = gobj.AddComponent<Boid>();
                boid.boidSettingsResult = boidsSettingsResult[i];
                boid.m_collider = colliders[i];
                boid.m_animator = gobj.GetComponent<ModelAnimator>();
                AddBoid(boid);
            }
        }

        m_state = STATE.ready;
        if (m_nextState == STATE.unloading)
        {
            Unload();
        }
        else
        {
            m_callbackChunkInitialized.Invoke(m_chunkCoords);
        }
    }

    void BoidsInitDataReady(PersistentBoid[] boids)
    {
        BoidSettings[] boidsSettingsResult = m_terrainSettings.m_boidsSettings.boids;
        for (int i=0;i<boids.Length;i++)
        {
            m_boids.Add(Boid.GetBoid(boids[i], boidsSettingsResult));
        }

        m_state = STATE.ready;
        if (m_nextState == STATE.unloading)
        {
            Unload();
        }
        else
        {
            m_callbackChunkInitialized.Invoke(m_chunkCoords);
        }
    }

    public void AddBoid(Boid boid)
    {
        m_boids.Add(boid);
        boid.m_chunk = this;
    }

    public void RemoveBoid(Boid boid)
    {
        m_boids.Remove(boid);
    }

    public void AddTree(Cylinder tree)
    {
        m_trees.Add(tree);
    }

    public void Render()
    {
        Graphics.DrawProcedural(m_terrainSettings.m_material, m_bounds, MeshTopology.Triangles, m_trianglesCount, 1);
        //Graphics.DrawProceduralIndirect(m_terrainSettings.m_material, m_bounds, MeshTopology.Triangles, m_renderArgsBuffer, 0, m_camera, m_propertyBlock, ShadowCastingMode.On, true, 0);
    }

    public void CopyCollisionData(RectInt gridRect, MeshComputer.MeshCollisionData[] meshColData)
    {
        MeshComputer.MeshCollisionData[] meshCollisionData = m_meshComputer.meshCollisionData;
        RectInt intersection = m_gridRect.Intersection(gridRect);
        RectInt gridRectLocal = new RectInt(
            intersection.x - m_gridRect.x,
            (intersection.y - m_gridRect.y) * 2,
            intersection.width,
            intersection.height * 2
        );

        Vector2Int originOffset = new Vector2Int(m_gridRect.x - gridRect.x, (m_gridRect.y - gridRect.y)*2);

        /*
        Debug.Log("  m_gridRect = " + m_gridRect
            + "  gridRect = "
            + gridRect + "  gridRectLocal = "
            + gridRectLocal
            + "  originOffset = " + originOffset
            + "  collision origin = " + (gridRectLocal.min + originOffset)
            );
        */

        for(int i= gridRectLocal.y; i< gridRectLocal.yMax; i++)
        {
            for (int j = gridRectLocal.x; j < gridRectLocal.xMax; j++)
            {
                int localIndex = i * chunkSideCellsCount + j;
                int colJ = j + originOffset.x;
                int colI = i + originOffset.y;
                int colIndex = colI * gridRect.width + colJ;
                //Debug.Log(new Vector2Int(j, i) + "  " + new Vector2Int(colJ, colI));
                meshColData[colIndex] = meshCollisionData[localIndex];
            }
        }
    }

    public void Clear()
    {
        for(int i=0;i<m_boids.Count;i++)
        {
            GameObject.Destroy(m_boids[i].gameObject);
        }
        m_boids.Clear();

        for (int i = 0; i < m_trees.Count; i++)
        {
            GameObject.Destroy(m_trees[i].gameObject);
        }
        m_trees.Clear();
    }

    public void Unload()
    {
        if (m_state != STATE.ready)
        {
            m_nextState = STATE.unloading;
            return;
        }

        m_nextState = STATE.ready;
        m_gameObject.SetActive(false);

        if (m_savedPosition == m_position)
        {
            m_callbackChunkUnloaded.Invoke(m_chunkCoords);
            return;
        }

        m_state = STATE.unloading;
        ThreadStart threadStart = delegate {
            PersistentBoids.Save(m_boids, m_chunkCoords);
            m_savedPosition = m_position;
            m_state = STATE.ready;
            if(m_nextState == STATE.initializing)
            {
                InitMesh(m_nextPosition);
            }
            else
            {
                m_callbackChunkUnloaded.Invoke(m_chunkCoords);
            }
        };
        new Thread(threadStart).Start();
    }

    public void Destroy()
    {
        m_mesh = null;
        if (m_gameObject)
            GameObject.Destroy(m_gameObject);
        m_gameObject = null;

        if (m_heightMapComputer != null)
            m_heightMapComputer.Release();
        m_heightMapComputer = null;

        if (m_biomesComputer != null)
            m_biomesComputer.Release();
        m_biomesComputer = null;

        if (m_meshComputer != null)
            m_meshComputer.Release();
        m_meshComputer = null;

        if (m_treesInitComputer != null)
            m_treesInitComputer.Release();
        m_treesInitComputer = null;

        if (m_boidsInitComputer != null)
            m_boidsInitComputer.Release();
        m_boidsInitComputer = null;
        ThreadStart threadStart = delegate {
            PersistentBoids.Save(m_boids, m_chunkCoords);
        };
        new Thread(threadStart).Start();
    }

    public bool ActiveSelf
    {
        get { return m_gameObject.activeSelf; }
        set { m_gameObject.SetActive(value); }
    }

    public void GetHeightMapImage(RenderTexture image)
    {
        m_heightMapComputer.GetHeightMapImage(image);
    }

    public void LogCollisionMesh()
    {
        m_meshComputer.LogCollisionMesh();
    }

    public void LogVerts()
    {
        m_meshComputer.LogVerts(new Vector3(m_position.x, 0, m_position.y));
        //m_meshComputer.LogTriangres(new Vector3(m_position.x, 0, m_position.y));
    }
}
