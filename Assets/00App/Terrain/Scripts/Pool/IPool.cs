using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPool
{
    GameObject Get();
    void Return(GameObject gobj);
}
