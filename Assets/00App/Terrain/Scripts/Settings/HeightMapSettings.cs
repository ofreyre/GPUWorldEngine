using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class HeightMapSettings: PerlinNoiseSettings
{
    [Range(1, 16)] public int octaves = 6;
    public Vector2 offset;
    [Range(64, 1024)] public int mapSize = 64;
    public AnimationCurve curve;
    public ComputeShader computeShader;
    public string computeKernel;
}
