using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class StaticObjectSettings
{
    public GameObject prefab;
    public BoidsInitComputer.Cyllinder collider;
    public float probability;
}

[Serializable]
public class BiomeData
{
    public Texture2D texture;
    public Texture2D normal;
    [Range(0.0f, 1.0f)] public float blend;
    [Range(0.0f, 1.0f)] public float order;
    public StaticObjectSettings[] trees;
}

[Serializable]
public class BiomesSettings: PerlinNoiseSettings
{
    [Range(1, 16)] public int octaves = 6;
    public Vector2 offset;
    public AnimationCurve curve;
    public BiomeData water;
    public BiomeData ice;
    public BiomeData sand;
    public BiomeData soil;
    public BiomeData drySoil;
    public BiomeData rocks;
    public BiomeData grass;
    public BiomeData forest;
    public BiomeData tundra;

    public float staticObjectNormalTolerance;
    public ComputeShader computeShader;
    public string computeKernel;
    public ComputeShader initTreesComputeShader;
    public string initTreesComputeKernel;

    public Vector2Int TexturesSize
    {
        get {
            return new Vector2Int(water.texture.width, water.texture.height);
        }
    }

    public Texture2D[] Textures
    {
        get
        {
            Texture2D[] textures = new Texture2D[9];
            textures[0] = water.texture;
            textures[1] = ice.texture;
            textures[2] = sand.texture;
            textures[3] = soil.texture;
            textures[4] = drySoil.texture;
            textures[5] = rocks.texture;
            textures[6] = grass.texture;
            textures[7] = forest.texture;
            textures[8] = tundra.texture;
            return textures;
        }
    }

    public Texture2D[] Normals
    {
        get
        {
            Texture2D[] textures = new Texture2D[9];
            textures[0] = water.normal;
            textures[1] = ice.normal;
            textures[2] = sand.normal;
            textures[3] = soil.normal;
            textures[4] = drySoil.normal;
            textures[5] = rocks.normal;
            textures[6] = grass.normal;
            textures[7] = forest.normal;
            textures[8] = tundra.normal;
            return textures;
        }
    }

    public BiomeData[] Biomes
    {
        get
        {
            BiomeData[] biomes = new BiomeData[9];
            biomes[0] = water;
            biomes[1] = ice;
            biomes[2] = sand;
            biomes[3] = soil;
            biomes[4] = drySoil;
            biomes[5] = rocks;
            biomes[6] = grass;
            biomes[7] = forest;
            biomes[8] = tundra;
            return biomes;
        }
    }

    public GameObject[] Trees
    {
        get
        {
            List<GameObject> trees = new List<GameObject>();
            if(water.trees != null && water.trees.Length > 0)
            {
                water.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (ice.trees != null && ice.trees.Length > 0)
            {
                ice.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (sand.trees != null && sand.trees.Length > 0)
            {
                sand.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (soil.trees != null && soil.trees.Length > 0)
            {
                soil.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (drySoil.trees != null && drySoil.trees.Length > 0)
            {
                drySoil.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (rocks.trees != null && rocks.trees.Length > 0)
            {
                rocks.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (grass.trees != null && grass.trees.Length > 0)
            {
                grass.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (forest.trees != null && forest.trees.Length > 0)
            {
                forest.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }
            if (tundra.trees != null && tundra.trees.Length > 0)
            {
                tundra.trees.ToList().ForEach(tree => trees.Add(tree.prefab));
            }

            return trees.ToArray();
        }
    }

    public BoidsInitComputer.Cyllinder[] TreesColliders
    {
        get
        {
            List<BoidsInitComputer.Cyllinder> colliders = new List<BoidsInitComputer.Cyllinder>();
            if (water.trees != null && water.trees.Length > 0)
            {
                water.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (ice.trees != null && ice.trees.Length > 0)
            {
                ice.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (sand.trees != null && sand.trees.Length > 0)
            {
                sand.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (soil.trees != null && soil.trees.Length > 0)
            {
                soil.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (drySoil.trees != null && drySoil.trees.Length > 0)
            {
                drySoil.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (rocks.trees != null && rocks.trees.Length > 0)
            {
                rocks.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (grass.trees != null && grass.trees.Length > 0)
            {
                grass.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (forest.trees != null && forest.trees.Length > 0)
            {
                forest.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }
            if (tundra.trees != null && tundra.trees.Length > 0)
            {
                tundra.trees.ToList().ForEach(tree => colliders.Add(tree.collider));
            }

            return colliders.ToArray();
        }
    }

    public float[] TreesProbabilities
    {
        get
        {
            List<float> probs = new List<float>();
            if (water.trees != null && water.trees.Length > 0)
            {
                water.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (ice.trees != null && ice.trees.Length > 0)
            {
                ice.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (sand.trees != null && sand.trees.Length > 0)
            {
                sand.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (soil.trees != null && soil.trees.Length > 0)
            {
                soil.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (drySoil.trees != null && drySoil.trees.Length > 0)
            {
                drySoil.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (rocks.trees != null && rocks.trees.Length > 0)
            {
                rocks.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (grass.trees != null && grass.trees.Length > 0)
            {
                grass.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (forest.trees != null && forest.trees.Length > 0)
            {
                forest.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }
            if (tundra.trees != null && tundra.trees.Length > 0)
            {
                tundra.trees.ToList().ForEach(tree => probs.Add(tree.probability));
            }

            return probs.ToArray();
        }
    }

    public Vector2Int[] BiomeTreeMap
    {
        get
        {
            List<Vector2Int> map = new List<Vector2Int>();
            int c = 0;
            if (water.trees != null && water.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + water.trees.Length));
                c += water.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (ice.trees != null && ice.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + ice.trees.Length));
                c += ice.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (sand.trees != null && sand.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + sand.trees.Length));
                c += sand.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (soil.trees != null && soil.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + soil.trees.Length));
                c += soil.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (drySoil.trees != null && drySoil.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + drySoil.trees.Length));
                c += drySoil.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (rocks.trees != null && rocks.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + rocks.trees.Length));
                c += rocks.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (grass.trees != null && grass.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + grass.trees.Length));
                c += grass.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (forest.trees != null && forest.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + forest.trees.Length));
                c += forest.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            if (tundra.trees != null && tundra.trees.Length > 0)
            {
                map.Add(new Vector2Int(c, c + tundra.trees.Length));
                c += tundra.trees.Length;
            }
            else
            {
                map.Add(new Vector2Int(-1, 0));
            }

            return map.ToArray();
        }
    }

    public Texture2D Tiles
    {
        get { return UtilsTexture.MergeTextures2D(3,3,new Vector2Int(512, 512), Textures, TextureFormat.ARGB32, true, FilterMode.Point); }
    }

    public Texture2D TilesNormals
    {
        get { return UtilsTexture.MergeTextures2D(3, 3, new Vector2Int(512, 512), Normals, TextureFormat.ARGB32, true, FilterMode.Bilinear); }
    }

    public Vector2 TileUvSize
    {
        get
        {
            float size = 512 / (512 * 3.0f);
            return new Vector2(size, size);
        }
    }

    public void ApplyToMaterial(Material material)
    {
        material.SetTexture("Albedos", UtilsTexture.ToTextureArray(512, 512, Textures, TextureFormat.ARGB32, true, FilterMode.Bilinear));
        material.SetTexture("Normals", UtilsTexture.ToTextureArray(512, 512, Normals, TextureFormat.ARGB32, true, FilterMode.Bilinear));
    }
}
