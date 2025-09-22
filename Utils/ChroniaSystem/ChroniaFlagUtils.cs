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
        if (!name.CheckFlag())
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
        if (!flag.IsNormalFlag())
        {
            Md.SaveData.ChroniaFlags.Enter(name, flag);
        }
        name.SetFlag(flag.Active);
        FlagRefresh();
    }

    public static void FlagRefresh()
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
        MaP.session.SetFlag(name, active);
    }

    public static void SetFlag(this string name, bool active, bool global)
    {
        name.PullFlag().Active = active;
        name.PullFlag().Global = global;
        name.SetFlag(active);

        FlagRefresh();
    }

    public static void SetFlag(this string name, bool active, bool global, bool temporary)
    {
        name.PullFlag().Active = active;
        name.PullFlag().Global = global;
        name.PullFlag().Temporary = temporary;
        name.SetFlag(active);

        FlagRefresh();
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
        name.PullFlag().Active = basicState;
        name.PullFlag().Global = global;
        name.PullFlag().Temporary = temporary;
        name.PullFlag().Timed = timer;
        name.SetFlag(basicState);

        FlagRefresh();
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
            return name.CheckFlag() ?
                Md.SaveData.ChroniaFlags[name].Active : false;
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
