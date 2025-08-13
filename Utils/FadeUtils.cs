using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public static class FadeUtils
{
    public static float ClampProgress(this float source, float a, float b, EaseMode ease = EaseMode.Linear)
    {
        if(a == b) { return 1f; }

        float m = float.Clamp(source, Calc.Min(a, b), Calc.Max(a, b));
        
        return EaseUtils.EaseMatch[ease]((m - a) / (b - a));
    }

    public static float ClampProgress(this int source, int a, int b, EaseMode ease = EaseMode.Linear)
    {
        if (a == b) { return 1f; }

        int n = int.Clamp(source, int.Min(a, b), int.Max(a, b));
        return EaseUtils.EaseMatch[ease]((float)(n - a) / (float)(b - a));
    }

    public static float ClampProgress(this Vector2 point, Vector2 a, Vector2 b, EaseMode ease = EaseMode.Linear)
    {
        if(a == b) { return 1f; }

        GeometryUtils.Line AB = new GeometryUtils.Line(a, b);
        float L = AB.GetADLengthSigned(point);
        float P = float.Clamp(L, 0f, AB.Length());

        return EaseUtils.EaseMatch[ease](P / AB.Length());
    }

    public static int LerpValue(this float source, float clampA, float clampB, int valueA, int valueB, EaseMode ease = EaseMode.Linear)
    {
        // redefine
        Vector2 a = new(clampA, valueA), b = new Vector2(clampB, valueB), d = b - a;

        if (clampA == clampB) { return valueB; }

        float p = source.ClampProgress(clampA, clampB, ease);

        var r = (a + d * p).Y;

        return (int)r;
    }

    public static float LerpValue(this float source, float clampA, float clampB, float valueA, float valueB, EaseMode ease = EaseMode.Linear)
    {
        // redefine
        Vector2 a = new(clampA, valueA), b = new Vector2(clampB, valueB), d = b - a;

        if (clampA == clampB) { return valueB; }

        float p = source.ClampProgress(clampA, clampB, ease);

        var r = (a + d * p).Y;

        return r;
    }

    public static Vector2 LerpValue(this float source, float clampA, float clampB, Vector2 valueA, Vector2 valueB, EaseMode ease = EaseMode.Linear)
    {
        Vector3 a = new(clampA, valueA.X, valueA.Y), b = new(clampB, valueB.X, valueB.Y);
        Vector3 d = b - a;

        if (clampA == clampB) { return valueB; }

        float p = source.ClampProgress(clampA, clampB, ease);

        var r = a + d * p;

        return new Vector2(r.Y, r.Z);
    }
}
