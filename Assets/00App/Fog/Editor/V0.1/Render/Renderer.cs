using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Texture3DBaker
{
    public class Renderer: TextureRenderer
    {
        public int[][] m_computeArgs;
        protected Texture2D[] m_textures;
        public ComputeBuffer[] m_noiseResults;
        public int m_resolutionIndex;

        public Renderer(TextureBakerSettings editorSettings, ComputeShader shader, string kernel) :base(editorSettings, shader, kernel)
        {
            //Init();
        }

        protected override void Init()
        {
            int n = UtilsMath.UpperPow2(1024);
            m_textures = new Texture2D[n];
            m_computeArgs = new int[n][];
            m_noiseResults = new ComputeBuffer[n];
            for (int i = 1; i < n; i++)
            {
                int res = (int)Mathf.Pow(2, i);
                m_textures[i] = new Texture2D(res, res, TextureFormat.ARGB32, false);
                m_noiseResults[i] = new ComputeBuffer(res * res, sizeof(float) * 4);
                m_computeArgs[i] = UtilsComputeShader.GetThreadGroups(
                    m_editorSettings.computeRender,
                    m_editorSettings.computeRenderKernel,
                    new Vector3Int(res, 1, res),
                    m_computeArgs[i]
                );
            }
        }

        protected override void UpdateCurrentTexture()
        {
            m_currentTexture = m_textures[m_resolutionIndex];
            Texture2D tex = ((Texture2D)m_currentTexture);
            tex.SetPixels(m_result);
            tex.Apply();
        }

        public override void Release()
        {
            int n = UtilsMath.UpperPow2(1024);
            for (int i = 3; i < n; i++)
            {
                if (m_noiseResults[i] != null)
                    m_noiseResults[i].Release();
            }
        }
    }
}
