using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        int N = (int)float.Ceiling(max);

        return RandomInt(N) + CreateRandom(seed).NextFloat() * (max - (int)float.Floor(max));
    }
}
