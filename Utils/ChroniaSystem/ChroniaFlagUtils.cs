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
    public static bool CheckFlag(this string flag)
    {
        return Md.SaveData.ChroniaFlags.ContainsKey(flag);
    }

    public static bool CheckFlagTag(this string flag, string tag)
    {
        if (!flag.CheckFlag())
        {
            return false;
        }
        
        return Md.SaveData.ChroniaFlags[flag].Tags.Contains(tag);
    }

    public static bool CheckFlagCustomData(this string flag, string dataName)
    {
        if (!flag.CheckFlag()) { return false; }

        return Md.SaveData.ChroniaFlags[flag].CustomData.ContainsKey(dataName);
    }

    public static string GetFlagCustomData(this string flag, string dataName)
    {
        if (!flag.CheckFlagCustomData(dataName))
        {
            return string.Empty;
        }

        return Md.SaveData.ChroniaFlags[flag].CustomData[dataName];
    }

    public static bool CheckFlagPresetTag(this string flag, Labels label)
    {
        if (!flag.CheckFlag()) { return false; }
        
        return Md.SaveData.ChroniaFlags[flag].PresetTags.Contains(label);
    }

    /// <summary>
    /// Search through the savedata ChroniaFlags, and pull the item out
    /// </summary>
    /// <param name="name"></param>
    /// <returns>By default, you'll get the item stored in ChroniaFlags. If the item doesn't exist, you'll get an empty ChroniaFlag (name, false, false, false)</returns>
    public static ChroniaFlag PullFlag(this string name)
    {
        ChroniaFlag flag = new()
        {
            Active = name.GetFlag()
        };

        Md.SaveData.ChroniaFlags.Create(name, flag);

        return Md.SaveData.ChroniaFlags[name];
    }

    /// <summary>
    /// The function is only for processing data, for flag managements please use SetFlag()
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="slotName"></param>
    public static void PushFlag(this ChroniaFlag flag, string name)
    {
        if ((flag.HasCustomData || flag.HasCustomState) && ((name.GetSensitivity() & Sens.AllowNoRegister) == 0))
        {
            Md.SaveData.ChroniaFlags.Enter(name, flag);
        }
        if((name.GetSensitivity() & Sens.AllowNoSetFlag) == 0)
        {
            name.SetFlag(flag.Active);
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

    public static void SetTimedFlag(this string name, bool basicState, float timer, bool global = false, bool temporary = false)
    {
        var flag = name.PullFlag();
        flag.Active = basicState;
        flag.Global = global;
        flag.ResetOnDeath = temporary;
        flag.ResetTimer(timer);
        flag.PushFlag(name);
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

    /// <summary>
    /// Get the state of the required flag
    /// </summary>
    /// <param name="name"></param>
    /// <param name="checkRecordState"></param>
    /// <returns>The state of the flag in-game, or in the records</returns>
    public static bool GetFlag(this string name, bool checkRecordState = false)
    {
        if (checkRecordState)
        {
            return name.PullFlag().Active;
        }
        else
        {
            return MaP.session.GetFlag(name);
        }
    }
    public static bool GetConditionalInvertedFlag(this string name, bool invertIndicator = false)
    {
        return invertIndicator? !MaP.session.GetFlag(name) : MaP.session.GetFlag(name);
    }
}
