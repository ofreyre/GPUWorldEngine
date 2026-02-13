using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Texture3DBaker
{
    public interface IRenderComputeInit
    {
        void Run(Computer renderer);
    }
}
