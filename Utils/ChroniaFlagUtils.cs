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
    public static void SetFlag(this string name, bool active = true, bool global = false, bool temporary = false)
    {
        ChroniaHelperSaveData.ChroniaFlags.Enter(name, new (name, active, global, temporary));
        MapProcessor.session.SetFlag(name, active);
        Refresh();
    }

    public static void SetGlobalFlag(this string name, bool active, bool temporary = false)
    {
        name.SetFlag(active, true, temporary);
    }

    public static void SetTemporaryFlag(this string name, bool active)
    {
        name.SetFlag(active, false, true);
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
                ChroniaHelperSaveData.ChroniaFlags[name].Active : false;
        }
        else
        {
            return MapProcessor.session.GetFlag(name);
        }
    }

    public static bool Check(this string name)
    {
        return ChroniaHelperSaveData.ChroniaFlags.ContainsKey(name);
    }
    
    /// <summary>
    /// Search through the savedata ChroniaFlags, and pull the item out
    /// </summary>
    /// <param name="name"></param>
    /// <returns>By default, you'll get the item stored in ChroniaFlags. If the item doesn't exist, you'll get an empty ChroniaFlag (name, false, false, false)</returns>
    public static ChroniaFlag PullFlag(this string name)
    {
        if (name.Check())
        {
            return ChroniaHelperSaveData.ChroniaFlags[name];
        }
        else
        {
            return new(name, false, false, false);
        }
    }

    /// <summary>
    /// The function is only for processing data, for flag managements please use SetFlag()
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="slotName"></param>
    public static void PushFlag(this ChroniaFlag flag)
    {
        ChroniaHelperSaveData.ChroniaFlags.Enter(flag.Name, flag);
        MapProcessor.session.SetFlag(flag.Name, flag.Active);
        Refresh();
    }

    public static void Refresh()
    {
        foreach(var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            if(!item.Value.Active && !item.Value.Global)
            {
                ChroniaHelperSaveData.ChroniaFlags.SafeRemove(item.Key);
            }
        }
    }
}
