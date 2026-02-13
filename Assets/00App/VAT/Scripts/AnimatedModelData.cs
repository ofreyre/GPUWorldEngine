using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimatedModel
{
    [Serializable]
    public struct MeshAnimationData
    {
        //Base mesh vertex1
        public int vertexCount;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Vector2[] uv;
        public int[] triangles;

        //Vertices bounds bindings
        public int[] bonesWeightsPerVertexStart;
        public BoneWeight1[] bonesWeightsPerVertex;

        public Material material;

        public string BonesWeightsPerVertexToString()
        {
            string a = "";
            for(int i=0;i< bonesWeightsPerVertex.Length;i++)
            {
                a += bonesWeightsPerVertex[i].boneIndex + ", " + bonesWeightsPerVertex[i].weight+"  #  ";
            }
            return a;
        }
    }

    [Serializable]
    public class AnimatedClipData
    {
        public AnimationClip clip;
        public int fps;
    }

    [Serializable]
    public class SkinnedMeshData
    {
        public SkinnedMeshRenderer renderer;
        public Material material;
    }

    [CreateAssetMenu()]
    public class AnimatedModelData : ScriptableObject
    {
        public GameObject gameObject;
        public List<AnimatedClipData> clipsData = new List<AnimatedClipData>();


        //Baked data
        public MeshAnimationData[] animatedMeshes;
        public int bonesCount;
        public int[] clipsFramesStart;
        public Matrix4x4[] boneKey;
        public List<SkinnedMeshData> renderers;
        public bool baked;


        public string GetAnimatedMeshesInfo()
        {
            string d = "animatedMeshes.Length = " + animatedMeshes.Length + "\n";
            for (int i = 0; i < animatedMeshes.Length; i++)
            {
                d += "-----" + i + ":\n";
                d += animatedMeshes[i].ToString();
            }
            return d;
        }

        public GameObject GetGameObject()
        {
            GameObject prefab = new GameObject();
            prefab.name = gameObject.name;

            for (int i = 0; i < renderers.Count; i++)
            {
                //Debug.Log(renderers[i].renderer.gameObject.name);
                //GameObject child = Instantiate(renderers[i].renderer.gameObject);
                GameObject child = new GameObject();
                child.name = renderers[i].renderer.gameObject.name;
                child.transform.SetParent(prefab.transform);
                MeshRenderer renderer = child.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = child.AddComponent<MeshFilter>();
                Mesh mesh = new Mesh();
                mesh.vertices = animatedMeshes[i].vertices;
                mesh.normals = animatedMeshes[i].normals;
                mesh.tangents = animatedMeshes[i].tangents;
                mesh.uv = animatedMeshes[i].uv;
                mesh.triangles = animatedMeshes[i].triangles;
                mesh.RecalculateBounds();
                meshFilter.mesh = mesh;                    
                meshFilter.sharedMesh = mesh;
            }

            prefab.SetActive(true);
            return prefab;
        }
    }

}
