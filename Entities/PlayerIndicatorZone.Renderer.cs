using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using ChroniaHelper;
using ChroniaHelper.Utils;
using ChroniaHelper.Modules;

namespace Celeste.Mod.ChroniaHelperIndicatorZone;

partial class PlayerIndicatorZone
{
    [Tracked]
    public sealed class IconRenderer : Entity
    {
        private List<MTexture> icons;
        private List<Vector2> iconOffsets;
        private List<Color> iconColors;

        public IconRenderer()
        {
            Tag |= Tags.Global;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            var session = ChroniaHelperModule.Session;
            session.ProcessZoneSaves();
            icons = session.PIZ_Icons;
            iconOffsets = session.PIZ_IconOffsets;
            iconColors = session.PIZ_IconColors;
            Depth = session.PIZ_ZoneDepth;
        }

        public void SwitchToHandle(PlayerIndicatorZone zone)
        {
            var session = ChroniaHelperModule.Session;
            if (zone is not null)
            {
                icons = zone.Icons;
                iconOffsets = zone.IconOffsets;
                iconColors = zone.IconColors;
                Depth = zone.Depth;
                session.RecordZoneSave(zone);
            }
            else
            {
                icons = null;
                iconOffsets = null;
                iconColors = null;
                session.RecordZoneSave(null);
            }
        }

        public override void Render()
        {
            base.Render();
            if (icons is null) return;
            var player = Scene.Tracker.GetEntity<Player>();
            if (player is null) return;
            DrawIcons(player.Position, icons, iconOffsets, iconColors);
        }
    }
}
