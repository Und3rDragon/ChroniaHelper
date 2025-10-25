using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils.ChroniaSystem;


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
    [LoadHook]
    public static void Onload()
    {
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.Level.LoadLevel += OnLoadLevel;
        On.Celeste.Level.Update += OnLevelUpdate;
        On.Monocle.Scene.Update += GlobalUpdate;
        On.Celeste.Level.TransitionRoutine += OnLevelTransition;
        On.Celeste.Level.Begin += OnLevelBegin;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Celeste.Level.Update -= OnLevelUpdate;
        On.Monocle.Scene.Update -= GlobalUpdate;
        On.Celeste.Level.TransitionRoutine -= OnLevelTransition;
        On.Celeste.Level.Begin -= OnLevelBegin;
    }
    
    public static void OnLevelBegin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self); // Only once when getting in

        MaP.level = self;
        
        foreach(var f in Md.SaveData.ChroniaFlags)
        {
            // initialize normal flags
            if (!f.Value.IsNormalFlag && !f.Value.IsCustomFlag)
            {
                f.Value.Active = false;
                f.Key.SetFlag(false);
            }
        }
    }

    public static IEnumerator OnLevelTransition(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData levelData, Vector2 dir)
    {
        // Remove temporary flags on transitions
        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            if (item.Value.Temporary)
            {
                item.Key.SetFlag(item.Value.ResetTo);
                Md.SaveData.ChroniaFlags.SafeRemove(item.Key);
            }
        }

        yield return new SwapImmediately(orig(self, levelData, dir)); //On transition
    }

    public static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        orig(self); // Once per reload, not on first enter, not on F5
        // After LoadLevel
    }

    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader)
    {
        orig(self, intro, fromLoader); // Once per level load, also reload
        
        // Remove temporary flags
        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            if (item.Value.Temporary)
            {
                item.Key.SetFlag(item.Value.ResetTo);
                Md.SaveData.ChroniaFlags.SafeRemove(item.Key);
            }
        }

        // Apply global flags
        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            if (item.Value.Global && !item.Value.Temporary)
            {
                item.Key.SetFlag(item.Value.Active);
            }
        }
    }

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);
        
        if (Md.SaveData.IsNull()) { return; }

        HashSet<string> toBeRemoved = new();

        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            // Refreshing
            if(item.Value.IsNormalFlag && !item.Value.IsCustomFlag)
            {
                toBeRemoved.Enter(item.Key);
                continue;
            }
            
            // Security Check
            item.Value.ChroniaFlagDataCheck();

            // Global stuffs only
            if (!item.Value.Global && !(self is Level)) { continue; }
            
            if (item.Value.Timed > 0f)
            {
                item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
            }
            else if (item.Value.Timed == 0f)
            {
                item.Key.SetFlag(item.Value.ResetTo);
                Md.SaveData.ChroniaFlags.SafeRemove(item.Key);
            }

            if (item.Value.Force)
            {
                item.Key.SetFlag(item.Value.Active);
            }
        }
        
        foreach(var item in toBeRemoved)
        {
            Md.SaveData.ChroniaFlags.SafeRemove(item);
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
    
    public bool IsNormalFlag => Tags.Count == 0 && PresetTags.Count == 0 && CustomData.Count == 0;
    public bool IsCustomFlag => Global || Temporary || Force || Timed >= 0f;
    public bool ResetTo => DefaultResetState switch 
    { 
        ExpectedResetState.False => false,
        ExpectedResetState.True => true,
        ExpectedResetState.ReversedActive => !Active,
        _ => false,
    };

    public void ChroniaFlagDataCheck()
    {
        if(PresetTags.Contains(Labels.Serial) && !CustomData.ContainsKey("serialHolder", false))
        {
            PresetTags.SafeRemove(Labels.Serial);
        }
    }
    
    public void ClearTags()
    {
        Tags = new();
    }
    
    public void ClearCustomData()
    {
        CustomData = new();
    }
    
    public void ClearPresetTags()
    {
        PresetTags = new();
    }
    
    public void ClearAll()
    {
        ClearTags();
        ClearCustomData();
        ClearPresetTags();
    }
}

