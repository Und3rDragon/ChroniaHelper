using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public class ChroniaFlag
{
    public static void Onload()
    {
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.Level.LoadLevel += OnLoadLevel;
        On.Celeste.Level.Update += OnLevelUpdate;
        On.Monocle.Scene.Update += GlobalUpdate;
    }

    public static void Unload()
    {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Celeste.Level.Update -= OnLevelUpdate;
        On.Monocle.Scene.Update -= GlobalUpdate;
    }

    public static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        // Remove temporary flags
        foreach (var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            if (item.Value.Temporary)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.DefineResetState());
                ChroniaHelperSaveData.ChroniaFlags.SafeRemove(item.Key);
            }
        }

        // Apply global flags
        foreach (var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            if (item.Value.Global)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.Active);
            }
        }

        orig(self);
    }

    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader)
    {
        // Apply global flags
        foreach(var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            if (item.Value.Global && !item.Value.Temporary)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.Active);
            }
        }

        orig(self, intro, fromLoader);
    }

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        foreach(var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            // Non-global stuffs only
            if (!item.Value.Global)
            {
                if (item.Value.Timed > 0f)
                {
                    item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
                }
                else if (item.Value.Timed == 0f)
                {
                    MapProcessor.session.SetFlag(item.Key, item.Value.DefineResetState());
                    ChroniaHelperSaveData.ChroniaFlags.SafeRemove(item.Key);
                }
            }

            if (item.Value.Force)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.Active);
            }
        }
        orig(self);
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        foreach(var item in ChroniaHelperSaveData.ChroniaFlags)
        {
            // Global stuffs only
            if (item.Value.Global)
            {
                if (item.Value.Timed > 0f)
                {
                    item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
                }
                else if (item.Value.Timed == 0f)
                {
                    MapProcessor.session.SetFlag(item.Key, item.Value.DefineResetState());
                    ChroniaHelperSaveData.ChroniaFlags.SafeRemove(item.Key);
                }
            }

            // Global Force Flags have no effect yet
            
        }
        orig(self);
    }


    public string Name { get; set; }
    public bool Active { get; set; } = false;
    public bool Global { get; set; } = false;
    public bool Temporary { get; set; } = false;
    public bool Force { get; set; } = false;
    public float Timed { get; set; } = -1f;
    public enum ExpectedResetState { False, True, ReversedActive }
    public ExpectedResetState DefaultResetState { get; set; } = (ExpectedResetState)0;

    public ChroniaFlag (string name)
    {
        Name = name;
    }
    public ChroniaFlag(string name, bool active = true, bool global = false, bool temporary = false,
        float timed = -1f)
    {
        Name = name;
        Active = active;
        Global = global;
        Temporary = temporary;
        Timed = timed;
    }

    public void SetFlag()
    {
        ChroniaHelperSaveData.ChroniaFlags.Enter(Name, this);
        MapProcessor.session.SetFlag(Name, Active);
        ChroniaFlagUtils.Refresh();
    }

    public bool DefineResetState()
    {
        switch (DefaultResetState)
        {
            case ExpectedResetState.False:
                return false;
            case ExpectedResetState.True: 
                return true;
            case ExpectedResetState.ReversedActive:
                return !Active;
            default:
                return false;
        }
    }
}
