using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.ChroniaHelperIndicatorZone;

partial class PlayerIndicatorZone
{
    public sealed class ZoneConfig
    {
        public ZoneMode ZoneMode;
        public string ControlFlag;
        public bool RenderBorder;
        public bool RenderInside;
        public bool RenderContinuousLine;
        public Color ZoneColor;
        public int Depth;
        public List<MTexture> Icons;
        public List<Vector2> IconOffsets;
        public List<Color> IconColors;
        public FlagMode FlagMode;
        public string Flag;

        public static ZoneConfig FromEntityData(EntityData data)
        {
            try
            {
                if (data.Has("iconPrefix"))
                    return FromNew(data);
                else if (data.Has("slotDirectories"))
                    return FromOldCustom(data);
                else if (data.Has("slot1Directory"))
                    return FromOldNonCustom(data);
                else
                    throw new Exception("Unrecognized EntityData version.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load entity data.", e);
            }
        }

        private static ZoneConfig FromOldNonCustom(EntityData data)
        {
            ZoneConfig config = new();
            ReadOldNonSlotParams(config, data);

            bool slot1 = data.Bool("slot1Enabled", true);
            bool slot2 = data.Bool("slot2Enabled", false);
            bool slot3 = data.Bool("slot3Enabled", false);

            List<MTexture> icons = new();
            List<Vector2> offsets = new();
            List<Color> colors = new();

            if (slot1)
            {
                icons.Add(GFX.Game[data.Attr("slot1Directory", "ChroniaHelper/PlayerIndicator/chevron")]);
                offsets.Add(new(-11f, 6f));
                colors.Add(data.HexColor("slot1Color", Color.White));
            }
            if (slot2)
            {
                icons.Add(GFX.Game[data.Attr("slot2Directory", "ChroniaHelper/PlayerIndicator/triangle")]);
                offsets.Add(new(0f, 6f));
                colors.Add(data.HexColor("slot2Color", Color.White));
            }
            if (slot3)
            {
                icons.Add(GFX.Game[data.Attr("slot3Directory", "ChroniaHelper/PlayerIndicator/square")]);
                offsets.Add(new(12f, 6f));
                colors.Add(data.HexColor("slot3Color", Color.White));
            }
            config.Icons = icons;
            config.IconOffsets = offsets;
            config.IconColors = colors;
            return config;
        }

        private static ZoneConfig FromOldCustom(EntityData data)
        {
            ZoneConfig config = new();
            ReadOldNonSlotParams(config, data);

            string slotDirectories = data.Attr(
                "slotDirectories",
                "ChroniaHelper/PlayerIndicator/chevron,ChroniaHelper/PlayerIndicator/triangle"
                );
            string iconOffsets = data.Attr("iconCoordinates", "0,6;-11,6");
            string iconColors = data.Attr("iconColors", "ffffff,ffffff");

            StringSplitOptions option = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            config.Icons = slotDirectories.Split(',', option).Select(s => GFX.Game[s]).ToList();
            config.IconOffsets = iconOffsets.Split(';', option).Select(ParseVector2).ToList();
            config.IconColors = iconColors.Split(',', option).Select(Calc.HexToColor).ToList();
            return config;
        }

        private static void ReadOldNonSlotParams(ZoneConfig config, EntityData data)
        {
            config.ZoneMode = data.Attr("mode") switch
            {
                "limited" => ZoneMode.Limited,
                "none" => ZoneMode.None,
                "toggle" => ZoneMode.Toggle,
                _ => ZoneMode.Limited
            };
            config.ControlFlag = data.Attr("zoneControlFlag", "");
            config.RenderBorder = data.Bool("renderBorder", false);
            config.RenderInside = data.Bool("renderInside", false);
            config.RenderContinuousLine = data.Bool("continuousLine", false);
            config.ZoneColor = data.HexColor("zoneColor", Color.White);
            config.Depth = data.Int("depth", -20000);
            config.FlagMode = data.Attr("flagToggle") switch
            {
                "enableFlag" => FlagMode.Enable,
                "disableFlag" => FlagMode.Disable,
                "inZone" => FlagMode.Zone,
                "disabled" => FlagMode.None,
                _ => FlagMode.None
            };
            config.Flag = data.Attr("flag", "flag");
        }

        private static ZoneConfig FromNew(EntityData data)
        {
            ZoneConfig config = new()
            {
                ZoneMode = (ZoneMode)data.Int("mode", 0),
                ControlFlag = data.Attr("controlFlag", ""),
                RenderBorder = data.Bool("renderBorder", false),
                RenderInside = data.Bool("renderInside", false),
                RenderContinuousLine = data.Bool("renderContinuousLine", false),
                ZoneColor = data.HexColor("zoneColor", Color.White),
                Depth = data.Int("depth", -20000),
                FlagMode = (FlagMode)data.Int("flagMode"),
                Flag = data.Attr("flag", "flag")
            };
            // TODO convert old data
            string iconPrefix = data.Attr("iconPrefix", "ChroniaHelper/PlayerIndicator/");
            string icons = data.Attr("icons", "chevron,triangle");
            string iconOffsets = data.Attr("iconOffsets", "0,6;-11,6");
            string iconColors = data.Attr("iconColors", "ffffff,ffffff");

            StringSplitOptions option = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            config.Icons = icons.Split(',', option).Select(s => GFX.Game[iconPrefix + s]).ToList();
            config.IconOffsets = iconOffsets.Split(';', option).Select(ParseVector2).ToList();
            config.IconColors = iconColors.Split(',', option).Select(Calc.HexToColor).ToList();
            return config;
        }

        private static Vector2 ParseVector2(string s)
        {
            int index = s.IndexOf(',');
            float part1 = float.Parse(s[..index]);
            float part2 = float.Parse(s[(index + 1)..]);
            return new Vector2(part1, part2);
        }
    }

}
