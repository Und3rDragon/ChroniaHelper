using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System;
using ChroniaHelper.Utils;
using ChroniaHelper.Cores;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/OmniZipMover")]
public class OmniZipMover : OmniZipSolid
{

    public enum Themes
    {
        Normal,
        Moon,
        Cliffside
    }
    public readonly Themes theme;

    private readonly MTexture[,] edges = new MTexture[3, 3];
    private readonly MTexture[,] innerCorners = new MTexture[2, 2];
    
    private readonly List<MTexture> innerCogs;
    private readonly MTexture temp = new();

    private readonly BloomPoint bloom;

    private readonly Coroutine seq;


    public OmniZipMover(EntityData data, Vector2 offset)
        : this(data, data.Position + offset, data.Width, data.Height, data.NodesWithPosition(offset),
              data.Enum("theme", Themes.Normal),
              data.Bool("permanent"),
              data.Bool("waiting"),
              data.Bool("ticking"),
              data.Attr("customSkin").Trim(),
              data.Attr("backgroundColor").Trim(),
              data.Attr("ropeColor").Trim(),
              data.Attr("ropeLightColor").Trim(),
              data.Attr("customBlockTexture").Trim(),
              data.Attr("delays").Trim(),
              data.Attr("easing","sinein"),
              data.Attr("nodeSpeeds").Trim(),
              data.Int("ticks"),
              data.Float("tickDelay"),
              data.Bool("synced"),
              data.Float("startDelay"),
              data.Attr("startSound", "event:/CommunalHelperEvents/game/zipMover/normal/start"),
              data.Attr("impactSound", "event:/CommunalHelperEvents/game/zipMover/normal/impact"),
              data.Attr("tickSound", "event:/CommunalHelperEvents/game/zipMover/normal/tick"),
              data.Attr("returnSound", "event:/CommunalHelperEvents/game/zipMover/normal/return"),
              data.Attr("finishSound", "event:/CommunalHelperEvents/game/zipMover/normal/finish"),
              data.Bool("customSound",false),
              data.Bool("startShaking", true),
              data.Bool("nodeShaking",true),
              data.Bool("returnShaking",true),
              data.Bool("tickingShaking",true),
              data.Bool("permanentArrivalShaking",true),
              data.Attr("syncTag",""),
              data.Bool("hideRope",false),
              data.Bool("hideCog",false),
              data.Bool("dashable", false),
              data.Bool("onlyDashActivate", false),
              data.Bool("dashableOnce", true),
              data.Bool("dashableRefill", false),
              data.Attr("returnDelays", "0.2,0.2"),
              data.Attr("returnSpeeds","1,1"),
              data.Float("returnedIrrespondingTime", 0.5f),
              data.Attr("returnEasing", "sinein"),
              data.Bool("nodeSound",true),
              data.Bool("timeUnits", false),
              data.Bool("sideFlag",false)
              )
    { }

    public OmniZipMover(EntityData data, Vector2 position,
        int width, int height, Vector2[] nodes,
        Themes theme, bool permanent, bool waits,
        bool ticking, string customSkin,
        //新增变量
        string bgColor, string ropeColor, string ropeLightColor,
        string legacyCustomTexture, string delays, string ease,
        string speeds, int ticks, float tickDelay, bool sync, float startdelay,
        string startSfx, string impactSfx, string tickSfx, string returnSfx,
        string finishSfx, bool customSound,
        bool startShake, bool nodeShake, bool returnShake,
        bool tickShake, bool permaShake, string syncTag,
        bool hideRope, bool hideCog, bool dashable, bool onlyDash, bool dashableOnce,
        bool dashableRefill, string returnDelays, string returnSpeeds,
        float irrespondTime, string returnEase, bool nodeSound,
        bool unit, bool sideFlag
        )
        : base(data, position, width, height)
    {
        Depth = Depths.FGTerrain + 1;

        this.nodes = nodes;

        this.theme = theme;
        this.permanent = permanent;
        this.waits = waits;
        this.ticking = ticking;

        Add(seq = new Coroutine(Sequence()));
        Add(new LightOcclude());

        SurfaceSoundIndex = SurfaceIndex.Girder;

        string path, id, key, corners;
        if (!string.IsNullOrEmpty(customSkin))
        {
            path = customSkin + "/light";
            id = customSkin + "/block";
            key = customSkin + "/innercog";
            corners = customSkin + "/innerCorners";
            cog = GFX.Game[customSkin + "/cog"];
            themePath = "normal";
            backgroundColor = Color.Black;
            if (this.theme == Themes.Moon)
                themePath = "moon";
        }
        else
        {
            switch (this.theme)
            {
                default:
                case Themes.Normal:
                    path = "objects/zipmover/light";
                    id = "objects/zipmover/block";
                    key = "objects/zipmover/innercog";
                    corners = "objects/ChroniaHelper/zipmover/innerCorners";
                    cog = GFX.Game["objects/zipmover/cog"];
                    themePath = "normal";
                    drawBlackBorder = true;
                    backgroundColor = Color.Black;
                    break;

                case Themes.Moon:
                    path = "objects/zipmover/moon/light";
                    id = "objects/zipmover/moon/block";
                    key = "objects/zipmover/moon/innercog";
                    corners = "objects/ChroniaHelper/zipmover/moon/innerCorners";
                    cog = GFX.Game["objects/zipmover/moon/cog"];
                    themePath = "moon";
                    drawBlackBorder = false;
                    backgroundColor = Color.Black;
                    break;

                case Themes.Cliffside:
                    path = "objects/ChroniaHelper/omniZipMover/cliffside/light";
                    id = "objects/ChroniaHelper/omniZipMover/cliffside/block";
                    key = "objects/ChroniaHelper/omniZipMover/cliffside/innercog";
                    corners = "objects/ChroniaHelper/omniZipMover/cliffside/innerCorners";
                    cog = GFX.Game["objects/ChroniaHelper/omniZipMover/cliffside/cog"];
                    themePath = "normal";
                    drawBlackBorder = true;
                    backgroundColor = Calc.HexToColor("171018");
                    break;
            }
        }

        if (!string.IsNullOrEmpty(bgColor)) { this.backgroundColor = Calc.HexToColor(bgColor); }
        if (!string.IsNullOrEmpty(ropeColor)) { this.ropeColor = Calc.HexToColor(ropeColor); }
        if (!string.IsNullOrEmpty(ropeLightColor)) { this.ropeLightColor = Calc.HexToColor(ropeLightColor); }

        innerCogs = GFX.Game.GetAtlasSubtextures(key);
        streetlight = new Sprite(GFX.Game, path)
        {
            Active = false
        };
        streetlight.Add("frames", "", 1f);
        streetlight.Play("frames");
        streetlight.SetAnimationFrame(1);
        streetlight.Position = new Vector2((Width / 2f) - (streetlight.Width / 2f), 0f);
        Add(bloom = new BloomPoint(1f, 6f)
        {
            Position = new Vector2(Width / 2f, 4f)
        });

        // Check inner corners
        if (!GFX.Game.HasAtlasSubtextures(corners))
        {
            corners = "objects/ChroniaHelper/omniZipMover/empty_preset";
        }

        if (legacyCustomTexture != "")
        {
            Tuple<MTexture[,], MTexture[,]> customTiles = SetupCustomTileset(legacyCustomTexture);
            edges = customTiles.Item1;
            innerCorners = customTiles.Item2;
        }
        else
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8);

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    innerCorners[i, j] = GFX.Game[corners].GetSubtexture(i * 8, j * 8, 8, 8);
        }

        Add(sfx = new SoundSource()
        {
            Position = new Vector2(Width, Height) / 2f
        });

        // New params

        this.delays = delays.Split(',',StringSplitOptions.TrimEntries);
        this.easing = ease.Split(',',StringSplitOptions.TrimEntries);
        this.speeds = speeds.Split(',', StringSplitOptions.TrimEntries);
        if (ticks <= 0) { ticks = 5; }
        if (tickDelay <= Engine.DeltaTime) { tickDelay = Engine.DeltaTime; }
        this.ticks = ticks;
        this.tickDelay = tickDelay;

        if (this.synced = sync)
        {
            if(syncTag.Trim() == "")
            {
                this.zmtag = ropeColor;
            }
            else
            {
                this.zmtag = syncTag;
            }
        }
        
        this.startDelay = Math.Abs(startdelay);

        this.startSound = startSfx;
        this.impactSound = impactSfx;
        this.tickSound = tickSfx;
        this.returnSound = returnSfx;
        this.finishSound = finishSfx;

        this.customSound = customSound;

        this.startShake = startShake;
        this.tickShake = tickShake;
        this.permaShake = permaShake;
        this.returnShake = returnShake;
        this.nodeShake = nodeShake;
        this.hideRope = hideRope;
        this.hideCog = hideCog;

        // OnDash
        onDash = data.Enum<OnDash>("onDash", OnDash.Normal);
        this.dashable = dashable;
        if (!string.IsNullOrEmpty(data.Attr("dashable"))){
            if (this.dashable) { onDash = OnDash.Rebound; } // old data
        }
        this.onlyDash = onlyDash;
        this.dashableOnce = dashableOnce;
        this.dashableRefill = dashableRefill;
        
        OnDashCollide = OnDashed;
        

        // Return params
        this.returnDelays = returnDelays.Split(',', StringSplitOptions.TrimEntries);
        this.returnSpeeds = returnSpeeds.Split(',', StringSplitOptions.TrimEntries);
        irrespondingTime = irrespondTime;
        returnEasing = returnEase.Split(',',StringSplitOptions.TrimEntries);

        // Node Sound
        this.nodeSound = nodeSound;

        // Time Units
        timeUnits = unit;

        sideflag = sideFlag;

        // flag nodes?
        flagConditions = data.Attr("nodeFlags");
        conditionEmpty = string.IsNullOrEmpty(flagConditions);
        flags = FlagUtils.ParseList(flagConditions);

        // touch sensitive
        sensitive = data.Enum<TouchSensitive>("touchSensitive", TouchSensitive.none);

        // render
        recPos = position;
        recWidth = width;
        recHeight = height;
        
    }


    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        AutoTile(edges, innerCorners);

        Add(streetlight);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        scene.Add(pathRenderer = new(this, nodes, ropeColor, ropeLightColor));
    }

    public override void Removed(Scene scene)
    {
        scene.Remove(pathRenderer);
        pathRenderer = null;
        base.Removed(scene);
    }

    public override void Update()
    {
        base.Update();

        bloom.Visible = streetlight.CurrentAnimationFrame != 0;
        bloom.Y = (theme == Themes.Normal) ? streetlight.CurrentAnimationFrame * 3 : (theme == Themes.Cliffside) ? 5 : 9;

        // OnDash
        dcr = DashCollisionResults.NormalCollision;
        scale = Calc.Approach(scale, Vector2.One, 3f * Engine.DeltaTime);

        streetlight.Scale = scale;
        Vector2 zeroCenter = new Vector2(Width, Height) / 2f;
        streetlight.Position = zeroCenter + (new Vector2(zeroCenter.X - streetlight.Width / 2f, 0) - zeroCenter) * scale;
    }

    public override void Render()
    {
        Vector2 originalPosition = Position;
        Position += Shake;

        foreach (Hitbox extension in Colliders)
        {
            if (theme == Themes.Moon)
            {
                Draw.Rect(extension.Left + 2f + X, extension.Top + Y, extension.Width - 4f, extension.Height, backgroundColor);
                Draw.Rect(extension.Left + X, extension.Top + 2f + Y, extension.Width, extension.Height - 4f, backgroundColor);
                foreach (Image t in InnerCornerTiles)
                    Draw.Rect(t.X + X, t.Y + Y, 8, 8, backgroundColor);
            }
            else
                Draw.Rect(extension.Left + X, extension.Top + Y, extension.Width, extension.Height, backgroundColor);
        }

        int n = 1;
        float rot = 0f;
        int count = innerCogs.Count;

        float w = GroupBoundsMax.X - GroupBoundsMin.X;
        float h = GroupBoundsMax.Y - GroupBoundsMin.Y;
        Vector2 offset = new Vector2(-8, -8) + GroupOffset;

        for (int i = 4; i <= h + 4; i += 8)
        {
            int oldN = n;
            for (int j = 4; j <= w + 4; j += 8)
            {
                int index = (int) (NumberUtils.Mod((rot + (n * percent * (float) Math.PI * 4f)) / ((float) Math.PI / 2f), 1f) * count);
                MTexture mTexture = innerCogs[index];
                Rectangle rectangle = new(0, 0, mTexture.Width, mTexture.Height);
                Vector2 zero = Vector2.Zero;

                int x = (j - 4) / 8;
                int y = (i - 4) / 8;
                if (GroupTiles[x, y])
                {
                    // Rescaling the SubTexture Rectangle if the current cog can be rendered outside the Zip Mover

                    if (!GroupTiles[x - 1, y]) // Left
                    {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    }
                    if (!GroupTiles[x + 1, y]) // Right
                    {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }
                    if (!GroupTiles[x, y - 1]) // Up
                    {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    }
                    if (!GroupTiles[x, y + 1]) // Down
                    {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }

                    mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                    mTexture.DrawCentered(Position + new Vector2(j, i) + zero + offset, Color.White * ((n < 0) ? 0.5f : 1f));
                }

                n *= -1;
                rot += MathHelper.Pi / 3f;
            }

            // Ensures the checkboard pattern for innercogs
            if (oldN == n)
                n *= -1;
        }

        base.Render();
        Position = originalPosition;
    }

}
