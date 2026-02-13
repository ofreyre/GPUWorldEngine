using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Threading.Tasks;
using DBG.Utils.IO;

public class RuntimeAnimData
{
    public int vertexCount;
    public ComputeBuffer clipsDataBuffer;
    public ComputeBuffer geometryBuffer;
    public GameObject gameObject;
}

public struct GameClip
{
    public int boidIndex;
    public string clip;
}

public class BakedModelsManager : MonoBehaviour
{
    [SerializeField] TerrainSettings m_settings;
    FullMeshClipsData[] m_models;
    int m_loadCount;
    RuntimeAnimData[] m_animationData;
    Dictionary<GameClip, int> m_gameObjectClipIndex = new Dictionary<GameClip, int>();

    int m_modelsCount;

    void Start()
    {
        m_loadCount = 0;
        m_modelsCount = m_settings.m_boidsSettings.boids.Length;
        m_models = new FullMeshClipsData[m_modelsCount];
        string path = Application.streamingAssetsPath + "/" + m_settings.m_boidsSettings.bakeSavePath + "/";

        for (int i=0;i< m_modelsCount; i++)
        {
            LoadModel(i, path + m_settings.m_boidsSettings.boids[i].prefab.name + ".ani");
        }
    }

    void LoadModel(int i, string path)
    {
        Debug.Log("LoadModel " + i + "  " + path);
        ThreadStart threadStart = delegate {
            Debug.Log("dddddddd " + i + "  " + path);
            m_models[i] = UtilsIO.LoadAbsolute<FullMeshClipsData>(path);
            m_loadCount++;
            Debug.Log(m_loadCount);
        };
        new Thread(threadStart).Start();
    }

    void Update()
    {
        if(m_loadCount == m_modelsCount)
        {
            Debug.Log("eeeeeeeeeeeeeee");
            CreateObjects();
            m_loadCount = m_modelsCount + 1;
        }
    }

    void CreateObjects()
    {
        m_animationData = new RuntimeAnimData[m_modelsCount];
        for (int i = 0;i< m_modelsCount;i++)
        {
            m_animationData[i] = CreateAnimatedObject(i);
        }
    }

    RuntimeAnimData CreateAnimatedObject(int modelIndex)
    {
        FullMeshClipsData data = m_models[modelIndex];
        GameObject gobj = new GameObject();
        MeshRenderer renderer = gobj.AddComponent<MeshRenderer>();
        MeshFilter filter = gobj.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Debug.Log(data.triangles);
        Debug.Log(data.uvs);
        Debug.Log(data.geometry);
        mesh.uv = data.UnityUVs;
        mesh.triangles = data.triangles;
        Vector3[] vectors = new Vector3[data.vertexCount];
        mesh.vertices = vectors;
        mesh.normals = vectors;
        RuntimeAnimData runtimeData = new RuntimeAnimData {
            vertexCount = data.vertexCount,
            clipsDataBuffer = new ComputeBuffer(data.clips.Length, sizeof(int) * 2),
            geometryBuffer = new ComputeBuffer(data.geometry.Length, sizeof(float) * 6)
        };

        List<Vector2Int> clipsInfo = new List<Vector2Int>();
        for(int i=0;i< data.clips.Length;i++)
        {
            clipsInfo.Add(new Vector2Int(data.clips[i].startIndex, data.clips[i].frames));
            m_gameObjectClipIndex.Add(new GameClip { boidIndex = modelIndex, clip = data.clips[i].clip }, i);
        }
        runtimeData.clipsDataBuffer.SetData(clipsInfo);
        runtimeData.geometryBuffer.SetData(data.geometry);

        //renderer.sharedMaterial = m_settings.m_boidsSettings.boids[modelIndex].material;
        renderer.sharedMaterial.SetBuffer("clipsData", runtimeData.clipsDataBuffer);
        renderer.sharedMaterial.SetBuffer("geometry", runtimeData.geometryBuffer);
        renderer.sharedMaterial.SetFloat("_VertexCount", runtimeData.vertexCount);
        renderer.sharedMaterial.SetFloat("_FPS", m_settings.m_boidsSettings.bakeFPS);

        return runtimeData;
    }

    public int GetClipIndex(int boidInt, string clip)
    {
        return m_gameObjectClipIndex[new GameClip { boidIndex = boidInt, clip = clip }];
    }
}
