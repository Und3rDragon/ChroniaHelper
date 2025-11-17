using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Utils.ChroniaSystem;

public class ChroniaSystem
{
    [LoadHook]
    public static void Onload()
    {
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.Level.LoadLevel += OnLoadLevel;
        On.Monocle.Scene.Update += GlobalUpdate;
        On.Celeste.Level.TransitionRoutine += OnLevelTransition;
        On.Celeste.Player.Die += OnPlayerDeath;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Monocle.Scene.Update -= GlobalUpdate;
        On.Celeste.Level.TransitionRoutine -= OnLevelTransition;
        On.Celeste.Player.Die -= OnPlayerDeath;
    }


    public static IEnumerator OnLevelTransition(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData levelData, Vector2 dir)
    {
        foreach (var item in Md.Session.flagsPerRoom)
        {
            item.SetFlag(false);
        }

        Md.Session.flagsPerRoom.Clear();

        foreach (var item in Md.Session.countersPerRoom)
        {
            item.Key.SetCounter(item.Value);
        }

        Md.Session.countersPerRoom.Clear();

        foreach (var item in Md.Session.slidersPerRoom)
        {
            item.Key.SetSlider(item.Value);
        }

        Md.Session.slidersPerRoom.Clear();

        foreach (var item in Md.SaveData.flags)
        {
            item.SetFlag(true);
        }
        
        foreach(var item in Md.SaveData.counters)
        {
            item.Key.SetCounter(item.Value);
        }
        
        foreach(var item in Md.SaveData.sliders)
        {
            item.Key.SetSlider(item.Value);
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

        foreach (var item in Md.Session.flagsPerRoom)
        {
            item.SetFlag(false);
        }

        Md.Session.flagsPerRoom.Clear();

        foreach (var item in Md.Session.countersPerRoom)
        {
            item.Key.SetCounter(item.Value);
        }

        Md.Session.countersPerRoom.Clear();

        foreach (var item in Md.Session.slidersPerRoom)
        {
            item.Key.SetSlider(item.Value);
        }

        Md.Session.slidersPerRoom.Clear();

        foreach (var item in Md.SaveData.flags)
        {
            item.SetFlag(true);
        }

        foreach (var item in Md.SaveData.counters)
        {
            item.Key.SetCounter(item.Value);
        }

        foreach (var item in Md.SaveData.sliders)
        {
            item.Key.SetSlider(item.Value);
        }
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);
    }
    
    public static PlayerDeadBody OnPlayerDeath(On.Celeste.Player.orig_Die orig, Player self, Vc2 dir, bool eii, bool reg)
    {
        foreach (var item in Md.Session.flagsPerDeath)
        {
            item.SetFlag(false);
        }

        Md.Session.flagsPerRoom.Clear();

        foreach (var item in Md.Session.countersPerDeath)
        {
            item.Key.SetCounter(item.Value);
        }

        Md.Session.countersPerRoom.Clear();

        foreach (var item in Md.Session.slidersPerDeath)
        {
            item.Key.SetSlider(item.Value);
        }

        Md.Session.slidersPerRoom.Clear();
        
        return orig(self, dir, eii, reg);
    }
}
