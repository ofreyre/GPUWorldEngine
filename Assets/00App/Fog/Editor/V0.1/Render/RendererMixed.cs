using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Texture3DBaker
{
    public class RendererMixed : Renderer
    {
        public RendererMixed(TextureBakerSettings editorSettings, ComputeShader compute, string kernel) : base(editorSettings, compute, kernel)
        {
            m_computeInit = new MixedSliceComputeInit();
        }
    }
}
