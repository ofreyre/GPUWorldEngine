using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uvs;
    public int[] triangles;

    public MeshData(int verticesLength)
    {
        vertices = new Vector3[verticesLength * verticesLength];
        normals = new Vector3[verticesLength * verticesLength];
        uvs = new Vector2[verticesLength * verticesLength];
        triangles = new int[(verticesLength - 1) * (verticesLength - 1) * 6];
    }
}
