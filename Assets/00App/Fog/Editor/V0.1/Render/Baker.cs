using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;


namespace Texture3DBaker
{
    public class Baker : TextureRenderer
    {
        public int m_textureSize = -1;
        int m_prevTextureSize = -1;

        public Baker(TextureBakerSettings editorSettings, ComputeShader compute, string kernel) :base(editorSettings, compute, kernel)
        {
            m_computeInit = new MixedComputer3DInit();
        }

        public override void Run(Action<Texture> readyHandler)
        {
            base.Run(readyHandler);
        }

        protected override void UpdateCurrentTexture()
        {
            if (m_prevTextureSize != m_textureSize)
            {
                m_currentTexture = new Texture3D(m_textureSize, m_textureSize, m_textureSize, TextureFormat.RGBA32, false);
            }
            m_prevTextureSize = m_textureSize;
            Texture3D tex = ((Texture3D)m_currentTexture);
            tex.SetPixels(m_result);
            tex.Apply();
        }
    }
}