using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public enum ExpectedResetState
{
    False = 0,
    True = 1,
    ReversedActive = 2,
}

/// <summary>
/// For "Serial" label, there must be a "serialHolder" data in CustomData
/// </summary>
public enum Labels
{
    Serial = 0,
}

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
        orig(self);

        // Remove temporary flags
        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            if (item.Value.Temporary)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.DefineResetState());
                Md.SaveData.ChroniaFlags.SafeRemove(item.Key);
            }
        }

        // Apply global flags
        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            if (item.Value.Global)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.Active);
            }
        }
    }

    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader)
    {
        orig(self, intro, fromLoader);

        // Apply global flags
        foreach(var item in Md.SaveData.ChroniaFlags)
        {
            if (item.Value.Global && !item.Value.Temporary)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.Active);
            }
        }
    }

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        foreach (var item in Md.SaveData.ChroniaFlags)
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
                    Md.SaveData.ChroniaFlags.SafeRemove(item.Key);
                }
            }

            if (item.Value.Force)
            {
                MapProcessor.session.SetFlag(item.Key, item.Value.Active);
            }
        }
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);

        if (Md.SaveData.IsNotNull())
        {
            foreach (var item in Md.SaveData.ChroniaFlags)
            {
                // Security Check
                item.Value.ChroniaFlagDataCheck();

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
                        Md.SaveData.ChroniaFlags.SafeRemove(item.Key);
                    }

                    if (item.Value.Force)
                    {
                        MaP.session.SetFlag(item.Key, item.Value.Active);
                    }
                }
            }
        }
    }
    
    public bool Active { get; set; } = false;
    public bool Global { get; set; } = false;
    public bool Temporary { get; set; } = false;
    public bool Force { get; set; } = false;
    public float Timed { get; set; } = -1f;
    
    public ExpectedResetState DefaultResetState { get; set; } = 0;
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> CustomData { get; set; } = new();
    
    public List<Labels> PresetTags { get; set; } = new();

    public ChroniaFlag() { }

    public ChroniaFlag(bool active = true, bool global = false, bool temporary = false,
        float timed = -1f)
    {
        Active = active;
        Global = global;
        Temporary = temporary;
        Timed = timed;
    }

    public bool IsNormalFlag()
    {
        return Tags.Count == 0 && PresetTags.Count == 0 && CustomData.Count == 0;
    }

    public bool Using()
    {
        return Active || Global || Temporary || Force || Timed >= 0f;
    }

    public bool DefineResetState()
    {
        switch ((ExpectedResetState)DefaultResetState)
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

    public void ChroniaFlagDataCheck()
    {
        if(PresetTags.Contains(Labels.Serial) && !CustomData.ContainsKey("serialHolder", false))
        {
            PresetTags.SafeRemove(Labels.Serial);
        }
    }
}

