using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Texture3DBaker
{
    public interface ITextureComputer
    {
        void Run(Action<Texture> Texture);
        void Release();

        Texture texture { get;}
    }
}
