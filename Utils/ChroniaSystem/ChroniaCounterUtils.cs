using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace ChroniaHelper.Utils.ChroniaSystem;

public static class ChroniaCounterUtils
{
    public static bool CheckCounter(this string name)
    {
        if (MaP.level.Session == null) { return false; }

        foreach(var item in MaP.level.Session.Counters)
        {
            if(item.Key == name) { return true; }
        }

        return false;
    }
    
    public static int GetCounter(this string name)
    {
        return MaP.level.Session.GetCounter(name);
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
