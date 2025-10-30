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

public class Displayers
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

        if (Md.Settings.stateMachineDisplayer.enabled)
        {
            string target = $"{PUt.player?.StateMachine.GetCurrentStateName() ?? "null"}";
            
            stateMachine_UI.origin = ((int)Md.Settings.stateMachineDisplayer.aligning + 4).ToJustify();
            stateMachine_UI.distance = Md.Settings.stateMachineDisplayer.letterDistance;

            stateMachine_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.stateMachineDisplayer.displayPosition,
                new Vc2(Md.Settings.stateMachineDisplayer.X, Md.Settings.stateMachineDisplayer.Y)));
        }

        if (Md.Settings.realTimeClock.enabled)
        {
            string target = Md.Settings.realTimeClock.hasSeconds ?
                $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}" :
                $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}";

            realTimeClock_UI.origin = ((int)Md.Settings.realTimeClock.aligning + 4).ToJustify();
            realTimeClock_UI.distance = Md.Settings.realTimeClock.letterDistance;

            realTimeClock_UI.Render(target, (c) =>
            {
                return $"{c}".ParseInt(c == ':' ? 10 : 0);
            }, GetRenderPosition(Md.Settings.realTimeClock.displayPosition,
                new Vc2(Md.Settings.realTimeClock.X, Md.Settings.realTimeClock.Y)));
        }

        if (Md.Settings.staminaMeterMenu.enabled)
        {
            string target = $"{float.Round(PUt.player?.Stamina ?? 0f).ClampMin(0).ForceTo<int>()}";

            staminaMeter_UI.origin = ((int)Md.Settings.staminaMeterMenu.aligning + 4).ToJustify();
            staminaMeter_UI.distance = Md.Settings.staminaMeterMenu.letterDistance;

            staminaMeter_UI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(Md.Settings.staminaMeterMenu.displayPosition,
                new Vc2(Md.Settings.staminaMeterMenu.X, Md.Settings.staminaMeterMenu.Y)));
        }

        if (Md.Settings.dashesCounter.enabled)
        {
            string target = $"{PUt.player?.Dashes ?? 0}";

            dashes_UI.origin = ((int)Md.Settings.dashesCounter.aligning + 4).ToJustify();
            dashes_UI.distance = Md.Settings.dashesCounter.letterDistance;

            dashes_UI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(Md.Settings.dashesCounter.displayPosition,
                new Vc2(Md.Settings.dashesCounter.X, Md.Settings.dashesCounter.Y)));
        }

        if (Md.Settings.speedDisplayer.enabled)
        {
            string target = string.Empty;

            switch (Md.Settings.speedDisplayer.speedDisplay)
            {
                case Sts.SpeedDisplayer.SpeedDisplay.speedX:
                    target = $"{PUt.player?.Speed.X.ParseInt() ?? 0}";
                    break;
                case Sts.SpeedDisplayer.SpeedDisplay.speedY:
                    target = $"{PUt.player?.Speed.Y.ParseInt() ?? 0}";
                    break;
                case Sts.SpeedDisplayer.SpeedDisplay.speedToCoordinates:
                    target = $"({PUt.player?.Speed.X.ParseInt() ?? 0},{PUt.player?.Speed.Y.ParseInt() ?? 0})";
                    break;
                default:
                    target = $"{PUt.player?.Speed.Length().ParseInt() ?? 0}";
                    break;
            }

            speed_UI.origin = ((int)Md.Settings.speedDisplayer.aligning + 4).ToJustify();
            speed_UI.distance = Md.Settings.speedDisplayer.letterDistance;
            
            speed_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.speedDisplayer.displayPosition,
                new Vc2(Md.Settings.speedDisplayer.X, Md.Settings.speedDisplayer.Y)));
        }

        if (Md.Settings.playerSpriteDisplayer.enabled)
        {
            string target = string.Empty;
            switch (Md.Settings.playerSpriteDisplayer.displaySprite)
            {
                case Modules.ChroniaHelperSettings.PlayerSpriteDisplayer.DisplaySprite.Animation:
                    target = $"{PUt.player?.Sprite.CurrentAnimationID ?? "null"}";
                    break;
                case Modules.ChroniaHelperSettings.PlayerSpriteDisplayer.DisplaySprite.File:
                    target = $"{PUt.player?.Sprite.Texture.AtlasPath ?? "null"}";
                    break;
                default:
                    target = $"{PUt.player?.Sprite.CurrentAnimationID ?? "null"} => {PUt.player?.Sprite.Texture.AtlasPath ?? "null"}";
                    break;
            }

            playerSprite_UI.origin = ((int)Md.Settings.playerSpriteDisplayer.aligning + 4).ToJustify();
            playerSprite_UI.distance = Md.Settings.playerSpriteDisplayer.letterDistance;

            playerSprite_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.playerSpriteDisplayer.displayPosition,
                new Vc2(Md.Settings.playerSpriteDisplayer.X, Md.Settings.playerSpriteDisplayer.Y)));
        }

        if (Md.Settings.playerPositionDisplayer.enabled)
        {
            string target = string.Empty;

            Vc2 playerPos = PUt.player?.Position ?? Vc2.Zero;

            Vc2 lvlPos = new Vc2(MaP.level.Bounds.Left, MaP.level.Bounds.Top);

            switch (Md.Settings.playerPositionDisplayer.displayCoordinates)
            {
                case Sts.PlayerPositionDisplayer.DisplayCoordinates.X:
                    target = Md.Settings.playerPositionDisplayer.useGlobalCoordinates ?
                        $"{(playerPos.X - lvlPos.X).ParseInt()}" :
                        $"{playerPos.X.ParseInt()}";
                    break;
                case Sts.PlayerPositionDisplayer.DisplayCoordinates.Y:
                    target = Md.Settings.playerPositionDisplayer.useGlobalCoordinates ?
                        $"{(playerPos.Y - lvlPos.Y).ParseInt()}" :
                        $"{playerPos.Y.ParseInt()}";
                    break;
                default:
                    target = Md.Settings.playerPositionDisplayer.useGlobalCoordinates ?
                        $"({(playerPos.X - lvlPos.X).ParseInt()},{(playerPos.Y - lvlPos.Y).ParseInt()})" :
                        $"({playerPos.X.ParseInt()},{playerPos.Y.ParseInt()})";
                    break;
            }

            playerPos_UI.origin = ((int)Md.Settings.playerPositionDisplayer.aligning + 4).ToJustify();
            playerPos_UI.distance = Md.Settings.playerPositionDisplayer.letterDistance;
            
            playerPos_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.playerPositionDisplayer.displayPosition,
                new Vc2(Md.Settings.playerPositionDisplayer.X, Md.Settings.playerPositionDisplayer.Y)));
        }
    }

    public static string generalReference = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =,";

    public static SerialImage stateMachine_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public static SerialImage realTimeClock_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public static SerialImage staminaMeter_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public static SerialImage dashes_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public static SerialImage speed_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public static SerialImage playerSprite_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public static SerialImage playerPos_UI = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public static Vc2 GetRenderPosition(Sts.DisplayPosition pos, Vc2 setup)
    {
        if (pos == Sts.DisplayPosition.PlayerBased)
        {
            return new Vc2((int)(PUt.player?.Center.X ?? 0), (int)(PUt.player?.Center.Y ?? 0)) + setup;
        }
        else if (pos == Sts.DisplayPosition.StaticScreen)
        {
            return MaP.cameraPos + setup;
        }

        return Vc2.Zero;
    }
}
