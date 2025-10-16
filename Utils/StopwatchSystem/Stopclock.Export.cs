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
        $"{year:000}Years {month:00}Months {day:00}Days {hour:00}:{minute:00}:{second:00}.{millisecond:000}";
    
    public string DigitalTime =>
        $"{year:000}:{month:00}:{day:00}:{hour:00}:{minute:00}:{second:00}:{millisecond:000}";
    
    public string Digitals =>
        $"{year:000}{month:00}{day:00}{hour:00}{minute:00}{second:00}{millisecond:000}";

    /// <summary>
    /// 将计时器数字输出成整数组
    /// </summary>
    /// <param name="digitals">输出整数组</param>
    /// <param name="trim">是否去除多余的前置0</param>
    /// <param name="reverse">从ms的最后一位开始逆序排列</param>
    public void GetDigitals(out int[] digitals, bool trim = false, bool reverse = true)
    {
        string d = Digitals;
        if (trim) { d = d.TrimStart('0'); }

        digitals = new int[d.Length];
        for(int i = 0; i < d.Length; i++)
        {
            $"{(reverse ? d[d.Length - 1 - i] : d[i])}".ParseInt(out digitals[i], 0);
        }
    }

    /// <summary>
    /// 获取总毫秒数（近似值）
    /// </summary>
    public long TotalMilliseconds => _accumulatedTicks;

    /// <summary>
    /// 获取简化的时间字符串（适合显示）
    /// </summary>
    public string GetSimpleTimeString()
    {
        if (year > 0)
            return $"{year}年{month}月{day}天 {hour:00}:{minute:00}:{second:00}";
        else if (month > 0)
            return $"{month}月{day}天 {hour:00}:{minute:00}:{second:00}";
        else if (day > 0)
            return $"{day}天 {hour:00}:{minute:00}:{second:00}";
        else if (hour > 0)
            return $"{hour:00}:{minute:00}:{second:00}";
        else
            return $"{minute:00}:{second:00}.{millisecond:000}";
    }

}
