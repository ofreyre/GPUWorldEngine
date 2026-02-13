using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ComputeNoise3DComputer
{

    public RenderTexture texture;
    RenderTextureDescriptor textureDescriptor;
    int[] computeArgs;
    ComputeBuffer bufferSettings;

    RenderTexture GetRenderTexture(int textureSize)
    {
        if (texture == null)
        {
            InitRenderTextureDescriptor(textureSize);
            CreateRenderTexture();
        }
        else if(!CompareTextureSize(textureSize))
        {
            SetRenderTextureDescriptorSize(textureSize);
            CreateRenderTexture();
        }
        return texture;
    }

    void InitRenderTextureDescriptor(int textureSize)
    {
        textureDescriptor = new RenderTextureDescriptor();
        textureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
        textureDescriptor.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        textureDescriptor.enableRandomWrite = true;
        textureDescriptor.msaaSamples = 1; //Texture3D does not support antialiasing
        textureDescriptor.depthBufferBits = 0; //Texture3D does not support depth
        textureDescriptor.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
        textureDescriptor.width = textureSize;
        textureDescriptor.height = textureSize;
        textureDescriptor.volumeDepth = textureSize;
        textureDescriptor.autoGenerateMips = false;
        textureDescriptor.enableRandomWrite = true;
        textureDescriptor.mipCount = 0;
        texture = new RenderTexture(textureDescriptor);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.wrapModeU = TextureWrapMode.Repeat;
        texture.wrapModeV = TextureWrapMode.Repeat;
        texture.wrapModeW = TextureWrapMode.Repeat;
        texture.useMipMap = false;
        texture.filterMode = FilterMode.Point;
        texture.antiAliasing = 1; //Texture3D does not support antialiasing
        //texture.enableRandomWrite = true;
        texture.Create();
        texture.wrapMode = TextureWrapMode.Repeat;
    }

    void CreateRenderTexture()
    {
        if (texture != null)
            texture.Release();

        texture = new RenderTexture(textureDescriptor);
    }

    bool CompareTextureSize(int textureSize)
    {
        return textureDescriptor.width == textureSize &&
            textureDescriptor.height == textureSize &&
            textureDescriptor.volumeDepth == textureSize;
    }
    void SetRenderTextureDescriptorSize(int textureSize)
    {
        textureDescriptor.width = textureSize;
        textureDescriptor.height = textureSize;
        textureDescriptor.volumeDepth = textureSize;
    }

    public void Compute(ComputeShader computeShader, string computeKernel, int textureSize,float perlinWeight, NoiseSettings[] noiseSettings, Action readyHandler)
    {
        //Debug.Log(string.Join(",", noiseSettings[0]));
        int kernelHandle = computeShader.FindKernel(computeKernel);

        computeShader.SetFloat("perlinWeight", perlinWeight);

        computeShader.SetTexture(kernelHandle, "noise", GetRenderTexture(textureSize));

        bufferSettings = new ComputeBuffer(noiseSettings.Length, sizeof(float) * 5 + sizeof(int));
        bufferSettings.SetData(noiseSettings);
        computeShader.SetBuffer(kernelHandle, "settings", bufferSettings);

        computeArgs = UtilsComputeShader.GetThreadGroups(
            computeShader,
            computeKernel,
            new Vector3Int(textureSize, textureSize, textureSize),
            computeArgs
        );

        //Debug.Log(string.Join(",", computeArgs));
        computeShader.Dispatch(kernelHandle, computeArgs[0], computeArgs[1], computeArgs[2]);

        bufferSettings.Release();
        readyHandler.Invoke();
    }

    public void Release()
    {
        if (texture != null)
            texture.Release();
    }
}
