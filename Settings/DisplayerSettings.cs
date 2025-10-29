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

public class DisplayerSettings
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

        if (Md.Settings.stateMachineDisplayer.enableStateMachineDisplayer)
        {
            string dashText = $"{PUt.player?.StateMachine.GetCurrentStateName() ?? "null"}";

            stateMachine_UI.origin = ((int)Md.Settings.stateMachineDisplayer.aligning + 4).ToJustify();
            
            stateMachine_UI.Render(dashText, (c) =>
            {
                return stateMachine_Reference.Contains(c) ? stateMachine_Reference.IndexOf(c) : stateMachine_Reference.IndexOf(" ");
            }, StateMachine_GetRenderPosition(PUt.player));
        }

        if (Md.Settings.realTimeClock.enableTimeClock)
        {
            string dashText = Md.Settings.realTimeClock.hasSeconds ?
                $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}" :
                $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}";

            realTimeClock_UI.origin = ((int)Md.Settings.realTimeClock.aligning + 4).ToJustify();

            realTimeClock_UI.Render(dashText, (c) =>
            {
                return $"{c}".ParseInt(c == ':' ? 10 : 0);
            }, RealTimeClock_GetRenderPosition(PUt.player));
        }

        if (Md.Settings.staminaMeterMenu.enableStaminaMeter)
        {
            string staminaText = $"{float.Round(PUt.player?.Stamina ?? 0f).ClampMin(0).ForceTo<int>()}";

            staminaMeter_UI.origin = ((int)Md.Settings.staminaMeterMenu.aligning + 4).ToJustify();

            staminaMeter_UI.Render(staminaText, (c) =>
            {
                return $"{c}".ParseInt(0);
            }, StaminaMeter_GetRenderPosition(PUt.player));
        }

        if (Md.Settings.dashesCounter.enableDashCounter)
        {
            string dashText = $"{PUt.player?.Dashes ?? 0}";

            dashes_UI.origin = ((int)Md.Settings.dashesCounter.aligning + 4).ToJustify();

            dashes_UI.Render(dashText, (c) =>
            {
                return $"{c}".ParseInt(0);
            }, Dashes_GetRenderPosition(PUt.player));
        }
    }

    public static string stateMachine_Reference = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =,";

    public static SerialImage stateMachine_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
        distance = 0f
    };

    public static SerialImage realTimeClock_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"))
    {
        distance = 0f
    };

    public static SerialImage staminaMeter_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"))
    {
        distance = 0f
    };

    public static SerialImage dashes_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"))
    {
        distance = 0f
    };

    public static Vc2 Dashes_GetRenderPosition(Player player)
    {
        var mode = Md.Settings.dashesCounter.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.dashesCounter.X, Md.Settings.dashesCounter.Y);

        if (mode == Sts.DashesDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(player?.Center.X ?? 0), (int)(player?.Center.Y ?? 0)) + setup;
        }
        else if (mode == Sts.DashesDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }

    public static Vc2 StateMachine_GetRenderPosition(Player player)
    {
        var mode = Md.Settings.stateMachineDisplayer.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.stateMachineDisplayer.X, Md.Settings.stateMachineDisplayer.Y);
        
        if(mode == Sts.StateMachineDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(player?.Center.X ?? 0), (int)(player?.Center.Y ?? 0)) + setup;
        }
        else if(mode == Sts.StateMachineDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }
    
    public static Vc2 RealTimeClock_GetRenderPosition(Player player)
    {
        var mode = Md.Settings.realTimeClock.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.realTimeClock.X, Md.Settings.realTimeClock.Y);

        if (mode == Sts.RealTimeClockDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(player?.Center.X ?? 0), (int)(player?.Center.Y ?? 0)) + setup;
        }
        else if (mode == Sts.RealTimeClockDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }

    public static Vc2 StaminaMeter_GetRenderPosition(Player player)
    {
        var mode = Md.Settings.staminaMeterMenu.displayPosition;
        Vc2 setup = new Vc2(Md.Settings.staminaMeterMenu.X, Md.Settings.staminaMeterMenu.Y);

        if (mode == Sts.StaminaDisplayer.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(player?.Center.X ?? 0f), (int)(player?.Center.Y ?? 0f)) + setup;
        }
        else if (mode == Sts.StaminaDisplayer.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }
}
