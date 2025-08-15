using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;

namespace ChroniaHelper.Utils;

public static class ChroniaFlagUtils
{
    public static bool Check(this string flag)
    {
        return Md.SaveData.ChroniaFlags.ContainsKey(flag);
    }

    public static bool CheckTag(this string flag, string tag)
    {
        if (!flag.Check())
        {
            return false;
        }
        
        return Md.SaveData.ChroniaFlags[flag].Tags.Contains(tag);
    }

    public static bool CheckCustomData(this string flag, string dataName)
    {
        if (!flag.Check()) { return false; }

        return Md.SaveData.ChroniaFlags[flag].CustomData.ContainsKey(dataName);
    }

    public static string GetCustomData(this string flag, string dataName)
    {
        if (!flag.CheckCustomData(dataName))
        {
            return string.Empty;
        }

        return Md.SaveData.ChroniaFlags[flag].CustomData[dataName];
    }

    public static bool CheckPresetTag(this string flag, Labels label)
    {
        if (!flag.Check()) { return false; }
        
        return Md.SaveData.ChroniaFlags[flag].PresetTags.Contains(label);
    }

    /// <summary>
    /// Search through the savedata ChroniaFlags, and pull the item out
    /// </summary>
    /// <param name="name"></param>
    /// <returns>By default, you'll get the item stored in ChroniaFlags. If the item doesn't exist, you'll get an empty ChroniaFlag (name, false, false, false)</returns>
    public static ChroniaFlag PullFlag(this string name)
    {
        if (!name.Check())
        {
            Md.SaveData.ChroniaFlags.Enter(name, new());
            //return new();
        }

        return Md.SaveData.ChroniaFlags[name];
    }

    /// <summary>
    /// The function is only for processing data, for flag managements please use SetFlag()
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="slotName"></param>
    public static void PushFlag(this ChroniaFlag flag, string name)
    {
        Md.SaveData.ChroniaFlags.Enter(name, flag);
        MaP.session.SetFlag(name, flag.Active);
        Refresh();
    }

    public static void Refresh()
    {
        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            if (!item.Value.IsCustomFlag() && item.Value.IsNormalFlag())
            {
                Md.SaveData.ChroniaFlags.SafeRemove(item.Key);
            }
        }
    }

    public static void SetFlag(this string name, bool active)
    {
        //ChroniaFlag flag = name.PullFlag();
        //flag.Active = active;
        //flag.PushFlag(name);
        name.PullFlag().Active = active;
        MaP.session.SetFlag(name, active);

        Refresh();
    }

    public static void SetFlag(this string name, bool active, bool global)
    {
        //ChroniaFlag flag = name.PullFlag();
        //flag.Global = global;
        //flag.Active = active;
        //flag.PushFlag(name);
        name.PullFlag().Active = active;
        name.PullFlag().Global = global;
        MaP.session.SetFlag(name, active);

        Refresh();
    }

    public static void SetFlag(this string name, bool active, bool global, bool temporary)
    {
        //ChroniaFlag flag = name.PullFlag();
        //flag.Active = active;
        //flag.Global = global;
        //flag.Temporary = temporary;
        //flag.PushFlag(name);
        name.PullFlag().Active = active;
        name.PullFlag().Global = global;
        name.PullFlag().Temporary = temporary;
        MaP.session.SetFlag(name, active);

        Refresh();
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
        //ChroniaFlag flag = name.PullFlag();
        //flag.Active = basicState;
        //flag.Global = global;
        //flag.Temporary = temporary;
        //flag.Timed = timer;
        //flag.PushFlag(name);
        name.PullFlag().Active = basicState;
        name.PullFlag().Global = global;
        name.PullFlag().Temporary = temporary;
        name.PullFlag().Timed = timer;
        MaP.session.SetFlag(name, basicState);

        Refresh();
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
            return name.Check() ?
                Md.SaveData.ChroniaFlags[name].Active : false;
        }
        else
        {
            return MaP.session.GetFlag(name);
        }
    }
}
