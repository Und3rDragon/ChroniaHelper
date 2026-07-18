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
        On.Celeste.Level.Begin += OnLevelBegin;
        On.Celeste.Level.End += OnLevelEnd;
        On.Celeste.Level.Update += OnLevelUpdate;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Monocle.Scene.Update -= GlobalUpdate;
        On.Celeste.Level.TransitionRoutine -= OnLevelTransition;
        On.Celeste.Player.Die -= OnPlayerDeath;
        On.Celeste.Level.Begin -= OnLevelBegin;
        On.Celeste.Level.End -= OnLevelEnd;
        On.Celeste.Level.Update -= OnLevelUpdate;
    }


    public static IEnumerator OnLevelTransition(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData levelData, Vector2 dir)
    {
        ResetPerRoom(self);
        ApplyGlobals(self);

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

        ResetPerRoom(self);
        ApplyGlobals(self);
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);
    }

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        if (!Md.InstanceReady) { return; }

        if (MaP.level.Session is null || self.Session is null) { return; }

        foreach(var counter in MaP.level.Session.Counters)
        {
            if (counter.Key.StartsWith("ChroniaHelper_ChroniaColor_"))
            {
                string identifier = counter.Key.Remove(0, "ChroniaHelper_ChroniaColor_".Length);
                string key = identifier.Remove(identifier.Length - 2, 2);

                bool valid = identifier.EndsWith("_R") || identifier.EndsWith("_G") ||
                    identifier.EndsWith("_B");

                if (!valid) { continue; }

                if (identifier.EndsWith("_R"))
                {
                    var current = Md.Session.chroniaColors.GetValueOrDefault(key, CColor.White);
                    current.color.R = (byte)counter.Value.Clamp(0, 255);
                    Md.Session.chroniaColors[key] = current;
                }

                if (identifier.EndsWith("_G"))
                {
                    var current = Md.Session.chroniaColors.GetValueOrDefault(key, CColor.White);
                    current.color.G = (byte)counter.Value.Clamp(0, 255);
                    Md.Session.chroniaColors[key] = current;
                }

                if (identifier.EndsWith("_B"))
                {
                    var current = Md.Session.chroniaColors.GetValueOrDefault(key, CColor.White);
                    current.color.B = (byte)counter.Value.Clamp(0, 255);
                    Md.Session.chroniaColors[key] = current;
                }
            }
        }

        foreach (var slider in MaP.sliders)
        {
            if (slider.Key.StartsWith("ChroniaHelper_ChroniaColor_"))
            {
                string identifier = slider.Key.Remove(0, "ChroniaHelper_ChroniaColor_".Length);
                string key = identifier.Remove(identifier.Length - 2, 2);

                bool valid = identifier.EndsWith("_A");

                if (!valid) { continue; }

                var current = Md.Session.chroniaColors.GetValueOrDefault(key, CColor.White);
                current.alpha = slider.Value.Value.Clamp(0f, 1f);
                Md.Session.chroniaColors[key] = current;
            }
        }

        //if (self.OnInterval(0.5f))
        //{
        //    foreach(var i in Md.Session.chroniaColors)
        //    {
        //        Log.Info(i.Key, i.Value);
        //    }
        //    Log.Error(Log.divider);
        //}
    }
    
    public static PlayerDeadBody OnPlayerDeath(On.Celeste.Player.orig_Die orig, Player self, Vc2 dir, bool eii, bool reg)
    {
        ResetPerDeath(MaP.level);
        
        return orig(self, dir, eii, reg);
    }

    public static void OnLevelBegin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self);

        foreach (var item in Md.Session.chroniaColors)
        {
            string name = item.Key;

            $"ChroniaHelper_ChroniaColor_{name}_R".SetCounter(item.Value.color.R);
            $"ChroniaHelper_ChroniaColor_{name}_G".SetCounter(item.Value.color.G);
            $"ChroniaHelper_ChroniaColor_{name}_B".SetCounter(item.Value.color.B);
            $"ChroniaHelper_ChroniaColor_{name}_A".SetSlider(item.Value.alpha);
        }

        foreach (var item in Md.SaveData.chroniaColors)
        {
            string name = item.Key;

            $"ChroniaHelper_ChroniaColor_{name}_R".SetCounter(item.Value.color.R);
            $"ChroniaHelper_ChroniaColor_{name}_G".SetCounter(item.Value.color.G);
            $"ChroniaHelper_ChroniaColor_{name}_B".SetCounter(item.Value.color.B);
            $"ChroniaHelper_ChroniaColor_{name}_A".SetSlider(item.Value.alpha);
        }
    }

    public static void OnLevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {

        orig(self);
    }

    public static void ResetPerRoom(Level self)
    {
        foreach (var item in Md.Session.flagsPerRoom)
        {
            self.Session.SetFlag(item, false);
        }

        Md.Session.flagsPerRoom.Clear();

        foreach (var item in Md.Session.countersPerRoom)
        {
            self.Session.SetCounter(item.Key, item.Value);
        }

        Md.Session.countersPerRoom.Clear();

        foreach (var item in Md.Session.slidersPerRoom)
        {
            self.Session.SetSlider(item.Key, item.Value);
        }

        Md.Session.slidersPerRoom.Clear();
    }

    public static void ResetPerDeath(Level self)
    {
        foreach (var item in Md.Session.flagsPerDeath)
        {
            self.Session.SetFlag(item, false);
        }

        Md.Session.flagsPerDeath.Clear();

        foreach (var item in Md.Session.countersPerDeath)
        {
            self.Session.SetCounter(item.Key, item.Value);
        }

        Md.Session.countersPerDeath.Clear();

        foreach (var item in Md.Session.slidersPerDeath)
        {
            self.Session.SetSlider(item.Key, item.Value);
        }

        Md.Session.slidersPerDeath.Clear();
    }

    public static void ApplyGlobals(Level self)
    {
        foreach (var item in Md.SaveData.flags)
        {
            self.Session.SetFlag(item, true);
        }

        foreach (var item in Md.SaveData.counters)
        {
            self.Session.SetCounter(item.Key, item.Value);
        }

        foreach (var item in Md.SaveData.sliders)
        {
            self.Session.SetSlider(item.Key, item.Value);
        }
    }
}
