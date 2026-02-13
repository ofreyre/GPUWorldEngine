using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MeshSettings
{
    public float meshScale = 1;
    public float heightScale = 50;
    [Range(62, 1022)] public int meshSize;
    public float meshWorldSize = 1022;

    public ComputeShader computeShader;
    public string computeKernel;

    public Vector3 MeshWorldSize
    {
        get { return new Vector3(meshSize, heightScale, meshSize); }
    }
}
