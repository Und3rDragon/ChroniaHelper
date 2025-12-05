using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public static class TimeUtils
{
    public static long deltaTicksRaw => CalculateInterval(Engine.RawDeltaTime, 1000).Ticks;
    public static long deltaTicks => CalculateInterval(Engine.DeltaTime, 1000).Ticks;
    public static TimeSpan CalculateInterval(double value, int scale)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("TimeSpan does not accept floating point Not-a-Number values.");
        }
        double millis = value * scale + (value >= 0.0 ? 0.5 : -0.5);
        if (millis > 922337203685477.0 || millis < -922337203685477.0)
        {
            throw new OverflowException("TimeSpan overflowed because the duration is too long.");
        }
        return new TimeSpan((long)millis * 10000);
    }
}
