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

public class DashDisplayer
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

        if (Md.Settings.dashesCounter.enableDashCounter)
        {
            string dashText = $"{PUt.player?.Dashes ?? 0}";

            dashes_UI.Render(dashText, (c) =>
            {
                return $"{c}".ParseInt(0);
            }, GetRenderPosition(PUt.player));
        }
    }

    public static SerialImage dashes_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"))
    {
        origin = Vc2.One * 0.5f,
        distance = 1f
    };
    
    public static Vc2 GetRenderPosition(Player player)
    {
        var mode = Md.Settings.dashesCounter.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.dashesCounter.X, Md.Settings.dashesCounter.Y);
        
        if(mode == Sts.DashesDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(player?.Center.X ?? 0), (int)(player?.Center.Y ?? 0)) + setup;
        }
        else if(mode == Sts.DashesDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }
}
