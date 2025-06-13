using System;
using System.Collections.Generic;
using System.Linq;
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

    public static bool GetFlag(this string name, bool checkRecordState = false)
    {
        return checkRecordState? 
            (Check(name) ? ChroniaHelperSaveData.ChroniaFlags[name].Active : 
                MapProcessor.session.GetFlag(name)
            ) : MapProcessor.session.GetFlag(name);
    }

    public static bool IsGlobal(this string name)
    {
        return Check(name) ? ChroniaHelperSaveData.ChroniaFlags[name].Global : false;
    }

    public static void SetGlobal(this string name, bool set)
    {
        if (Check(name))
        {
            ChroniaHelperSaveData.ChroniaFlags[name].Global = set;
        }
    }

    public static bool IsTemporary(this string name)
    {
        return Check(name) ? ChroniaHelperSaveData.ChroniaFlags[name].Temporary : false;
    }

    public static void SetTemporary(this string name, bool set)
    {
        if (Check(name))
        {
            ChroniaHelperSaveData.ChroniaFlags[name].Temporary = set;
        }
    }

    public static void ForceRefresh(this string name, bool set)
    {
        if (Check(name))
        {
            ChroniaHelperSaveData.ChroniaFlags[name].Force = set;
        }
    }

    public static bool Check(this string name, bool fix = false)
    {
        GeneralCheck(fix);

        if (!ChroniaHelperSaveData.ChroniaFlags.ContainsKey(name)) { return false; }

        if (ChroniaHelperSaveData.ChroniaFlags[name].Name != name) { return false; }

        return true;
    }

    public static bool GeneralCheck(bool autoFix = false)
    {
        foreach(var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            if (item.Key != item.Value.Name)
            {
                if (autoFix)
                {
                    ChroniaHelperSaveData.ChroniaFlags[item.Key].Name = item.Key;
                }
                else { return false; }
            }
        }

        return true;
    }

    public static void Refresh()
    {
        GeneralCheck(true);
        foreach(var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            if(!item.Value.Active && !item.Value.Global)
            {
                ChroniaHelperSaveData.ChroniaFlags.SafeRemove(item.Key);
            }
        }
    }
}
