using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System;
using ChroniaHelper.Utils;
using ChroniaHelper.Cores;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using static Celeste.Slider;
using System.IO;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/ZipDream")]
public class ZipDream : OmniZipSolid
{
    // Dream Block funcs

    public struct DreamParticle
    {
        public Vector2 Position;

        public int Layer;

        public Color Color;

        public float TimeOffset;
    }

    public sealed class _003C_003Ec__DisplayClass22_0
    {
        public Vector2 start;

        public Vector2 end;

        public DreamBlock _003C_003E4__this;

        public void _003CAdded_003Eb__0(Tween t)
        {
            double num = (double)start.X + ((double)end.X - (double)start.X) * (double)t.Eased;
            double num2 = (double)start.Y + ((double)end.Y - (double)start.Y) * (double)t.Eased;
            float moveH = (float)(num - (double)_003C_003E4__this.Position.X - (double)_003C_003E4__this._movementCounter.X);
            float moveV = (float)((double)JITBarrier((float)num2) - (double)_003C_003E4__this.Position.Y - (double)_003C_003E4__this._movementCounter.Y);
            if (_003C_003E4__this.Collidable)
            {
                _003C_003E4__this.MoveH(moveH);
                _003C_003E4__this.MoveV(moveV);
            }
            else
            {
                _003C_003E4__this.MoveHNaive(moveH);
                _003C_003E4__this.MoveVNaive(moveV);
            }
        }

        public static float JITBarrier(float v)
        {
            return v;
        }
    }

    
    public static Color activeBackColor = Color.Black;

    
    public static Color disabledBackColor = Calc.HexToColor("1f2e2d");

    
    public static Color activeLineColor = Color.White;

    
    public static Color disabledLineColor = Calc.HexToColor("6a8480");

    
    public bool playerHasDreamDash;

    public LightOcclude occlude;

    public Vector2? node;

    public MTexture[] particleTextures;

    
    public DreamParticle[] particles;

    
    public float whiteFill;

    
    public float whiteHeight = 1f;

    
    public Vector2 shake;

    
    public float animTimer;

    
    public Shaker shaker;

    
    public bool fastMoving;

    
    public bool oneUse;

    
    public float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

    
    public float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

    
    public float wobbleEase;

    private int randomSeed;

    public new Vector2 movementCounter
    {
        get;
    }

    // Main funcs


    public enum Themes
    {
        water,
        glass
    }
    public readonly Themes theme;

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


    public ZipDream(EntityData data, Vector2 offset)
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
              data.Enum<Themes>("theme",Themes.water),
              data.Attr("starColors", "7f9fba, 9bd1cd, bacae3"),
              data.Attr("starPath", "particles/ChroniaHelper/star"),
              data.Bool("timeUnits",false),
              data.Bool("sideFlag", false)
              )
    { }

    private bool customSkin;

    public struct Line
    {
        public Vector2 A;

        public Vector2 B;

        public Line(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }
    }

    public ZipDream(EntityData data, Vector2 position,
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
        bool topSurface, bool bottomSurface, Themes theme,
        string starColor, string starPath, bool unit, bool sideFlag
        )
        : base(data, position, width, height)
    {
        this.theme = theme;
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

        // Dream Block params

        base.Depth = -11000;
        this.fastMoving = fastMoving;
        this.oneUse = oneUse;

        SurfaceSoundIndex = 11;
        particleTextures = new MTexture[4]
        {
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
        };

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

        // Dream Block Added

        playerHasDreamDash = SceneAs<Level>().Session.Inventory.DreamDash;
        if (playerHasDreamDash && node.HasValue)
        {
            Vector2 start = Position;
            Vector2 end = node.Value;
            float num = Vector2.Distance(start, end) / 12f;
            if (fastMoving)
            {
                num /= 3f;
            }

            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                double num2 = (double)start.X + ((double)end.X - (double)start.X) * (double)t.Eased;
                double num3 = (double)start.Y + ((double)end.Y - (double)start.Y) * (double)t.Eased;
                float moveH = (float)(num2 - (double)Position.X - (double)_movementCounter.X);
                float moveV = (float)((double)_003C_003Ec__DisplayClass22_0.JITBarrier((float)num3) - (double)Position.Y - (double)_movementCounter.Y);
                if (Collidable)
                {
                    MoveH(moveH);
                    MoveV(moveV);
                }
                else
                {
                    MoveHNaive(moveH);
                    MoveVNaive(moveV);
                }
            };
            Add(tween);
        }

        if (!playerHasDreamDash)
        {
            Add(occlude = new LightOcclude());
        }

        Setup();

    }

    // << Dream Block

    public void Setup()
    {
        particles = new DreamParticle[(int)((double)base.Width / 8.0 * ((double)base.Height / 8.0) * 0.699999988079071)];
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
            particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
            particles[i].TimeOffset = Calc.Random.NextFloat();
            particles[i].Color = Color.LightGray * (0.5f + (float)particles[i].Layer / 2f * 0.5f);
            if (playerHasDreamDash)
            {
                switch (particles[i].Layer)
                {
                    case 0:
                        particles[i].Color = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                        break;
                    case 1:
                        particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                        break;
                    case 2:
                        particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
                        break;
                }
            }
        }
    }

    public void OnPlayerExit(Player player)
    {
        Dust.Burst(player.Position, player.Speed.Angle(), 16, null);
        Vector2 vector = Vector2.Zero;
        if (CollideCheck(player, Position + Vector2.UnitX * 4f))
        {
            vector = Vector2.UnitX;
        }
        else if (CollideCheck(player, Position - Vector2.UnitX * 4f))
        {
            vector = -Vector2.UnitX;
        }
        else if (CollideCheck(player, Position + Vector2.UnitY * 4f))
        {
            vector = Vector2.UnitY;
        }
        else if (CollideCheck(player, Position - Vector2.UnitY * 4f))
        {
            vector = -Vector2.UnitY;
        }

        _ = vector != Vector2.Zero;
        if (oneUse)
        {
            OneUseDestroy();
        }
    }

    public void OneUseDestroy()
    {
        Collidable = (Visible = false);
        DisableStaticMovers();
        RemoveSelf();
    }

    public bool BlockedCheck()
    {
        TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !TryActorWiggleUp(theoCrystal))
        {
            return true;
        }

        Player player = CollideFirst<Player>();
        if (player != null && !TryActorWiggleUp(player))
        {
            return true;
        }

        return false;
    }

    public bool TryActorWiggleUp(Entity actor)
    {
        bool collidable = Collidable;
        Collidable = true;
        for (int i = 1; i <= 4; i++)
        {
            if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
            {
                actor.Position -= Vector2.UnitY * i;
                Collidable = collidable;
                return true;
            }
        }

        Collidable = collidable;
        return false;
    }

    public Vector2 PutInside(Vector2 pos)
    {
        if (pos.X > base.Right)
        {
            pos.X -= (float)Math.Ceiling((pos.X - base.Right) / base.Width) * base.Width;
        }
        else if (pos.X < base.Left)
        {
            pos.X += (float)Math.Ceiling((base.Left - pos.X) / base.Width) * base.Width;
        }

        if (pos.Y > base.Bottom)
        {
            pos.Y -= (float)Math.Ceiling((pos.Y - base.Bottom) / base.Height) * base.Height;
        }
        else if (pos.Y < base.Top)
        {
            pos.Y += (float)Math.Ceiling((base.Top - pos.Y) / base.Height) * base.Height;
        }

        return pos;
    }

    public void WobbleLine(Vector2 from, Vector2 to, float offset)
    {
        float num = (to - from).Length();
        Vector2 vector = Vector2.Normalize(to - from);
        Vector2 vector2 = new Vector2(vector.Y, 0f - vector.X);
        Color color = (playerHasDreamDash ? activeLineColor : disabledLineColor);
        Color color2 = (playerHasDreamDash ? activeBackColor : disabledBackColor);
        if (whiteFill > 0f)
        {
            color = Color.Lerp(color, Color.White, whiteFill);
            color2 = Color.Lerp(color2, Color.White, whiteFill);
        }

        float num2 = 0f;
        int num3 = 16;
        for (int i = 2; (float)i < num - 2f; i += num3)
        {
            float num4 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
            if ((float)(i + num3) >= num)
            {
                num4 = 0f;
            }

            float num5 = Math.Min(num3, num - 2f - (float)i);
            Vector2 vector3 = from + vector * i + vector2 * num2;
            Vector2 vector4 = from + vector * ((float)i + num5) + vector2 * num4;
            Draw.Line(vector3 - vector2, vector4 - vector2, color2);
            Draw.Line(vector3 - vector2 * 2f, vector4 - vector2 * 2f, color2);
            Draw.Line(vector3, vector4, color);
            num2 = num4;
        }
    }

    public float LineAmplitude(float seed, float index)
    {
        return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
    }

    public float Lerp(float a, float b, float percent)
    {
        return a + (b - a) * percent;
    }

    public IEnumerator Activate()
    {
        Level level = SceneAs<Level>();
        yield return 1f;
        Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
        Add(shaker = new Shaker(on: true, delegate (Vector2 t)
        {
            shake = t;
        }));
        shaker.Interval = 0.02f;
        shaker.On = true;
        for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
        {
            whiteFill = Ease.CubeIn(p2);
            yield return null;
        }

        shaker.On = false;
        yield return 0.5f;
        ActivateNoRoutine();
        whiteHeight = 1f;
        whiteFill = 1f;
        for (float p2 = 1f; p2 > 0f; p2 -= Engine.DeltaTime * 0.5f)
        {
            whiteHeight = p2;
            if (level.OnInterval(0.1f))
            {
                for (int i = 0; (float)i < Width; i += 4)
                {
                    level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(X + (float)i, Y + Height * whiteHeight + 1f));
                }
            }

            if (level.OnInterval(0.1f))
            {
                level.Shake();
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            yield return null;
        }

        while (whiteFill > 0f)
        {
            whiteFill -= Engine.DeltaTime * 3f;
            yield return null;
        }
    }

    public void ActivateNoRoutine()
    {
        if (!playerHasDreamDash)
        {
            playerHasDreamDash = true;
            Setup();
            Remove(occlude);
        }

        whiteHeight = 0f;
        whiteFill = 0f;
        if (shaker != null)
        {
            shaker.On = false;
        }
    }

    public void FootstepRipple(Vector2 position)
    {
        if (playerHasDreamDash)
        {
            DisplacementRenderer.Burst burst = (base.Scene as Level).Displacement.AddBurst(position, 0.5f, 0f, 40f);
            burst.WorldClipCollider = base.Collider;
            burst.WorldClipPadding = 1;
        }
    }

    public void DeactivateNoRoutine()
    {
        if (playerHasDreamDash)
        {
            playerHasDreamDash = false;
            Setup();
            if (occlude == null)
            {
                occlude = new LightOcclude();
            }

            Add(occlude);
            whiteHeight = 1f;
            whiteFill = 0f;
            if (shaker != null)
            {
                shaker.On = false;
            }

            SurfaceSoundIndex = 11;
        }
    }

    public IEnumerator Deactivate()
    {
        Level level = SceneAs<Level>();
        yield return 1f;
        Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
        if (shaker == null)
        {
            shaker = new Shaker(on: true, delegate (Vector2 t)
            {
                shake = t;
            });
        }

        Add(shaker);
        shaker.Interval = 0.02f;
        shaker.On = true;
        for (float alpha2 = 0f; alpha2 < 1f; alpha2 += Engine.DeltaTime)
        {
            whiteFill = Ease.CubeIn(alpha2);
            yield return null;
        }

        shaker.On = false;
        yield return 0.5f;
        DeactivateNoRoutine();
        whiteHeight = 1f;
        whiteFill = 1f;
        for (float alpha2 = 1f; alpha2 > 0f; alpha2 -= Engine.DeltaTime * 0.5f)
        {
            whiteHeight = alpha2;
            if (level.OnInterval(0.1f))
            {
                for (int i = 0; (float)i < Width; i += 4)
                {
                    level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(X + (float)i, Y + Height * whiteHeight + 1f));
                }
            }

            if (level.OnInterval(0.1f))
            {
                level.Shake();
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            yield return null;
        }

        while (whiteFill > 0f)
        {
            whiteFill -= Engine.DeltaTime * 3f;
            yield return null;
        }
    }

    public IEnumerator FastDeactivate()
    {
        Level level = SceneAs<Level>();
        yield return null;
        Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        if (shaker == null)
        {
            shaker = new Shaker(on: true, delegate (Vector2 t)
            {
                shake = t;
            });
        }

        Add(shaker);
        shaker.Interval = 0.02f;
        shaker.On = true;
        for (float alpha = 0f; alpha < 1f; alpha += Engine.DeltaTime * 3f)
        {
            whiteFill = Ease.CubeIn(alpha);
            yield return null;
        }

        shaker.On = false;
        yield return 0.1f;
        DeactivateNoRoutine();
        whiteHeight = 1f;
        whiteFill = 1f;
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, TopCenter, Vector2.UnitX * Width / 2f, Color.White, (float)Math.PI);
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, BottomCenter, Vector2.UnitX * Width / 2f, Color.White, 0f);
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterLeft, Vector2.UnitY * Height / 2f, Color.White, 4.712389f);
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterRight, Vector2.UnitY * Height / 2f, Color.White, (float)Math.PI / 2f);
        level.Shake();
        yield return 0.1f;
        while (whiteFill > 0f)
        {
            whiteFill -= Engine.DeltaTime * 3f;
            yield return null;
        }
    }

    public IEnumerator FastActivate()
    {
        Level level = SceneAs<Level>();
        yield return null;
        Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        if (shaker == null)
        {
            shaker = new Shaker(on: true, delegate (Vector2 t)
            {
                shake = t;
            });
        }

        Add(shaker);
        shaker.Interval = 0.02f;
        shaker.On = true;
        for (float alpha = 0f; alpha < 1f; alpha += Engine.DeltaTime * 3f)
        {
            whiteFill = Ease.CubeIn(alpha);
            yield return null;
        }

        shaker.On = false;
        yield return 0.1f;
        ActivateNoRoutine();
        whiteHeight = 1f;
        whiteFill = 1f;
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, TopCenter, Vector2.UnitX * Width / 2f, Color.White, (float)Math.PI);
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, BottomCenter, Vector2.UnitX * Width / 2f, Color.White, 0f);
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterLeft, Vector2.UnitY * Height / 2f, Color.White, 4.712389f);
        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterRight, Vector2.UnitY * Height / 2f, Color.White, (float)Math.PI / 2f);
        level.Shake();
        yield return 0.1f;
        while (whiteFill > 0f)
        {
            whiteFill -= Engine.DeltaTime * 3f;
            yield return null;
        }
    }

    // >> Dream Block

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

        // OnDash
        dcr = DashCollisionResults.NormalCollision;
        scale = Calc.Approach(scale, Vector2.One, 3f * Engine.DeltaTime);

        streetlight.Scale = scale;
        Vector2 zeroCenter = new Vector2(Width, Height) / 2f;
        streetlight.Position = zeroCenter + (new Vector2(zeroCenter.X - streetlight.Width / 2f, 0) - zeroCenter) * scale;

        // Dream Block
        if (playerHasDreamDash)
        {
            animTimer += 6f * Engine.DeltaTime;
            wobbleEase += Engine.DeltaTime * 2f;
            if (wobbleEase > 1f)
            {
                wobbleEase = 0f;
                wobbleFrom = wobbleTo;
                wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
            }

            SurfaceSoundIndex = 12;
        }
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

        // Dream Block

        Camera camera = SceneAs<Level>().Camera;
        if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom)
        {
            return;
        }

        Draw.Rect(shake.X + base.X, shake.Y + base.Y, base.Width, base.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
        Vector2 position = SceneAs<Level>().Camera.Position;
        for (int i = 0; i < particles.Length; i++)
        {
            int layer = particles[i].Layer;
            Vector2 position2 = particles[i].Position;
            position2 += position * (0.3f + 0.25f * (float)layer);
            position2 = PutInside(position2);
            Color color = particles[i].Color;
            MTexture mTexture;
            switch (layer)
            {
                case 0:
                    {
                        int num2 = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                        mTexture = particleTextures[3 - num2];
                        break;
                    }
                case 1:
                    {
                        int num = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                        mTexture = particleTextures[1 + num];
                        break;
                    }
                default:
                    mTexture = particleTextures[2];
                    break;
            }

            if (position2.X >= base.X + 2f && position2.Y >= base.Y + 2f && position2.X < base.Right - 2f && position2.Y < base.Bottom - 2f)
            {
                mTexture.DrawCentered(position2 + shake, color);
            }
        }

        if (whiteFill > 0f)
        {
            Draw.Rect(base.X + shake.X, base.Y + shake.Y, base.Width, base.Height * whiteHeight, Color.White * whiteFill);
        }

        WobbleLine(shake + new Vector2(base.X, base.Y), shake + new Vector2(base.X + base.Width, base.Y), 0f);
        WobbleLine(shake + new Vector2(base.X + base.Width, base.Y), shake + new Vector2(base.X + base.Width, base.Y + base.Height), 0.7f);
        WobbleLine(shake + new Vector2(base.X + base.Width, base.Y + base.Height), shake + new Vector2(base.X, base.Y + base.Height), 1.5f);
        WobbleLine(shake + new Vector2(base.X, base.Y + base.Height), shake + new Vector2(base.X, base.Y), 2.5f);
        Draw.Rect(shake + new Vector2(base.X, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
        Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
        Draw.Rect(shake + new Vector2(base.X, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
        Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
    }

    private bool _IsShallowAtRectangle(Rectangle rectangle)
    {
        return base.Scene.CollideCheck<Solid>(new Vector2(rectangle.Left, base.Top + 8f), new Vector2(rectangle.Right, base.Top + 8f));
    }

}
