using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.ChroniaSystem;

public static class ChroniaCounterUtils
{
    public static void CounterRefresh()
    {
        HashSet<string> removing = new();

        Md.SaveData.ChroniaCounters.EachDo((c) =>
        {
            if (!c.Value.Operating())
            {
                removing.Add(c.Key);
            }
        });

        removing.EachDo((i) =>
        {
            Md.SaveData.ChroniaCounters.SafeRemove(i);
        });
    }
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
        Md.SaveData.ChroniaCounters.Create(name, new());

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

    public static int GetCounter(this string name)
    {
        return MaP.session.GetCounter(name);
    }

    public static void SetCounter(this string name, int value)
    {
        MaP.level.Session.SetCounter(name, value);
    }

    public static void SetCounter(this ICollection<string> source, int state)
    {
        foreach (var item in source)
        {
            item.SetCounter(state);
        }
    }

    public static void SetCounter<Type>(this ICollection<Type> source, Func<Type, string> translator, int state)
    {
        foreach (var item in source)
        {
            translator(item).SetCounter(state);
        }
    }

    public static void SetCounter<Type>(this ICollection<Type> source, Func<Type, string> createItem, Func<Type, int> getState)
    {
        foreach (var entry in source)
        {
            createItem(entry).SetCounter(getState(entry));
        }
    }
}
