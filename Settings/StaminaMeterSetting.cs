using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using FASF2025Helper.Utils;
using static Celeste.Mod.ChroniaHelperIndicatorZone.PlayerIndicatorZone;

namespace ChroniaHelper.Settings;

public class StaminaMeterSetting
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.GrabbyIcon.Render += IconRender;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.GrabbyIcon.Render -= IconRender;
    }
    
    public static void IconRender(On.Celeste.GrabbyIcon.orig_Render orig, GrabbyIcon icon)
    {
        orig(icon);
        
        if (Md.Settings.staminaMeterMenu.enableStaminaMeter)
        {
            string staminaText = $"{float.Round(PUt.player?.Stamina ?? 0f).ClampMin(0).ForceTo<int>()}";
            
            staminaMeter_UI.origin = ((int)Md.Settings.staminaMeterMenu.aligning + 4).ToJustify();
            
            staminaMeter_UI.Render(staminaText, (c) =>
            {
                return $"{c}".ParseInt(0);
            }, GetRenderPosition(PUt.player));
        }
    }
    
    public static SerialImage staminaMeter_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"))
    {
        distance = 1f
    };
    
    public static Vc2 GetRenderPosition(Player player)
    {
        var mode = Md.Settings.staminaMeterMenu.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.staminaMeterMenu.X, Md.Settings.staminaMeterMenu.Y);
        
        if(mode == Sts.StaminaDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(player?.Center.X ?? 0f), (int)(player?.Center.Y ?? 0f)) + setup;
        }
        else if(mode == Sts.StaminaDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }
}
