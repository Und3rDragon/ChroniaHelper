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

        float n = float.Clamp(source, float.Min(a, b), float.Max(a, b));
        return (n - float.Min(a, b)) / (float.Max(a,b) - float.Min(a,b));
    }

    public static float ClampProgress(this int source, int a, int b)
    {
        if (a == b) { return 1f; }

        int n = int.Clamp(source, int.Min(a, b), int.Max(a, b));
        return (float)(n - int.Min(a, b)) / (float)(int.Max(a, b) - int.Min(a, b));
    }

    public static int LerpValue(this float source, float clampA, float clampB, int valueA, int valueB)
    {
        // redefine
        int va = int.Min(valueA, valueB), vb = int.Max(valueA, valueB);

        float p = source.ClampProgress(clampA, clampB);

        return va + (int)((vb - va) * p);
    }

    public static float LerpValue(this float source, float clampA, float clampB, float valueA, float valueB)
    {
        // redefine
        float va = float.Min(valueA, valueB), vb = float.Max(valueA, valueB);

        float p = source.ClampProgress(clampA, clampB);

        return va + ((vb - va) * p);
    }
}
