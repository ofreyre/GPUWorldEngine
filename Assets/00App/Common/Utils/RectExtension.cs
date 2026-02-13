using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectExtension
{
    public static bool Intersection(this Rect r1, Rect r2, out Rect intersection)
    {
        if (r2.Overlaps(r1))
        {
            float x1 = Mathf.Min(r1.xMax, r2.xMax);
            float x2 = Mathf.Max(r1.xMin, r2.xMin);
            float y1 = Mathf.Min(r1.yMax, r2.yMax);
            float y2 = Mathf.Max(r1.yMin, r2.yMin);

            intersection = new Rect(
                Mathf.Min(x1, x2),
                Mathf.Min(y1, y2),
                Mathf.Max(0.0f, x1 - x2),
                Mathf.Max(0.0f, y1 - y2)
            );
            return true;
        }

        intersection = new Rect();
        return false;
    }

    public static Rect Intersection(this Rect r1, Rect r2)
    {
        float x1 = Mathf.Min(r1.xMax, r2.xMax);
        float x2 = Mathf.Max(r1.xMin, r2.xMin);
        float y1 = Mathf.Min(r1.yMax, r2.yMax);
        float y2 = Mathf.Max(r1.yMin, r2.yMin);
        return new Rect(
            Mathf.Min(x1, x2),
            Mathf.Min(y1, y2),
            Mathf.Max(0.0f, x1 - x2),
            Mathf.Max(0.0f, y1 - y2)
            );
    }

    public static bool Intersection(this RectInt r1, RectInt r2, out RectInt intersection)
    {
        if (r2.Overlaps(r1))
        {
            int x1 = Mathf.Min(r1.xMax, r2.xMax);
            int x2 = Mathf.Max(r1.xMin, r2.xMin);
            int y1 = Mathf.Min(r1.yMax, r2.yMax);
            int y2 = Mathf.Max(r1.yMin, r2.yMin);

            intersection = new RectInt(
                Mathf.Min(x1, x2),
                Mathf.Min(y1, y2),
                Mathf.Max(0, x1 - x2),
                Mathf.Max(0, y1 - y2)
            );
            return true;
        }

        intersection = new RectInt();
        return false;
    }

    public static RectInt Intersection(this RectInt r1, RectInt r2)
    {
        int x1 = Mathf.Min(r1.xMax, r2.xMax);
        int x2 = Mathf.Max(r1.xMin, r2.xMin);
        int y1 = Mathf.Min(r1.yMax, r2.yMax);
        int y2 = Mathf.Max(r1.yMin, r2.yMin);
        return new RectInt(
            Mathf.Min(x1, x2),
            Mathf.Min(y1, y2),
            Mathf.Max(0, x1 - x2),
            Mathf.Max(0, y1 - y2)
        );
    }

    public static string BoundsToString(this RectInt r)
    {
        return r.min + " " + r.max;
    }
}
