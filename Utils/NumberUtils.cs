using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using YoctoHelper.Cores;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;
using MonoMod;

namespace ChroniaHelper.Utils;

public static class NumberUtils
{

    public static int Increment(this int value)
    {
        return value + 1;
    }

    public static int Increment(this int value, int min, int max)
    {
        return value + 1 > max ? min : value + 1;
    }

    public static int Decrement(this int value)
    {
        return value - 1;
    }

    public static int Decrement(this int value, int min, int max)
    {
        return value - 1 < min ? max : value - 1;
    }

    public static int Mutation(this int value, bool decrement = false)
    {
        return decrement ? NumberUtils.Decrement(value) : NumberUtils.Increment(value);
    }

    public static int Mutation(this int value, int min, int max, bool decrement = false)
    {
        return decrement ? NumberUtils.Decrement(value, min, max) : NumberUtils.Increment(value, min, max);
    }

    public static void Increment(this ref int value, int increase = 1)
    {
        value += increase;
    }

    public static void IncrementLoop(this ref int value, int min, int max, int increase = 1)
    {
        value = (value + increase > max) ? min : (value + increase);
    }

    public static void Decrement(this ref int value, int decrease = 1)
    {
        value -= decrease;
    }

    public static void DecrementLoop(this ref int value, int min, int max, int decrease = 1)
    {
        value = (value - decrease < min) ? max : (value - decrease);
    }

    public static void IntFix(this ref int value, int min = int.MinValue, int max = int.MaxValue)
    {
        value = (value < min) ? min : ((value > max) ? max : value);
    }

    public static int IntFix(this int value, int min = int.MinValue, int max = int.MaxValue)
    {
        return (value < min) ? min : ((value > max) ? max : value);
    }

    public static void FloatFix(this ref float value, float min = float.MinValue, float max = float.MaxValue)
    {
        value = (value < min) ? min : ((value > max) ? max : value);
    }

    public static float FloatFix(this float value, float min = float.MinValue, float max = float.MaxValue)
    {
        return (value < min) ? min : ((value > max) ? max : value);
    }

    public static string ToHexWithFormat(int value, int padding, bool toUpper = false)
    {
        return $"0x{value.ToString((toUpper ? "X" : "x")).PadLeft(padding, '0')}";
    }
    

    public static int SafeRangeInteger(int value, int min, int max)
    {
        return (value < min) ? min : ((value > max) ? max : value);
    }

    public static T Mod<T>(this T x, T m) where T : INumber<T>
    {
        return ((x % m) + m) % m;
    }

    public static int CheckTime(this string input)
    {
        string format = input.ToLower().Trim();
        string[] formats = { "year", "month", "day", "hour", "minute", "second", "millisecond" };

        if (formats.Contains(format))
        {
            switch (format)
            {
                case "year":
                    return DateTime.Now.Year;
                case "month":
                    return DateTime.Now.Month;
                case "day":
                    return DateTime.Now.Day;
                case "hour":
                    return DateTime.Now.Hour;
                case "minute":
                    return DateTime.Now.Minute;
                case "second":
                    return DateTime.Now.Second;
                case "millisecond":
                    return DateTime.Now.Millisecond;
                default:
                    return 0;
            }
        }
        else { return 0; }
    }

    public static int CheckTimeLimit(this string input)
    {
        string format = input.ToLower().Trim();
        string[] formats = { "year", "month", "day", "hour", "minute", "second", "millisecond" };

        if (formats.Contains(format))
        {
            switch (format)
            {
                case "year":
                    return DateTime.MaxValue.Year;
                case "month":
                    return DateTime.MaxValue.Month;
                case "day":
                    return DateTime.MaxValue.Day;
                case "hour":
                    return DateTime.MaxValue.Hour;
                case "minute":
                    return DateTime.MaxValue.Minute;
                case "second":
                    return DateTime.MaxValue.Second;
                case "millisecond":
                    return DateTime.MaxValue.Millisecond;
                default:
                    return 0;
            }
        }
        else { return 0; }
    }



    public static int? OptionalInt(EntityData data, string key, int? defaultValue = null)
    {
        if (!data.Has(key))
        {
            return defaultValue;
        }

        if (int.TryParse(data.Attr(key), out var result))
        {
            return result;
        }

        return null;
    }

    public static float? OptionalFloat(EntityData data, string key, float? defaultValue = null)
    {
        if (!data.Has(key))
        {
            return defaultValue;
        }

        if (float.TryParse(data.Attr(key), out var result))
        {
            return result;
        }

        return null;
    }

    public static int MakeAbs(ref this int orig)
    {
        return orig = Math.Abs(orig);
    }

    public static long MakeAbs(ref this long orig)
    {
        return orig = Math.Abs(orig);
    }

    public static double MakeAbs(ref this double orig)
    {
        return orig = Math.Abs(orig);
    }

    public static float MakeAbs(ref this float orig)
    {
        return orig = Math.Abs(orig);
    }

    public static decimal MakeAbs(ref this decimal orig)
    {
        return orig = Math.Abs(orig);
    }

    public static T GetAbs<T>(this T orig) where T : INumber<T>
    {
        return orig < T.Zero ? -orig : orig;
    }

    public static float Closest(float baseline, params float[] values)
    {
        float r = baseline, a = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            if (i == 0)
            {
                r = values[i];
                a = (values[i] - baseline).GetAbs();
            }
            else
            {
                if ((values[i] - baseline).GetAbs() < a)
                {
                    r = values[i];
                }
            }
        }

        return r;
    }

    public enum ClosestConditions { Default = 0, UsePositive = 1, UseNegative = 2, IgnoreOverride = 3 }
    public static float Closest(float baseline, ClosestConditions conditions, params float[] values)
    {
        float r = baseline, a = 0f;
        int condition = (int)conditions;
        for (int i = 0; i < values.Length; i++)
        {
            if (i == 0)
            {
                r = values[i];
                a = (values[i] - baseline).GetAbs();
            }
            else
            {
                if ((values[i] - baseline).GetAbs() == a)
                {
                    bool isPositive = values[i] >= 0;
                    bool c1 = condition == 0, c2 = condition == 1 && isPositive, c3 = condition == 2 && !isPositive;

                    if (c1 || c2 || c3)
                    {
                        r = values[i];
                    }
                }

                else if ((values[i] - baseline).GetAbs() < a)
                {
                    r = values[i];
                }
            }
        }

        return r;
    }

    public static T Clamp<T>(this T value, T value1, T value2) where T : INumber<T>
    {
        T min = value1 < value2 ? value1 : value2;
        T max = value1 > value2 ? value1 : value2;
        return value < min ? min : (value > max ? max : value);
    }

    public static void Clamp<T>(this T value, T value1, T value2, out T result) where T : INumber<T>
    {
        T min = value1 < value2 ? value1 : value2;
        T max = value1 > value2 ? value1 : value2;
        result = value < min ? min : (value > max ? max : value);
    }

    public static T ClampMin<T>(this T value, T n) where T : INumber<T>
    {
        return value <= n ? n : value;
    }

    public static void ClampMin<T>(this T value, T n, out T result) where T : INumber<T>
    {
        result = value <= n ? n : value;
    }

    public static T ClampMax<T>(this T value, T n) where T : INumber<T>
    {
        return value >= n ? n : value;
    }

    public static void ClampMax<T>(this T value, T n, out T result) where T : INumber<T>
    {
        result = value >= n ? n : value;
    }

    public enum Comparator
    {
        Equals = 0,
        Lower = 1,
        Greater = 2,
        EqualsOrLower = 3,
        EqualsOrGreater = 4,
        WithinRange = 5,
    }
    public static bool Compare(this int source, int target, Comparator mode = 0, int anotherRange = 0) => mode switch
    {
        Comparator.Equals => source == target,
        Comparator.Lower => source < target,
        Comparator.Greater => source > target,
        Comparator.EqualsOrLower => source <= target,
        Comparator.EqualsOrGreater => source >= target,
        Comparator.WithinRange => source >= Math.Min(target, anotherRange) && source <= Math.Max(target,anotherRange),
        _ => false,
    };

    public static bool Compare(this float source, float target, Comparator mode = 0, float anotherRange = 0) => mode switch
    {
        Comparator.Equals => source == target,
        Comparator.Lower => source < target,
        Comparator.Greater => source > target,
        Comparator.EqualsOrLower => source <= target,
        Comparator.EqualsOrGreater => source >= target,
        Comparator.WithinRange => source >= Math.Min(target, anotherRange) && source <= Math.Max(target, anotherRange),
        _ => false,
    };

    public static bool Compare(this double source, double target, Comparator mode = 0, double anotherRange = 0) => mode switch
    {
        Comparator.Equals => source == target,
        Comparator.Lower => source < target,
        Comparator.Greater => source > target,
        Comparator.EqualsOrLower => source <= target,
        Comparator.EqualsOrGreater => source >= target,
        Comparator.WithinRange => source >= Math.Min(target, anotherRange) && source <= Math.Max(target, anotherRange),
        _ => false,
    };

    public static T Max<T>(this IEnumerable<T> source) where T : IComparable<T>
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) return default(T); // 空集合返回 default

        T max = enumerator.Current;
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.CompareTo(max) > 0)
                max = enumerator.Current;
        }
        return max;
    }

    public static T Min<T>(this IEnumerable<T> source) where T : IComparable<T>
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) return default(T);

        T min = enumerator.Current;
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.CompareTo(min) < 0)
                min = enumerator.Current;
        }
        return min;
    }

    public static N GetMax<T, N>(this IEnumerable<T> source, Func<T, N> selector)
        where N : IComparable
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Select(selector).Max();
    }

    public static N GetMin<T, N>(this IEnumerable<T> source, Func<T, N> selector)
        where N : IComparable
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Select(selector).Min();
    }

    public static T GetMaxItem<T, N>(this IEnumerable<T> source, Func<T, N> selector)
    where N : IComparable
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) return default(T);

        T maxItem = enumerator.Current;
        N maxValue = selector(maxItem);

        while (enumerator.MoveNext())
        {
            N currentValue = selector(enumerator.Current);
            if (currentValue.CompareTo(maxValue) > 0)
            {
                maxItem = enumerator.Current;
                maxValue = currentValue;
            }
        }

        return maxItem;
    }

    public static T GetMinItem<T, N>(this IEnumerable<T> source, Func<T, N> selector)
        where N : IComparable
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) return default(T);

        T minItem = enumerator.Current;
        N minValue = selector(minItem);

        while (enumerator.MoveNext())
        {
            N currentValue = selector(enumerator.Current);
            if (currentValue.CompareTo(minValue) < 0)
            {
                minItem = enumerator.Current;
                minValue = currentValue;
            }
        }

        return minItem;
    }

    public static bool TryParse<T>(this string source, NumberStyles style, IFormatProvider? provider, out T output) where T : INumber<T>
    {
        return T.TryParse(source, style, provider, out output);
    }

    public static bool TryParse<T>(this string source, out T output) where T : INumber<T>
    {
        return T.TryParse(source, NumberStyles.Any, null, out output);
    }

    public static T Parse<T>(this string source, T fallback) where T : INumber<T>
    {
        bool b = T.TryParse(source, NumberStyles.Any, null, out var output);
        return b ? output : fallback;
    }

    public static U Parse<T, U>(this T source, U fallback) 
        where T : INumber<T>
        where U : INumber<U>
    {
        bool b = U.TryParse(source.ToString(), NumberStyles.Any, null, out var output);
        return b ? output : fallback;
    }

    public static float Approach(this float value, float target, float maxMove)
    {
        return Calc.Approach(value, target, maxMove.GetAbs());
    }
    
    public static int Approach(this int val, int target, int delta)
    {
        var maxMove = delta.GetAbs();
        
        if(val < target)
        {
            return Math.Min(val + maxMove, target);
        }
        
        return Math.Max(val - maxMove, target);
    }

    public static double Approach(this double val, double target, double delta)
    {
        var maxMove = delta.GetAbs();

        if (val < target)
        {
            return Math.Min(val + maxMove, target);
        }

        return Math.Max(val - maxMove, target);
    }

    public static Vc2 Approach(this Vc2 val, Vc2 target, Vc2 delta)
    {
        var maxMove = new Vc2(delta.X.GetAbs(), delta.Y.GetAbs());

        var x = val.X.Approach(target.X, maxMove.X);
        var y = val.Y.Approach(target.Y, maxMove.Y);

        return new Vc2(x, y);
    }

    public static bool IsBetween(this float value, float n1, float n2)
    {
        if(n1 == n2)
        {
            return value == n1;
        }

        return (value >= float.Min(n1, n2)) && (value <= float.Max(n1, n2));
    }

    public static bool IsBetween(this double value, double n1, double n2)
    {
        if (n1 == n2)
        {
            return value == n1;
        }

        return (value >= double.Min(n1, n2)) && (value <= double.Max(n1, n2));
    }

    public static bool IsBetween(this int value, int n1, int n2)
    {
        if (n1 == n2)
        {
            return value == n1;
        }

        return (value >= int.Min(n1, n2)) && (value <= int.Max(n1, n2));
    }

    public static bool IsBetween(this decimal value, decimal n1, decimal n2)
    {
        if (n1 == n2)
        {
            return value == n1;
        }

        return (value >= decimal.Min(n1, n2)) && (value <= decimal.Max(n1, n2));
    }

    public static float GetBalance(this float value1, float scale1, float value2, float scale2)
    {
        return (value1 * scale1 + value2 * scale2) / (scale1 + scale2);
    }

    public static T ClampLoop<T>(this T value, T border1, T border2) where T : INumber<T>
    {
        if(border1 == border2) { return border1; }

        T min = border1 < border2 ? border1 : border2;
        T max = border1 > border2 ? border1 : border2;

        return Mod(value, (max - min)) + min;
    }
}
