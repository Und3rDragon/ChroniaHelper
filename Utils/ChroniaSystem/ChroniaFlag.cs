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
        On.Monocle.Scene.Update += GlobalUpdate;
        On.Celeste.Level.TransitionRoutine += OnLevelTransition;
        On.Celeste.Level.Begin += OnLevelBegin;
    }
    
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Monocle.Scene.Update -= GlobalUpdate;
        On.Celeste.Level.TransitionRoutine -= OnLevelTransition;
        On.Celeste.Level.Begin -= OnLevelBegin;
    }
    
    public static void OnLevelBegin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self); // Only once when getting in

        MaP.level = self;

        HashSet<string> removing = new();

        Md.SaveData.ChroniaFlags.EachDo((f) =>
        {
            // initialize normal flags
            if (!f.Value.HasCustomData && !f.Value.HasCustomState)
            {
                f.Value.Active = false;
                f.Key.SetFlag(false);
                removing.Add(f.Key);
            }
        });

        removing.EachDo((i) =>
        {
            Md.SaveData.ChroniaFlags.SafeRemove(i);
        });
    }

    public static IEnumerator OnLevelTransition(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData levelData, Vector2 dir)
    {
        HashSet<string> removing = new();

        // Remove transition flags on transitions
        Md.SaveData.ChroniaFlags.EachDo((f) =>
        {
            if (f.Value.ResetOnTransition)
            {
                f.Key.SetFlag(f.Value.ResetTo);
                if (f.Value.RemoveWhenReset)
                {
                    removing.Add(f.Key);
                }
            }
        });

        removing.EachDo((i) =>
        {
            Md.SaveData.ChroniaFlags.SafeRemove(i);
        });

        yield return new SwapImmediately(orig(self, levelData, dir)); //On transition
    }

    public static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        orig(self); // Once per reload, not on first enter, not on F5
        // After LoadLevel

        HashSet<string> removing = new();

        Md.SaveData.ChroniaFlags.EachDo((f) =>
        {
            if (f.Value.ResetOnReload)
            {
                f.Key.SetFlag(f.Value.ResetTo);
                if (f.Value.RemoveWhenReset)
                {
                    removing.Add(f.Key);
                }
            }
        });

        removing.EachDo((i) =>
        {
            Md.SaveData.ChroniaFlags.SafeRemove(i);
        });
    }

    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader)
    {
        orig(self, intro, fromLoader); // Once per level load, also reload
        
        HashSet<string> removing = new();
        
        // Remove temporary flags
        Md.SaveData.ChroniaFlags.EachDo((item) =>
        {
            if (item.Value.ResetOnDeath)
            {
                item.Key.SetFlag(item.Value.ResetTo);
                if (item.Value.RemoveWhenReset)
                {
                    removing.Add(item.Key);
                }
            }
        });

        removing.EachDo((i) =>
        {
            Md.SaveData.ChroniaFlags.SafeRemove(i);
        });

        // Apply global flags
        Md.SaveData.ChroniaFlags.EachDo((f) =>
        {
            if (f.Value.Global)
            {
                f.Key.SetFlag(true);
            }
        });
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);
        
        if (Md.SaveData.IsNull()) { return; }

        ChroniaFlagUtils.FlagRefresh();

        HashSet<string> removing = new();

        foreach (var item in Md.SaveData.ChroniaFlags)
        {
            // Security Check
            item.Value.ChroniaFlagDataCheck();

            if(self is not Level)
            {
                if (item.Value.Global && item.Value.Timed > 0f)
                {
                    item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
                }
            }
            else
            {
                if (item.Value.Force)
                {
                    item.Key.SetFlag(item.Value.Active);
                }

                if (!item.Value.Global && item.Value.Timed > 0f)
                {
                    item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
                }
            }

            if(item.Value.Timed == 0f)
            {
                item.Key.SetFlag(item.Value.ResetTo);

                if (item.Value.RemoveWhenReset)
                {
                    removing.Add(item.Key);
                }
            }
        }
        
        foreach(var item in removing)
        {
            Md.SaveData.ChroniaFlags.SafeRemove(item);
        }
    }
    
    public bool Active { get; set; } = false;
    public bool Global { get; set; } = false;
    public bool ResetOnDeath { get; set; } = false;
    public bool ResetOnTransition { get; set; } = false;
    public bool ResetOnReload { get; set; } = false;
    public bool Force { get; set; } = false;
    public float Timed { get; set; } = -1f;
    public float Orig_Timed { get; set; } = -1f;
    public bool RemoveWhenReset { get; set; } = true;
    
    public ExpectedResetState DefaultResetState { get; set; } = 0;
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> CustomData { get; set; } = new();
    
    public List<Labels> PresetTags { get; set; } = new();

    public ChroniaFlag() { }
    
    public bool HasCustomData => Tags.Count > 0 || PresetTags.Count > 0 || CustomData.Count > 0;
    public bool HasCustomState => Global || ResetOnDeath || ResetOnTransition || ResetOnReload
        || Force || Timed >= 0f || Orig_Timed >= 0f || !RemoveWhenReset;
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

    public void ResetTimer(float t)
    {
        Timed = t;
        Orig_Timed = t;
    }
}

