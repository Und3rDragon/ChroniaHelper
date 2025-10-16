using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

public static class StopclockUtils
{
    public static bool GetStopclock(this string name, out Stopclock? clock)
    {
        if (Md.SaveData.globalStopwatches.ContainsKey(name))
        {
            clock = Md.SaveData.globalStopwatches[name];
            return true;
        }

        if (Md.Session.sessionStopwatches.ContainsKey(name))
        {
            clock = Md.Session.sessionStopwatches[name];
            return true;
        }

        clock = null;
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
}
