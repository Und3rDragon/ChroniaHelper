using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using FASF2025Helper.Utils;

namespace ChroniaHelper.Settings;

public class StaminaMeter
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Player.Render += PlayerRender;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Player.Render -= PlayerRender;
    }

    public static void PlayerRender(On.Celeste.Player.orig_Render orig, Celeste.Player self)
    {
        orig(self);

        if (Md.Settings.staminaMeterMenu.enableStaminaMeter)
        {
            string staminaText = $"{(int)self.Stamina}";

            staminaMeter_UI.Render(staminaText, (c) =>
            {
                return $"{c}".ParseInt(0);
            }, GetRenderPosition(self));
        }
    }

    public static SerialImage staminaMeter_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"))
    {
        origin = Vc2.One * 0.5f,
        distance = 1f
    };
    
    public static Vc2 GetRenderPosition(Player player)
    {
        var mode = Md.Settings.staminaMeterMenu.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.staminaMeterMenu.X, Md.Settings.staminaMeterMenu.Y);
        
        if(mode == Sts.StaminaDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)player.Center.X, (int)player.Center.Y) + setup;
        }
        else if(mode == Sts.StaminaDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }
}
