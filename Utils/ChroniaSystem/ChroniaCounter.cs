using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Utils.ChroniaSystem;

public class ChroniaCounter
{
    public static void Onload()
    {
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.Level.LoadLevel += OnLoadLevel;
        On.Celeste.Level.Update += OnLevelUpdate;
        On.Monocle.Scene.Update += GlobalUpdate;
        On.Celeste.Level.TransitionRoutine += OnLevelTransition;
    }

    public static void Unload()
    {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Celeste.Level.Update -= OnLevelUpdate;
        On.Monocle.Scene.Update -= GlobalUpdate;
        On.Celeste.Level.TransitionRoutine -= OnLevelTransition;
    }

    public static IEnumerator OnLevelTransition(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData levelData, Vector2 dir)
    {
        // Remove temporary on transitions
        foreach (var item in Md.SaveData.ChroniaCounters)
        {
            if (item.Value.Temporary)
            {
                foreach (var counter in MaP.session.Counters)
                {
                    if (counter.Key == item.Key) { MaP.session.Counters.Remove(counter); }
                }
                Md.SaveData.ChroniaCounters.SafeRemove(item.Key);
            }
        }

        yield return new SwapImmediately(orig(self, levelData, dir)); //On transition
    }

    public static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        orig(self); // Once per reload, not on first enter

        // Remove temporary
        foreach (var item in Md.SaveData.ChroniaCounters)
        {
            foreach (var counter in MaP.session.Counters)
            {
                if (counter.Key == item.Key) { MaP.session.Counters.Remove(counter); }
            }
            Md.SaveData.ChroniaCounters.SafeRemove(item.Key);
        }

        // Apply global
        foreach (var item in Md.SaveData.ChroniaCounters)
        {
            if (item.Value.Global)
            {
                item.Value.SetCounter(item.Key);
            }
        }
    }

    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader)
    {
        orig(self, intro, fromLoader); // Once per level load, also reload

        // Remove temporary
        foreach (var item in Md.SaveData.ChroniaCounters)
        {
            foreach (var counter in MaP.session.Counters)
            {
                if (counter.Key == item.Key) { MaP.session.Counters.Remove(counter); }
            }
            Md.SaveData.ChroniaCounters.SafeRemove(item.Key);
        }

        // Apply global flags
        foreach (var item in Md.SaveData.ChroniaCounters)
        {
            if (item.Value.Global && !item.Value.Temporary)
            {
                item.Value.SetCounter(item.Key);
            }
        }
    }

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        foreach (var item in Md.SaveData.ChroniaCounters)
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
                    foreach (var counter in MaP.session.Counters)
                    {
                        if (counter.Key == item.Key) { MaP.session.Counters.Remove(counter); }
                    }
                    Md.SaveData.ChroniaCounters.SafeRemove(item.Key);
                }
            }
        }
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);

        if (Md.SaveData.IsNotNull())
        {
            foreach (var item in Md.SaveData.ChroniaCounters)
            {
                // Security Check

                // Global stuffs only
                if (item.Value.Global)
                {
                    if (item.Value.Timed > 0f)
                    {
                        item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
                    }
                    else if (item.Value.Timed == 0f)
                    {
                        foreach (var counter in MaP.session.Counters)
                        {
                            if (counter.Key == item.Key) { MaP.session.Counters.Remove(counter); }
                        }
                        Md.SaveData.ChroniaCounters.SafeRemove(item.Key);
                    }
                }
            }
        }
    }

    public int Value { get; set; } = 0;
    public float Timed { get; set; } = -1f;
    public bool Global { get; set; } = false;
    public bool Temporary { get; set; } = false;
    public int DefaultValue { get; set; } = 0;

    public ChroniaCounter() { }

    public void SetCounter(string name)
    {
        Md.SaveData.ChroniaCounters.Enter(name, this);
        MaP.session.SetCounter(name, Value);
    }

    public void Reset()
    {
        Value = DefaultValue;
    }
}
