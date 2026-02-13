using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilsComputeShader
{
    public static int[] GetThreadGroups(ComputeShader shader, string kernel, Vector3Int dataLength)
    {
        int kernelHandle = shader.FindKernel(kernel);
        return GetThreadGroups(shader, kernelHandle, dataLength);
    }

    public static int[] GetThreadGroups(ComputeShader shader, int kernelHandle, Vector3Int dataLength)
    {
        shader.GetKernelThreadGroupSizes(kernelHandle, out uint threadGroupSize1, out uint threadGroupSize2, out uint threadGroupSize3);
        return new int[] {
               Mathf.CeilToInt( Mathf.Max(dataLength.x, 1.0f) / threadGroupSize1),
               Mathf.CeilToInt( Mathf.Max(dataLength.y, 1.0f) / threadGroupSize2),
               Mathf.CeilToInt( Mathf.Max(dataLength.z, 1.0f) / threadGroupSize3)
        };
    }

    public static int[] GetThreadGroups(ComputeShader shader, string kernel, Vector3Int dataLength, int[] data)
    {
        if(data == null)
        {
            data = GetThreadGroups(shader, kernel, dataLength);
        }
        else
        {
            int kernelHandle = shader.FindKernel(kernel);
            data = GetThreadGroups(shader, kernelHandle, dataLength, data);
        }
        return data;
    }

    public static int[] GetThreadGroups(ComputeShader shader, int kernelHandle, Vector3Int dataLength, int[] data)
    {
        if (data == null)
        {
            data = GetThreadGroups(shader, kernelHandle, dataLength);
        }
        else
        {
            shader.GetKernelThreadGroupSizes(kernelHandle, out uint threadGroupSize1, out uint threadGroupSize2, out uint threadGroupSize3);
            data[0] = Mathf.CeilToInt(Mathf.Max(dataLength.x, 1.0f) / threadGroupSize1);
            data[1] = Mathf.CeilToInt(Mathf.Max(dataLength.y, 1.0f) / threadGroupSize2);
            data[2] = Mathf.CeilToInt(Mathf.Max(dataLength.z, 1.0f) / threadGroupSize3);
        }
        return data;
    }

    public static ComputeBuffer GetArgsComputeBuffer()
    {
        return new ComputeBuffer(3, sizeof(int) * 3);
    }

    public static ComputeBuffer GetArgsComputeBuffer(int[] args)
    {
        ComputeBuffer argsBuffer = GetArgsComputeBuffer();
        argsBuffer.SetData(args);
        return argsBuffer;
    }

    public static ComputeBuffer GetArgsComputeBuffer(ComputeShader shader, string kernel, Vector3Int dataLength)
    {
        return GetArgsComputeBuffer(GetThreadGroups(shader, kernel, dataLength));
    }

    public static ComputeBuffer GetArgsComputeBuffer(ComputeShader shader, int kernelHandle, Vector3Int dataLength)
    {
        return GetArgsComputeBuffer(GetThreadGroups(shader, kernelHandle, dataLength));
    }

}
