using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System;

public class BoidsInitComputer
{
    [Serializable]
    public struct Cyllinder
    {
        public float r;
        public float h;
        public float y;

        public override string ToString()
        {
            return "(" + r + ", " + h+", " + y+")";
        }
    }

    public struct BoidInitSettings
    {
        public float maxSpeed;
        public float maxForce;
        public float seekWeight;
        public float r;
        public float minScale;
        public float maxScale;
        public float spawnProb;
        public float stamina;
    };

    public struct BoidSettingsResult
    {
        public Vector3 pos;
        public float r;
        public float scale;
        public float maxSpeed;
        public float maxForce;
        public float seekWeight; //]0,1[
        public Vector2Int gridPos;
        public Vector2Int chunkCoords;
        public float stamina;
    };

    static int[] computeArgs;

    public static int boidsInitSettingsLength;
    public int chunkCellsCount;
    public Vector2 meshPos;
    public float cellLength;
    public BoidSettingsResult[] boidsSettingsResult;
    public Cyllinder[] boidCollidersResult;
    public int[] hasBoidResult;

    public ComputeBuffer normalsBuffer;
    public ComputeBuffer cellContentBuffer;
    public static ComputeBuffer boidsInitSettingsBuffer;
    public static ComputeBuffer boidInitColliderBuffer;
    public ComputeBuffer boidsSettingsResultBuffer;
    public ComputeBuffer boidCollidersResultBuffer;
    public ComputeBuffer hasBoidResultBuffer;
    ComputeBuffer meshCollisionDataBuffer;


    public TerrainSettings settings;

    Action callbackReady;
    int gpuDataCount;
    Vector2 chunkCoords;

    public BoidsInitComputer(TerrainSettings settings, Vector2 meshPos, ComputeBuffer normalsBuffer, MeshComputer.MeshCollisionData[] meshCollisionData, ComputeBuffer cellContentBuffer)
    {
        this.settings = settings;
        chunkCellsCount = settings.ChunkSideCellsCount;
        this.meshPos = meshPos;
        cellLength = settings.m_meshSettings.meshScale;
        chunkCoords = new Vector2((int)(meshPos.x / (cellLength * chunkCellsCount)), (int)(meshPos.y / (cellLength * chunkCellsCount)));

        if (boidsInitSettingsBuffer == null)
        {
            BoidInitSettings[] boidsInitSettings;
            Cyllinder[] boidInitCollider;
            settings.m_boidsSettings.GetBoidsInitData(out boidsInitSettings, out boidInitCollider);

            boidsInitSettingsBuffer = new ComputeBuffer(boidsInitSettings.Length, sizeof(float) * 8);
            boidsInitSettingsBuffer.SetData(boidsInitSettings);
            boidsInitSettingsLength = boidsInitSettings.Length;

            boidInitColliderBuffer = new ComputeBuffer(boidsInitSettings.Length, sizeof(float) * 3);
            boidInitColliderBuffer.SetData(boidInitCollider);

            computeArgs = UtilsComputeShader.GetThreadGroups(
                settings.m_boidsSettings.boidsInitShader,
                settings.m_boidsSettings.boidsInitKernel,
                new Vector3Int(chunkCellsCount, chunkCellsCount, 0)
            );
        }
        InitBuffers(meshCollisionData, normalsBuffer, cellContentBuffer);
    }

    public void InitBuffers(MeshComputer.MeshCollisionData[] meshCollisionData, ComputeBuffer normalsBuffer, ComputeBuffer cellContentBuffer)
    {
        this.normalsBuffer = normalsBuffer;
        this.cellContentBuffer = cellContentBuffer;
        boidsSettingsResultBuffer = new ComputeBuffer(chunkCellsCount * chunkCellsCount, sizeof(float) * 9 + sizeof(int) * 4);
        //hasBoidResult = new int[boidsSettingsResult.Length];
        boidCollidersResultBuffer = new ComputeBuffer(chunkCellsCount * chunkCellsCount, sizeof(float) * 3);
        hasBoidResultBuffer = new ComputeBuffer(chunkCellsCount * chunkCellsCount, sizeof(int));
        //hasBoidResultBuffer.SetData(hasBoidResult);

        meshCollisionDataBuffer = new ComputeBuffer(meshCollisionData.Length, sizeof(float) * 7);
        meshCollisionDataBuffer.SetData(meshCollisionData);
    }

    public void GetData(bool releaseAll = false)
    {
        boidsSettingsResultBuffer.GetData(boidsSettingsResult);
        hasBoidResultBuffer.GetData(hasBoidResult);

        ReleaseTemp();
        if (releaseAll)
        {
            Release();
        }
    }

    public void ReleaseTemp()
    {
        boidsSettingsResultBuffer.Release();
        hasBoidResultBuffer.Release();
    }

    public void Release()
    {
        if (boidsInitSettingsBuffer != null)
            boidsInitSettingsBuffer.Release();

        if(boidInitColliderBuffer != null)
        {
            boidInitColliderBuffer.Release();
        }

        if (boidsSettingsResultBuffer != null)
            boidsSettingsResultBuffer.Release();

        if(boidCollidersResultBuffer != null)
        {
            boidCollidersResultBuffer.Release();
        }

        if (hasBoidResultBuffer != null)
            hasBoidResultBuffer.Release();

        if (normalsBuffer != null)
            normalsBuffer.Release();
    }



    public void Compute(float time, Action callerCalback, Vector2 exclusionC, float exclusionR)
    {
        //Debug.Log(exclusionC + " " + exclusionR);
        //Debug.Log("BoidsInitComputer.Compute  meshPos = " + meshPos + "   chunkCellsCount = "+ chunkCellsCount + "   chunkCoords = "+ chunkCoords);
        gpuDataCount = 0;
        this.callbackReady = callerCalback;
        ComputeShader shader = settings.m_boidsSettings.boidsInitShader;
        int kernelHandle = shader.FindKernel(settings.m_boidsSettings.boidsInitKernel);

        shader.SetInt("boidsInitSettingsLength", boidsInitSettingsLength);
        shader.SetInt("chunkCellsCount", chunkCellsCount);
        shader.SetVector("meshCollisionOrigin", meshPos);
        shader.SetFloat("cellLength", cellLength);
        shader.SetFloat("time", time);
        shader.SetVector("chunkCoords", chunkCoords);

        shader.SetVector("exclusionC", exclusionC);
        shader.SetFloat("exclusionR", exclusionR);

        shader.SetFloat("cellCollisionWidth", settings.m_meshSettings.meshScale);
        shader.SetInt("collisionGridWidthInCells", settings.ChunkSideCellsCount);
        shader.SetFloat("normalTolerance", settings.m_biomesSettings.staticObjectNormalTolerance);

        shader.SetBuffer(kernelHandle, "normals", normalsBuffer);
        shader.SetBuffer(kernelHandle, "cellContent", cellContentBuffer);
        shader.SetBuffer(kernelHandle, "boidsInitSettings", boidsInitSettingsBuffer);
        shader.SetBuffer(kernelHandle, "boidsInitCollisions", boidInitColliderBuffer);
        shader.SetBuffer(kernelHandle, "boidsSettingsResult", boidsSettingsResultBuffer);
        shader.SetBuffer(kernelHandle, "boidCollidersResult", boidCollidersResultBuffer);
        shader.SetBuffer(kernelHandle, "meshCollisionData", meshCollisionDataBuffer);

        shader.SetBuffer(kernelHandle, "hasBoidResult", hasBoidResultBuffer);

        shader.Dispatch(kernelHandle, computeArgs[0], computeArgs[1], computeArgs[2]);
        AsyncGPUReadback.Request(boidsSettingsResultBuffer, BoidSettingsResultBufferCallback);
        AsyncGPUReadback.Request(boidCollidersResultBuffer, BoidCollidersResultBufferCallback);
        AsyncGPUReadback.Request(hasBoidResultBuffer, HasBoidResultBufferCallback);
    }

    void BoidSettingsResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if(!readbackRequest.hasError)
        {
            boidsSettingsResult = readbackRequest.GetData<BoidSettingsResult>().ToArray();
            boidsSettingsResultBuffer.Release();
            meshCollisionDataBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void BoidCollidersResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            boidCollidersResult = readbackRequest.GetData<Cyllinder>().ToArray();
            boidCollidersResultBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void HasBoidResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            hasBoidResult = readbackRequest.GetData<int>().ToArray();
            hasBoidResultBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void DataReady()
    {
        if(gpuDataCount >= 3)
        {
            callbackReady.Invoke();
        }
    }
}
