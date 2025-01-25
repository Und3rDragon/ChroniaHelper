using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System;
using ChroniaHelper.Utils;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using static On.Celeste.Player;

namespace ChroniaHelper.Entities;

// Sorted as a model for the omni zip entities

[Tracked(false)]
[CustomEntity("ChroniaHelper/OmniZipWater")]
public class OmniZipWater : OmniZipEntity
{
    #region Water funcs
    public class Ripple
    {
        public float Position;

        public float Speed;

        public float Height;

        public float Percent;

        public float Duration;
    }

    public class Tension
    {
        public float Position;

        public float Strength;
    }

    public class Ray
    {
        public float Position;

        public float Percent;

        public float Duration;

        public float Width;

        public float Length;

        public float MaxWidth;

        public Ray(float maxWidth)
        {
            MaxWidth = maxWidth;
            Reset(Calc.Random.NextFloat());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Reset(float percent)
        {
            Position = Calc.Random.NextFloat() * MaxWidth;
            Percent = percent;
            Duration = Calc.Random.Range(2f, 8f);
            Width = Calc.Random.Range(2, 16);
            Length = Calc.Random.Range(8f, 128f);
        }
    }

    public class Surface
    {
        public const int Resolution = 4;

        public const float RaysPerPixel = 0.2f;

        public const float BaseHeight = 6f;

        public readonly Vector2 Outwards;

        public readonly int Width;

        public readonly int BodyHeight;

        public Vector2 Position;

        public List<Ripple> Ripples = new List<Ripple>();

        public List<Ray> Rays = new List<Ray>();

        public List<Tension> Tensions = new List<Tension>();

        public float timer;

        public VertexPositionColor[] mesh;

        public int fillStartIndex;

        public int rayStartIndex;

        public int surfaceStartIndex;
        public Surface(Vector2 position, Vector2 outwards, float width, float bodyHeight)
        {
            Position = position;
            Outwards = outwards;
            Width = (int)width;
            BodyHeight = (int)bodyHeight;
            int num = (int)(width / 4f);
            int num2 = (int)(width * 0.2f);
            Rays = new List<Ray>();
            for (int i = 0; i < num2; i++)
            {
                Rays.Add(new Ray(width));
            }

            fillStartIndex = 0;
            rayStartIndex = num * 6;
            surfaceStartIndex = (num + num2) * 6;
            mesh = new VertexPositionColor[(num * 2 + num2) * 6];
            for (int j = fillStartIndex; j < fillStartIndex + num * 6; j++)
            {
                mesh[j].Color = FillColor;
            }

            for (int k = rayStartIndex; k < rayStartIndex + num2 * 6; k++)
            {
                mesh[k].Color = Color.Transparent;
            }

            for (int l = surfaceStartIndex; l < surfaceStartIndex + num * 6; l++)
            {
                mesh[l].Color = SurfaceColor;
            }
        }

        public float GetPointAlong(Vector2 position)
        {
            Vector2 vector = Outwards.Perpendicular();
            Vector2 vector2 = Position + vector * (-Width / 2);
            Vector2 lineB = Position + vector * (Width / 2);
            Vector2 vector3 = Calc.ClosestPointOnLine(vector2, lineB, position);
            return (vector2 - vector3).Length();
        }

        public Tension SetTension(Vector2 position, float strength)
        {
            Tension tension = new Tension
            {
                Position = GetPointAlong(position),
                Strength = strength
            };
            Tensions.Add(tension);
            return tension;
        }

        public void RemoveTension(Tension tension)
        {
            Tensions.Remove(tension);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DoRipple(Vector2 position, float multiplier)
        {
            float num = 80f;
            float num2 = 3f;
            float pointAlong = GetPointAlong(position);
            int num3 = 2;
            if (Width < 200)
            {
                num2 *= Calc.ClampedMap(Width, 0f, 200f, 0.25f);
                multiplier *= Calc.ClampedMap(Width, 0f, 200f, 0.5f);
            }

            Ripples.Add(new Ripple
            {
                Position = pointAlong,
                Speed = 0f - num,
                Height = (float)num3 * multiplier,
                Percent = 0f,
                Duration = num2
            });
            Ripples.Add(new Ripple
            {
                Position = pointAlong,
                Speed = num,
                Height = (float)num3 * multiplier,
                Percent = 0f,
                Duration = num2
            });
        }

        public void Update()
        {
            timer += Engine.DeltaTime;
            Vector2 vector = Outwards.Perpendicular();
            for (int num = Ripples.Count - 1; num >= 0; num--)
            {
                Ripple ripple = Ripples[num];
                if (ripple.Percent > 1f)
                {
                    Ripples.RemoveAt(num);
                }
                else
                {
                    ripple.Position += ripple.Speed * Engine.DeltaTime;
                    if (ripple.Position < 0f || ripple.Position > (float)Width)
                    {
                        ripple.Speed = 0f - ripple.Speed;
                        ripple.Position = Calc.Clamp(ripple.Position, 0f, Width);
                    }

                    ripple.Percent += Engine.DeltaTime / ripple.Duration;
                }
            }

            int num2 = 0;
            int num3 = fillStartIndex;
            int num4 = surfaceStartIndex;
            while (num2 < Width)
            {
                int num5 = num2;
                float surfaceHeight = GetSurfaceHeight(num5);
                int num6 = Math.Min(num2 + 4, Width);
                float surfaceHeight2 = GetSurfaceHeight(num6);
                mesh[num3].Position = new Vector3(Position + vector * (-Width / 2 + num5) + Outwards * surfaceHeight, 0f);
                mesh[num3 + 1].Position = new Vector3(Position + vector * (-Width / 2 + num6) + Outwards * surfaceHeight2, 0f);
                mesh[num3 + 2].Position = new Vector3(Position + vector * (-Width / 2 + num5), 0f);
                mesh[num3 + 3].Position = new Vector3(Position + vector * (-Width / 2 + num6) + Outwards * surfaceHeight2, 0f);
                mesh[num3 + 4].Position = new Vector3(Position + vector * (-Width / 2 + num6), 0f);
                mesh[num3 + 5].Position = new Vector3(Position + vector * (-Width / 2 + num5), 0f);
                mesh[num4].Position = new Vector3(Position + vector * (-Width / 2 + num5) + Outwards * (surfaceHeight + 1f), 0f);
                mesh[num4 + 1].Position = new Vector3(Position + vector * (-Width / 2 + num6) + Outwards * (surfaceHeight2 + 1f), 0f);
                mesh[num4 + 2].Position = new Vector3(Position + vector * (-Width / 2 + num5) + Outwards * surfaceHeight, 0f);
                mesh[num4 + 3].Position = new Vector3(Position + vector * (-Width / 2 + num6) + Outwards * (surfaceHeight2 + 1f), 0f);
                mesh[num4 + 4].Position = new Vector3(Position + vector * (-Width / 2 + num6) + Outwards * surfaceHeight2, 0f);
                mesh[num4 + 5].Position = new Vector3(Position + vector * (-Width / 2 + num5) + Outwards * surfaceHeight, 0f);
                num2 += 4;
                num3 += 6;
                num4 += 6;
            }

            Vector2 vector2 = Position + vector * ((float)(-Width) / 2f);
            int num7 = rayStartIndex;
            foreach (Ray ray in Rays)
            {
                if (ray.Percent > 1f)
                {
                    ray.Reset(0f);
                }

                ray.Percent += Engine.DeltaTime / ray.Duration;
                float num8 = 1f;
                if (ray.Percent < 0.1f)
                {
                    num8 = Calc.ClampedMap(ray.Percent, 0f, 0.1f);
                }
                else if (ray.Percent > 0.9f)
                {
                    num8 = Calc.ClampedMap(ray.Percent, 0.9f, 1f, 1f, 0f);
                }

                float num9 = Math.Max(0f, ray.Position - ray.Width / 2f);
                float num10 = Math.Min(Width, ray.Position + ray.Width / 2f);
                float num11 = Math.Min(BodyHeight, 0.7f * ray.Length);
                float num12 = 0.3f * ray.Length;
                Vector2 value = vector2 + vector * num9 + Outwards * GetSurfaceHeight(num9);
                Vector2 value2 = vector2 + vector * num10 + Outwards * GetSurfaceHeight(num10);
                Vector2 value3 = vector2 + vector * (num10 - num12) - Outwards * num11;
                Vector2 value4 = vector2 + vector * (num9 - num12) - Outwards * num11;
                mesh[num7].Position = new Vector3(value, 0f);
                mesh[num7].Color = RayTopColor * num8;
                mesh[num7 + 1].Position = new Vector3(value2, 0f);
                mesh[num7 + 1].Color = RayTopColor * num8;
                mesh[num7 + 2].Position = new Vector3(value4, 0f);
                mesh[num7 + 3].Position = new Vector3(value2, 0f);
                mesh[num7 + 3].Color = RayTopColor * num8;
                mesh[num7 + 4].Position = new Vector3(value3, 0f);
                mesh[num7 + 5].Position = new Vector3(value4, 0f);
                num7 += 6;
            }
        }

        public float GetSurfaceHeight(Vector2 position)
        {
            return GetSurfaceHeight(GetPointAlong(position));
        }

        public float GetSurfaceHeight(float position)
        {
            if (position < 0f || position > (float)Width)
            {
                return 0f;
            }

            float num = 0f;
            foreach (Ripple ripple in Ripples)
            {
                float num2 = Math.Abs(ripple.Position - position);
                float num3 = 0f;
                num3 = ((!(num2 < 12f)) ? Calc.ClampedMap(num2, 16f, 32f, -0.75f, 0f) : Calc.ClampedMap(num2, 0f, 16f, 1f, -0.75f));
                num += num3 * ripple.Height * Ease.CubeIn(1f - ripple.Percent);
            }

            num = Calc.Clamp(num, -4f, 4f);
            foreach (Tension tension in Tensions)
            {
                float t = Calc.ClampedMap(Math.Abs(tension.Position - position), 0f, 24f, 1f, 0f);
                num += Ease.CubeOut(t) * tension.Strength * 12f;
            }

            float val = position / (float)Width;
            num *= Calc.ClampedMap(val, 0f, 0.1f, 0.5f);
            num *= Calc.ClampedMap(val, 0.9f, 1f, 1f, 0.5f);
            num += (float)Math.Sin(timer + position * 0.1f);
            return num + 6f;
        }

        public void Render(Camera camera)
        {
            GFX.DrawVertices(camera.Matrix, mesh, mesh.Length);
        }
    }

    // Main funcs
    public static ParticleType P_Splash;

    // LightSkyBlue = 87cefa;
    public static Color FillColor = Color.LightSkyBlue * 0.3f;

    public static Color SurfaceColor = Color.LightSkyBlue * 0.8f;

    public static Color RayTopColor = Color.LightSkyBlue * 0.6f;

    public static Vector2 RayAngle = new Vector2(-4f, 8f).SafeNormalize();

    public Surface TopSurface;

    public Surface BottomSurface;

    public List<Surface> Surfaces = new List<Surface>();

    public Rectangle fill;

    public bool[,] grid;

    public Tension playerBottomTension;

    public HashSet<WaterInteraction> contains = new HashSet<WaterInteraction>();

    //新增参数

    //变更参数


    #endregion

    private BloomPoint bloom;

    public OmniZipWater(EntityData data, Vector2 offset)
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
              data.Bool("timeUnits", false),
              data.Bool("sideFlag", false),
              // Water params
              data.Bool("topSurface", true),
              data.Bool("bottomSurface", false),
              data.Attr("starColors", "7f9fba, 9bd1cd, bacae3"),
              data.Attr("starPath", "particles/ChroniaHelper/star"),
              data.Bool("drawTank", false)
              )
    { }

    public OmniZipWater(EntityData data, Vector2 position,
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
        bool unit, bool sideFlag,
        // Water params
        bool topSurface, bool bottomSurface,
        string starColor, string starPath, bool drawTank
        )
        : base(data, position)
    {
        #region common params

        Depth = Depths.FGTerrain + 1;
        entityPos = position;

        this.nodes = nodes;

        this.permanent = permanent;
        this.waits = waits;
        this.ticking = ticking;

        Add(seq = new Coroutine(Sequence()));
        Add(new LightOcclude());


        // Path Render
        string path;
        if (this.customSkin = !string.IsNullOrEmpty(customSkin))
        {
            cog = GFX.Game[customSkin + "/cog"];
            path = customSkin + "/light";
        }
        else
        {
            cog = GFX.Game["objects/zipmover/cog"];
            path = "objects/zipmover/light";
        }

        // Color Process
        if (!string.IsNullOrEmpty(bgColor)) { this.backgroundColor = Calc.HexToColor(bgColor); }
        if (!string.IsNullOrEmpty(ropeColor)) { this.ropeColor = Calc.HexToColor(ropeColor); }
        if (!string.IsNullOrEmpty(ropeLightColor)) { this.ropeLightColor = Calc.HexToColor(ropeLightColor); }

        
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
            Position = new Vector2(width / 2f, 4f)
        });

        Add(sfx = new SoundSource()
        {
            Position = new Vector2(Width, Height) / 2f
        });

        // New params

        this.delays = delays.Split(',', StringSplitOptions.TrimEntries);
        this.easing = ease.Split(',', StringSplitOptions.TrimEntries);
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
        this.drawTank = drawTank;

        // OnDash
        this.dashable = dashable;
        this.onlyDash = onlyDash;
        this.dashableOnce = dashableOnce;
        this.dashableRefill = dashableRefill;
        // OnDashCollide = OnDashed;

        // Return params
        this.returnDelays = returnDelays.Split(',', StringSplitOptions.TrimEntries);
        this.returnSpeeds = returnSpeeds.Split(',', StringSplitOptions.TrimEntries);
        irrespondingTime = irrespondTime;
        returnEasing = returnEase.Split(',', StringSplitOptions.TrimEntries);
         
        // new params
        timeUnits = unit;
        sideflag = sideFlag;

        #endregion common params

        #region water params
        Color waterC = this.backgroundColor;
        FillColor = waterC * 0.3f;
        SurfaceColor = waterC * 0.8f;
        RayTopColor = waterC * 0.6f;
        base.Tag = Tags.TransitionUpdate;
        // base.Depth = -9999;
        base.Collider = new Hitbox(width, height);
        grid = new bool[(int)(width / 8f), (int)(height / 8f)];
        fill = new Rectangle(0, 0, (int)width, (int)height);
        if (topSurface)
        {
            TopSurface = new Surface(Position, new Vector2(0f, -1f), width, height);
            Surfaces.Add(TopSurface);
            fill.Y += 8;
            fill.Height -= 8;
        }

        if (bottomSurface)
        {
            BottomSurface = new Surface(Position, new Vector2(0f, 1f), width, height);
            Surfaces.Add(BottomSurface);
            fill.Height -= 8;
        }

        Add(new DisplacementRenderHook(RenderDisplacement));
        #endregion

        // flag nodes?
        flagConditions = data.Attr("nodeFlags");
        conditionEmpty = string.IsNullOrEmpty(flagConditions);
        flags = FlagUtils.ParseList(flagConditions);
    }

    #region Water funcs
    public static void Load()
    {
        On.Celeste.Player.SwimCheck += Player_SwimCheck;
        On.Celeste.Player.SwimUnderwaterCheck += Player_SwimUnderwaterCheck;
        On.Celeste.Player.SwimJumpCheck += Player_SwimJumpCheck;
        On.Celeste.Player.SwimRiseCheck += Player_SwimRiseCheck;
        On.Celeste.Player.UnderwaterMusicCheck += Player_UnderwaterMusicCheck;
    }

    public static void Unload()
    {
        On.Celeste.Player.SwimCheck -= Player_SwimCheck;
        On.Celeste.Player.SwimUnderwaterCheck -= Player_SwimUnderwaterCheck;
        On.Celeste.Player.SwimJumpCheck -= Player_SwimJumpCheck;
        On.Celeste.Player.SwimRiseCheck -= Player_SwimRiseCheck;
        On.Celeste.Player.UnderwaterMusicCheck -= Player_UnderwaterMusicCheck;
    }

    public static bool Player_SwimCheck(On.Celeste.Player.orig_SwimCheck orig, Player self)
    {
        if (self.CollideCheck<OmniZipWater>(self.Position + Vector2.UnitY * -8f))
        {
            return self.CollideCheck<OmniZipWater>(self.Position);
        }

        return orig(self);
    }

    
    public static bool Player_SwimUnderwaterCheck(On.Celeste.Player.orig_SwimUnderwaterCheck orig, Player self)
    {
        if (self.CollideCheck<OmniZipWater>())
        {
            return self.CollideCheck<OmniZipWater>(self.Position + Vector2.UnitY * -9f);
        }

        return orig(self);
    }

    public static bool Player_SwimJumpCheck(orig_SwimJumpCheck orig, global::Celeste.Player self)
    {
        if(self.CollideCheck<OmniZipWater>())
        {
            return !self.CollideCheck<OmniZipWater>(self.Position + Vector2.UnitY * -14f);
        }

        return orig(self);
    }

    public static bool Player_SwimRiseCheck(orig_SwimRiseCheck orig, global::Celeste.Player self)
    {
        if (self.CollideCheck<OmniZipWater>())
        {
            return !self.CollideCheck<OmniZipWater>(self.Position + Vector2.UnitY * -18f);
        }

        return orig(self);
    }

    public static bool Player_UnderwaterMusicCheck(orig_UnderwaterMusicCheck orig, global::Celeste.Player self)
    {
        if (self.CollideCheck<OmniZipWater>(self.Position))
        {
            return self.CollideCheck<OmniZipWater>(self.Position + Vector2.UnitY * -12f);
        }

        return orig(self);
    }

    public void RenderDisplacement()
    {
        Color color = new Color(0.5f, 0.5f, 0.25f, 1f);
        int i = 0;
        int length = grid.GetLength(0);
        int length2 = grid.GetLength(1);
        for (; i < length; i++)
        {
            if (length2 > 0 && grid[i, 0])
            {
                Draw.Rect(base.X + (float)(i * 8), base.Y + 3f, 8f, 5f, color);
            }

            for (int j = 1; j < length2; j++)
            {
                if (grid[i, j])
                {
                    int k;
                    for (k = 1; j + k < length2 && grid[i, j + k]; k++)
                    {
                    }

                    Draw.Rect(base.X + (float)(i * 8), base.Y + (float)(j * 8), 8f, k * 8, color);
                    j += k - 1;
                }
            }
        }
    }
    #endregion

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        scene.Add(pathRenderer = new(this, nodes, ropeColor, ropeLightColor));

        #region water added
        // Water
        int i = 0;
        for (int length = grid.GetLength(0); i < length; i++)
        {
            int j = 0;
            for (int length2 = grid.GetLength(1); j < length2; j++)
            {
                grid[i, j] = !base.Scene.CollideCheck<Solid>(new Rectangle((int)base.X + i * 8, (int)base.Y + j * 8, 8, 8));
            }
        }
        #endregion
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
        bloom.Y = customSkin ? streetlight.CurrentAnimationFrame * 3 : 9;

        #region water update
        // Water
        foreach (Surface surface in Surfaces)
        {
            surface.Position = Position + new Vector2(Width /2, 8);
            surface.Update();
        }

        foreach (WaterInteraction component in base.Scene.Tracker.GetComponents<WaterInteraction>())
        {
            Vector2 absoluteCenter = component.AbsoluteCenter;
            Entity entity = component.Entity;
            bool flag = contains.Contains(component);
            bool flag2 = component.Check(this);
            if (flag != flag2)
            {
                if (absoluteCenter.Y <= base.Center.Y && TopSurface != null)
                {
                    TopSurface.DoRipple(absoluteCenter, 1f);
                }
                else if (absoluteCenter.Y > base.Center.Y && BottomSurface != null)
                {
                    BottomSurface.DoRipple(absoluteCenter, 1f);
                }

                bool flag3 = component.IsDashing();
                int num = ((absoluteCenter.Y < base.Center.Y && !_IsShallowAtRectangle(component.Bounds)) ? 1 : 0);
                if (flag)
                {
                    if (flag3)
                    {
                        Audio.Play("event:/char/madeline/water_dash_out", absoluteCenter, "deep", num);
                    }
                    else
                    {
                        Audio.Play("event:/char/madeline/water_out", absoluteCenter, "deep", num);
                    }

                    component.DrippingTimer = 2f;
                }
                else
                {
                    if (flag3 && num == 1)
                    {
                        Audio.Play("event:/char/madeline/water_dash_in", absoluteCenter, "deep", num);
                    }
                    else
                    {
                        Audio.Play("event:/char/madeline/water_in", absoluteCenter, "deep", num);
                    }

                    component.DrippingTimer = 0f;
                }

                if (flag)
                {
                    contains.Remove(component);
                }
                else
                {
                    contains.Add(component);
                }
            }

            if (BottomSurface == null || !(entity is Player))
            {
                continue;
            }

            if (flag2 && entity.Y > base.Bottom - 8f)
            {
                if (playerBottomTension == null)
                {
                    playerBottomTension = BottomSurface.SetTension(entity.Position, 0f);
                }

                playerBottomTension.Position = BottomSurface.GetPointAlong(entity.Position);
                playerBottomTension.Strength = Calc.ClampedMap(entity.Y, base.Bottom - 8f, base.Bottom + 4f);
            }
            else if (playerBottomTension != null)
            {
                BottomSurface.RemoveTension(playerBottomTension);
                playerBottomTension = null;
            }
        }

        #endregion
    }


    public override void Render()
    {
        Vector2 originalPosition = Position;

        if (isShaking)
        {
            Vector2 dPos = Calc.Random.Range(new Vector2(-2f, -2f), new Vector2(2f, 2f));
            Position += dPos;
        }


        base.Render();
        Position = originalPosition;

        #region water tank
        // Draw Tank
        if (drawTank)
        {
            foreach (Line line in lines)
            {
                Draw.Line(Position + line.A, Position + line.B, lineColor);
            }
        }
        #endregion
        #region water render
        // Water
        Draw.Rect(base.X + (float)fill.X, base.Y + (float)fill.Y, fill.Width, fill.Height, FillColor);
        GameplayRenderer.End();
        foreach (Surface surface in Surfaces)
        {
            surface.Render((base.Scene as Level).Camera);
        }

        GameplayRenderer.Begin();
        #endregion
    }
}
