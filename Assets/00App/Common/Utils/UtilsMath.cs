using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilsMath
{
    public struct CurveKeyframe
    {
        public float t;
        public float tangent;
        public float value;

        public override string ToString()
        {
            return "time = " + t + "\n" +
                "tangent = " + tangent + "\n" +
                "value = " + value + "\n";
        }
    }

    public static int LowerPow2(int n)
    {
        return (int)Mathf.Floor(Mathf.Log10(n) / Mathf.Log10(2));
    }

    public static int UpperPow2(int n)
    {
        return (int)Mathf.Ceil(Mathf.Log10(n) / Mathf.Log10(2));
    }

    public static int NearestPow2(int n)
    {
        return (int)Mathf.Round(Mathf.Log10(n) / Mathf.Log10(2));
    }

    public static int LowerPower2(int n)
    {
        return (int)Mathf.Pow(2, LowerPow2(n));
    }

    public static int UpperPower2(int n)
    {
        return (int)Mathf.Pow(2, UpperPow2(n));
    }

    public static int NearestPower2(int n)
    {
        return (int)Mathf.Pow(2, NearestPow2(n));
    }

    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        return matrix.GetColumn(3);
    }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        return new Vector3(
        matrix.GetColumn(0).magnitude,
        matrix.GetColumn(1).magnitude,
        matrix.GetColumn(2).magnitude
        );
    }

    public static CurveKeyframe[] AnimationCurveToKeysArray(AnimationCurve curve)
    {
        CurveKeyframe[] keysArray = new CurveKeyframe[curve.length];
        for (int i = 0; i < curve.length; i++)
        {
            keysArray[i] = new CurveKeyframe
            {
                t = curve.keys[i].time,
                tangent = curve.keys[i].inTangent,
                value = curve.keys[i].value
            };
        }
        return keysArray;
    }

    public static Vector2[] GetOctaveOffsets(HeightMapSettings settings)
    {
        System.Random prng = new System.Random(0);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x;
            float offsetY = prng.Next(-100000, 100000) + settings.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        return octaveOffsets;
    }
}
