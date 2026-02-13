using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Texture3DBaker
{
    public class MixedComputer3DInit : MixedComputeInit , IRenderComputeInit
    {
        public override void Run(Computer computer)
        {
            base.Run(computer);

            Baker baker = (Baker)computer;
            int res = baker.m_editorSettings.SelectedTextureSize;
            if (baker.m_textureSize != res)
            {
                baker.m_textureSize = res;
                baker.compArg = UtilsComputeShader.GetThreadGroups(
                    computer.m_compute,
                    computer.m_kernel,
                    new Vector3Int(res, res, res),
                    baker.compArg
                );
            }

            if (baker.m_currentResultBuffer != null)
            {
                baker.m_currentResultBuffer.Release();
            }
            baker.m_currentResultBuffer = new ComputeBuffer(res * res * res, sizeof(float) * 4);
        }
    }
}
