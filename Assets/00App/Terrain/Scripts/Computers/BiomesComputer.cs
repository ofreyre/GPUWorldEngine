using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class BiomesComputer
{
    //public RenderTexture biomes;
    public static int[] computeArgs;
    public static ComputeBuffer biomesMappingBuffer;
    public static TerrainSettings settings;
    public static ComputeBuffer keysArrayBuffer;
    public static ComputeBuffer octaveOffsetsBuffer;

    public ComputeBuffer biomes;
    Action readyHandler;

    public BiomesComputer(TerrainSettings settings)
    {
        BiomesComputer.settings = settings;

        InitBuffers();
    }

    static bool biomesLogged;

    public void InitBuffers()
    {
        if(biomes == null)
        {
            /*
            biomes = new RenderTexture(settings.m_heightMapSettings.mapSize, settings.m_heightMapSettings.mapSize, 32, RenderTextureFormat.ARGB32, 0);
            biomes.filterMode = FilterMode.Point;
            biomes.antiAliasing = 1;
            biomes.autoGenerateMips = false;
            biomes.enableRandomWrite = true;
            biomes.Create();*/

            biomes = new ComputeBuffer(settings.m_heightMapSettings.mapSize * settings.m_heightMapSettings.mapSize, sizeof(uint));
        }

        if (biomesMappingBuffer == null)
        {
            uint[] biomesMapping = NewBiomesMapping(settings.m_biomesSettings);
            biomesMappingBuffer = new ComputeBuffer(biomesMapping.Length, sizeof(uint));
            biomesMappingBuffer.SetData(biomesMapping);

            UtilsMath.CurveKeyframe[] keysArray = UtilsMath.AnimationCurveToKeysArray(settings.m_heightMapSettings.curve);
            keysArrayBuffer = new ComputeBuffer(keysArray.Length, sizeof(float) * 3);
            keysArrayBuffer.SetData(keysArray);

            Vector2[] octaveOffsets = UtilsMath.GetOctaveOffsets(settings.m_heightMapSettings);
            octaveOffsetsBuffer = new ComputeBuffer(octaveOffsets.Length, sizeof(float) * 2);
            octaveOffsetsBuffer.SetData(octaveOffsets);


            int biomesLength = settings.m_heightMapSettings.mapSize;
            computeArgs = UtilsComputeShader.GetThreadGroups(
                settings.m_biomesSettings.computeShader,
                settings.m_biomesSettings.computeKernel,
                new Vector3Int(biomesLength, biomesLength, 0)
            );
        }
    }

    public void Release()
    {
        if (biomes != null)
            biomes.Release();

        if (biomesMappingBuffer != null)
            biomesMappingBuffer.Release();

        if (keysArrayBuffer != null)
            keysArrayBuffer.Release();

        if (octaveOffsetsBuffer != null)
            octaveOffsetsBuffer.Release();
    }

    public void Compute(Vector2 perlinOffset, ComputeBuffer heightMapBuffer, Action readyHandler)
    {
        this.readyHandler = readyHandler;
        BiomesSettings biomesSettings = settings.m_biomesSettings;
        ClimateSettings climateSettings = settings.m_climateSettings;

        int biomesLength = settings.m_heightMapSettings.mapSize;

        ComputeShader shader = biomesSettings.computeShader;
        int kernelHandle = shader.FindKernel(biomesSettings.computeKernel);

        //shader.SetTexture(kernelHandle, "biomes", biomes);
        shader.SetBuffer(kernelHandle, "biomes", biomes);

        shader.SetBuffer(kernelHandle, "heightMap", heightMapBuffer);
        shader.SetInt("CubicHermiteSplineFramesCount", keysArrayBuffer.count);

        //Vector3[] biomesMapping = GetBiomesMapping(biomesSettings);
        shader.SetBuffer(kernelHandle, "biomesMapping", biomesMappingBuffer);

        shader.SetBuffer(kernelHandle, "CurveKeyframes", keysArrayBuffer);

        //PerlinHeightMap parameters
        shader.SetFloat("perlinScale", biomesSettings.scale);
        shader.SetFloat("persistance", biomesSettings.persistance);
        shader.SetFloat("lacunarity", biomesSettings.lacunarity);
        float heightNormalK = (1 - biomesSettings.persistance) / (1 - Mathf.Pow(biomesSettings.persistance, biomesSettings.octaves)) * 1.25f;
        shader.SetFloat("heightNormalK", heightNormalK);

        shader.SetBuffer(kernelHandle, "octaveOffsets", octaveOffsetsBuffer);
        //PerlinHeightMapCompute parameters
        shader.SetInt("octaves", biomesSettings.octaves);

        shader.SetInt("mCount", 4);
        shader.SetInt("hCount", 3);
        shader.SetInt("biomesLength", biomesLength);
        shader.SetFloat("latitudeOffset", 0);
        shader.SetFloat("latitudeRange", settings.LatitudeRange);
        shader.SetFloat("maxHeight", 1.0f);
        shader.SetInt("heightMapLength", settings.m_heightMapSettings.mapSize);
        shader.SetVector("perlinOffset", perlinOffset + biomesSettings.offset);
        shader.SetFloat("perlinScale", climateSettings.scale);

        shader.Dispatch(kernelHandle, computeArgs[0], computeArgs[1], computeArgs[2]);

        AsyncGPUReadback.Request(biomes, BiomesCallback);
    }

    void BiomesCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            readyHandler.Invoke();
        }
    }

    public void LogBiomes()
    {
        /*
        if (!biomesLogged)
        {
            biomesLogged = true;
            Texture2D tex = new Texture2D(settings.m_heightMapSettings.mapSize, settings.m_heightMapSettings.mapSize, TextureFormat.RGBA32, false);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = biomes;
            tex.ReadPixels(new Rect(0, 0, settings.m_heightMapSettings.mapSize, settings.m_heightMapSettings.mapSize), 0, 0);
            tex.Apply();
            Color[] colors = tex.GetPixels();
            string a = "";
            for (int i = 0; i < colors.Length; i++)
            {
                a += colors[i].g + ",";
            }
            Debug.Log(a);
        }
        */
        if (!biomesLogged)
        {
            biomesLogged = true;
            int[] bb = new int[settings.m_heightMapSettings.mapSize * settings.m_heightMapSettings.mapSize];
            biomes.GetData(bb);
            Debug.Log(string.Join(",", bb));
        }

     }

    static uint[] NewBiomesMapping(BiomesSettings biomesSettings)
    {
        uint[] mapping = new uint[3 * 4 * 3];
        for(int t=0;t<3;t++)
        {
            for (int m = 0; m < 4; m++)
            {
                for (int h = 0; h < 3; h++)
                {
                    int i = h + m * 3 + t * 3 * 4;
                    if(t == 0)
                    {
                        mapping[i] = 1; //, biomesSettings.ice.blend, biomesSettings.ice.order);
                    }
                    else if(h == 0)
                    {
                        mapping[i] = 0;//, biomesSettings.water.blend, biomesSettings.water.order);
                    }
                    else if(h==1)
                    {
                        if (m == 0)
                            mapping[i] = 2;//, biomesSettings.sand.blend, biomesSettings.sand.order);
                        if(m == 1)
                            mapping[i] = 3;//f, biomesSettings.soil.blend, biomesSettings.soil.order);
                        else
                            mapping[i] = 6;//f, biomesSettings.grass.blend, biomesSettings.grass.order);
                    }
                    else if(t == 1)
                    {
                        if(m == 0)
                        {
                            mapping[i] = 5;//f, biomesSettings.rocks.blend, biomesSettings.rocks.order);
                        }
                        else if(m == 1)
                        {
                            mapping[i] = 3;//f, biomesSettings.soil.blend, biomesSettings.soil.order); //soil
                        }
                        else
                        {
                            mapping[i] = 8;//f, biomesSettings.tundra.blend, biomesSettings.tundra.order); //tundra
                        }
                    }
                    else
                    {
                        if (m == 0)
                        {
                            mapping[i] = 4;//f, biomesSettings.drySoil.blend, biomesSettings.drySoil.order); //dry
                        }
                        else if (m == 1)
                        {
                            mapping[i] = 6;//f, biomesSettings.grass.blend, biomesSettings.grass.order); //grass
                        }
                        else
                        {
                            mapping[i] = 7;//f, biomesSettings.tundra.blend, biomesSettings.tundra.order); //tundra
                        }
                    }

                    //Debug.Log(mapping[i]);
                }
            }
        }

        return mapping;
    }


}
