using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;
using DBG.Utils.IO;

[Serializable]
public struct AnimData
{
    public SVector3 p, n;
}

[Serializable]
public struct ClipData
{
    public string clip;
    public int frames;
}

[Serializable]
public struct MeshClipsData
{
    public string name;
    public int level;
    public int vertexCount;
    public ClipData[] clips;
    public AnimData[] geometry;
}

[Serializable]
public struct ObjectClipsData
{
    public MeshClipsData[] data;
}

[Serializable]
public struct SVector3
{
    public float x, y, z;

    public SVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
}


[Serializable]
public struct SVector2
{
    public float x, y;

    public SVector2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }
}

[Serializable]
public struct ClipInfo
{
    public string clip;
    public int startIndex;
    public int frames;
}

[Serializable]
public struct FullMeshClipsData
{
    public int vertexCount;
    public ClipInfo[] clips;
    public SVector2[] uvs;
    public int[] triangles;
    public AnimData[] geometry;

    public Vector2[] UnityUVs
    {
        get
        {
            Debug.Log(uvs);
            Vector2[] uuv = new Vector2[uvs.Length];
            for(int i=0;i< uuv.Length;i++)
            {
                SVector2 uv = uvs[i];
                uuv[i] = new Vector2(uv.x, uv.y);
            }
            return uuv;
        }
    }
}

public class AnimationsBaker:MonoBehaviour
{
    [SerializeField] TerrainSettings m_settings;

    async void Start()
    {
        //MeshClipsData[][] tasks = new MeshClipsData[m_gameObjects.Length][];
        //StartCoroutine(BakeToFile(m_gameObjects, m_fps));

        /*
        for (int i = 0; i < m_gameObjects.Length; i++)
        {
            var task = await BakeToFile(m_gameObjects[i], m_fps);
            Save(task, m_savePath + "/" + m_gameObjects[i].name + ".ani");
        }
        */

        var boids = m_settings.m_boidsSettings.boids;
        string path = Application.streamingAssetsPath + "/" + m_settings.m_boidsSettings.bakeSavePath + "/";
        Debug.Log(path);
        //return;

        for (int i = 0; i < boids.Length; i++)
        {
            GameObject gobj = Instantiate(boids[i].prefab);
            gobj.SetActive(true);
            var task = await BakeConsolidatedMeshToFile(gobj, m_settings.m_boidsSettings.bakeFPS);
            SaveConsolidatedMesh(task, path + boids[i].prefab.name + ".bytes");
            Destroy(gobj);
        }

        Debug.Log("ddddddddddddddddd");
    }

    void Save(MeshClipsData[] meshClipsData, string path)
    {
        ThreadStart threadStart = delegate {
            ObjectClipsData data = new ObjectClipsData { data = meshClipsData };
            UtilsIO.SaveAbsolute(data, path);
        };
        new Thread(threadStart).Start();
    }

    async Task<MeshClipsData[]> BakeToFile(GameObject gobj, float fps)
    {

        List<ComponentSearchData<SkinnedMeshRenderer>> renderers = UtilsGameObject.GetComponents<SkinnedMeshRenderer>(gobj);
        Animator animator = gobj.GetComponent<Animator>();
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        List<int> vertexCounts = new List<int>();
        vertexCounts.AddRange(Enumerable.Range(0, renderers.Count).Select(i => renderers[i].component.sharedMesh.vertexCount));

        Mesh mesh = new Mesh();
        MeshClipsData[] meshesClipsData = new MeshClipsData[renderers.Count];

        for (int q = 0; q < renderers.Count; q++)
        {
            ComponentSearchData<SkinnedMeshRenderer> renderer = renderers[q];
            MeshClipsData meshClipData = new MeshClipsData
            {
                name = renderer.transform.name,
                level = renderer.level,
                vertexCount = renderer.component.sharedMesh.vertexCount,
                clips = new ClipData[clips.Length],
            };
            meshesClipsData[q] = meshClipData;
            List<AnimData> geometry = new List<AnimData>();
            //Transform ts = renderer.transform;
            Debug.Log(meshClipData.vertexCount);
            for (int i=0;i<clips.Length;i++)
            {
                AnimationClip clip = clips[i];
                ClipData clipData = new ClipData
                {
                    clip = clip.name,
                    //frames = (int)Mathf.NextPowerOfTwo((int)(clip.length * 0))
                    frames = (int)(clip.length * fps)
                };
                meshesClipsData[q].clips[i] = clipData;
                Debug.Log(i + " " + ((int)(clip.length * fps) * meshClipData.vertexCount * 2) + " "+ clipData.frames);

                animator.Play(clip.name);
                for (int j = 0; j < clipData.frames; j++)
                {
                    await Task.Yield();
                    animator.Play(clip.name, 0, (float)j / clipData.frames);
                    renderer.component.BakeMesh(mesh);
                    Debug.Log(i+" "+j);
                    geometry.AddRange(Enumerable.Range(0, meshClipData.vertexCount).Select(m => new AnimData
                    {
                        p = new SVector3(mesh.vertices[m]),
                        n = new SVector3(mesh.normals[m])
                        //p = new SVector3(ts.TransformPoint(mesh.vertices[m])),
                        //n = new SVector3(ts.TransformPoint(mesh.normals[m]))
                    }));
                    //yield return null;
                    mesh.Clear();
                }
            }
            meshesClipsData[q].geometry = geometry.ToArray();
        }
        Debug.Log("End "+ meshesClipsData[0].geometry.Length);
        return meshesClipsData;
    }

    void SaveConsolidatedMesh(FullMeshClipsData meshClipsData, string path)
    {
        Debug.Log("Save " + path);
        ThreadStart threadStart = delegate {
            UtilsIO.SaveAbsolute(meshClipsData, path);
        };
        new Thread(threadStart).Start();
    }

    async Task<FullMeshClipsData> BakeConsolidatedMeshToFile(GameObject gobj, float fps)
    {

        List<ComponentSearchData<SkinnedMeshRenderer>> renderers = UtilsGameObject.GetComponents<SkinnedMeshRenderer>(gobj);
        Animator animator = gobj.GetComponent<Animator>();
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        List<int> vertexCounts = new List<int>();
        vertexCounts.AddRange(Enumerable.Range(0, renderers.Count).Select(i => renderers[i].component.sharedMesh.vertexCount));

        Mesh mesh = new Mesh();

        int vertexCount = 0;
        int trianglesCount = 0;

        for (int i=0;i< renderers.Count;i++)
        {
            vertexCount += renderers[i].component.sharedMesh.vertexCount;
            trianglesCount += renderers[i].component.sharedMesh.triangles.Length;
        }

        SVector2[] uvs = new SVector2[vertexCount];
        int[] triangles = new int[trianglesCount];

        int n = 0;
        int m = 0;
        for (int i = 0; i < renderers.Count; i++)
        {
            int j1 = renderers[i].component.sharedMesh.vertexCount;
            //Vector2[] uv = renderers[i].component.sharedMesh.uv;
            for (int j = 0; j < j1; j++)
            {
                uvs[n] = new SVector2(renderers[i].component.sharedMesh.uv[j]);
                n++;
            }

            j1 = renderers[i].component.sharedMesh.triangles.Length;
            for (int j = 0; j < j1; j++)
            {
                triangles[m] = renderers[i].component.sharedMesh.triangles[j];
                m++;
            }
        }


        FullMeshClipsData meshClipsData = new FullMeshClipsData {
            vertexCount = vertexCount,
            clips = new ClipInfo[clips.Length],
            uvs = uvs,
            triangles = triangles
        };


        List<AnimData> geometry = new List<AnimData>();

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            Debug.Log(gobj.name + " " + clip.name);

            ClipInfo clipData = new ClipInfo
            {
                clip = clip.name,
                startIndex = geometry.Count,
                frames = (int)(clip.length * fps)
            };
            meshClipsData.clips[i] = clipData;

            for (int q = 0; q < renderers.Count; q++)
            {
                ComponentSearchData<SkinnedMeshRenderer> renderer = renderers[q];
                Transform ts = renderer.transform;
                vertexCount = renderers[q].component.sharedMesh.vertexCount;
                animator.Play(clip.name);
                for (int j = 0; j < clipData.frames; j++)
                {
                    await Task.Yield();
                    animator.Play(clip.name, 0, (float)j / clipData.frames);
                    renderer.component.BakeMesh(mesh);
                    geometry.AddRange(Enumerable.Range(0, vertexCount).Select(m => new AnimData
                    {
                        p = new SVector3(ts.TransformPoint(mesh.vertices[m])),
                        n = new SVector3(ts.TransformPoint(mesh.normals[m]))
                    }));
                    //yield return null;
                    mesh.Clear();
                }
            }
        }
        meshClipsData.geometry = geometry.ToArray();
        Debug.Log("End geom: " + meshClipsData.geometry.Length);
        return meshClipsData;
    }
}
