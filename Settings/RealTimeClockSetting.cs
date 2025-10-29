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

public class RealTimeClockSetting
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

        if (Md.Settings.realTimeClock.enableTimeClock)
        {
            string dashText = Md.Settings.realTimeClock.hasSeconds ? 
                $"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}" :
                $"{DateTime.Now.Hour}:{DateTime.Now.Minute}";

            realTimeClock_UI.origin = ((int)Md.Settings.realTimeClock.aligning + 4).ToJustify();
            
            realTimeClock_UI.Render(dashText, (c) =>
            {
                return $"{c}".ParseInt(c == ':' ? 10 : 0);
            }, GetRenderPosition(PUt.player));
        }
    }

    public static SerialImage realTimeClock_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"))
    {
        distance = 0f
    };
    
    public static Vc2 GetRenderPosition(Player player)
    {
        var mode = Md.Settings.realTimeClock.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.realTimeClock.X, Md.Settings.realTimeClock.Y);
        
        if(mode == Sts.RealTimeClockDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(player?.Center.X ?? 0), (int)(player?.Center.Y ?? 0)) + setup;
        }
        else if(mode == Sts.RealTimeClockDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }
}
