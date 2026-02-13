using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;
using System;


namespace AnimatedModel
{
    public static class ModelBaker
    {
        public class ModelBakeData
        {
            public MeshAnimationData[] animatedMeshes;
            public int bonesCount;
            public int[] clipsFramesStart;
            public Matrix4x4[] boneKey;
            public List<SkinnedMeshData> renderers;
        }

        static bool m_ready;
        static int m_totalProgress;
        static int m_progress;
        static string m_info;
        static bool m_cancel;

        static int GetTotalProgress(GameObject gameObject, List<AnimatedClipData> clipsData)
        {
            List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
            SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                renderers.Add(renderer);
            }
            renderers.AddRange(gameObject.GetComponentsInChildren<SkinnedMeshRenderer>());

            int bakeProgress = renderers.Count * 2;
            List<Transform> bones = new List<Transform>();

            for (int i = 0; i < renderers.Count; i++)
            {
                Transform[] _bones = renderers[i].bones;
                for (int j = 0; j < _bones.Length; j++)
                {
                    if (!bones.Contains(_bones[j]))
                    {
                        bones.Add(_bones[j]);
                    }
                }
            }
            bakeProgress += bones.Count;

            for (int i=0;i<clipsData.Count;i++)
            {
                AnimatedClipData clipData = clipsData[i];
                bakeProgress += Mathf.CeilToInt(clipData.clip.length * clipData.fps) * bones.Count;
            }

            return bakeProgress;
        }

        public static async Task<ModelBakeData> Bake(GameObject model, List<AnimatedClipData> clipsData, Action yield)
        {
            m_ready = false;
            ModelBakeData bakeData = new ModelBakeData();


            GameObject gameObject = GameObject.Instantiate(model, Vector3.zero, Quaternion.identity);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            gameObject.SetActive(false);

            m_progress = 0;
            m_totalProgress = GetTotalProgress(gameObject, clipsData);

            List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();

            SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                renderers.Add(renderer);
            }
            renderers.AddRange(gameObject.GetComponentsInChildren<SkinnedMeshRenderer>());

            List<Transform> bones = new List<Transform>();
            List<Matrix4x4> bindPoses = new List<Matrix4x4>();
            int[][] bonesMap = new int[renderers.Count][];

            for (int i=0;i< renderers.Count;i++)
            {
                m_progress++;
                m_info = "Remaping mesh bones: " + renderers[i].name + ". Mesh: " + i + " / " + renderers.Count;
                yield();
                if (m_cancel)
                    return bakeData;

                Transform[] _bones = renderers[i].bones;
                bonesMap[i] = new int[_bones.Length];
                for (int j=0;j<_bones.Length;j++)
                {
                    int k = bones.IndexOf(_bones[j]);
                    if (k == -1)
                    {
                        bonesMap[i][j] = bones.Count;
                        bones.Add(_bones[j]);
                        bindPoses.Add(renderers[i].sharedMesh.bindposes[j]);
                    }
                    else
                    {
                        bonesMap[i][j] = k;
                    }
                }
            }

            int bonesCount = bones.Count;
            int clipsCount = clipsData.Count;

            List<Matrix4x4> boneKey = new List<Matrix4x4>();
            int[] clipsFramesStart = new int[clipsCount];

            AnimationMode.StartAnimationMode();
            for (int i = 0; i < clipsCount; i++)
            {
                var clipData = clipsData[i];
                float frameTime = 1.0f / clipData.fps;
                clipsFramesStart[i] = boneKey.Count;
                int frame = 1;
                int totalFrames = Mathf.CeilToInt(clipData.clip.length * clipData.fps);
                for (float time = 0; time < clipData.clip.length; time += frameTime)
                {
                    var oldWrapMode = clipData.clip.wrapMode;
                    clipData.clip.wrapMode = WrapMode.Clamp;
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(gameObject, clipData.clip, time);
                    AnimationMode.EndSampling();
                    clipData.clip.wrapMode = oldWrapMode;
                    for (int j = 0; j < bonesCount; j++)
                    {
                        boneKey.Add(bones[j].localToWorldMatrix * bindPoses[j]);
                        //boneKey.Add(bones[j].localToWorldMatrix);
                        await Task.Yield();
                        m_progress++;
                        m_info = "Beking clips: " + clipData.clip.name + ". Frame: " + frame + " / " + totalFrames + ". Bone: " + bones[j].name;
                        yield();
                        if (m_cancel)
                            return bakeData;
                    }
                    frame++;
                }
            }
            AnimationMode.StopAnimationMode();

            MeshAnimationData[] animatedMeshes = new MeshAnimationData[renderers.Count];

            for (int k = 0; k < renderers.Count; k++)
            {
                renderer = renderers[k];
                int[] renderernBonesMap = bonesMap[k];

                var bonesPerVertex = renderer.sharedMesh.GetBonesPerVertex();
                var boneWeights = renderer.sharedMesh.GetAllBoneWeights();
                int[] vertexBonesStart = new int[bonesPerVertex.Length];
                int start = 0;
                for (int i = 0; i < bonesPerVertex.Length; i++)
                {
                    vertexBonesStart[i] = start;
                    start += bonesPerVertex[i];
                }

                BoneWeight1[] boneWeightsMapped = new BoneWeight1[boneWeights.Length];
                for (int i = 0; i < boneWeights.Length; i++)
                {
                    boneWeightsMapped[i] = new BoneWeight1 {
                        weight = boneWeights[i].weight,
                        boneIndex = renderernBonesMap[boneWeights[i].boneIndex]
                    };
                }

                animatedMeshes[k] = new MeshAnimationData
                {
                    vertexCount = renderer.sharedMesh.vertexCount,
                    vertices = renderer.sharedMesh.vertices,
                    normals = renderer.sharedMesh.normals,
                    tangents = renderer.sharedMesh.tangents,
                    uv = renderer.sharedMesh.uv,
                    triangles = renderer.sharedMesh.triangles,

                    bonesWeightsPerVertexStart = vertexBonesStart,
                    bonesWeightsPerVertex = boneWeightsMapped,
                };

                await Task.Yield();
                m_progress++;
                m_info = "Remaping meshes: " + renderer.name + " "+k+"/"+ renderers.Count;
                yield();
                if (m_cancel)
                    return bakeData;
            }

            bakeData.animatedMeshes = animatedMeshes;
            bakeData.bonesCount = bonesCount;
            bakeData.clipsFramesStart = clipsFramesStart;
            bakeData.boneKey = boneKey.ToArray();

            return bakeData;
        }

        public static bool ready { get { return m_ready; } }

        public static float Progress { get { return ((float)m_progress) / m_totalProgress; } }

        public static bool cancel
        {
            get { return m_cancel; }
        }

        public static void Cancel()
        {
            m_cancel = true;
        }

        public static string info { get { return m_info; } }
    }
}
