using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering;
using System.Threading.Tasks;

public class HeightMapComputer
{
    public static int[] computeArgs;
    public ComputeBuffer heightMapBuffer;
    public static ComputeBuffer octaveOffsetsBuffer;
    public static ComputeBuffer keysArrayBuffer;
    public static TerrainSettings settings;
    Action readyHandler;

    public HeightMapComputer(TerrainSettings settings)
    {
        HeightMapComputer.settings = settings;
        //heightMap = new float[settings.m_heightMapSettings.mapSize * settings.m_heightMapSettings.mapSize];
        InitBuffers();
    }

    public void InitBuffers()
    {
        if (heightMapBuffer != null)
        {
            heightMapBuffer.Release();
        }
        heightMapBuffer = new ComputeBuffer(settings.m_heightMapSettings.mapSize * settings.m_heightMapSettings.mapSize, sizeof(float));
        //heightMapBuffer.SetData(heightMap);

        if (octaveOffsetsBuffer == null)
        {
            Vector2[] octaveOffsets = UtilsMath.GetOctaveOffsets(settings.m_heightMapSettings);
            octaveOffsetsBuffer = new ComputeBuffer(octaveOffsets.Length, sizeof(float) * 2);
            octaveOffsetsBuffer.SetData(octaveOffsets);

            UtilsMath.CurveKeyframe[] keysArray = UtilsMath.AnimationCurveToKeysArray(settings.m_heightMapSettings.curve);
            keysArrayBuffer = new ComputeBuffer(keysArray.Length, sizeof(float) * 3);
            keysArrayBuffer.SetData(keysArray);

            computeArgs = UtilsComputeShader.GetThreadGroups(
                    settings.m_heightMapSettings.computeShader,
                    settings.m_heightMapSettings.computeKernel,
                    new Vector3Int(settings.m_heightMapSettings.mapSize, settings.m_heightMapSettings.mapSize, 0)
            );
        }
    }

    public void GetData(bool releaseAll = false)
    {
        float[] heightMap = new float[settings.m_heightMapSettings.mapSize * settings.m_heightMapSettings.mapSize];
        heightMapBuffer.GetData(heightMap);
        ReleaseTemp();

        if (releaseAll)
        {
            Release();
        }
    }

    public void ReleaseTemp()
    {
        octaveOffsetsBuffer.Release();
        keysArrayBuffer.Release();
    }

    public void Release()
    {
        if(octaveOffsetsBuffer != null)
            octaveOffsetsBuffer.Release();

        if(keysArrayBuffer != null)
            keysArrayBuffer.Release();

        if(heightMapBuffer != null)
            heightMapBuffer.Release();
    }

    public void ReleaseHeightmapBuffer()
    {
        if (heightMapBuffer != null)
            heightMapBuffer.Release();
    }

    public void Compute(Vector2 perlinOffset, Action readyHandler)
    {
        this.readyHandler = readyHandler;
        HeightMapSettings heightMapSettings = settings.m_heightMapSettings;
        ComputeShader shader = heightMapSettings.computeShader;
        int kernelHandle = shader.FindKernel(heightMapSettings.computeKernel);

        //PerlinHeightMap parameters
        shader.SetFloat("perlinScale", heightMapSettings.scale);
        shader.SetFloat("persistance", heightMapSettings.persistance);
        shader.SetFloat("lacunarity", heightMapSettings.lacunarity);
        float heightNormalK = (1 - heightMapSettings.persistance) / (1 - Mathf.Pow(heightMapSettings.persistance, heightMapSettings.octaves)) * 1.25f;
        shader.SetFloat("heightNormalK", heightNormalK);

        shader.SetBuffer(kernelHandle, "octaveOffsets", octaveOffsetsBuffer);
        //PerlinHeightMapCompute parameters
        shader.SetInt("octaves", heightMapSettings.octaves);
        shader.SetVector("perlinHeightMapOffset", new Vector4(perlinOffset.x + heightMapSettings.offset.x, perlinOffset.y + heightMapSettings.offset.y, 0, 0));

        shader.SetInt("mapLength", heightMapSettings.mapSize);
        shader.SetBuffer(kernelHandle, "heightMap", heightMapBuffer);

        shader.SetBuffer(kernelHandle, "CurveKeyframes", keysArrayBuffer);
        shader.SetInt("CubicHermiteSplineFramesCount", keysArrayBuffer.count);

        shader.Dispatch(kernelHandle, computeArgs[0], computeArgs[1], computeArgs[2]);

        AsyncGPUReadback.Request(heightMapBuffer, ReadyCallback);
    }

    void ReadyCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            readyHandler.Invoke();
        }
    }

    public void Log()
    {
        float[] heightMap = new float[settings.m_heightMapSettings.mapSize * settings.m_heightMapSettings.mapSize];

        heightMapBuffer.GetData(heightMap);
        //Debug.Log(string.Join(",", heightMap));

        int heightMapLength = settings.m_heightMapSettings.mapSize;
        float meshScale = settings.m_meshSettings.meshScale;
        float heightScale = settings.m_meshSettings.heightScale;
        int meshLength = settings.MeshLineVertexCount;
        int x = 0, y = 0;
        int vertexIndex = y * meshLength + x;
        int heightmapIndex = (y + 1) * heightMapLength + x + 1;


        Vector3 p = new Vector3(
            x * meshScale,
            heightMap[heightmapIndex] * heightScale,
            y * meshScale
        );

        Vector3 p1 = new Vector3(
            (x + 1) * meshScale,
            heightMap[heightmapIndex + heightMapLength + 1] * heightScale,
            (y + 1) * meshScale
        );

        Vector3 p2 = new Vector3(
            x * meshScale,
            heightMap[heightmapIndex + heightMapLength] * heightScale,
            (y + 1) * meshScale
        );

        Vector3 normal = Vector3.Cross(p2 - p, p1 - p).normalized;

        //2 Right
        p2 = new Vector3(
            (x + 1) * meshScale,
            heightMap[heightmapIndex + 1] * heightScale,
            y * meshScale
        );


        normal += Vector3.Cross(p1 - p, p2 - p).normalized;

        //3 Bottom Right
        p1 = new Vector3(
            (x + 1) * meshScale,
            heightMap[heightmapIndex - heightMapLength + 1] * heightScale,
            (y - 1) * meshScale
        );

        normal += Vector3.Cross(p2 - p, p1 - p).normalized;

        //4 Bottom
        p2 = new Vector3(
            x * meshScale,
            heightMap[heightmapIndex - heightMapLength] * heightScale,
            (y - 1) * meshScale
        );

        normal += Vector3.Cross(p1 - p, p2 - p).normalized;

        //5 Bottom Left
        p1 = new Vector3(
            (x - 1) * meshScale,
            heightMap[heightmapIndex - heightMapLength - 1] * heightScale,
            (y - 1) * meshScale
        );

        normal += Vector3.Cross(p2 - p, p1 - p).normalized;

        //6 Left
        p2 = new Vector3(
            (x - 1) * meshScale,
            heightMap[heightmapIndex - 1] * heightScale,
            y * meshScale
        );

        normal += Vector3.Cross(p1 - p, p2 - p).normalized;

        //7 Top Left
        p1 = new Vector3(
            (x - 1) * meshScale,
            heightMap[heightmapIndex + heightMapLength - 1] * heightScale,
            (y + 1) * meshScale
        );

        normal += Vector3.Cross(p2 - p, p1 - p).normalized;

        //8 Top
        p2 = new Vector3(
            x * meshScale,
            heightMap[heightmapIndex + heightMapLength] * heightScale,
            (y + 1) * meshScale
        );

        normal += Vector3.Cross(p1 - p, p2 - p).normalized;

        Debug.Log(vertexIndex + "  " + (normal/8));
    }

    public void GetHeightMapImage(RenderTexture image)
    {
        int mapSize = settings.m_heightMapSettings.mapSize;
        float[] heightMap = new float[mapSize * mapSize];
        heightMapBuffer.GetData(heightMap);
        Texture2D texture = new Texture2D(mapSize, mapSize);

        Color[] colors = new Color[mapSize * mapSize];
        for (int y=0;y< mapSize;y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                int i = y * mapSize + x;
                colors[i] = new Color(heightMap[i], heightMap[i], heightMap[i]);
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        Graphics.Blit(texture, image);
    }
}
