using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMoveInput
{
    bool GetValue(out Vector3 value);
}

public interface IPlayerRotate
{
    bool GetValue(out Vector2 value);
}
