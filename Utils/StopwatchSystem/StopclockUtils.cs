using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

public static class StopclockUtils
{
    public static bool GetStopclock(this string name, out Stopclock clock)
    {
        if (Md.SaveData.stopclocks.ContainsKey(name))
        {
            clock = Md.SaveData.stopclocks[name];
            return true;
        }

        if (Md.Session.Stopclocks.ContainsKey(name))
        {
            clock = Md.Session.Stopclocks[name];
            return true;
        }

        clock = new Stopclock();
        return false;
    }

    public static bool GetStopclock(this string name, bool fromGlobal, out Stopclock clock)
    {
        if (fromGlobal && Md.SaveData.stopclocks.ContainsKey(name))
        {
            clock = Md.SaveData.stopclocks[name];
            return true;
        }

        if (!fromGlobal && Md.Session.Stopclocks.ContainsKey(name))
        {
            clock = Md.Session.Stopclocks[name];
            return true;
        }

        clock = new Stopclock();
        return false;
    }

    /// <summary>
    /// 将xxx:xxx:xxx转换为int[]
    /// </summary>
    /// <param name="str"></param>
    /// <param name="reverse">将ms作为第一项</param>
    /// <returns></returns>
    public static int[] TimeToDigitals(this string str, bool reverse = true)
    {
        str.Split(':', StringSplitOptions.TrimEntries).ApplyTo(out string[] t);

        int[] n = new int[t.Length];
        for(int i = 0; i < t.Length; i++)
        {
            if (reverse)
            {
                t[t.Length - 1 - i].TrimStart('0').ParseInt(out n[i], 0);
            }
            else
            {
                t[i].TrimStart('0').ParseInt(out n[i], 0);
            }
        }

        return n;
    }

    public static string TrimLeadingZeroUnits(string timeStr)
    {
        if (string.IsNullOrEmpty(timeStr))
            return "0"; // 或抛异常，根据需求

        var units = timeStr.Split(':');

        // 找到第一个“非全零”单位的索引
        int firstNonZeroIndex = -1;
        for (int i = 0; i < units.Length; i++)
        {
            string unit = units[i];
            // 判断是否全由 '0' 组成（且非空）
            if (!string.IsNullOrEmpty(unit) && unit.All(c => c == '0'))
            {
                continue; // 是全零，跳过
            }
            else
            {
                firstNonZeroIndex = i;
                break;
            }
        }

        // 如果全是全零单位
        if (firstNonZeroIndex == -1)
        {
            return "0";
        }

        // 从第一个非全零单位开始，保留剩余所有部分
        return string.Join(":", units.Skip(firstNonZeroIndex));
    }
}
