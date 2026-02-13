using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

public class MeshComputer
{
    public struct MeshCollisionData
    {
        public Vector3 p;
        public Vector3 n;
        public float d;

        public override string ToString()
        {
            return p + "  " + n + "  " + d;
        }
    }

    public static int[] computeArgs;
    public ComputeBuffer verticesBuffer;
    //public ComputeBuffer uvsBuffer;
    public ComputeBuffer trianglesBuffer;
    public ComputeBuffer normalsBuffer;
    public ComputeBuffer meshCollisionDataBuffer;
    public Vector3[] vertices;
    public Vector3[] normals;
    //public Vector2[] uvs;
    public int[] triangles;
    public MeshCollisionData[] meshCollisionData;
    public TerrainSettings settings;
    Action callbackReady;
    int gpuDataCount;

    public MeshComputer(TerrainSettings settings)
    {
        this.settings = settings;
        int verticesLength = settings.MeshLineVertexCount;
        //vertices = new Vector3[verticesLength * verticesLength];
        //normals = new Vector3[verticesLength * verticesLength];
        //uvs = new Vector2[verticesLength * verticesLength];
        //triangles = new int[(verticesLength - 1) * (verticesLength - 1) * 6];
        //meshCollisionData = new MeshCollisionData[(verticesLength - 1) * (verticesLength - 1) * 2];

        InitBuffers();
    }

    public void InitBuffers()
    {
        int meshVertexCount = settings.MeshVertexCount;
        verticesBuffer = new ComputeBuffer(settings.MeshVertexCount, sizeof(float) * 3);
        //verticesBuffer.SetData(vertices);
        //uvsBuffer = new ComputeBuffer(verticesLength * verticesLength, sizeof(float) * 2);
        //uvsBuffer.SetData(uvs);
        trianglesBuffer = new ComputeBuffer(settings.MeshTriangleCount, sizeof(int));
        //trianglesBuffer.SetData(triangles);
        normalsBuffer = new ComputeBuffer(settings.MeshVertexCount, sizeof(float) * 3);
        //normalsBuffer.SetData(normals);
        meshCollisionDataBuffer = new ComputeBuffer(settings.MeshCollisionCount, sizeof(float) * 7);
        //meshCollisionDataBuffer.SetData(meshCollisionData);

        if (computeArgs == null)
        {
            computeArgs = UtilsComputeShader.GetThreadGroups(
                settings.m_meshSettings.computeShader,
                settings.m_meshSettings.computeKernel,
                new Vector3Int(settings.MeshLineVertexCount, settings.MeshLineVertexCount, 0)
            );
        }
    }

    public void GetData(bool releaseAll = false)
    {
        verticesBuffer.GetData(vertices);
        //uvsBuffer.GetData(uvs);
        trianglesBuffer.GetData(triangles);
        normalsBuffer.GetData(normals);
        meshCollisionDataBuffer.GetData(meshCollisionData);

        //ReleaseTemp();
        if (releaseAll)
        {
            Release();
        }
    }

    public void ReleaseTemp()
    {
        verticesBuffer.Release();
        //uvsBuffer.Release();
        trianglesBuffer.Release();
    }

    public void ReleaseMesh()
    {
        if (verticesBuffer != null)
            verticesBuffer.Release();
        /*
        if (uvsBuffer != null)
        {
            uvsBuffer.Release();
        }
        */

        if (trianglesBuffer != null)
            trianglesBuffer.Release();
    }

    public void ReleaseNormalBuffer()
    {
        //Debug.Log("ReleaseNormalBuffer");
        if (normalsBuffer != null)
            normalsBuffer.Release();
    }

    public void Release()
    {
        ReleaseMesh();

        ReleaseNormalBuffer();

        if(meshCollisionDataBuffer != null)
            meshCollisionDataBuffer.Release();
    }

    //static int[] computeArgs;

    public void Compute(ComputeBuffer heightMapBuffer, Vector3 chunkPos, Action readyCallback)
    {
        gpuDataCount = 0;
        this.callbackReady = readyCallback;
        HeightMapSettings heightMapSettings = settings.m_heightMapSettings;
        MeshSettings meshSettings = settings.m_meshSettings;
        ComputeShader shader = meshSettings.computeShader;
        int kernelHandle = shader.FindKernel(meshSettings.computeKernel);

        shader.SetFloat("meshScale", meshSettings.meshScale);
        shader.SetFloat("heightScale", meshSettings.heightScale);
        shader.SetInt("meshLength", settings.MeshLineVertexCount);
        shader.SetInt("trianglesLength", settings.MeshLineVertexCount - 1);
        shader.SetInt("heightMapLength", heightMapSettings.mapSize);
        shader.SetInt("numCollisionVertsPerLine", settings.MeshLineCollisionCount);
        shader.SetVector("chunkPos", chunkPos);

        shader.SetBuffer(kernelHandle, "heightMap", heightMapBuffer);
        
        shader.SetBuffer(kernelHandle, "vertices", verticesBuffer);
        
        //shader.SetBuffer(kernelHandle, "uvs", uvsBuffer);
        
        shader.SetBuffer(kernelHandle, "triangles", trianglesBuffer);
        
        shader.SetBuffer(kernelHandle, "normals", normalsBuffer);

        shader.SetBuffer(kernelHandle, "meshCollisionData", meshCollisionDataBuffer);

        shader.Dispatch(kernelHandle, computeArgs[0], computeArgs[1], computeArgs[2]);
        AsyncGPUReadback.Request(verticesBuffer, VerticesBufferCallback);
        AsyncGPUReadback.Request(trianglesBuffer, TrianglesBufferCallback);
        AsyncGPUReadback.Request(normalsBuffer, NormalsBufferCallback);
        AsyncGPUReadback.Request(meshCollisionDataBuffer, MeshCollisionDataBufferCallback);
    }

    void VerticesBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            vertices = readbackRequest.GetData<Vector3>().ToArray();
            verticesBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void TrianglesBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            triangles = readbackRequest.GetData<int>().ToArray();
            trianglesBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void NormalsBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            normals = readbackRequest.GetData<Vector3>().ToArray();
            //normalsBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void MeshCollisionDataBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            meshCollisionData = readbackRequest.GetData<MeshCollisionData>().ToArray();
            meshCollisionDataBuffer.Release();
            //LogCollisionData();
            gpuDataCount++;
            DataReady();
        }
    }

    void LogCollisionData()
    {
        Debug.Log(string.Join(",", meshCollisionData));
        /*
        string a = "";
        for (int i = 0; i < meshCollisionData.Length;i++)
        {
            if(meshCollisionData[i].n.y == 0)
            {
                a += i + ", ";
            }
        }
        */
    }

    void DataReady()
    {
        if (gpuDataCount >= 4)
        {
            callbackReady.Invoke();
        }
    }

    public void LogCollisionMesh()
    {
        if(meshCollisionData != null)
        {
            for(int i=0; i < meshCollisionData.Length; i++)
            {
                MeshCollisionData data = meshCollisionData[i];
                Gizmos.DrawCube(data.p, new Vector3(0.1f, 0.1f, 0.1f));
                Gizmos.DrawRay(data.p, data.n);
            }
        }
    }

    public void LogVerts(Vector3 meshPosition)
    {   
        if (vertices != null)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 p = vertices[i] + meshPosition;
                Gizmos.DrawCube(p, new Vector3(0.1f, 0.1f, 0.1f));
                Gizmos.DrawRay(p, normals[i]);
            }
        }
    }

    public void LogTriangres(Vector3 meshPosition)
    {
        if (triangles != null)
        {
            var gizmoColor = Gizmos.color;
            Gizmos.color = Color.green;
            string verts = "";
            for (int i = 0; i < triangles.Length; i++)
            {
                if (i > triangles.Length - 1 - 6 * 2)
                {
                    Vector3 p = vertices[triangles[i]] + meshPosition;
                    Gizmos.DrawCube(p, new Vector3(0.3f, 0.3f, 0.3f));
                    verts += triangles[i] + " ";
                }
                //Gizmos.DrawRay(p, normals[i]);
            }
            Gizmos.color = gizmoColor;
            Debug.Log(verts+"    "+ triangles.Length + "   MeshTriangleCount = " + settings.MeshTriangleCount + "   MeshVertexCount = " + settings.MeshVertexCount+" " + settings.MeshLineVertexCount + " " + (settings.MeshTriangleCount / (settings.MeshLineVertexCount -1)/ (settings.MeshLineVertexCount - 1)));
        }
    }

    public void LogNormals()
    {
        int meshLength = settings.MeshLineVertexCount;
        int x = 0, y = 0;
        int i = y * meshLength + x;
        Debug.Log(i + "  " + normals[i]);
    }
}
