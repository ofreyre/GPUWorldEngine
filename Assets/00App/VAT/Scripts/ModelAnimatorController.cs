using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ModelAnimatorController : ScriptableObject
{
    [Serializable]
    public class RuntimeMeshAnimationData
    {
        public int[] bonesWeightsPerVertexStart;
        public BoneWeight1[] bonesWeightsPerVertex;
        public Renderer renderer;
        public Material editorMaterials;
        public Material rendererMaterials;
    }

    [Serializable]
    public class ClipData
    {
        public float length;
        public int fps;
    }

    public RuntimeMeshAnimationData[] animationData;
    public ClipData[] clipsData;
    public int bonesCount;
    public int[] clipsFramesStart;
    public Matrix4x4[] boneKey;
    public GameObject prefab;

    public bool initialized;
    ComputeBuffer[] bonesWeightsPerVertexStartBuffer;
    ComputeBuffer[] bonesWeightsPerVertexBuffer;
    ComputeBuffer boneKeyBuffer;



    public void InitMaterials()
    {
        if (initialized)
            return;

        initialized = true;

        bonesWeightsPerVertexStartBuffer = new ComputeBuffer[animationData.Length];
        bonesWeightsPerVertexBuffer = new ComputeBuffer[animationData.Length];

        boneKeyBuffer = new ComputeBuffer(boneKey.Length, sizeof(float) * 16);
        boneKeyBuffer.SetData(boneKey);

        for (int i = 0; i < animationData.Length; i++)
        {
            animationData[i].rendererMaterials.SetInt("_BonesCount", bonesCount);
            animationData[i].rendererMaterials.SetBuffer("boneKey", boneKeyBuffer);

            //block.SetInt("_VertexCount", animationData[i].bonesWeightsPerVertexStart.Length);

            bonesWeightsPerVertexStartBuffer[i] = new ComputeBuffer(animationData[i].bonesWeightsPerVertexStart.Length, sizeof(int));
            bonesWeightsPerVertexStartBuffer[i].SetData(animationData[i].bonesWeightsPerVertexStart);
            animationData[i].rendererMaterials.SetBuffer("bonesWeightsPerVertexStart", bonesWeightsPerVertexStartBuffer[i]);

            bonesWeightsPerVertexBuffer[i] = new ComputeBuffer(animationData[i].bonesWeightsPerVertex.Length, sizeof(int) + sizeof(float));
            bonesWeightsPerVertexBuffer[i].SetData(animationData[i].bonesWeightsPerVertex);
            animationData[i].rendererMaterials.SetBuffer("bonesWeightsPerVertex", bonesWeightsPerVertexBuffer[i]);

            animationData[i].rendererMaterials.SetFloat("_Speed", 1);
            //renderers[i].SetPropertyBlock(block);
        }
    }

    public void SetClip(int clip, List<MeshRenderer> renderers)
    {
        for (int i = 0; i < animationData.Length; i++)
        {
            /*
            animationData[i].rendererMaterials.SetFloat("_ClipFrameLength", GetFrameDuration(clip));
            animationData[i].rendererMaterials.SetInt("_ClipStart", GetClipStart(clip));
            animationData[i].rendererMaterials.SetInt("_ClipFramesCount", GetClipFramesCount(clip));
            animationData[i].rendererMaterials.SetFloat("_ClipFrameLength", GetFrameDuration(clip));
            */

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetFloat("_ClipFrameLength", GetFrameDuration(clip));
            block.SetInt("_ClipStart", GetClipStart(clip));
            block.SetInt("_ClipFramesCount", GetClipFramesCount(clip));
            block.SetFloat("_ClipFrameLength", GetFrameDuration(clip));
            block.SetFloat("_StartTime", UnityEngine.Random.Range(0.0f, 0.5f));
            renderers[i].SetPropertyBlock(block);
        }
    }

    public int GetClipStart(int clip)
    {
        return clipsFramesStart[clip];
    }

    public int GetClipFramesCount(int clip)
    {
        return Mathf.CeilToInt(clipsData[clip].length * clipsData[clip].fps);
    }

    public float GetFrameDuration(int clip)
    {
        return 1.0f / clipsData[clip].fps;
    }

    public void ReleaseBuffers()
    {
        if (bonesWeightsPerVertexStartBuffer != null)
        {
            initialized = false;
            for (int i = 0; i < animationData.Length; i++)
            {
                bonesWeightsPerVertexStartBuffer[i].Release();
                bonesWeightsPerVertexBuffer[i].Release();
            }
            boneKeyBuffer.Release();
        }
    }
}
