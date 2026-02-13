using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;


namespace Texture3DBaker
{
    public class TextureRenderer : Computer, ITextureComputer
    {
        public Texture m_currentTexture;
        Action<Texture> m_readyHandler;

        public TextureRenderer(TextureBakerSettings editorSettings, ComputeShader compute, string kernel) : base(editorSettings, compute, kernel)
        {
        }

        public virtual void Run(Action<Texture> readyHandler)
        {
            m_readyHandler = readyHandler;
            base.Run();
        }

        protected virtual void UpdateCurrentTexture()
        {
        }

        protected override void ReadyAction(AsyncGPUReadbackRequest readbackRequest)
        {
            UpdateCurrentTexture();
            base.ReadyAction(readbackRequest);
            m_readyHandler.Invoke(m_currentTexture);
        }

        public Texture texture { get { return m_currentTexture; } }
    }
}
