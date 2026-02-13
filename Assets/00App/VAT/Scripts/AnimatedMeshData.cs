using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu()]
public class AnimatedMeshData : ScriptableObject
{
    [Serializable]
    public struct WeightData
    {
        public int vertexIndex;
        public float weight;
    }

    [Serializable]
    public struct MeshAnimationData
    {
        //Base mesh vertex
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Vector2[] uv;
        public int[] triangles;

        //Vertices bounds bindings
        public Matrix4x4[] bindPoses;
        public int[] bonesWeightsPerVertexStart;
        public BoneWeight1[] bonesWeightsPerVertex;

        //Animation data
        public int boneKeysSize;
        public int[] boneKeysStart;
        public float[] boneKeysTimes;
        public KeyCurve[] boneKeycurves;

        public override string ToString()
        {
            string d = "boneKeysSize = " + boneKeysSize + "\n";

            if (bonesWeightsPerVertexStart == null)
                d += "bonesWeightsPerVertexStart = null\n";
            else
                d += "bonesWeightsPerVertexStart.Length = " + bonesWeightsPerVertexStart.Length + "\n";

            if (bonesWeightsPerVertex == null)
                d += "bonesWeightsPerVertex = null\n";
            else
                d += "bonesWeightsPerVertex.Length = " + bonesWeightsPerVertex.Length + "\n";

            if (boneKeysStart == null)
                d += "boneKeysStart = null\n";
            else
                d += "boneKeysStart.Length = " + boneKeysStart.Length + "\n";

            if (boneKeysTimes == null)
                d += "boneKeysTimes = null\n";
            else
                d += "boneKeysTimes.Length = " + boneKeysTimes.Length + "\n";

            if (boneKeycurves == null)
                d += "boneKeycurves = null\n";
            else
                d += "boneKeycurves.Length = " + boneKeycurves.Length;

            return d;
        }
    }

    [Serializable]
    public struct KeyCurve
    {
        Vector3 inTangent;
        Vector3 inWeight;
        Vector3 outTangent;
        Vector3 outWeight;
        Vector3 translation;
        Vector3 rotation;
        Vector3 scale;
        WeightedMode weightedMode;

        public void Set(string property, Keyframe keyframe)
        {
            string prop = property.ToLower();
            if (prop.Contains("position"))
            {
                inTangent.x = keyframe.inTangent;
                inWeight.x = keyframe.inWeight;
                outTangent.x = keyframe.inTangent;
                outWeight.x = keyframe.inWeight;
                if (prop.Contains('x'))
                    translation.x = keyframe.value;
                else if(prop.Contains('y'))
                    translation.y = keyframe.value;
                else
                    translation.z = keyframe.value;
            }
            else if(prop.Contains("rotation"))
            {
                inTangent.y = keyframe.inTangent;
                inWeight.y = keyframe.inWeight;
                outTangent.y = keyframe.inTangent;
                outWeight.y = keyframe.inWeight;
                if (prop.Contains('x'))
                    rotation.x = keyframe.value;
                else if (prop.Contains('y'))
                    rotation.y = keyframe.value;
                else
                    rotation.z = keyframe.value;
            }
            else
            {
                inTangent.z = keyframe.inTangent;
                inWeight.z = keyframe.inWeight;
                outTangent.y = keyframe.inTangent;
                outWeight.y = keyframe.inWeight;
                if (prop.Contains('x'))
                    scale.x = keyframe.value;
                else if (prop.Contains('y'))
                    scale.y = keyframe.value;
                else
                    scale.z = keyframe.value;
            }

        }
    }

    public GameObject gameObject;
    public List<AnimationClip> clips = new List<AnimationClip>();
    public MeshAnimationData[] animatedMeshes;

    public string GetAnimatedMeshesInfo()
    {
        string d = "animatedMeshes.Length = "+ animatedMeshes.Length+"\n";
        for(int i=0; i < animatedMeshes.Length; i++)
        {
            d += "-----" + i + ":\n";
            d += animatedMeshes[i].ToString();
        }
        return d;
    }
}
