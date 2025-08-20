using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.ChroniaSystem;

public static class ChroniaCounterUtils
{
    public static bool CheckCounter(this string name)
    {
        if (MaP.session == null) { return false; }

        foreach(var item in MaP.session.Counters)
        {
            if(item.Key == name) { return true; }
        }

        return false;
    }

    public static bool CheckCounterRecord(this string name)
    {
        return Md.SaveData.ChroniaCounters.ContainsKey(name);
    }

    public static ChroniaCounter PullCounter(this string name)
    {
        if (!Md.SaveData.ChroniaCounters.ContainsKey(name))
        {
            return new();
        }

        return Md.SaveData.ChroniaCounters[name];
    }

    public static void PushCounter(this ChroniaCounter counter, string name)
    {
        Md.SaveData.ChroniaCounters.Enter(name, counter);
        counter.SetCounter(name);
    }

    public static void ResetCounter(this string name)
    {
        if (name.CheckCounterRecord())
        {
            name.PullCounter().Reset();
            name.PullCounter().SetCounter(name);
        }
    }
}
