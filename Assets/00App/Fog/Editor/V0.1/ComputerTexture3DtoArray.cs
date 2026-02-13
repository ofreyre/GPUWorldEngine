using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class ComputerTexture3DtoArray
{
    int[] computeArgs;
    public Color[] array;
    ComputeBuffer arrayBuffer;
    Action readyHandler;

    public void Compute(ComputeShader computeShader, string computeKernel, RenderTexture texture, Action readyHandler)
    {
        this.readyHandler = readyHandler;
        int kernelHandle = computeShader.FindKernel(computeKernel);

        computeShader.SetTexture(kernelHandle, "sourceTexture", texture);
        
        arrayBuffer = new ComputeBuffer(texture.width * texture.height * texture.volumeDepth, sizeof(float) * 4);
        computeShader.SetBuffer(kernelHandle, "array", arrayBuffer);

        computeArgs = UtilsComputeShader.GetThreadGroups(
            computeShader,
            computeKernel,
            new Vector3Int(texture.width, texture.height, texture.volumeDepth),
            computeArgs
        );
        computeShader.Dispatch(kernelHandle, computeArgs[0], computeArgs[1], computeArgs[2]);
        AsyncGPUReadback.Request(arrayBuffer, ArrayBufferCallback);

        arrayBuffer.Release();
    }

    void ArrayBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            array = readbackRequest.GetData<Color>().ToArray();
            arrayBuffer.Release();
            readyHandler.Invoke();
        }
    }
}
