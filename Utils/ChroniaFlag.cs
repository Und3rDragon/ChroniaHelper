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
                MapProcessor.session.SetFlag(item.Key, false);
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
            if (item.Value.Force)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.Active);
            }
            //Log.Info("[key]", item.Key, "[name]", item.Value.Name, "[active]", item.Value.Active, "[global]", item.Value.Global,
            //    "[temporary]", item.Value.Temporary, "[force]", item.Value.Force);
        }
        //Log.Info("_________________________");
        orig(self);
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);
    }


    public string Name { get; set; }
    public bool Active { get; set; } = false;
    public bool Global { get; set; } = false;
    public bool Temporary { get; set; } = false;
    public bool Force { get; set; } = false;

    public ChroniaFlag(string name, bool active = true, bool global = false, bool temporary = false)
    {
        Name = name;
        Active = active;
        Global = global;
        Temporary = temporary;
    }

    public void SetFlag()
    {
        ChroniaHelperSaveData.ChroniaFlags.Enter(Name, this);
        MapProcessor.session.SetFlag(Name, Active);
        ChroniaFlagUtils.Refresh();
    }
}
