using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public static class FadeUtils
{
    public static float ClampProgress(this float source, float a, float b)
    {
        if(a == b) { return 1f; }

        float m = float.Clamp(source, Calc.Min(a, b), Calc.Max(a, b));

        return (m - a) / (b - a);
    }

    public static float ClampProgress(this int source, int a, int b)
    {
        if (a == b) { return 1f; }

        int n = int.Clamp(source, int.Min(a, b), int.Max(a, b));
        return (float)(n - a) / (float)(b - a);
    }

    public static int LerpValue(this float source, float clampA, float clampB, int valueA, int valueB)
    {
        // redefine
        Vector2 a = new(clampA, valueA), b = new Vector2(clampB, valueB), d = b - a;

        if (clampA == clampB) { return valueB; }

        float p = source.ClampProgress(clampA, clampB);

        var r = (a + d * p).Y;

        return (int)r;
    }

    public static float LerpValue(this float source, float clampA, float clampB, float valueA, float valueB)
    {
        // redefine
        Vector2 a = new(clampA, valueA), b = new Vector2(clampB, valueB), d = b - a;

        if (clampA == clampB) { return valueB; }

        float p = source.ClampProgress(clampA, clampB);

        var r = (a + d * p).Y;

        return r;
    }

    public static Vector2 LerpValue(this float source, float clampA, float clampB, Vector2 valueA, Vector2 valueB)
    {
        Vector3 a = new(clampA, valueA.X, valueA.Y), b = new(clampB, valueB.X, valueB.Y);
        Vector3 d = b - a;

        if (clampA == clampB) { return valueB; }

        float p = source.ClampProgress(clampA, clampB);

        var r = a + d * p;

        return new Vector2(r.Y, r.Z);
    }
}
