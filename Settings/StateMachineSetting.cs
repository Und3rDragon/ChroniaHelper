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

public class StateMachineSetting
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
            }, GetRenderPosition(PUt.player));
        }
    }

    public static string stateMachine_Reference = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =,";

    public static SerialImage stateMachine_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
        distance = 1f
    };
    
    public static Vc2 GetRenderPosition(Player player)
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
}
