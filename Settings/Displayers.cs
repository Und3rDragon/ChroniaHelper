using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Mod.KoseiHelper.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Cores.Graphical;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.StopwatchSystem;
using Microsoft.Xna.Framework.Media;
using static Celeste.Mod.ChroniaHelperIndicatorZone.PlayerIndicatorZone;

namespace ChroniaHelper.Settings;

[Tracked(true)]
public class Displayers : HDRenderEntity
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Begin += OnLevelBegin;
        On.Celeste.Level.End += OnLevelEnd;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Begin -= OnLevelBegin;
        On.Celeste.Level.End -= OnLevelEnd;
    }

    public static Displayers Instance = null;
    public static void OnLevelBegin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self);

        self.Add(Instance = new Displayers(new EntityData(), Vc2.Zero));
    }

    public static void OnLevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        Instance.Buffer?.Dispose();
        self.Remove(Instance);

        orig(self);
    }

    public Displayers(EntityData d,Vc2 o) : base(d, o)
    {
        Tag |= Tags.Global;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Md.Settings.entityInfoDisplayer.renderTarget.Clear();
    }

    private bool mouseLeftDown = false;
    public override void Update()
    {
        base.Update();

        if (MInput.Mouse.PressedLeftButton)
        {
            Collider = new Hitbox(4f, 4f,
                InputUtils.MouseLevelPosition.X,
                InputUtils.MouseLevelPosition.Y);

            if (Md.Settings.DisplayEntityInfoInConsole)
            {
                Log.Info("==== Your mouse is around these entities ====");
            }
            Md.Settings.entityInfoDisplayer.renderTarget.Clear();
            foreach(var entity in MaP.level.Entities)
            {
                if (entity.CollideCheck(this))
                {
                    Md.Settings.entityInfoDisplayer.renderTarget.Add($"{entity.GetType()}");
                    
                    if (Md.Settings.DisplayEntityInfoInConsole)
                    {
                        Log.Info(entity.GetType());
                    }
                }
            }

            Collider = null;
        }
    }

    protected override void HDRender()
    {
        if (Md.Settings.commandStopclockDisplayer.enabled)
        {
            var displayer = Md.Settings.commandStopclockDisplayer;
            var displayUI = commandClock_UI;

            if (Cons.CommandStopclockID.GetStopclock(out Stopclock clock))
            {
                clock.GetClampedTimeData(out int[] data,
                    Md.Settings.commandStopclockDisplayer.minUnit,
                    Md.Settings.commandStopclockDisplayer.maxUnit);

                string target = "";
                for (int i = 0; i < data.Length; i++)
                {
                    if (i == 0)
                    {
                        target = Md.Settings.commandStopclockDisplayer.minUnit == 0 ?
                            $"{data[i]:000}" : $"{data[i]:00}";
                        continue;
                    }
                    else
                    {
                        target = $"{data[i]:00}:" + target;
                    }
                }

                if (Md.Settings.commandStopclockDisplayer.trimZeros)
                {
                    target = Regex.Replace(target, "0+:+", "");
                }

                displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
                displayUI.distance = displayer.letterDistance;
                displayUI.scale = displayer.scale * 0.1f;

                displayUI.Render(target, (c) =>
                {
                    return $"{c}".ParseInt(c == ':' ? 10 : 0);
                }, GetRenderPosition(displayer.displayPosition,
                    new Vc2(displayer.X, displayer.Y)));
            }
        }
        
        if (!Md.Settings.HUDMainControl) { return; }
        
        if (Md.Settings.stateMachineDisplayer.enabled)
        {
            var displayer = Md.Settings.stateMachineDisplayer;
            var displayUI = stateMachine_UI;
            
            string target = $"State {PUt.player?.StateMachine.State} {PUt.player?.StateMachine.GetCurrentStateName() ?? "null"}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.realTimeClock.enabled)
        {
            var displayer = Md.Settings.realTimeClock;
            var displayUI = realTimeClock_UI;
            
            string target = displayer.hasSeconds ?
                $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}" :
                $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return $"{c}".ParseInt(c == ':' ? 10 : 0);
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.staminaMeterMenu.enabled)
        {
            var displayer = Md.Settings.staminaMeterMenu;
            var displayUI = staminaMeter_UI;
            
            string target = $"{float.Round(PUt.player?.Stamina ?? 0f).ClampMin(0).ForceTo<int>()}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.dashesCounter.enabled)
        {
            var displayer = Md.Settings.dashesCounter;
            var displayUI = dashes_UI;
            
            string target = $"{PUt.player?.Dashes ?? 0}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.speedDisplayer.enabled)
        {
            var displayer = Md.Settings.speedDisplayer;
            var displayUI = speed_UI;
            string target = string.Empty;

            switch (displayer.speedDisplay)
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

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.playerSpriteDisplayer.enabled)
        {
            var displayer = Md.Settings.playerSpriteDisplayer;
            var displayUI = playerSprite_UI;
            
            string target = string.Empty;
            switch (displayer.displaySprite)
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

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.playerPositionDisplayer.enabled)
        {
            var displayer = Md.Settings.playerPositionDisplayer;
            var displayUI = playerPos_UI;
            
            string target = string.Empty;

            Vc2 playerPos = PUt.player?.Position ?? Vc2.Zero;

            Vc2 lvlPos = new Vc2(MaP.level.Bounds.Left, MaP.level.Bounds.Top);

            switch (displayer.displayCoordinates)
            {
                case Sts.PlayerPositionDisplayer.DisplayCoordinates.X:
                    target = displayer.useGlobalCoordinates ?
                        $"{(playerPos.X - lvlPos.X).ParseInt()}" :
                        $"{playerPos.X.ParseInt()}";
                    break;
                case Sts.PlayerPositionDisplayer.DisplayCoordinates.Y:
                    target = displayer.useGlobalCoordinates ?
                        $"{(playerPos.Y - lvlPos.Y).ParseInt()}" :
                        $"{playerPos.Y.ParseInt()}";
                    break;
                default:
                    target = displayer.useGlobalCoordinates ?
                        $"({(playerPos.X - lvlPos.X).ParseInt()},{(playerPos.Y - lvlPos.Y).ParseInt()})" :
                        $"({playerPos.X.ParseInt()},{playerPos.Y.ParseInt()})";
                    break;
            }

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.deathsDisplayer.enabled)
        {
            var displayer = Md.Settings.deathsDisplayer;
            var displayUI = deaths_UI;
            
            string target = $"{MaP.level?.Session.DeathsInCurrentLevel ?? 0}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.totalDeathsDisplayer.enabled)
        {
            var displayer = Md.Settings.totalDeathsDisplayer;
            var displayUI = totalDeaths_UI;
            
            string target = $"{MaP.level?.Session.Deaths ?? 0}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.saveFileDeathsDisplayer.enabled)
        {
            var displayer = Md.Settings.saveFileDeathsDisplayer;
            var displayUI = saveDeaths_UI;
            
            string target = $"{SaveData.Instance?.TotalDeaths ?? 0}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return $"{c}".ParseInt();
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.mapNameDisplayer.enabled)
        {
            var displayer = Md.Settings.mapNameDisplayer;
            var displayUI = mapName_UI;
            
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
                
            if (displayer.prefix)
            {
                target = "Map Name: " + target;
            }
            if (displayer.suffixAuthor)
            {
                string ssid = sid == "null" ? "null" : sid.Trim().Replace(' ', '_').Replace('-', '_').Replace('/', '_');
                bool dialogHas = Dialog.Has(ssid + "_author", lang);
                string fileAuthor = sid.Split('/', StringSplitOptions.TrimEntries).SafeGet(0, "null");
                target = target + $" -> {(dialogHas ? Dialog.Clean(ssid + "_author", lang) : fileAuthor)}";
            }

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }
        
        if(Md.Settings.roomNameDisplayer.enabled)
        {
            var displayer = Md.Settings.roomNameDisplayer;
            var displayUI = roomName_UI;
            
            string target = MaP.level?.Session.LevelData.Name ?? "null";
            if (displayer.prefix)
            {
                target = "Room Name: " + target;
            }

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.mapAuthorNameDisplayer.enabled)
        {
            var displayer = Md.Settings.mapAuthorNameDisplayer;
            var displayUI = authorName_UI;
            
            Language lang = Dialog.Languages["english"];

            string target = string.Empty;
            string sid = MaP.level?.Session.Area.SID ?? "null";
            string ssid = sid == "null" ? "null" : sid.Trim().Replace(' ', '_').Replace('-', '_').Replace('/', '_');
            bool dialogHas = Dialog.Has(ssid + "_author", lang);
            string fileAuthor = sid.Split('/', StringSplitOptions.TrimEntries).SafeGet(0, "null");
            target = dialogHas ? Dialog.Clean(ssid + "_author", lang) : fileAuthor;
            
            if (displayer.prefix)
            {
                target = "Author Name: " + target;
            }

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.levelBloomDisplayer.enabled)
        {
            var displayer = Md.Settings.levelBloomDisplayer;
            var displayUI = bloom_UI;
            
            string target = $"Bloom: {float.Round(MaP.level?.Bloom.Base ?? 0, 2)} base x {float.Round(MaP.level?.Bloom.Strength ?? 0, 2)} strength";

            if (Md.FrostHelperLoaded)
            {
                Color? getBloom = APIFrostHelper.GetBloomColor?.Invoke();
                target = target + $" (Color: {(getBloom is null ? "Not Available" : ((Color)getBloom).RgbaToHex())})";
            }
            
            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.levelLightingDisplayer.enabled)
        {
            var displayer = Md.Settings.levelLightingDisplayer;
            var displayUI = lighting_UI;

            string target = $"Lighting Alpha: {float.Round(MaP.level?.Lighting.Alpha ?? 0, 2)} (Color: {MaP.level?.Lighting.BaseColor.RgbaToHex() ?? Color.Black.RgbaToHex()})";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.cameraOffsetDisplayer.enabled)
        {
            var displayer = Md.Settings.cameraOffsetDisplayer;
            var displayUI = camera_UI;
            
            string target = "Camera: ";
            switch (displayer.cameraDisplay)
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

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.inputDisplayer.enabled)
        {
            var displayer = Md.Settings.inputDisplayer;
            var displayUI = input_UI;

            displayUI.template.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.template.renderMode = 1;
            displayUI.template.distance = displayer.letterDistance;
            displayUI.scales = new(){ displayer.scale * 0.1f };
            displayUI.memberDistance = displayer.lineDistance;
            displayUI.groupOrigin = displayer.overallAligning.ToJustify();

            //string input = "ZXCUDLR";
            string input = $"{(Input.Grab.Check ? "Z" : "")}{(Input.Dash.Check ? "X" : "")}{(Input.Jump.Check ? "C" : "")}{(Input.MenuUp.Check ? "U" : "")}{(Input.MenuDown.Check ? "D" : "")}{(Input.MenuLeft.Check ? "L" : "")}{(Input.MenuRight.Check ? "R" : "")}";
            input = string.IsNullOrEmpty(input) ? " " : input;
            if (displayer.renderTarget.Count == 0)
            {
                displayer.renderTarget.Add(input);
            }
            else
            {
                if(input != displayer.renderTarget.Last())
                {
                    displayer.renderTarget.Add(input);
                }
            }
            if (displayer.renderTarget.Count > displayer.maxDisplays)
            {
                displayer.renderTarget.RemoveAt(0);
            }

            displayUI.Render(displayer.renderTarget, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Sts.DisplayPosition.StaticScreen, new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.mousePositionDisplayer.enabled)
        {
            var displayer = Md.Settings.mousePositionDisplayer;
            var displayUI = mousePos_UI;

            string target = "Mouse: ";
            Vc2 data = Vc2.Zero;
            if(displayer.displayType == Modules.ChroniaHelperSettings.MousePositionDisplayer.DisplayType.LD)
            {
                data = InputUtils.MousePositionOnScreen;
            }
            else if(displayer.displayType == Modules.ChroniaHelperSettings.MousePositionDisplayer.DisplayType.Level)
            {
                data = InputUtils.MouseLevelPosition;
            }
            else
            {
                data = InputUtils.MousePosition;
            }
            target = target + $"X = {float.Round(data.X, 2)}, Y = {float.Round(data.Y, 2)}";

            displayUI.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.distance = displayer.letterDistance;
            displayUI.scale = displayer.scale * 0.1f;

            displayUI.Render(target, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(displayer.displayPosition,
                new Vc2(displayer.X, displayer.Y)));
        }

        if (Md.Settings.showMouse)
        {
            GFX.Game["Chroniahelper/LoennIcons/Mouse"].Draw(InputUtils.MousePosition);
        }

        if (Md.Settings.entityInfoDisplayer.enabled)
        {
            var displayer = Md.Settings.entityInfoDisplayer;
            var displayUI = entityInfo_UI;

            displayUI.template.origin = ((int)displayer.aligning + 4).ToJustify();
            displayUI.template.renderMode = 0;
            displayUI.template.distance = displayer.letterDistance;
            displayUI.template.segmentOrigin = Vc2.Zero;
            displayUI.scales = new() { displayer.scale * 0.1f };
            displayUI.memberDistance = displayer.lineDistance;
            displayUI.groupOrigin = displayer.overallAligning.ToJustify();

            displayUI.Render(displayer.renderTarget, (c) =>
            {
                return generalReference.Contains(c) ? generalReference.IndexOf(c) : generalReference.IndexOf(" ");
            }, GetRenderPosition(Sts.DisplayPosition.StaticScreen, new Vc2(displayer.X, displayer.Y)));
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

    public SerialImageRaw bloom_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw lighting_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw camera_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageRaw commandClock_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/StopclockFonts/fontB"));

    public SerialImageGroupRaw input_UI = new SerialImageGroupRaw("ChroniaHelper/DisplayFonts/font");

    public SerialImageRaw mousePos_UI = new SerialImageRaw(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"))
    {
        segmentOrigin = Vc2.Zero,
    };

    public SerialImageGroupRaw entityInfo_UI = new SerialImageGroupRaw("ChroniaHelper/DisplayFonts/font");

    public Vc2 GetRenderPosition(Sts.DisplayPosition pos, Vc2 setup)
    {
        if (pos == Sts.DisplayPosition.PlayerBased)
        {
            Vc2 p = new Vc2((int)(PUt.player?.Center.X ?? 0), (int)(PUt.player?.Center.Y ?? 0)) + setup;
            return (p - MaP.cameraPos) * Cons.HDScale;
        }
        else if (pos == Sts.DisplayPosition.StaticScreen)
        {
            return setup * Cons.HDScale;
        }

        return Vc2.Zero;
    }
}
