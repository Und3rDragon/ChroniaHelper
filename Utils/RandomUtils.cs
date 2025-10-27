using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FMOD.Studio;

namespace ChroniaHelper.Utils;

public static class RandomUtils
{
    public static Random CreateRandom(int? overrideValue = null)
    {
        if (overrideValue.IsNull())
        {
            // Maximum integer: 2,147,483,647
            // hhm,mss,MMM
            $"{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}{DateTime.Now.Millisecond}".ParseInt(out int seed, 0);
            return new Random(seed);
        }

        return new Random(overrideValue ?? 0);
    }

    public static void CreateRandom(out Random random, int? overrideValue = null)
    {
        random = CreateRandom(overrideValue);
    }

    public static T[] RandomPick<T>(this T[] source, int count, int? seed = null)
    {
        return CreateRandom(seed).GetItems(source, count.ClampMin(1));
    }

    public static void RandomPick<T>(this T[] source, int count, out T[] picked, int? seed = null)
    {
        picked = source.RandomPick(count, seed);
    }

    public static T[] RandomPick<T>(this ReadOnlySpan<T> source, int count, int? seed = null)
    {
        return CreateRandom(seed).GetItems(source, count.ClampMin(1));
    }

    public static void RandomPick<T>(this ReadOnlySpan<T> source, int count, out T[] picked, int? seed = null)
    {
        picked = source.RandomPick(count, seed);
    }

    public static int RandomInt(int? seed = null)
    {
        return CreateRandom(seed).Next();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="max">Exclusive max</param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static int RandomInt(int max, int? seed = null)
    {
        return CreateRandom(seed).Next(max);
    }

    public static int RandomInt(int min, int max, int? seed = null)
    {
        return int.Min(min, max) + RandomInt(int.Max(min, max) - int.Min(min, max));
    }

    public static void RandomInt(out int N, int? min = null, int? max = null, int? seed = null)
    {
        if(min.IsNull() && max.IsNull())
        {
            N = RandomInt(seed);
            return;
        }

        if (min.IsNull())
        {
            N = RandomInt(max ?? 1, seed);
            return;
        }

        if (max.IsNull())
        {
            N = RandomInt(min ?? 1, seed);
            return;
        }

        N = RandomInt(min ?? 0, max ?? 1, seed);
    }

    /// <summary>
    /// A random value 0 <= x < 1
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static float RandomFloat(int? seed = null)
    {
        return CreateRandom(seed).NextFloat();
    }

    /// <summary>
    /// A random value 0 <= x < max
    /// </summary>
    /// <param name="max">Exclusive max</param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static float RandomFloat(float max, int? seed = null)
    {
        return RandomFloat(seed) * max;
    }

    public static float RandomFloat(float min, float max, int? seed = null)
    {
        return Calc.Min(min, max) + (Calc.Max(min, max) - Calc.Min(max, min)) * RandomFloat(seed);
    }

    public static void RandomFloat(out float N, float? min = null, float? max = null, int? seed = null)
    {
        if (min.IsNull() && max.IsNull())
        {
            N = RandomFloat(seed);
            return;
        }

        if (min.IsNull())
        {
            N = RandomFloat(max ?? 1f, seed);
            return;
        }

        if (max.IsNull())
        {
            N = RandomFloat(min ?? 1f, seed);
        }

        N = RandomFloat(min ?? 0, max ?? 1f, seed);
    }

    public static Vc2[] GetRandomPoints(Vc2 a, Vc2 b, int count, int? seed = null)
    {
        if((a - b).LengthSquared() < 0.01f) 
        { 
            return RandomPick(new Vc2[] { a, b }, count, seed);
        }

        Vc2 p1 = new Vc2(float.Min(a.X, b.X), float.Min(a.Y, b.Y)),
            p2 = new Vc2(float.Max(a.X, b.X), float.Max(a.Y, b.Y));

        Vc2[] points = new Vc2[count];
        for(int i = 0; i < count; i++)
        {
            Vc2 p = Vc2.Zero;
            p.X = RandomFloat(p1.X, p2.X, seed);
            p.Y = RandomFloat(p1.Y, p2.Y, seed);

            points[i] = p;
        }
        return points;
    }

    public static void GetRandomPoints(Vc2 a, Vc2 b, int count, out Vc2[] points, int? seed = null)
    {
        points = GetRandomPoints(a, b, count, seed);
    }
}
