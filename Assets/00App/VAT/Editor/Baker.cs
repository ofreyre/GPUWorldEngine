using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace DBG.VATEditor
{
    [Serializable]
    public struct AnimData
    {
        public Vector3 p, n;
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
    public struct ClipData
    {
        public string clip;
        public int frames;
    }

    public class Baker
    {
        public async void Bake(GameObject gobj, float fps)
        {
            var meshClipData = await BakeToFile(gobj, fps);
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
                for (int i=0;i<clips.Length;i++)
                {
                    AnimationClip clip = clips[i];
                    ClipData clipData = new ClipData
                    {
                        clip = clip.name,
                        frames = (int)Mathf.NextPowerOfTwo((int)(clip.length / fps))
                    };
                    meshesClipsData[q].clips[i] = clipData;

                    animator.Play(clip.name);
                    for (int j = 0; j < clipData.frames; j++)
                    {
                        animator.Play(clip.name, 0, (float)j / clipData.frames);
                        renderer.component.BakeMesh(mesh);
                        geometry.AddRange(Enumerable.Range(0, meshClipData.vertexCount).Select(m => new AnimData
                        {
                            p = mesh.vertices[m],
                            n = mesh.normals[m]
                        }));
                        await Task.Yield();
                        mesh.Clear();
                    }
                }
                meshesClipsData[q].geometry = geometry.ToArray();
            }
            return meshesClipsData;
        }
    }
}
