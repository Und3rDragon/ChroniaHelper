using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using YamlDotNet.Core;

namespace ChroniaHelper.Utils.ChroniaSystem;

public class ChroniaSlider
{
    [LoadHook]
    public static void Onload()
    {
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.Level.LoadLevel += OnLoadLevel;
        On.Monocle.Scene.Update += GlobalUpdate;
        On.Celeste.Level.TransitionRoutine += OnLevelTransition;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Monocle.Scene.Update -= GlobalUpdate;
        On.Celeste.Level.TransitionRoutine -= OnLevelTransition;
    }

    public static IEnumerator OnLevelTransition(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData levelData, Vector2 dir)
    {
        HashSet<string> removing = new();

        // Reset temporary on transitions
        foreach (var item in Md.SaveData.ChroniaSliders)
        {
            if (item.Value.ResetOnTransition)
            {
                item.Key.SetSlider(item.Value.DefaultValue);
                item.Value.Reset();
                if (item.Value.RemoveWhenReset)
                {
                    removing.Add(item.Key);
                }
            }
        }

        removing.EachDo((i) =>
        {
            MaP.sliders.SafeRemove(i);
            Md.SaveData.ChroniaSliders.SafeRemove(i);
        });

        yield return new SwapImmediately(orig(self, levelData, dir)); //On transition
    }

    public static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        orig(self); // Once per reload, not on first enter
    }

    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader)
    {
        orig(self, intro, fromLoader); // Once per level load, also reload

        HashSet<string> removing = new();

        // Remove temporary
        foreach (var item in Md.SaveData.ChroniaSliders)
        {
            if (item.Value.ResetOnDeath)
            {
                item.Key.SetSlider(item.Value.DefaultValue);
                item.Value.Reset();
                if (item.Value.RemoveWhenReset)
                {
                    removing.Add(item.Key);
                }
            }
        }

        removing.EachDo((i) =>
        {
            MaP.sliders.SafeRemove(i);
            Md.SaveData.ChroniaSliders.SafeRemove(i);
        });

        // Apply global flags
        foreach (var item in Md.SaveData.ChroniaSliders)
        {
            if (item.Value.Global)
            {
                item.Value.SetSlider(item.Key);
            }
        }
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);

        if (Md.SaveData.IsNotNull())
        {
            ChroniaSliderUtils.SliderRefresh();

            HashSet<string> removing = new();

            foreach (var item in Md.SaveData.ChroniaSliders)
            {
                if(self is not Level)
                {
                    if(item.Value.Global && item.Value.Timed > 0f)
                    {
                        item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
                    }
                }
                else
                {
                    if (!item.Value.Global && item.Value.Timed > 0f)
                    {
                        item.Value.Timed = Calc.Approach(item.Value.Timed, 0f, Engine.DeltaTime);
                    }
                }

                if(item.Value.Timed == 0f)
                {
                    item.Key.SetSlider(item.Value.DefaultValue);

                    if (item.Value.RemoveWhenReset)
                    {
                        removing.Add(item.Key);
                    }
                }
            }

            removing.EachDo((i) =>
            {
                MaP.sliders.SafeRemove(i);
                Md.SaveData.ChroniaSliders.SafeRemove(i);
            });
        }
    }

    public float Value { get; set; } = 0;
    public float Timed { get; set; } = -1f;
    public float Orig_Timed { get; set; } = -1f;
    public bool Global { get; set; } = false;
    public bool ResetOnDeath { get; set; } = false;
    public bool ResetOnTransition { get; set; } = false;
    public float DefaultValue { get; set; } = 0f;
    public bool RemoveWhenReset { get; set; } = true;

    public ChroniaSlider() { }

    public void SetSlider(string name)
    {
        Md.SaveData.ChroniaSliders.Enter(name, this);
        MaP.session.SetSlider(name, Value);
    }

    public void Reset()
    {
        Value = DefaultValue;
    }

    public void SetTimer(float t)
    {
        Orig_Timed = t;
        Timed = t;
    }

    public void ResetTimer()
    {
        Timed = Orig_Timed;
    }

    public bool Operating()
    {
        return Value != DefaultValue || Timed >= 0f || Orig_Timed >= 0f|| Global
            || ResetOnDeath || ResetOnTransition || DefaultValue != 0 
            || !RemoveWhenReset;
    }
}
