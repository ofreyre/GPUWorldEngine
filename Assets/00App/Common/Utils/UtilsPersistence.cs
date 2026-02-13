using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DBG.Utils.Persistence
{
    [Serializable]
    public struct S_Vector3
    {
        public float x,y,z;

        public S_Vector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static implicit operator S_Vector3(Vector3 v)
        {
            return new S_Vector3(v);
        }

        public static implicit operator Vector3(S_Vector3 v)
        {   
            return new Vector3(v.x, v.y, v.z);
        }
    }
}
