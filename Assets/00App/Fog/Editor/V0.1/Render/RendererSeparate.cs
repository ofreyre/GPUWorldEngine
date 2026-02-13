using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Texture3DBaker
{
    public class RendererSeparate : Renderer
    {
        public ComputeBuffer m_bufferNoiseSettings;
        public ComputeBuffer m_perlinWeights;

        public RendererSeparate(TextureBakerSettings editorSettings, ComputeShader compute, string kernel) :base(editorSettings, compute, kernel)
        {
            m_bufferNoiseSettings = new ComputeBuffer(8, sizeof(float) * 5 + sizeof(int));
            m_perlinWeights = new ComputeBuffer(4, sizeof(float));
            m_computeInit = new SeparateComputeInit();
        }

        public override void Release()
        {

            if (m_bufferNoiseSettings != null)
                m_bufferNoiseSettings.Release();

            if (m_perlinWeights != null)
            {
                m_perlinWeights.Release();
            }
        }
    }
}
