using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.DotNet.Code.Cil;
using Microsoft.VisualBasic;
using YamlDotNet.Serialization;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

// This class was moved from CommunalHelperModule, so let's keep the same namespace.
public static class Miscs
{

    public static int ToBitFlag(params bool[] b)
    {
        int ret = 0;
        for (int i = 0; i < b.Length; i++)
            ret |= BoolUtils.ToInt(b[i]) << i;
        return ret;
    }

    public static Vector2 RandomDir(float length)
    {
        return Calc.AngleToVector(Calc.Random.NextAngle(), length);
    }

    public static string StrTrim(string str)
    {
        return str.Trim();
    }

    public static Vector2 Min(Vector2 a, Vector2 b)
    {
        return new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }

    public static Vector2 Max(Vector2 a, Vector2 b)
    {
        return new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    public static Rectangle Rectangle(Vector2 a, Vector2 b)
    {
        Vector2 min = Min(a, b);
        Vector2 size = Max(a, b) - min;
        return new((int) min.X, (int) min.Y, (int) size.X, (int) size.Y);
    }

    /// <summary>
    /// Triangle wave function.
    /// </summary>
    public static float TriangleWave(float x)
    {
        return (2 * Math.Abs(NumberUtils.Mod(x, 2) - 1)) - 1;
    }

    /// <summary>
    /// Triangle wave between mapped between two values.
    /// </summary>
    /// <param name="x">The input value.</param>
    /// <param name="from">The ouput when <c>x</c> is an even integer.</param>
    /// <param name="to">The output when <c>x</c> is an odd integer.</param>
    public static float MappedTriangleWave(float x, float from, float to)
    {
        return ((from - to) * Math.Abs(NumberUtils.Mod(x, 2) - 1)) + to;
    }

    public static float PowerBounce(float x, float p)
    {
        return -(float) Math.Pow(Math.Abs(2 * (NumberUtils.Mod(x, 1) - .5f)), p) + 1;
    }

    public static bool Blink(float time, float duration)
    {
        return time % (duration * 2) < duration;
    }

    /// <summary>
    /// Checks if two line segments are intersecting.
    /// </summary>
    /// <param name="p0">The first end of the first line segment.</param>
    /// <param name="p1">The second end of the first line segment.</param>
    /// <param name="p2">The first end of the second line segment.</param>
    /// <param name="p3">The second end of the second line segment.</param>
    /// <returns>The result of the intersection check.</returns>
    public static bool SegmentIntersection(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float sax = p1.X - p0.X; float say = p1.Y - p0.Y;
        float sbx = p3.X - p2.X; float sby = p3.Y - p2.Y;

        float s = (-say * (p0.X - p2.X) + sax * (p0.Y - p2.Y)) / (-sbx * say + sax * sby);
        float t = (sbx * (p0.Y - p2.Y) - sby * (p0.X - p2.X)) / (-sbx * say + sax * sby);

        return s is >= 0 and <= 1
            && t is >= 0 and <= 1;
    }

    public static IEnumerator Interpolate(float duration, Action<float> action)
    {
        float t = duration;
        while (t > 0.0f)
        {
            action(1 - t / duration);
            t = Calc.Approach(t, 0.0f, Engine.DeltaTime);
            yield return null;
        }
        action(1.0f);
    }

    public static Rectangle GetBounds(this Camera camera)
    {
        int top = (int)camera.Top;
        int bottom = (int)camera.Bottom;
        int left = (int)camera.Left;
        int right = (int)camera.Right;

        return new(left, top, right - left, bottom - top);
    }

    public static float OZMTime(bool isTimeUnit, float param, bool isReturn)
    {
        if (!isTimeUnit)
        {
            return isReturn ? param * 0.5f * Engine.DeltaTime : param * 2f * Engine.DeltaTime;
        }

        if(param <= Engine.DeltaTime)
        {
            return 1f;
        }

        if(param > 1000000f)
        {
            return 0;
        }

        return Engine.DeltaTime / param;
    }

    public static void RenderProgressRectangle(Vector2 Position, float width, float height, float progress, Color color, float expansion = 0f, bool average = false)
    {
        expansion = expansion < 0 && expansion.GetAbs() >= Calc.Min(width, height) / 2f ? -Calc.Min(width, height) / 2f : expansion;
        float newWidth = width + 2 * expansion, newHeight = height + 2 * expansion;
        Vector2 p0 = Position + new Vector2(-expansion, -expansion),
            p1 = Position + new Vector2(width, 0f) + new Vector2(expansion, -expansion),
            p2 = Position + new Vector2(width, height) + new Vector2(expansion, expansion),
            p3 = Position + new Vector2(0f, height) + new Vector2(-expansion, expansion);

        if (average)
        {
            float C = 2 * width + 2 * height + 8 * expansion;
            float L = progress * C;

            if (L >= 0)
            {
                Draw.Line(p0, p0 + new Vector2(Calc.Min(L, newWidth), 0f), color);
            }
            if (L >= newWidth)
            {
                Draw.Line(p1, p1 + new Vector2(0f, Calc.Min(L - newWidth, newWidth + newHeight)), color);
            }
            if (L >= newWidth + newHeight)
            {
                Draw.Line(p2, p2 + new Vector2(-Calc.Min(L - newWidth - newHeight, 0f)), color);
            }
            if (L >= newWidth * 2 + newHeight)
            {
                Draw.Line(p3, p3 + new Vector2(0f, -Calc.Min(L - newWidth * 2 - newHeight)), color);
            }
        }
        else
        {
            Vector2 d1 = p1 - p0, d2 = p2 - p1, d3 = p3 - p2, d4 = p0 - p3;

            if (progress >= 0)
            {
                Draw.Line(p0, p0 + d1 * Calc.Min(progress, 0.25f) / 0.25f, color);
            }
            if (progress >= 0.25f)
            {
                Draw.Line(p1, p1 + d2 * Calc.Min(progress - 0.25f, 0.25f) / 0.25f, color);
            }
            if (progress >= 0.5f)
            {
                Draw.Line(p2, p2 + d3 * Calc.Min(progress - 0.5f, 0.25f) / 0.25f, color);
            }
            if (progress >= 0.75f)
            {
                Draw.Line(p3, p3 + d4 * Calc.Min(progress - 0.75f, 0.25f) / 0.25f, color);
            }
        }
    }
}
