using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Entities;

public class SeamlessSpinnerAlphaSlider
{
    public const string SliderName = "SeamlessSpinnerAlphaSlider";

    [Cores.LoadHook]
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += OnLoadLevel;
        On.Celeste.Level.Update += LevelUpdate;
    }
    [Cores.UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Celeste.Level.Update -= LevelUpdate;
    }
    
    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool loader)
    {
        orig(self, intro, loader);
    }
    
    public static void LevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        float s = SliderName.GetSlider().Clamp(0f, 1f);
        
        foreach(var e in self.Tracker.GetEntities<SeamlessSpinner>())
        {
            var spin = e as SeamlessSpinner;

            spin.sprite.color.alpha = spin.spriteColor.alpha * (1f - s);
            spin.bgSprites.EachDo((i) =>
            {
                i.color.alpha = spin.bgSpriteColor.alpha * (1f - s);
            });
        }
    }
}
