﻿using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using YoctoHelper.Cores;

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

    public static sbyte ParseSbyte(this string str, sbyte defaultValue = 0)
    {
        return sbyte.TryParse(str, out sbyte value) ? value : defaultValue;
    }

    public static byte ParseByte(this string str, byte defaultValue = 0)
    {
        return byte.TryParse(str, out byte value) ? value : defaultValue;
    }

    public static short ParseShort(this string str, short defaultValue = 0)
    {
        return short.TryParse(str, out short value) ? value : defaultValue;
    }

    public static ushort ParseUshort(this string str, ushort defaultValue = 0)
    {
        return ushort.TryParse(str, out ushort value) ? value : defaultValue;
    }

    public static int ParseInt(this string str, int defaultValue = 0)
    {
        return int.TryParse(str, out int value) ? value : defaultValue;
    }

    public static int ParseInt(this string str, NumberStyles style, IFormatProvider provider, int defaultValue = 0)
    {
        return int.TryParse(str, style, provider, out int value) ? value : defaultValue;
    }

    public static uint ParseUint(this string str, uint defaultValue = 0)
    {
        return uint.TryParse(str, out uint value) ? value : defaultValue;
    }

    public static uint ParseUint(this string str, NumberStyles style, IFormatProvider provider, uint defaultValue = 0)
    {
        return uint.TryParse(str, style, provider, out uint value) ? value : defaultValue;
    }

    public static long ParseLong(this string str, long defaultValue = 0L)
    {
        return long.TryParse(str, out long value) ? value : defaultValue;
    }

    public static long ParseLong(this string str, NumberStyles style, IFormatProvider provider, long defaultValue = 0L)
    {
        return long.TryParse(str, style, provider, out long value) ? value : defaultValue;
    }

    public static ulong ParseUlong(this string str, ulong defaultValue = 0L)
    {
        return ulong.TryParse(str, out ulong value) ? value : defaultValue;
    }

    public static ulong ParseUlong(this string str, NumberStyles style, IFormatProvider provider, ulong defaultValue = 0L)
    {
        return ulong.TryParse(str, style, provider, out ulong value) ? value : defaultValue;
    }

    public static float ParseFloat(this string str, float defaultValue = 0F)
    {
        return float.TryParse(str, out float value) ? value : defaultValue;
    }

    public static double ParseDouble(this string str, double defaultValue = 0D)
    {
        return double.TryParse(str, out double value) ? value : defaultValue;
    }

    public static decimal ParseDecimal(this string str, decimal defaultValue = 0M)
    {
        return decimal.TryParse(str, out decimal value) ? value : defaultValue;
    }

    public static int SafeRangeInteger(int value, int min, int max)
    {
        return (value < min) ? min : ((value > max) ? max : value);
    }

    public static float Mod(float x, float m)
    {
        return ((x % m) + m) % m;
    }

    public static int Mod(int x, int m)
    {
        return ((x % m) + m) % m;
    }

    public static float MathMod(float x, float m)
    {
        if (m == 0) return x;

        float b = x / m;
        return b - float.Floor(b);
    }

    public static int MathMod(int x, int m)
    {
        if (m == 0) return x;

        int b = x / m;
        bool f = m * b < x;
        return f ? x - m * b : x - m * (b - 1);
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

    public static int GetAbs(this int orig)
    {
        return Math.Abs(orig);
    }

    public static long GetAbs(this long orig)
    {
        return Math.Abs(orig);
    }

    public static double GetAbs(this double orig)
    {
        return Math.Abs(orig);
    }

    public static float GetAbs(this float orig)
    {
        return Math.Abs(orig);
    }

    public static decimal GetAbs(this decimal orig)
    {
        return Math.Abs(orig);
    }

    public static float Closest(float baseline, params float[] values)
    {
        float r = baseline, a = 0f;
        for(int i = 0; i< values.Length; i++)
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

    public static int Clamp(this int value, int value1, int value2)
    {
        int min = value1 <= value2 ? value1 : value2, max = value2 >= value1 ? value2 : value1;
        return int.Clamp(value, min, max);
    }

    public static float Clamp(this float value, float value1, float value2)
    {
        float min = value1 <= value2 ? value1 : value2, max = value2 >= value1 ? value2 : value1;
        return float.Clamp(value, min, max);
    }

    public static Vector2 Clamp(this Vector2 value, Vector2 value1, Vector2 value2)
    {
        return new(value.X.Clamp(value1.X, value2.X), value.Y.Clamp(value1.Y, value2.Y));
    }

}
