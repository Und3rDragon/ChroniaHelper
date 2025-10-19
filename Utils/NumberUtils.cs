using System.Globalization;
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

    // Value based
    public static sbyte ParseSbyte(this string str, NumberStyles style, IFormatProvider provider, sbyte defaultValue = 0)
    {
        return (sbyte.TryParse(str, style, provider, out sbyte value)) ? value : defaultValue;
    }

    public static byte ParseByte(this string str, NumberStyles style, IFormatProvider provider, byte defaultValue = 0)
    {
        return (byte.TryParse(str, style, provider, out byte value)) ? value : defaultValue;
    }

    public static short ParseShort(this string str, NumberStyles style, IFormatProvider provider, short defaultValue = 0)
    {
        return (short.TryParse(str, style, provider, out short value)) ? value : defaultValue;
    }

    public static ushort ParseUshort(this string str, NumberStyles style, IFormatProvider provider, ushort defaultValue = 0)
    {
        return (ushort.TryParse(str, style, provider, out ushort value)) ? value : defaultValue;
    }

    public static float ParseFloat(this string str, NumberStyles style, IFormatProvider provider, float defaultValue = 0L)
    {
        return (float.TryParse(str, style, provider, out float value)) ? value : defaultValue;
    }

    public static double ParseDouble(this string str, NumberStyles style, IFormatProvider provider, double defaultValue = 0L)
    {
        return (double.TryParse(str, style, provider, out double value)) ? value : defaultValue;
    }

    public static decimal ParseDecimal(this string str, NumberStyles style, IFormatProvider provider, decimal defaultValue = 0L)
    {
        return (decimal.TryParse(str, style, provider, out decimal value)) ? value : defaultValue;
    }

    // Variable based
    public static void ParseSbyte(this string str, out sbyte value, sbyte defaultValue = 0)
    {
        value = sbyte.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseSbyte(this string str, NumberStyles style, IFormatProvider provider, out sbyte value, sbyte defaultValue = 0)
    {
        value = sbyte.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseByte(this string str, out byte value, byte defaultValue = 0)
    {
        value = byte.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseByte(this string str, NumberStyles style, IFormatProvider provider, out byte value, byte defaultValue = 0)
    {
        value = byte.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseShort(this string str, out short value, short defaultValue = 0)
    {
        value = short.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseShort(this string str, NumberStyles style, IFormatProvider provider, out short value, short defaultValue = 0)
    {
        value = short.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseUshort(this string str, out ushort value, ushort defaultValue = 0)
    {
        value = ushort.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseUshort(this string str, NumberStyles style, IFormatProvider provider, out ushort value, ushort defaultValue = 0)
    {
        value = ushort.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseInt(this string str, out int value, int defaultValue = 0)
    {
        value = int.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseInt(this string str, NumberStyles style, IFormatProvider provider, out int value, int defaultValue = 0)
    {
        value = int.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseUint(this string str, out uint value, uint defaultValue = 0)
    {
        value = uint.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseUint(this string str, NumberStyles style, IFormatProvider provider, out uint value, uint defaultValue = 0)
    {
        value = uint.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseLong(this string str, out long value, long defaultValue = 0L)
    {
        value = long.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseLong(this string str, NumberStyles style, IFormatProvider provider, out long value, long defaultValue = 0L)
    {
        value = long.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseUlong(this string str, out ulong value, ulong defaultValue = 0UL)
    {
        value = ulong.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseUlong(this string str, NumberStyles style, IFormatProvider provider, out ulong value, ulong defaultValue = 0UL)
    {
        value = ulong.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseFloat(this string str, out float value, float defaultValue = 0F)
    {
        value = float.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseFloat(this string str, NumberStyles style, IFormatProvider provider, out float value, float defaultValue = 0F)
    {
        value = float.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseDouble(this string str, out double value, double defaultValue = 0D)
    {
        value = double.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseDouble(this string str, NumberStyles style, IFormatProvider provider, out double value, double defaultValue = 0D)
    {
        value = double.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    public static void ParseDecimal(this string str, out decimal value, decimal defaultValue = 0M)
    {
        value = decimal.TryParse(str, out value) ? value : defaultValue;
    }

    public static void ParseDecimal(this string str, NumberStyles style, IFormatProvider provider, out decimal value, decimal defaultValue = 0M)
    {
        value = decimal.TryParse(str, style, provider, out value) ? value : defaultValue;
    }

    // EntityData based
    public static void ParseSbyte(this EntityData data, string name, out sbyte value, sbyte defaultValue = 0)
    {
        value = sbyte.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseSbyte(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out sbyte value, sbyte defaultValue = 0)
    {
        value = sbyte.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseByte(this EntityData data, string name, out byte value, byte defaultValue = 0)
    {
        value = byte.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseByte(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out byte value, byte defaultValue = 0)
    {
        value = byte.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseShort(this EntityData data, string name, out short value, short defaultValue = 0)
    {
        value = short.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseShort(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out short value, short defaultValue = 0)
    {
        value = short.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseUshort(this EntityData data, string name, out ushort value, ushort defaultValue = 0)
    {
        value = ushort.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseUshort(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out ushort value, ushort defaultValue = 0)
    {
        value = ushort.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseInt(this EntityData data, string name, out int value, int defaultValue = 0)
    {
        value = int.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseInt(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out int value, int defaultValue = 0)
    {
        value = int.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseUint(this EntityData data, string name, out uint value, uint defaultValue = 0)
    {
        value = uint.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseUint(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out uint value, uint defaultValue = 0)
    {
        value = uint.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseLong(this EntityData data, string name, out long value, long defaultValue = 0L)
    {
        value = long.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseLong(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out long value, long defaultValue = 0L)
    {
        value = long.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseUlong(this EntityData data, string name, out ulong value, ulong defaultValue = 0UL)
    {
        value = ulong.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseUlong(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out ulong value, ulong defaultValue = 0UL)
    {
        value = ulong.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseFloat(this EntityData data, string name, out float value, float defaultValue = 0F)
    {
        value = float.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseFloat(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out float value, float defaultValue = 0F)
    {
        value = float.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseDouble(this EntityData data, string name, out double value, double defaultValue = 0D)
    {
        value = double.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseDouble(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out double value, double defaultValue = 0D)
    {
        value = double.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
    }

    public static void ParseDecimal(this EntityData data, string name, out decimal value, decimal defaultValue = 0M)
    {
        value = decimal.TryParse(data.Attr(name), out value) ? value : defaultValue;
    }

    public static void ParseDecimal(this EntityData data, string name, NumberStyles style, IFormatProvider provider, out decimal value, decimal defaultValue = 0M)
    {
        value = decimal.TryParse(data.Attr(name), style, provider, out value) ? value : defaultValue;
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

    public static int ClampWhole(this int value, int value1, int value2)
    {
        int min = value1 < value2 ? value1 : value2;
        int max = value1 > value2 ? value1 : value2;

        return value < min ? min : (value > max ? max : value);
    }

    public static float ClampWhole(this float value, float value1, float value2)
    {
        float min = value1 < value2 ? value1 : value2;
        float max = value1 > value2 ? value1 : value2;

        return value < min ? min : (value > max ? max : value);
    }

    public static Vector2 ClampWhole(this Vector2 value, Vector2 value1, Vector2 value2)
    {
        return new(value.X.ClampWhole(value1.X, value2.X), value.Y.ClampWhole(value1.Y, value2.Y));
    }

    public static int ClampMin(this int value, int n)
    {
        return value >= n ? value : n;
    }

    public static float ClampMin(this float value, float n)
    {
        return value >= n ? value : n;
    }

    public static int ClampMax(this int value, int n)
    {
        return value <= n ? value : n;
    }

    public static float ClampMax(this float value, float n)
    {
        return value <= n ? value : n;
    }

    // out as
    public static void Clamp(this int value, int value1, int value2, out int to)
    {
        int min = value1 <= value2 ? value1 : value2, max = value2 >= value1 ? value2 : value1;
        to = int.Clamp(value, min, max);
    }

    public static void Clamp(this float value, float value1, float value2, out float to)
    {
        float min = value1 <= value2 ? value1 : value2, max = value2 >= value1 ? value2 : value1;
        to = float.Clamp(value, min, max);
    }

    public static void Clamp(this Vector2 value, Vector2 value1, Vector2 value2, out Vc2 to)
    {
        to = new(value.X.Clamp(value1.X, value2.X), value.Y.Clamp(value1.Y, value2.Y));
    }

    public static void ClampWhole(this int value, int value1, int value2, out int to)
    {
        int min = value1 < value2 ? value1 : value2;
        int max = value1 > value2 ? value1 : value2;

        to = value < min ? min : (value > max ? max : value);
    }

    public static void ClampWhole(this float value, float value1, float value2, out float to)
    {
        float min = value1 < value2 ? value1 : value2;
        float max = value1 > value2 ? value1 : value2;

        to = value < min ? min : (value > max ? max : value);
    }

    public static void ClampWhole(this Vector2 value, Vector2 value1, Vector2 value2, out Vc2 to)
    {
        to = new(value.X.ClampWhole(value1.X, value2.X), value.Y.ClampWhole(value1.Y, value2.Y));
    }

    public static void ClampMin(this int value, int n, out int to)
    {
        to = value >= n ? value : n;
    }

    public static void ClampMin(this float value, float n, out float to)
    {
        to = value >= n ? value : n;
    }

    public static void ClampMax(this int value, int n, out int to)
    {
        to = value <= n ? value : n;
    }

    public static void ClampMax(this float value, float n, out float to)
    {
        to = value <= n ? value : n;
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
}
