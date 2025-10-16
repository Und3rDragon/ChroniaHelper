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
}
