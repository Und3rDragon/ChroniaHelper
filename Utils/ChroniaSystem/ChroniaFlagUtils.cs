using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.ChroniaSystem;

public static class ChroniaFlagUtils
{
    public static bool GetFlag(this string name)
    {
        return MaP.level?.Session?.GetFlag(name) ?? false;
    }

    public static void SetFlag(this string name, bool active = false, 
        bool global = false, bool perRoom = false, bool perDeath = false)
    {
        if ((name.GetSensitivity() & Sens.AllowNoSetFlag) != 0) { return; }
        
        MaP.level?.Session?.SetFlag(name, active);
        
        if (perDeath)
        {
            if (active)
            {
                Md.Session.flagsPerDeath.Add(name);
            }
            else
            {
                Md.Session.flagsPerDeath.Remove(name);
            }
        }
        
        if (perRoom)
        {
            if (active)
            {
                Md.Session.flagsPerRoom.Add(name);
            }
            else
            {
                Md.Session.flagsPerRoom.Remove(name);
            }
        }
        
        if (global && !perDeath && !perRoom)
        {
            if (active)
            {
                Md.SaveData.flags.Add(name);
            }
            else
            {
                Md.SaveData.flags.Remove(name);
            }
        }
    }

    public static bool GetConditionalInvertedFlag(this string name, Func<string, bool> invertCheck)
    {
        return invertCheck(name)? !name.GetFlag() : MaP.level.Session.GetFlag(name);
    }
    
    public static bool GetConditionalInvertedFlag(this string name, Func<string, bool> invertCheck, Func<string, string> invertParser)
    {
        return invertCheck(name) ? !invertParser(name).GetFlag() : MaP.level.Session.GetFlag(name);
    }
    
    public static bool GetGeneralInvertedFlag(this string name)
    {
        return name.GetConditionalInvertedFlag(
            (flag) => flag.StartsWith('!'),
            (flag) => flag.TrimStart('!')
            );
    }

    public static void SetGeneralFlags(this string flags, string separator = ",", 
        string invert = "!", string global = "*", string perDeath = "#", 
        string perRoom = "$", bool flip = false)
    {
        flags.Split(separator, StringSplitOptions.TrimEntries).ApplyTo(out string[] list);

        foreach(var item in list)
        {
            item.SetGeneralFlag(invert, global, perDeath, perRoom, flip);
        }
    }

    public static void SetGeneralFlags(this string[] flags, string invert = "!", 
        string global = "*", string perDeath = "#", 
        string perRoom = "$", bool flip = false)
    {
        foreach (var item in flags)
        {
            item.SetGeneralFlag(invert, global, perDeath, perRoom, flip);
        }
    }

    public static void SetGeneralFlag(this string flag, string invert = "!", 
        string global = "*", string perDeath = "#", string perRoom = "$", 
        bool flip = false)
    {
        bool _invert = flag.Contains(invert);
        bool _global = flag.Contains(global);
        bool _perDeath = flag.Contains(perDeath);
        bool _perRoom = flag.Contains(perRoom);
        string name = flag.RemoveAll(invert).RemoveAll(global).RemoveAll(perDeath)
            .RemoveAll(perRoom);

        name.SetFlag(flip ? _invert : !_invert, _global, _perRoom, _perDeath);
    }

    public static void ToggleGeneralFlags(this string flags, string separator = ",", string global = "*", string temporary = "#")
    {
        flags.Split(separator, StringSplitOptions.TrimEntries).ApplyTo(out string[] list);

        foreach (var item in list)
        {
            item.ToggleGeneralFlag(global, temporary);
        }
    }

    public static void ToggleGeneralFlags(this string[] flags, string global = "*", string perDeath = "#", string perRoom = "$")
    {
        foreach (var item in flags)
        {
            item.ToggleGeneralFlag(global, perDeath);
        }
    }

    public static void ToggleGeneralFlag(this string flag, string global = "*", string perDeath = "#", string perRoom = "$")
    {
        bool _global = flag.Contains(global);
        bool _perDeath = flag.Contains(perDeath);
        bool _perRoom = flag.Contains(perRoom);
        string name = flag.RemoveAll(global).RemoveAll(perDeath).RemoveAll(perRoom);

        name.SetFlag(!name.GetFlag(), _global, _perDeath, _perRoom);
    }

    public static bool GetGeneralFlags(this string flags, string separator = ",", string invert = "!")
    {
        if (string.IsNullOrEmpty(flags)) { return false; }
        
        flags.Split(separator, StringSplitOptions.TrimEntries).ApplyTo(out string[] f);

        bool r = true;
        foreach(var i in f)
        {
            r.TryNegative(i.GetConditionalInvertedFlag((s) => s.Contains(invert), (s) => s.RemoveAll(invert)));
        }

        return r;
    }

}
