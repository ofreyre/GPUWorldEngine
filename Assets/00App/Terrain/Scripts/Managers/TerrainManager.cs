using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RayData = RaysCollisionComputer.RayData;
using DBG.Utils.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainManager : MonoBehaviour
{
    [SerializeField] TerrainSettings m_settings;
    [SerializeField] RayManagerReadyEvent m_rayManagerReadyEvent;
    [SerializeField] RayDataEvent m_rayDataEvent;
    [SerializeField] Pool m_shotsPool;
    [SerializeField] Pool m_bloodPool;
    [SerializeField] Pool m_treeParticlesPool;
    public RenderTexture m_testImage;
    public Transform m_centerTransform;
    public Material m_activeBoidsMaterial;
    public Material m_notProcessedBoidsMaterial;
    [SerializeField] Transform m_testP1;


    ChunksManager m_chunksManager;
    BoidsManager m_boidsManager;
    RaysManager m_raysManager;
    Chunk[] m_chunks;
    Vector2 m_centerOfView;
    RectInt m_boidsRect;
    RectInt m_chunksRect;
    RectInt m_collisionRect;
    PlayerTransform m_playerBoid;

    bool m_raysReady;

    void Start()
    {
        m_settings.UpdateMaterialGlobalProperties();
        UtilsIO.persistentDataPath = Application.persistentDataPath;
        UtilsIO.CreateFolder("Boids");
        UtilsIO.ClearFolder("Boids");

        m_raysReady = true;
        m_rayDataEvent.Register(ListenRayDataEvent);
        m_playerBoid = m_centerTransform.GetComponent<PlayerTransform>();
        m_playerBoid.UpdateCoords(m_settings.m_meshSettings.meshScale, m_settings.MeshSideLength);
        SetMaterialGlobalProperties();
        UpdateCenterOfView();
        m_chunksManager = new ChunksManager(m_settings, ChunksInitReady, m_centerTransform);
        m_chunks = m_chunksManager.InitChunks(m_chunksRect);
        //UpdateChunks();
    }

    void ChunksInitReady()
    {
        //m_chunks[0].GetHeightMapImage(m_testImage);
        //m_chunksManager.Update(m_boidsRect);
        m_chunksManager.UpdateMeshCollisionData(m_collisionRect);
        m_boidsManager = new BoidsManager(m_settings, m_chunks, BoidsReady);
        m_raysManager = new RaysManager(m_settings, RaysReady, m_boidsManager.DamageBoid, m_shotsPool, m_bloodPool, m_treeParticlesPool);
        m_raysManager.m_testP1 = m_testP1;
        m_boidsManager.ComputeBoids(Time.deltaTime, m_centerOfView, m_boidsRect, m_collisionRect, m_playerBoid, m_chunksManager.m_meshCollisionData);
    }

    void BoidsReady()
    {
        //Debug.Log("eeeeeeeeeeeeeeeee " + m_boidsManager.m_activeBoids.Count);
        m_raysManager.ComputeRays(
            m_boidsManager.m_boidsComputer.boidBasesBuffer,
            m_boidsManager.m_boidsComputer.gridBoidsMapBuffer,
            m_boidsManager.m_boidsComputer.gridStartBuffer,
            m_boidsManager.m_boidColliders,
            m_boidsManager.m_boidsComputer.treesGridBuffer,
            m_boidsManager.m_boidsComputer.treesCollidersBuffer,
            m_boidsManager.m_boidsComputer.gridBoidsMapLength,
            m_boidsManager.m_activeBoids.Count,
            m_boidsRect
        );
    }

    void RaysReady()
    {
        //Debug.Log("TerrainManager.RaysReady");
        m_raysReady = true;
        m_rayManagerReadyEvent.Dispatch(true);
    }

    public void ListenRayDataEvent(RayData rayData)
    {
        m_raysManager.AddRay(rayData);
    }

    void SetMaterialGlobalProperties()
    {
        m_settings.m_biomesSettings.ApplyToMaterial(m_settings.m_material);
    }

    void UpdateCenterOfView()
    {
        m_centerOfView = new Vector2(m_centerTransform.position.x, m_centerTransform.position.z);
        m_boidsRect = m_settings.GetBoidRectInCells(m_centerOfView);
        m_collisionRect = m_settings.GetCollisionRectInCells(m_centerOfView);
        m_chunksRect = m_settings.GetViewRectInChunks(m_centerOfView);
    }

    // Update is called once per frame
    void Update()
    {
        m_playerBoid.UpdateInput();
        if (m_raysReady)
        {
            UpdateCenterOfView();
            m_chunksManager.Update(m_collisionRect, m_chunksRect);
            if (m_boidsManager != null)
            {
                //Debug.Log("TerrainManager.Update");
                m_raysReady = false;
                m_rayManagerReadyEvent.Dispatch(false);
                m_boidsManager.Update(Time.deltaTime, m_centerOfView, m_boidsRect, m_collisionRect, m_chunksRect, m_playerBoid, m_chunksManager.m_meshCollisionData);
            }
        }
    }

    private void OnDestroy()
    {
        m_chunksManager.OnDestroy();
        m_boidsManager.OnDestroy();
        m_raysManager.OnDestroy();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_centerTransform != null)
        {
            float cellLength = m_settings.m_meshSettings.meshScale;
            RectInt boidsRect = m_settings.GetBoidRectInCells(new Vector2(m_centerTransform.position.x, m_centerTransform.position.z));
            var gizmoColor = Gizmos.color;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(boidsRect.center.x, 0, boidsRect.center.y) * cellLength, (new Vector3(boidsRect.width, 6, boidsRect.height)) * cellLength);
            Gizmos.color = gizmoColor;
        }

        /*
        if(m_raysManager != null)
        {
            m_raysManager.OnDrawGizmos();
        }
        
        if(m_chunks != null && m_chunks.Length > 0 && m_chunks[0] != null)
        {
            m_chunks[0].LogVerts();
        }
        */

        //DrawMeshGizmo();
    }
#endif
}
