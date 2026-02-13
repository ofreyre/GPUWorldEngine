using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Texture3DBaker
{
    public class SeparateComputeInit : IRenderComputeInit
    {
        public void Run(Computer computer)
        {
            RendererSeparate rs = (RendererSeparate)computer;
            computer.m_compute.SetFloat("y", computer.m_currentDetail.y);
            rs.m_bufferNoiseSettings.SetData(rs.m_currentDetail.NoiseSettings);
            rs.m_perlinWeights.SetData(rs.m_editorSettings.GetPerlinWeightsWithVisibility(rs.m_currentDetail));
            rs.m_editorSettings.computeRender.SetBuffer(rs.m_kernelHandle, "settings", rs.m_bufferNoiseSettings);
            rs.m_editorSettings.computeRender.SetBuffer(rs.m_kernelHandle, "perlinWeights", rs.m_perlinWeights);
        }
    }
}
