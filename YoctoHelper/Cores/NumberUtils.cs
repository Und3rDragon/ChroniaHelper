using System;
using System.ComponentModel;
using System.Globalization;

namespace YoctoHelper.Cores;

public static class NumberUtils
{
    // Value based
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

}
