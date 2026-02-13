using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public struct SpacialData
{
    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 scale;
    public float r;
    public Vector2Int gridCoords;
    public Vector2Int chunkCoords;
};

public class TreesInitComputer
{
    TerrainSettings m_settings;
    public int m_chunkCellsCount;
    public Vector2 m_meshPos;
    public float m_cellLength;
    Vector2 m_chunkCoords;

    public BoidsInitComputer.Cyllinder[] m_collidersResult;
    public SpacialData[] m_spacialResult;
    public int[] m_hasResult;

    ComputeBuffer m_normalsBuffer;
    ComputeBuffer m_meshCollisionDataBuffer;
    ComputeBuffer m_collidersResultBuffer;
    ComputeBuffer m_spacialResultBuffer;
    public ComputeBuffer m_hasResultBuffer;

    static int[] m_computeArgs;
    static ComputeBuffer m_treesCollidersBuffer;
    static ComputeBuffer m_treesProbabilitiesBuffer;
    static ComputeBuffer m_biomeTreeMapBuffer;

    static int m_biomeTreeMapBufferLength;
    Action m_callbackReady;
    int m_gpuDataCount;

    public TreesInitComputer(TerrainSettings settings, Vector2 meshPos, ComputeBuffer normalsBuffer, MeshComputer.MeshCollisionData[] meshCollisionData)
    {
        m_settings = settings;
        m_chunkCellsCount = settings.ChunkSideCellsCount;
        m_meshPos = meshPos;
        m_cellLength = settings.m_meshSettings.meshScale;
        m_chunkCoords = new Vector2((int)(meshPos.x / (m_cellLength * m_chunkCellsCount)), (int)(meshPos.y / (m_cellLength * m_chunkCellsCount)));

        m_normalsBuffer = normalsBuffer;

        if (m_treesCollidersBuffer == null)
        {
            BoidsInitComputer.Cyllinder[] treesPColliders = settings.m_biomesSettings.TreesColliders;
            m_treesCollidersBuffer = new ComputeBuffer(treesPColliders.Length, sizeof(float) * 3);
            m_treesCollidersBuffer.SetData(treesPColliders);

            float[] treesProbabilities = settings.m_biomesSettings.TreesProbabilities;
            m_treesProbabilitiesBuffer = new ComputeBuffer(treesProbabilities.Length, sizeof(float));
            m_treesProbabilitiesBuffer.SetData(treesProbabilities);

            Vector2Int[] biomeTreeMap = settings.m_biomesSettings.BiomeTreeMap;
            m_biomeTreeMapBuffer = new ComputeBuffer(biomeTreeMap.Length, sizeof(int) * 2);
            m_biomeTreeMapBuffer.SetData(biomeTreeMap);
            m_biomeTreeMapBufferLength = biomeTreeMap.Length;

            m_computeArgs = UtilsComputeShader.GetThreadGroups(
                settings.m_biomesSettings.initTreesComputeShader,
                settings.m_biomesSettings.initTreesComputeKernel,
                new Vector3Int(m_chunkCellsCount, m_chunkCellsCount, 0)
            );

        }

        InitBuffers(meshCollisionData);
    }

    public void InitBuffers(MeshComputer.MeshCollisionData[] meshCollisionData)
    {
        m_collidersResultBuffer = new ComputeBuffer(m_chunkCellsCount * m_chunkCellsCount, sizeof(float) * 3);
        m_spacialResultBuffer = new ComputeBuffer(m_chunkCellsCount * m_chunkCellsCount, sizeof(float) * 10 + sizeof(int) * 4);
        m_hasResultBuffer = new ComputeBuffer(m_chunkCellsCount * m_chunkCellsCount, sizeof(int));

        m_meshCollisionDataBuffer = new ComputeBuffer(meshCollisionData.Length, sizeof(float) * 7);
        m_meshCollisionDataBuffer.SetData(meshCollisionData);
    }

    public void ReleaseHasResultBuffer()
    {
        m_hasResultBuffer.Release();
    }

    public void Release()
    {
        if (m_treesProbabilitiesBuffer != null)
            m_treesProbabilitiesBuffer.Release();

        if (m_biomeTreeMapBuffer != null)
        {
            m_biomeTreeMapBuffer.Release();
        }

        if (m_collidersResultBuffer != null)
            m_collidersResultBuffer.Release();

        if (m_spacialResultBuffer != null)
        {
            m_spacialResultBuffer.Release();
        }

        if (m_hasResultBuffer != null)
            m_hasResultBuffer.Release();

        if (m_meshCollisionDataBuffer != null)
            m_meshCollisionDataBuffer.Release();

        if (m_treesCollidersBuffer != null)
            m_treesCollidersBuffer.Release();

        if (m_treesProbabilitiesBuffer != null)
            m_treesProbabilitiesBuffer.Release();
    }

    public void Compute(ComputeBuffer biomes, float time, Action callerCalback, Vector2 exclusionC, float exclusionR)
    {
        //Debug.Log(exclusionC + " " + exclusionR);
        //Debug.Log("BoidsInitComputer.Compute  meshPos = " + meshPos + "   chunkCellsCount = "+ chunkCellsCount + "   chunkCoords = "+ chunkCoords);
        m_gpuDataCount = 0;
        m_callbackReady = callerCalback;
        ComputeShader shader = m_settings.m_biomesSettings.initTreesComputeShader;
        int kernelHandle = shader.FindKernel(m_settings.m_biomesSettings.initTreesComputeKernel);

        shader.SetInt("biomeTreeMapBufferLength", m_biomeTreeMapBufferLength);
        shader.SetInt("chunkCellsCount", m_chunkCellsCount);
        shader.SetInt("biomesLength", m_settings.m_heightMapSettings.mapSize);
        shader.SetVector("meshCollisionOrigin", m_meshPos);
        shader.SetFloat("cellLength", m_cellLength);
        shader.SetFloat("time", time);
        shader.SetVector("chunkCoords", m_chunkCoords);

        shader.SetVector("exclusionC", exclusionC);
        shader.SetFloat("exclusionR", exclusionR);

        shader.SetFloat("cellCollisionWidth", m_settings.m_meshSettings.meshScale);
        shader.SetInt("collisionGridWidthInCells", m_settings.ChunkSideCellsCount);
        shader.SetFloat("normalTolerance", m_settings.m_biomesSettings.staticObjectNormalTolerance);

        shader.SetBuffer(kernelHandle, "normals", m_normalsBuffer);
        shader.SetBuffer(kernelHandle, "biomeTreeMap", m_biomeTreeMapBuffer);
        shader.SetBuffer(kernelHandle, "treesProbabilities", m_treesProbabilitiesBuffer);
        shader.SetBuffer(kernelHandle, "treesColliders", m_treesCollidersBuffer);
        shader.SetBuffer(kernelHandle, "collidersResult", m_collidersResultBuffer);
        shader.SetBuffer(kernelHandle, "spacialResult", m_spacialResultBuffer);
        shader.SetBuffer(kernelHandle, "meshCollisionData", m_meshCollisionDataBuffer);
        shader.SetBuffer(kernelHandle, "biomes", biomes);

        shader.SetBuffer(kernelHandle, "hasResult", m_hasResultBuffer);

        shader.Dispatch(kernelHandle, m_computeArgs[0], m_computeArgs[1], m_computeArgs[2]);
        AsyncGPUReadback.Request(m_collidersResultBuffer, collidersResultBufferCallback);
        AsyncGPUReadback.Request(m_spacialResultBuffer, SpacialDataResultBufferCallback);
        AsyncGPUReadback.Request(m_hasResultBuffer, HasBoidResultBufferCallback);
    }

    void collidersResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            m_collidersResult = readbackRequest.GetData<BoidsInitComputer.Cyllinder>().ToArray();
            m_collidersResultBuffer.Release();
            m_gpuDataCount++;
            DataReady();
        }
    }

    void SpacialDataResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            m_spacialResult = readbackRequest.GetData<SpacialData>().ToArray();
            m_spacialResultBuffer.Release();
            m_gpuDataCount++;
            DataReady();
        }
    }

    void HasBoidResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            m_hasResult = readbackRequest.GetData<int>().ToArray();
            //m_hasBoidResultBuffer.Release();
            m_gpuDataCount++;
            DataReady();
        }
    }

    void DataReady()
    {
        if (m_gpuDataCount >= 3)
        {
            //Debug.Log("DataReady");
            m_meshCollisionDataBuffer.Release();
            m_callbackReady.Invoke();
        }
    }
}
