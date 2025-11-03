using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;

namespace ChroniaHelper.Utils.ChroniaSystem;

public static class ChroniaFlagUtils
{
    public static bool GetFlag(this string name)
    {
        return MaP.level.Session.GetFlag(name);
    }
    public static void SetFlag(this string name, bool active)
    {
        if ((name.GetSensitivity() & Sens.AllowNoSetFlag) != 0) { return; }
        MaP.level?.Session.SetFlag(name, active);
    }

    public static void SetFlag(this string name, bool active, bool global)
    {
        name.SetFlag(active);

        if (global)
        {
            if (active)
            {
                Md.SaveData.flags.Add(name);
            }
            else
            {
                Md.SaveData.flags.SafeRemove(name);
            }
        }
    }

    public static void SetFlag(this string name, bool active, bool global, bool temporary)
    {
        name.SetFlag(active);
        
        if (temporary)
        {
            if (active)
            {
                Md.Session.flagsPerDeath.Add(name);
            }
            else
            {
                Md.Session.flagsPerDeath.SafeRemove(name);
            }
        }
        else if (global)
        {
            if (active)
            {
                Md.SaveData.flags.Add(name);
            }
            else
            {
                Md.SaveData.flags.SafeRemove(name);
            }
        }
    }

    public static void FlagRefresh()
    {
        HashSet<string> removing = new();

        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            if (!item.Value.HasCustomData && !item.Value.HasCustomState)
            {
                removing.Add(item.Key);
            }
            
            if((item.Key.GetSensitivity() & Sens.AllowNoRegister) != 0)
            {
                removing.Add(item.Key);
            }
        }

        removing.EachDo((i) =>
        {
            Md.SaveData.ChroniaFlags.SafeRemove(i);
        });
    }

    public static void SetFlag(this string name, bool active)
    {
        if ((name.GetSensitivity() & Sens.AllowNoSetFlag) != 0) { return; }
        MaP.session.SetFlag(name, active);
    }

    public static void SetFlag(this string name, bool active, bool global)
    {
        var flag = name.PullFlag();
        flag.Active = active;
        flag.Global = global;
        flag.PushFlag(name);
    }

    public static void SetFlag(this string name, bool active, bool global, bool temporary)
    {
        var flag = name.PullFlag();
        flag.Active = active;
        flag.Global = global;
        flag.ResetOnDeath = temporary;
        flag.PushFlag(name);
    }

    public static void SetFlag(this string[] list, bool active)
    {
        foreach(var item in list)
        {
            item.SetFlag(active);
        }
    }
    public static void SetFlag(this string[] list, bool active, bool global)
    {
        foreach (var item in list)
        {
            item.SetFlag(active, global);
        }
    }
    public static void SetFlag(this string[] list, bool active, bool global, bool temporary)
    {
        foreach (var item in list)
        {
            item.SetFlag(active, global, temporary);
        }
    }

    public static void SetGlobalFlag(this string name, bool active, bool temporary = false)
    {
        name.SetFlag(active, true, temporary);
    }

    public static void SetTemporaryFlag(this string name, bool active)
    {
        name.SetFlag(active, false, true);
    }
    
    public static void SetFlag(this ICollection<string> source, bool state)
    {
        foreach (var item in source)
        {
            item.SetFlag(state);
        }
    }

    public static void SetFlag<Type>(this ICollection<Type> source, Func<Type, string> translator, bool state)
    {
        foreach(var item in source)
        {
            translator(item).SetFlag(state);
        }
    }

    public static void SetFlag<Type>(this ICollection<Type> source, Func<Type, string> createFlag, Func<Type, bool> getState)
    {
        foreach (var entry in source)
        {
            createFlag(entry).SetFlag(getState(entry));
        }
    }
    
    public static bool GetConditionalInvertedFlag(this string name, bool invertIndicator = false)
    {
        return invertIndicator? !MaP.level.Session.GetFlag(name) : MaP.level.Session.GetFlag(name);
    }
    
    public static void LogChroniaFlags()
    {
        Log.Error("=====================================");
        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            Log.Info($"==============flag: ", item.Key, "===================");
            Log.Info($"active: {item.Value.Active}");
            Log.Info($"global: {item.Value.Global}");
            Log.Info($"temporary: {item.Value.ResetOnDeath}");
            Log.Info($"force: {item.Value.Force}");
            Log.Info($"timer: {item.Value.Timed}");
            Log.Info($"resetTo: {item.Value.ResetTo}");
            Log.Info("tags:");
            Log.Each(item.Value.Tags);
            Log.Info("custom data: ");
            Log.Each(item.Value.CustomData);
            Log.Info("preset tags: ");
            Log.Each(item.Value.PresetTags);
        }
    }
}
