using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using FASF2025Helper.Utils;
using static Celeste.Mod.ChroniaHelperIndicatorZone.PlayerIndicatorZone;

namespace ChroniaHelper.Settings;

[Tracked(false)]
public class Displayers : HDRenderEntity
{
    public Displayers(EntityData d,Vc2 o) : base(d, o)
    {
        
    }
    
    protected override void HDRender()
    {
        if (!Md.Settings.HUDMainControl) { return; }
        
        if (Md.Settings.stateMachineDisplayer.enabled)
        {
            string target = $"{PUt.player?.StateMachine.GetCurrentStateName() ?? "null"}";

            stateMachine_UI.origin = ((int)Md.Settings.stateMachineDisplayer.aligning + 4).ToJustify();
            stateMachine_UI.distance = Md.Settings.stateMachineDisplayer.letterDistance;
            stateMachine_UI.scale = Md.Settings.stateMachineDisplayer.scale * 0.1f;

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
            realTimeClock_UI.scale = Md.Settings.realTimeClock.scale * 0.1f;

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
            staminaMeter_UI.scale = Md.Settings.staminaMeterMenu.scale * 0.1f;

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
            dashes_UI.scale = Md.Settings.dashesCounter.scale * 0.1f;

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
            speed_UI.scale = Md.Settings.speedDisplayer.scale * 0.1f;

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
            playerSprite_UI.scale = Md.Settings.playerSpriteDisplayer.scale * 0.1f;

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
            playerPos_UI.scale = Md.Settings.playerPositionDisplayer.scale * 0.1f;

            playerPos_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.playerPositionDisplayer.displayPosition,
                new Vc2(Md.Settings.playerPositionDisplayer.X, Md.Settings.playerPositionDisplayer.Y)));
        }

        if (Md.Settings.deathsDisplayer.enabled)
        {
            string target = $"{MaP.level?.Session.DeathsInCurrentLevel ?? 0}";

            deaths_UI.origin = ((int)Md.Settings.deathsDisplayer.aligning + 4).ToJustify();
            deaths_UI.distance = Md.Settings.deathsDisplayer.letterDistance;
            deaths_UI.scale = Md.Settings.deathsDisplayer.scale * 0.1f;

            deaths_UI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(Md.Settings.deathsDisplayer.displayPosition,
                new Vc2(Md.Settings.deathsDisplayer.X, Md.Settings.deathsDisplayer.Y)));
        }

        if (Md.Settings.totalDeathsDisplayer.enabled)
        {
            string target = $"{MaP.level?.Session.Deaths ?? 0}";

            totalDeaths_UI.origin = ((int)Md.Settings.totalDeathsDisplayer.aligning + 4).ToJustify();
            totalDeaths_UI.distance = Md.Settings.totalDeathsDisplayer.letterDistance;
            totalDeaths_UI.scale = Md.Settings.totalDeathsDisplayer.scale * 0.1f;

            totalDeaths_UI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(Md.Settings.totalDeathsDisplayer.displayPosition,
                new Vc2(Md.Settings.totalDeathsDisplayer.X, Md.Settings.totalDeathsDisplayer.Y)));
        }

        if (Md.Settings.saveFileDeathsDisplayer.enabled)
        {
            string target = $"{SaveData.Instance?.TotalDeaths ?? 0}";

            saveDeaths_UI.origin = ((int)Md.Settings.saveFileDeathsDisplayer.aligning + 4).ToJustify();
            saveDeaths_UI.distance = Md.Settings.saveFileDeathsDisplayer.letterDistance;
            saveDeaths_UI.scale = Md.Settings.saveFileDeathsDisplayer.scale * 0.1f;

            saveDeaths_UI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(Md.Settings.saveFileDeathsDisplayer.displayPosition,
                new Vc2(Md.Settings.saveFileDeathsDisplayer.X, Md.Settings.saveFileDeathsDisplayer.Y)));
        }

        if (Md.Settings.mapNameDisplayer.enabled)
        {
            Language lang = Dialog.Languages["english"];
            
            string target = string.Empty;
            string sid = MaP.level?.Session.Area.SID ?? "null";
            if (sid.StartsWith("Celeste/"))
            {
                target = sid.Remove(0, sid.IndexOf('-') + 1);
            }
            else
            {
                if(sid != "null")
                {
                    string ssid = sid.Trim().Replace(' ', '_').Replace('-', '_').Replace('/', '_');
                    
                    bool has = Dialog.Has(ssid, lang);
                    
                    target = has ? Dialog.Clean(ssid, lang) : sid.Trim();
                }
                else
                {
                    target = sid;
                }
            }
                
            if (Md.Settings.mapNameDisplayer.prefix)
            {
                target = "Map Name: " + target;
            }
            if (Md.Settings.mapNameDisplayer.suffixAuthor)
            {
                string ssid = sid == "null" ? "null" : sid.Trim().Replace(' ', '_').Replace('-', '_').Replace('/', '_');
                bool dialogHas = Dialog.Has(ssid + "_author", lang);
                string fileAuthor = sid.Split('/', StringSplitOptions.TrimEntries).SafeGet(0, "null");
                target = target + $" -> {(dialogHas ? Dialog.Clean(ssid + "_author", lang) : fileAuthor)}";
            }

            mapName_UI.origin = ((int)Md.Settings.mapNameDisplayer.aligning + 4).ToJustify();
            mapName_UI.distance = Md.Settings.mapNameDisplayer.letterDistance;
            mapName_UI.scale = Md.Settings.mapNameDisplayer.scale * 0.1f;

            mapName_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.mapNameDisplayer.displayPosition,
                new Vc2(Md.Settings.mapNameDisplayer.X, Md.Settings.mapNameDisplayer.Y)));
        }
        
        if(Md.Settings.roomNameDisplayer.enabled)
        {
            string target = MaP.level?.Session.LevelData.Name ?? "null";
            if (Md.Settings.roomNameDisplayer.prefix)
            {
                target = "Room Name: " + target;
            }

            roomName_UI.origin = ((int)Md.Settings.roomNameDisplayer.aligning + 4).ToJustify();
            roomName_UI.distance = Md.Settings.roomNameDisplayer.letterDistance;
            roomName_UI.scale = Md.Settings.roomNameDisplayer.scale * 0.1f;

            roomName_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.roomNameDisplayer.displayPosition,
                new Vc2(Md.Settings.roomNameDisplayer.X, Md.Settings.roomNameDisplayer.Y)));
        }

        if (Md.Settings.mapAuthorNameDisplayer.enabled)
        {
            Language lang = Dialog.Languages["english"];

            string target = string.Empty;
            string sid = MaP.level?.Session.Area.SID ?? "null";
            string ssid = sid == "null" ? "null" : sid.Trim().Replace(' ', '_').Replace('-', '_').Replace('/', '_');
            bool dialogHas = Dialog.Has(ssid + "_author", lang);
            string fileAuthor = sid.Split('/', StringSplitOptions.TrimEntries).SafeGet(0, "null");
            target = dialogHas ? Dialog.Clean(ssid + "_author", lang) : fileAuthor;
            
            if (Md.Settings.mapAuthorNameDisplayer.prefix)
            {
                target = "Author Name: " + target;
            }

            roomName_UI.origin = ((int)Md.Settings.mapAuthorNameDisplayer.aligning + 4).ToJustify();
            roomName_UI.distance = Md.Settings.mapAuthorNameDisplayer.letterDistance;
            roomName_UI.scale = Md.Settings.mapAuthorNameDisplayer.scale * 0.1f;

            roomName_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.mapAuthorNameDisplayer.displayPosition,
                new Vc2(Md.Settings.mapAuthorNameDisplayer.X, Md.Settings.mapAuthorNameDisplayer.Y)));
        }

        if (Md.Settings.cameraOffsetDisplayer.enabled)
        {
            string target = "Camera: ";
            switch (Md.Settings.cameraOffsetDisplayer.cameraDisplay)
            {
                case Modules.ChroniaHelperSettings.CameraOffsetDisplayer.CameraDisplay.CameraX:
                    target = target + $"X= {float.Round(MaP.level?.CameraOffset.X / 48f ?? 0, 2)}";
                    break;
                case Modules.ChroniaHelperSettings.CameraOffsetDisplayer.CameraDisplay.CameraY:
                    target = target + $"Y= {float.Round(MaP.level?.CameraOffset.Y / 32f ?? 0, 2)}";
                    break;
                default:
                    target = target + $"X= {float.Round(MaP.level?.CameraOffset.X / 48f ?? 0, 2)}, Y= {float.Round(MaP.level?.CameraOffset.Y / 32f ?? 0, 2)}";
                    break;
            }

            camera_UI.origin = ((int)Md.Settings.cameraOffsetDisplayer.aligning + 4).ToJustify();
            camera_UI.distance = Md.Settings.cameraOffsetDisplayer.letterDistance;
            camera_UI.scale = Md.Settings.cameraOffsetDisplayer.scale * 0.1f;

            camera_UI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Md.Settings.cameraOffsetDisplayer.displayPosition,
                new Vc2(Md.Settings.cameraOffsetDisplayer.X, Md.Settings.cameraOffsetDisplayer.Y)));
        }
    }

    public string generalReference = Cons.DisplayFontsReference;

    public SerialImageRaw stateMachine_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw realTimeClock_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public SerialImageRaw staminaMeter_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public SerialImageRaw dashes_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public SerialImageRaw speed_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw playerSprite_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw playerPos_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw deaths_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public SerialImageRaw totalDeaths_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public SerialImageRaw saveDeaths_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public SerialImageRaw mapName_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw roomName_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw authorName_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw camera_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public Vc2 GetRenderPosition(Sts.DisplayPosition pos, Vc2 setup)
    {
        if (pos == Sts.DisplayPosition.PlayerBased)
        {
            Vc2 p = new Vc2((int)(PUt.player?.Center.X ?? 0), (int)(PUt.player?.Center.Y ?? 0)) + setup;
            return (p - MaP.cameraPos) * HDScale;
        }
        else if (pos == Sts.DisplayPosition.StaticScreen)
        {
            return setup * HDScale;
        }

        return Vc2.Zero;
    }
}
