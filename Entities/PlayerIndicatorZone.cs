using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using ChroniaHelper;
using YoctoHelper.Cores;

namespace Celeste.Mod.ChroniaHelperIndicatorZone;

[CustomEntity(Cons.EntityStringId, Cons.EntityStringId2), Tracked]
public sealed partial class PlayerIndicatorZone : Entity
{
    public enum ZoneMode { Limited, Toggle, None }
    public enum FlagMode { None, Zone, Enable, Disable }


    public readonly List<MTexture> Icons;
    public readonly List<Vector2> IconOffsets;
    public readonly List<Color> IconColors;
    private readonly ZoneMode zoneMode;
    private readonly string controlFlag;
    private readonly bool renderBorder;
    private readonly bool renderInside;
    private readonly bool renderContinuousLine;
    private readonly Color zoneColor;
    private readonly FlagMode flagMode;
    private readonly string flag;

    private bool playerIn;
    private Player lastPlayer;

    private bool independentFlag;

    public PlayerIndicatorZone(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, ZoneConfig.FromEntityData(data))
    {
        independentFlag = data.Bool("independentFlag", false);
    }

    public PlayerIndicatorZone(Vector2 position, int width, int height, ZoneConfig config)
        : base(position)
    {
        Collider = new Hitbox(width, height);

        zoneMode = config.ZoneMode;
        controlFlag = config.ControlFlag;
        renderBorder = config.RenderBorder;
        renderInside = config.RenderInside;
        renderContinuousLine = config.RenderContinuousLine;
        zoneColor = config.ZoneColor;
        Depth = config.Depth;
        flagMode = config.FlagMode;
        flag = config.Flag;
        Icons = config.Icons;
        IconOffsets = config.IconOffsets;
        IconColors = config.IconColors;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        lastPlayer = null;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Session session = SceneAs<Level>().Session;
        Visible = string.IsNullOrEmpty(controlFlag) || session.GetFlag(controlFlag);
    }

    public override void Update()
    {
        base.Update();
        if (!independentFlag && zoneMode is ZoneMode.None) return;

        Session session = SceneAs<Level>().Session;
        if (!string.IsNullOrEmpty(controlFlag) && !session.GetFlag(controlFlag))
        {
            Visible = false;
            return;
        }
        else
        {
            Visible = true;
        }

        var player = CollideFirst<Player>();

        // on player enter
        if (!playerIn && player is not null)
        {
            playerIn = true;
            var renderer = Scene.Tracker.GetEntity<IconRenderer>();
            if (zoneMode is ZoneMode.Toggle)
                renderer.SwitchToHandle(this);
            else
                renderer.SwitchToHandle(null);
            lastPlayer = player;
            switch (flagMode)
            {
                case FlagMode.Zone:
                case FlagMode.Enable:
                    session.SetFlag(flag, true);
                    break;
                case FlagMode.Disable:
                    session.SetFlag(flag, false);
                    break;
            }
        }

        // on player leave
        if (playerIn && player is null)
        {
            playerIn = false;
            if (flagMode is FlagMode.Zone)
                session.SetFlag(flag, false);
        }

    }

    public override void Render()
    {
        base.Render();

        // these are doing 'MathF.Floor'
        float left = (int)Left;
        float right = (int)Right;
        float top = (int)Top;
        float bottom = (int)Bottom;

        float step = 2f;

        if (renderInside)
        { Draw.Rect(left + 4, top + 4, Width - 8f, Height - 8f, zoneColor * 0.25f); }


        if (renderBorder)
        {
            if (renderContinuousLine)
            {
                Draw.HollowRect(left, top, Width, Height, zoneColor);
            }
            else
            {
                for (float x = left; x < (right - 3); x += 3f)
                {
                    // top
                    Draw.Line(x, top, x + step, top, zoneColor);
                    // bottom
                    Draw.Line(x, bottom - 1, x + step, bottom - 1, zoneColor);
                }
                for (float y = top; y < (bottom - 3); y += 3f)
                {
                    // left
                    Draw.Line(left + 1, y, left + 1, y + step, zoneColor);
                    // right
                    Draw.Line(right, y, right, y + step, zoneColor);
                }
            }
            // top left
            Draw.Rect(left + 1, top, 1f, 2f, zoneColor);
            // top right
            Draw.Rect(right - 2, top, 2f, 2f, zoneColor);
            // bottom left
            Draw.Rect(left, bottom - 2, 2f, 2f, zoneColor);
            // bottom right
            Draw.Rect(right - 2, bottom - 2, 2f, 2f, zoneColor);
        }

        switch (zoneMode)
        {
        case ZoneMode.Limited:
            if (playerIn)
                DrawIcons(lastPlayer.Position, Icons, IconOffsets, IconColors);
            break;
        case ZoneMode.Toggle:
        // icons in toggle mode are handled by IconRenderer
        case ZoneMode.None:
            // do nothing
            break;
        }
    }

    private static void DrawIcons(Vector2 at, List<MTexture> icons, List<Vector2> iconOffsets, List<Color> iconColors)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            Vector2 offset = iconOffsets.Count > i ? iconOffsets[i] : Vector2.Zero;
            Color color = iconColors.Count > i ? iconColors[i] : Color.White;
            icons[i].DrawCentered(at + offset, color);
        }
    }
}