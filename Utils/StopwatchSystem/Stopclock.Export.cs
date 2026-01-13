using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

public partial class Stopclock
{

    /// <summary>
    /// 检查时间是否归零
    /// </summary>
    public bool ZeroState => year == 0 && month == 0 && day == 0 &&
        hour == 0 && minute == 0 && second == 0 && millisecond == 0;

    /// <summary>
    /// 获取格式化时间字符串
    /// </summary>
    public string FormattedTime =>
        $"{year:00}Years {month:00}Months {day:00}Days {hour:00}:{minute:00}:{second:00}.{millisecond:000}";
    
    public string DigitalTime =>
        $"{year:00}:{month:00}:{day:00}:{hour:00}:{minute:00}:{second:00}:{millisecond:000}";
    
    public string Digitals =>
        $"{year:00}{month:00}{day:00}{hour:00}{minute:00}{second:00}{millisecond:000}";
    
    public void GetTimeData(out int[] digitals)
    {
        digitals = new int[7] { millisecond, second, minute, hour, day, month, year };
    }
    
    public void GetDeltaSystemTimeData(out int[] digitals)
    {
        var dt = safeStartTime - DateTime.Now;
        int days = NumberUtils.Mod(dt.Days, 30);
        int months = NumberUtils.Mod(dt.Days, 360) / 30;
        int years = dt.Days / 360;
        digitals = new int[7] { dt.Milliseconds, dt.Seconds, dt.Minutes, dt.Hours, days, months, years };
    }

    public void GetClampedTimeData(out int[] digitals, int minUnit = 0, int maxUnit = 6)
    {
        minUnit.Clamp(0, 6, out int min);
        maxUnit.Clamp(min, 6, out int max);

        digitals = new int[max - min + 1];
        
        GetTimeData(out int[] n);
        
        for(int i = 0, j = min; j <= max; i++, j++)
        {
            digitals[i] = n[j];
        }
        
        int second = n[1], minute = n[2], hour = n[3], day = n[4], month = n[5], year = n[6];
        
        switch (max)
        {
            case 0:
                digitals[digitals.MaxIndex()] += (((((year * 12 + month) * 30 + day) * 24 + hour) * 60 + minute) * 60 + second) * 1000;
                break;
            case 1:
                digitals[digitals.MaxIndex()] += ((((year * 12 + month) * 30 + day) * 24 + hour) * 60 + minute) * 60;
                break;
            case 2:
                digitals[digitals.MaxIndex()] += (((year * 12 + month) * 30 + day) * 24 + hour) * 60;
                break;
            case 3:
                digitals[digitals.MaxIndex()] += ((year * 12 + month) * 30 + day) * 24;
                break;
            case 4:
                digitals[digitals.MaxIndex()] += (year * 12 + month) * 30;
                break;
            case 5:
                digitals[digitals.MaxIndex()] += year * 12;
                break;
            default:
                break;
        }
    }

    public void GetClampedDeltaSystemTimeData(out int[] digitals, int minUnit = 0, int maxUnit = 6)
    {
        minUnit.Clamp(0, 6, out int min);
        maxUnit.Clamp(min, 6, out int max);

        digitals = new int[max - min + 1];

        GetDeltaSystemTimeData(out int[] n);

        for (int i = 0, j = min; j <= max; i++, j++)
        {
            digitals[i] = n[j];
        }

        int second = n[1], minute = n[2], hour = n[3], day = n[4], month = n[5], year = n[6];

        switch (max)
        {
            case 0:
                digitals[digitals.MaxIndex()] += (((((year * 12 + month) * 30 + day) * 24 + hour) * 60 + minute) * 60 + second) * 1000;
                break;
            case 1:
                digitals[digitals.MaxIndex()] += ((((year * 12 + month) * 30 + day) * 24 + hour) * 60 + minute) * 60;
                break;
            case 2:
                digitals[digitals.MaxIndex()] += (((year * 12 + month) * 30 + day) * 24 + hour) * 60;
                break;
            case 3:
                digitals[digitals.MaxIndex()] += ((year * 12 + month) * 30 + day) * 24;
                break;
            case 4:
                digitals[digitals.MaxIndex()] += (year * 12 + month) * 30;
                break;
            case 5:
                digitals[digitals.MaxIndex()] += year * 12;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 获取总毫秒数（近似值）
    /// </summary>
    public long TotalMilliseconds => _accumulatedTicks;

    /// <summary>
    /// 获取简化的时间字符串（适合显示）
    /// </summary>
    public string GetTrimmedTimeString()
    {
        if (year > 0)
            return $"{year}:{month}:{day}:{hour:00}:{minute:00}:{second:00}";
        else if (month > 0)
            return $"{month}:{day}:{hour:00}:{minute:00}:{second:00}";
        else if (day > 0)
            return $"{day}:{hour:00}:{minute:00}:{second:00}";
        else if (hour > 0)
            return $"{hour:00}:{minute:00}:{second:00}";
        else if (minute > 0)
            return $"{minute:00}:{second:00}:{millisecond:000}";
        else if(second > 0)
            return $"{second:00}:{millisecond:000}";
        else
            return $"{millisecond}";
    }
    
    public void GetTrimmedTimeString(out string str)
    {
        str = GetTrimmedTimeString();
    }

    /// <summary>
    /// 获取简化的时间字符串（适合显示）
    /// </summary>
    public string GetTrimmedDeltaSystemTimeString()
    {
        var dt = DateTime.Now - safeStartTime;

        int millisecond = dt.Milliseconds, second = dt.Seconds, minute = dt.Minutes, hour = dt.Hours;
        int day = NumberUtils.Mod(dt.Days, 30);
        int month = NumberUtils.Mod(dt.Days, 360) / 30;
        int year = dt.Days / 360;

        if (year > 0)
            return $"{year}:{month}:{day}:{hour:00}:{minute:00}:{second:00}";
        else if (month > 0)
            return $"{month}:{day}:{hour:00}:{minute:00}:{second:00}";
        else if (day > 0)
            return $"{day}:{hour:00}:{minute:00}:{second:00}";
        else if (hour > 0)
            return $"{hour:00}:{minute:00}:{second:00}";
        else if (minute > 0)
            return $"{minute:00}:{second:00}:{millisecond:000}";
        else if (second > 0)
            return $"{second:00}:{millisecond:000}";
        else
            return $"{millisecond}";
    }

    public void GetTrimmedDeltaSystemTimeString(out string str)
    {
        str = GetTrimmedDeltaSystemTimeString();
    }

}
