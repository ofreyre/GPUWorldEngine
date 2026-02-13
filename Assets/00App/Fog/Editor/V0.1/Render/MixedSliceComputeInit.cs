using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Texture3DBaker
{
    public class MixedSliceComputeInit : MixedComputeInit, IRenderComputeInit
    {
        public override void Run(Computer computer)
        {
            base.Run(computer);
            RendererMixed r = (RendererMixed)computer;
            computer.m_compute.SetFloat("y", computer.m_currentDetail.y);

            int textureSize = r.m_currentDetail.TextureSize;
            r.m_resolutionIndex = UtilsMath.UpperPow2(textureSize);
            computer.compArg = r.m_computeArgs[r.m_resolutionIndex];

            r.m_currentResultBuffer = r.m_noiseResults[r.m_resolutionIndex];
        }
    }
}
