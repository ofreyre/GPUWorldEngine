using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontSRPBatch : MonoBehaviour
{
    void Start()
    {
        GetComponent<Renderer>().SetPropertyBlock(new MaterialPropertyBlock());
        Destroy(this);
    }
}
