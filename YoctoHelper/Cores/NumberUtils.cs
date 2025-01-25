using System;
using System.Globalization;

namespace YoctoHelper.Cores;

public static class NumberUtils
{

    public static sbyte ParseSbyte(this string str, sbyte defaultValue = 0)
    {
        return (sbyte.TryParse(str, out sbyte value)) ? value : defaultValue;
    }

    public static sbyte ParseSbyte(this string str, NumberStyles style, IFormatProvider provider, sbyte defaultValue = 0)
    {
        return (sbyte.TryParse(str, style, provider, out sbyte value)) ? value : defaultValue;
    }

    public static byte ParseByte(this string str, byte defaultValue = 0)
    {
        return (byte.TryParse(str, out byte value)) ? value : defaultValue;
    }

    public static byte ParseByte(this string str, NumberStyles style, IFormatProvider provider, byte defaultValue = 0)
    {
        return (byte.TryParse(str, style, provider, out byte value)) ? value : defaultValue;
    }

    public static short ParseShort(this string str, short defaultValue = 0)
    {
        return (short.TryParse(str, out short value)) ? value : defaultValue;
    }

    public static short ParseShort(this string str, NumberStyles style, IFormatProvider provider, short defaultValue = 0)
    {
        return (short.TryParse(str, style, provider, out short value)) ? value : defaultValue;
    }

    public static ushort ParseUshort(this string str, ushort defaultValue = 0)
    {
        return (ushort.TryParse(str, out ushort value)) ? value : defaultValue;
    }

    public static ushort ParseUshort(this string str, NumberStyles style, IFormatProvider provider, ushort defaultValue = 0)
    {
        return (ushort.TryParse(str, style, provider, out ushort value)) ? value : defaultValue;
    }

    public static int ParseInt(this string str, int defaultValue = 0)
    {
        return (int.TryParse(str, out int value)) ? value : defaultValue;
    }

    public static int ParseInt(this string str, NumberStyles style, IFormatProvider provider, int defaultValue = 0)
    {
        return (int.TryParse(str, style, provider, out int value)) ? value : defaultValue;
    }

    public static uint ParseUint(this string str, uint defaultValue = 0)
    {
        return (uint.TryParse(str, out uint value)) ? value : defaultValue;
    }

    public static uint ParseUint(this string str, NumberStyles style, IFormatProvider provider, uint defaultValue = 0)
    {
        return (uint.TryParse(str, style, provider, out uint value)) ? value : defaultValue;
    }

    public static long ParseLong(this string str, long defaultValue = 0L)
    {
        return (long.TryParse(str, out long value)) ? value : defaultValue;
    }

    public static long ParseLong(this string str, NumberStyles style, IFormatProvider provider, long defaultValue = 0L)
    {
        return (long.TryParse(str, style, provider, out long value)) ? value : defaultValue;
    }

    public static ulong ParseUlong(this string str, ulong defaultValue = 0L)
    {
        return (ulong.TryParse(str, out ulong value)) ? value : defaultValue;
    }

    public static ulong ParseUlong(this string str, NumberStyles style, IFormatProvider provider, ulong defaultValue = 0L)
    {
        return (ulong.TryParse(str, style, provider, out ulong value)) ? value : defaultValue;
    }

    public static float ParseFloat(this string str, float defaultValue = 0F)
    {
        return (float.TryParse(str, out float value)) ? value : defaultValue;
    }

    public static float ParseFloat(this string str, NumberStyles style, IFormatProvider provider, float defaultValue = 0L)
    {
        return (float.TryParse(str, style, provider, out float value)) ? value : defaultValue;
    }

    public static double ParseDouble(this string str, double defaultValue = 0D)
    {
        return (double.TryParse(str, out double value)) ? value : defaultValue;
    }

    public static double ParseDouble(this string str, NumberStyles style, IFormatProvider provider, double defaultValue = 0L)
    {
        return (double.TryParse(str, style, provider, out double value)) ? value : defaultValue;
    }

    public static decimal ParseDecimal(this string str, decimal defaultValue = 0M)
    {
        return (decimal.TryParse(str, out decimal value)) ? value : defaultValue;
    }

    public static decimal ParseDecimal(this string str, NumberStyles style, IFormatProvider provider, decimal defaultValue = 0L)
    {
        return (decimal.TryParse(str, style, provider, out decimal value)) ? value : defaultValue;
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

}
