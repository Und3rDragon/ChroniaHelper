using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System;
using ChroniaHelper.Utils;
using ChroniaHelper.Cores;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using Line = ChroniaHelper.Utils.GeometryUtils.Line;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/ZipGlass")]
public class ZipGlass : OmniZipSolid
{
    // GlassBlockBg funcs
    public void Dispose()
    {
        if (starsTarget != null && !starsTarget.IsDisposed)
        {
            starsTarget.Dispose();
        }

        if (beamsTarget != null && !beamsTarget.IsDisposed)
        {
            beamsTarget.Dispose();
        }

        starsTarget = null;
        beamsTarget = null;
    }


    public struct Star
    {
        public Vector2 Position;

        public MTexture Texture;

        public Color Color;

        public Vector2 Scroll;
    }

    public struct BGRay
    {
        public Vector2 Position;

        public float Width;

        public float Length;

        public Color Color;
    }

    public static Color[] starColors = new Color[3]
    {
        Calc.HexToColor("7f9fba"),
        Calc.HexToColor("9bd1cd"),
        Calc.HexToColor("bacae3")
    };

    public const int StarCount = 100;
    public const int RayCount = 50;
    public Star[] stars = new Star[100];
    public BGRay[] rays = new BGRay[50];
    public VertexPositionColor[] verts = new VertexPositionColor[2700];
    public Vector2 rayNormal = new Vector2(-5f, -8f).SafeNormalize();
    public Color bgColor = Calc.HexToColor("0d2e89");
    public VirtualRenderTarget beamsTarget;
    public VirtualRenderTarget starsTarget;
    public bool hasBlocks;

    

    public void BeforeRender()
    {
        List<Entity> entities = base.Scene.Tracker.GetEntities<ZipGlass>();
        if (!(hasBlocks = entities.Count > 0))
        {
            return;
        }

        Camera camera = (base.Scene as Level).Camera;
        int num = 320;
        int num2 = 180;
        if (starsTarget == null)
        {
            starsTarget = VirtualContent.CreateRenderTarget("glass-block-surfaces", 320, 180);
        }

        Engine.Graphics.GraphicsDevice.SetRenderTarget(starsTarget);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        Vector2 origin = new Vector2(8f, 8f);
        for (int i = 0; i < stars.Length; i++)
        {
            MTexture texture = stars[i].Texture;
            Color color = stars[i].Color;
            Vector2 scroll = stars[i].Scroll;
            Vector2 vector = default(Vector2);
            vector.X = Mod(stars[i].Position.X - camera.X * (1f - scroll.X), num);
            vector.Y = Mod(stars[i].Position.Y - camera.Y * (1f - scroll.Y), num2);
            texture.Draw(vector, origin, color);
            if (vector.X < origin.X)
            {
                texture.Draw(vector + new Vector2(num, 0f), origin, color);
            }
            else if (vector.X > (float)num - origin.X)
            {
                texture.Draw(vector - new Vector2(num, 0f), origin, color);
            }

            if (vector.Y < origin.Y)
            {
                texture.Draw(vector + new Vector2(0f, num2), origin, color);
            }
            else if (vector.Y > (float)num2 - origin.Y)
            {
                texture.Draw(vector - new Vector2(0f, num2), origin, color);
            }
        }

        Draw.SpriteBatch.End();
        int vertex = 0;
        for (int j = 0; j < rays.Length; j++)
        {
            Vector2 vector2 = default(Vector2);
            vector2.X = Mod(rays[j].Position.X - camera.X * 0.9f, num);
            vector2.Y = Mod(rays[j].Position.Y - camera.Y * 0.9f, num2);
            DrawRay(vector2, ref vertex, ref rays[j]);
            if (vector2.X < 64f)
            {
                DrawRay(vector2 + new Vector2(num, 0f), ref vertex, ref rays[j]);
            }
            else if (vector2.X > (float)(num - 64))
            {
                DrawRay(vector2 - new Vector2(num, 0f), ref vertex, ref rays[j]);
            }

            if (vector2.Y < 64f)
            {
                DrawRay(vector2 + new Vector2(0f, num2), ref vertex, ref rays[j]);
            }
            else if (vector2.Y > (float)(num2 - 64))
            {
                DrawRay(vector2 - new Vector2(0f, num2), ref vertex, ref rays[j]);
            }
        }

        if (beamsTarget == null)
        {
            beamsTarget = VirtualContent.CreateRenderTarget("glass-block-beams", 320, 180);
        }

        Engine.Graphics.GraphicsDevice.SetRenderTarget(beamsTarget);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        GFX.DrawVertices(Matrix.Identity, verts, vertex);
    }

    public void OnDisplacementRender()
    {
        foreach (Entity entity in base.Scene.Tracker.GetEntities<ZipGlass>())
        {
            Draw.Rect(entity.X, entity.Y, entity.Width, entity.Height, new Color(0.5f, 0.5f, 0.2f, 1f));
        }
    }

    public void DrawRay(Vector2 position, ref int vertex, ref BGRay ray)
    {
        Vector2 vector = new Vector2(0f - rayNormal.Y, rayNormal.X);
        Vector2 vector2 = rayNormal * ray.Width * 0.5f;
        Vector2 vector3 = vector * ray.Length * 0.25f * 0.5f;
        Vector2 vector4 = vector * ray.Length * 0.5f * 0.5f;
        Vector2 v = position + vector2 - vector3 - vector4;
        Vector2 v2 = position - vector2 - vector3 - vector4;
        Vector2 vector5 = position + vector2 - vector3;
        Vector2 vector6 = position - vector2 - vector3;
        Vector2 vector7 = position + vector2 + vector3;
        Vector2 vector8 = position - vector2 + vector3;
        Vector2 v3 = position + vector2 + vector3 + vector4;
        Vector2 v4 = position - vector2 + vector3 + vector4;
        Color transparent = Color.Transparent;
        Color color = ray.Color;
        Quad(ref vertex, v, vector5, vector6, v2, transparent, color, color, transparent);
        Quad(ref vertex, vector5, vector7, vector8, vector6, color, color, color, color);
        Quad(ref vertex, vector7, v3, v4, vector8, color, transparent, transparent, color);
    }

    public void Quad(ref int vertex, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color c0, Color c1, Color c2, Color c3)
    {
        verts[vertex].Position.X = v0.X;
        verts[vertex].Position.Y = v0.Y;
        verts[vertex++].Color = c0;
        verts[vertex].Position.X = v1.X;
        verts[vertex].Position.Y = v1.Y;
        verts[vertex++].Color = c1;
        verts[vertex].Position.X = v2.X;
        verts[vertex].Position.Y = v2.Y;
        verts[vertex++].Color = c2;
        verts[vertex].Position.X = v0.X;
        verts[vertex].Position.Y = v0.Y;
        verts[vertex++].Color = c0;
        verts[vertex].Position.X = v2.X;
        verts[vertex].Position.Y = v2.Y;
        verts[vertex++].Color = c2;
        verts[vertex].Position.X = v3.X;
        verts[vertex].Position.Y = v3.Y;
        verts[vertex++].Color = c3;
    }

    
    // Main funcs

    private readonly MTexture[,] edges = new MTexture[3, 3];
    private readonly MTexture[,] innerCorners = new MTexture[2, 2];
    
    private readonly List<MTexture> innerCogs;
    private readonly MTexture temp = new();

    private readonly BloomPoint bloom;

    private readonly Coroutine seq;


    // Glass Block
    public List<Line> lines = new List<Line>();

    public Color lineColor = Color.White;

    public bool intervene = true;


    public ZipGlass(EntityData data, Vector2 offset)
        : this(data, data.Position + offset, data.Width, data.Height, data.NodesWithPosition(offset),
              data.Bool("permanent"),
              data.Bool("waiting"),
              data.Bool("ticking"),
              data.Attr("customSkin").Trim(),
              // New params
              data.Attr("backgroundColor", "000000").Trim(),
              data.Attr("ropeColor").Trim(),
              data.Attr("ropeLightColor").Trim(),
              data.Attr("delays").Trim(),
              data.Attr("easing", "sinein"),
              data.Attr("nodeSpeeds").Trim(),
              data.Int("ticks"),
              data.Float("tickDelay"),
              data.Bool("synced"),
              data.Float("startDelay"),
              data.Bool("startShaking", true),
              data.Bool("nodeShaking", true),
              data.Bool("returnShaking", true),
              data.Bool("tickingShaking", true),
              data.Bool("permanentArrivalShaking", true),
              data.Attr("syncTag", ""),
              data.Bool("hideRope", false),
              data.Bool("hideCog", false),
              data.Bool("dashable", false),
              data.Bool("onlyDashActivate", false),
              data.Bool("dashableOnce", true),
              data.Bool("dashableRefill", false),
              data.Attr("returnDelays", "0.2,0.2"),
              data.Attr("returnSpeeds", "1,1"),
              data.Float("returnedIrrespondingTime", 0.5f),
              data.Attr("returnEasing", "sinein"),
              data.Bool("nodeSound", true),
              data.Bool("topSurface", true),
              data.Bool("bottomSurface", false),
              data.Attr("starColors", "7f9fba, 9bd1cd, bacae3"),
              data.Attr("starPath", "particles/ChroniaHelper/star"),
              data.Bool("timeUnits", false),
              data.Bool("sideFlag", false)
              )
    { }

    private bool customSkin;


    public ZipGlass(EntityData data, Vector2 position,
        int width, int height, Vector2[] nodes,
        bool permanent, bool waits,
        bool ticking, string customSkin,
        //新增变量
        string bgColor, string ropeColor, string ropeLightColor,
        string delays, string ease,
        string speeds, int ticks, float tickDelay, bool sync, float startdelay,
        bool startShake, bool nodeShake, bool returnShake,
        bool tickShake, bool permaShake, string syncTag,
        bool hideRope, bool hideCog, bool dashable, bool onlyDash, bool dashableOnce,
        bool dashableRefill, string returnDelays, string returnSpeeds,
        float irrespondTime, string returnEase, bool nodeSound,
        bool topSurface, bool bottomSurface,
        string starColor, string starPath, bool unit, bool sideFlag
        )
        : base(data, position, width, height)
    {
        Depth = Depths.FGTerrain + 1;

        this.nodes = nodes;

        this.permanent = permanent;
        this.waits = waits;
        this.ticking = ticking;

        Add(seq = new Coroutine(Sequence()));
        Add(new LightOcclude());

        
        // Path Render
        string path;
        if (this.customSkin = !string.IsNullOrEmpty(customSkin))
        { cog = GFX.Game[customSkin + "/cog"];
          path = customSkin + "/light";
        }
        else { cog = GFX.Game["objects/zipmover/cog"];
            path = "objects/zipmover/light";
        }

        // Color Process
        if (!string.IsNullOrEmpty(bgColor)) { this.backgroundColor = Calc.HexToColor(bgColor); }
        if (!string.IsNullOrEmpty(ropeColor)) { this.ropeColor = Calc.HexToColor(ropeColor); }
        if (!string.IsNullOrEmpty(ropeLightColor)) { this.ropeLightColor = Calc.HexToColor(ropeLightColor); }

        // Glass Block BG
        this.bgColor = this.backgroundColor;
        //base.Tag = Tags.Global;
        Add(new BeforeRenderHook(BeforeRender));
        Add(new DisplacementRenderHook(OnDisplacementRender));
        //base.Depth = -9990;
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(starPath);
        // Star Colors
        string[] getColors = starColor.Split(',', StringSplitOptions.TrimEntries);
        starColors = new Color[getColors.Length];
        for (int i = 0; i < getColors.Length; i++)
        {
            starColors[i] = Calc.HexToColor(getColors[i]);
        }
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Position.X = Calc.Random.Next(320);
            stars[i].Position.Y = Calc.Random.Next(180);
            stars[i].Texture = Calc.Random.Choose(atlasSubtextures);
            stars[i].Color = Calc.Random.Choose(starColors);
            stars[i].Scroll = Vector2.One * Calc.Random.NextFloat(0.05f);
        }

        for (int j = 0; j < rays.Length; j++)
        {
            rays[j].Position.X = Calc.Random.Next(320);
            rays[j].Position.Y = Calc.Random.Next(180);
            rays[j].Width = Calc.Random.Range(4f, 16f);
            rays[j].Length = Calc.Random.Choose(48, 96, 128);
            rays[j].Color = Color.White * Calc.Random.Range(0.2f, 0.4f);
        }

        // Glass Block
        Add(new MirrorSurface());
        SurfaceSoundIndex = 32;



        // Street Light
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


        Add(sfx = new SoundSource()
        {
            Position = new Vector2(Width, Height) / 2f
        });

        // New params

        this.delays = delays.Split(',', StringSplitOptions.TrimEntries);
        this.easing = ease.Split(',',StringSplitOptions.TrimEntries);
        this.speeds = speeds.Split(',', StringSplitOptions.TrimEntries);
        if (ticks <= 0) { ticks = 5; }
        if (tickDelay <= Engine.DeltaTime) { tickDelay = Engine.DeltaTime; }
        this.ticks = ticks;
        this.tickDelay = tickDelay;

        if (this.synced = sync)
        {
            if (syncTag.Trim() == "")
            {
                this.zmtag = ropeColor;
            }
            else
            {
                this.zmtag = syncTag;
            }
        }

        this.startDelay = Math.Abs(startdelay);

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
        if (!string.IsNullOrEmpty(data.Attr("dashable")))
        {
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
        returnEasing = returnEase.Split(',', StringSplitOptions.TrimEntries);

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

        Add(streetlight);

        int num = (int)base.Width / 8;
        int num2 = (int)base.Height / 8;
        AddSide(new Vector2(0f, 0f), new Vector2(0f, -1f), num);
        AddSide(new Vector2(num - 1, 0f), new Vector2(1f, 0f), num2);
        AddSide(new Vector2(num - 1, num2 - 1), new Vector2(0f, 1f), num);
        AddSide(new Vector2(0f, num2 - 1), new Vector2(-1f, 0f), num2);

    }

    public float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }

    public void AddSide(Vector2 start, Vector2 normal, int tiles)
    {
        Vector2 vector = new Vector2(0f - normal.Y, normal.X);
        for (int i = 0; i < tiles; i++)
        {
            if (Open(start + vector * i + normal))
            {
                Vector2 vector2 = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                if (!Open(start + vector * (i - 1)))
                {
                    vector2 -= vector;
                }

                for (; i < tiles && Open(start + vector * i + normal); i++)
                {
                }

                Vector2 vector3 = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                if (!Open(start + vector * i))
                {
                    vector3 += vector;
                }

                lines.Add(new Line(vector2 + normal, vector3 + normal));
            }
        }
    }

    public bool Open(Vector2 tile)
    {
        Vector2 point = new Vector2(base.X + tile.X * 8f + 4f, base.Y + tile.Y * 8f + 4f);
        // Solid Check?
        /*
        if (!base.Scene.CollideCheck<SolidTiles>(point))
        {
            return !base.Scene.CollideCheck<GlassBlock>(point);
        }
        */

        return true; // default false
    }

    public override void Added(Scene scene)
    {
        intervene = false;
        base.Added(scene);
        level = SceneAs<Level>();
        scene.Add(pathRenderer = new(this, nodes, ropeColor, ropeLightColor));

    }

    public override void Removed(Scene scene)
    {
        scene.Remove(pathRenderer);
        pathRenderer = null;
        base.Removed(scene);

        Dispose();
    }

    public override void Update()
    {
        base.Update();

        bloom.Visible = streetlight.CurrentAnimationFrame != 0;
        bloom.Y = customSkin ? streetlight.CurrentAnimationFrame * 3 : 9;

        // OnDash
        dcr = DashCollisionResults.NormalCollision;
        scale = Calc.Approach(scale, Vector2.One, 3f * Engine.DeltaTime);

        streetlight.Scale = scale;
        Vector2 zeroCenter = new Vector2(Width, Height) / 2f;
        streetlight.Position = zeroCenter + (new Vector2(zeroCenter.X - streetlight.Width / 2f, 0) - zeroCenter) * scale;

    }

    private Vector2 BasePosition;

    public override void Render()
    {
        BasePosition = Position;
        Position += Shake;

        // Add a rectangle in BG
        /*
        foreach (Hitbox extension in Colliders)
        {
            Draw.Rect(extension.Left + X, extension.Top + Y, extension.Width, extension.Height, backgroundColor);
        }
         */


        base.Render();

        Position = BasePosition;

        // Glass Block
        foreach (Line line in lines)
        {
            Draw.Line(Position + line.A, Position + line.B, lineColor);
        }

        // Glass Block BG
        if (!hasBlocks)
        {
            return;
        }

        Vector2 position = (base.Scene as Level).Camera.Position;
        List<Entity> entities = base.Scene.Tracker.GetEntities<ZipGlass>();
        foreach (Entity item in entities)
        {
            Draw.Rect(item.X, item.Y, item.Width, item.Height, bgColor);
        }

        if (starsTarget != null && !starsTarget.IsDisposed)
        {
            foreach (Entity item2 in entities)
            {
                Rectangle value = new Rectangle((int)(item2.X - position.X), (int)(item2.Y - position.Y), (int)item2.Width, (int)item2.Height);
                Draw.SpriteBatch.Draw((RenderTarget2D)starsTarget, item2.Position, value, Color.White);
            }
        }

        if (beamsTarget == null || beamsTarget.IsDisposed)
        {
            return;
        }

        foreach (Entity item3 in entities)
        {
            Rectangle value2 = new Rectangle((int)(item3.X - position.X), (int)(item3.Y - position.Y), (int)item3.Width, (int)item3.Height);
            Draw.SpriteBatch.Draw((RenderTarget2D)beamsTarget, item3.Position, value2, Color.White);
        }
    }

    private bool _IsShallowAtRectangle(Rectangle rectangle)
    {
        return base.Scene.CollideCheck<Solid>(new Vector2(rectangle.Left, base.Top + 8f), new Vector2(rectangle.Right, base.Top + 8f));
    }

}
