using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class RaysCollisionComputer
{
    public struct RayData
    {
        public Vector3 p0;
        public Vector3 p1;
        public float damage;

        public static RayData GetRayToScreenCenter(Camera camera, Vector3 origin, float length, float damage)
        {
            Vector3 destination = camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, length));
            return new RayData
            {
                p0 = origin,
                p1 = destination,
                damage = damage
            };
        }

        public float length
        {
            get { return Vector3.Magnitude(p1 - p0); }
        }
    };

    public struct RayCollision
    {
        public Vector3 p;
        public Vector3 n;
        public int boidI;
    };

    static int[] computeArgs;
    public RayCollision[] rays;

    TerrainSettings settings;
    ComputeBuffer boidBasesBuffer;
    ComputeBuffer boidCollidersBuffer;
    ComputeBuffer treesGridBuffer;
    ComputeBuffer treesCollidersBuffer;
    ComputeBuffer gridBoidsMapBuffer;
    ComputeBuffer gridStartBuffer;
    ComputeBuffer raysBuffer;
    ComputeBuffer raysResultBuffer;
    Action callbackReady;

    //ComputeBuffer rayCellsBuffer;
    //Vector2Int[] rayCells;

    int raysLength;

    public RaysCollisionComputer(TerrainSettings settings)
    {
        this.settings = settings;
    }

    public void InitBuffers(
        ComputeBuffer boidBasesBuffer,
        ComputeBuffer gridBoidsMapBuffer,
        ComputeBuffer gridStartBuffer,
        BoidsInitComputer.Cyllinder[] boidColliders,
        ComputeBuffer treesGridBuffer,
        ComputeBuffer treesCollidersBuffer,
        List<RayData> rays
        )
    {
        this.boidBasesBuffer = boidBasesBuffer;
        this.gridBoidsMapBuffer = gridBoidsMapBuffer;
        this.gridStartBuffer = gridStartBuffer;
        this.treesGridBuffer = treesGridBuffer;
        this.treesCollidersBuffer = treesCollidersBuffer;

        if (raysBuffer != null)
        {
            raysBuffer.Release();
            raysResultBuffer.Release();
            boidCollidersBuffer.Release();
            //rayCellsBuffer.Release();
        }

        raysLength = rays.Count;
        raysBuffer = new ComputeBuffer(raysLength, sizeof(float) * 7);
        raysBuffer.SetData(rays);
        raysResultBuffer = new ComputeBuffer(raysLength, sizeof(float) * 6 + sizeof(int));

        boidCollidersBuffer = new ComputeBuffer(boidColliders.Length, sizeof(float) * 3);
        boidCollidersBuffer.SetData(boidColliders);

        //rayCellsBuffer = new ComputeBuffer(20 * 10, sizeof(int) * 2);

        computeArgs = UtilsComputeShader.GetThreadGroups(
            settings.m_raysSettings.computeShader,
            settings.m_raysSettings.computeKernel,
            new Vector3Int(rays.Count, 0, 0),
            computeArgs
        );
    }

    public void Release()
    {
        if (raysBuffer != null)
            raysBuffer.Release();

        if (raysResultBuffer != null)
            raysResultBuffer.Release();

        if (boidCollidersBuffer != null)
            boidCollidersBuffer.Release();
    }

    public void Compute(RectInt boidsRect, int gridBoidsMapLength, int boidsLength, Action callbackReady)
    {
        gpuCount = 0;
        this.callbackReady = callbackReady;
        RaysSettings raysSettings = settings.m_raysSettings;
        ComputeShader shader = raysSettings.computeShader;
        int kernelHandle = shader.FindKernel(raysSettings.computeKernel);

        shader.SetInt("emptyCell", settings.EMPTY);
        shader.SetInt("gridWidth", boidsRect.width);
        shader.SetFloat("gridCellWidth", settings.m_meshSettings.meshScale);
        shader.SetVector("gridMin", new Vector4(boidsRect.xMin, boidsRect.yMin, 0, 0));
        shader.SetVector("gridMax", new Vector4(boidsRect.xMax, boidsRect.yMax, 0, 0));
        shader.SetInt("gridBoidsMapLength", gridBoidsMapLength);
        shader.SetInt("boidsLength", boidsLength);
        shader.SetInt("raysLength", raysLength);


        shader.SetBuffer(kernelHandle, "gridBoidsMap", gridBoidsMapBuffer);
        shader.SetBuffer(kernelHandle, "gridStart", gridStartBuffer);
        shader.SetBuffer(kernelHandle, "boids", boidBasesBuffer);
        shader.SetBuffer(kernelHandle, "boidColliders", boidCollidersBuffer);
        shader.SetBuffer(kernelHandle, "treesGrid", treesGridBuffer);
        shader.SetBuffer(kernelHandle, "treesColliders", treesCollidersBuffer);
        shader.SetBuffer(kernelHandle, "rays", raysBuffer);
        shader.SetBuffer(kernelHandle, "raysResult", raysResultBuffer);
        //shader.SetBuffer(kernelHandle, "rayCells", rayCellsBuffer);

        shader.Dispatch(kernelHandle, computeArgs[0], computeArgs[1], computeArgs[2]);
        AsyncGPUReadback.Request(raysResultBuffer, RaysResultBufferCallback);
        //AsyncGPUReadback.Request(rayCellsBuffer, RayCellsBufferCallback);
    }

    void RaysResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            gpuCount++;
            rays = readbackRequest.GetData<RayCollision>().ToArray();
            raysBuffer.Release();
            raysResultBuffer.Release();
            DataReady();
        }
    }

    void RayCellsBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            gpuCount++;
            //rayCells = readbackRequest.GetData<Vector2Int>().ToArray();
            //rayCellsBuffer.Release();
            DataReady();
        }
    }

    int gpuCount;
    void DataReady()
    {
        if (gpuCount >= 1)
        {
            callbackReady.Invoke();
        }
    }

    /*
    public void OnDrawGizmo()
    {
        if (rayCells != null)
        {
            var gizmoColor = Gizmos.color;
            Gizmos.color = Color.green;
            for (int i = 0; i < rayCells.Length; i++)
            {
                Vector3 center = new Vector3(rayCells[i].x + 0.5f, 10, rayCells[i].y + 0.5f);
                Gizmos.DrawCube(center, new Vector3(1, 0.2f, 1));
            }
            Gizmos.color = gizmoColor;
        }
    }
    */

}
