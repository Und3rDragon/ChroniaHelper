using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System;
using ChroniaHelper.Utils;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using static On.Celeste.Player;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/OmniZipBumper")]
public class OmniZipBumper : OmniZipEntity
{
    #region bumper params
    public static ParticleType P_Ambience;

    public static ParticleType P_Launch;

    public static ParticleType P_FireAmbience;

    public static ParticleType P_FireHit;
    
    public Sprite sprite;
    
    public Sprite spriteEvil;
    
    public VertexLight light;

    
    // public BloomPoint bloom;
    
    public Vector2? node;
    
    public bool goBack;
    
    public Vector2 anchor;
    
    public SineWave sine;
    
    public float respawnTimer;
    
    public bool fireMode;
    
    public Wiggler hitWiggler;
    
    public Vector2 hitDir;
    #endregion

    public OmniZipBumper(EntityData data, Vector2 offset)
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
    private BloomPoint bloom;

    public OmniZipBumper(EntityData data, Vector2 position,
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
        Tag = Tags.TransitionUpdate;

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

        #region bumper inputs
        base.Collider = new Circle(12f);
        base.Collider.Position += new Vector2(Width / 2, Height / 2);
        Add(new PlayerCollider(OnPlayer));
        Add(sine = new SineWave(0.44f, 0f).Randomize());
        Add(sprite = GFX.SpriteBank.Create(data.Attr("bumperSprite","bumper")));
        Add(spriteEvil = GFX.SpriteBank.Create($"{data.Attr("bumperSprite","bumper")}_evil"));
        spriteEvil.Visible = false;
        Add(light = new VertexLight(Color.Teal, 1f, 16, 32));
        Add(bloom = new BloomPoint(0.5f, 16f));
        anchor = Position;
        if (node.HasValue)
        {
            Vector2 start = Position;
            Vector2 end = node.Value;
            Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.CubeInOut, 1.81818187f, start: true);
            tween.OnUpdate = (Tween t) =>
            {
                if (goBack)
                {
                    anchor = Vector2.Lerp(end, start, t.Eased);
                }
                else
                {
                    anchor = Vector2.Lerp(start, end, t.Eased);
                }
            };
            tween.OnComplete = delegate
            {
                goBack = !goBack;
            };
            Add(tween);
        }

        UpdatePosition();
        Add(hitWiggler = Wiggler.Create(1.2f, 2f, [MethodImpl(MethodImplOptions.NoInlining)] (float v) =>
        {
            spriteEvil.Position = hitDir * hitWiggler.Value * 8f;
        }));

        sprite.Position += new Vector2(Width / 2, Height / 2);
        spriteEvil.Position += new Vector2(Width / 2, Height / 2);

        Add(new CoreModeListener(OnChangeMode));
        #endregion

        // flag nodes?
        flagConditions = data.Attr("nodeFlags");
        conditionEmpty = string.IsNullOrEmpty(flagConditions);
        flags = FlagUtils.ParseList(flagConditions);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        scene.Add(pathRenderer = new(this, nodes, ropeColor, ropeLightColor));

        #region bumper added

        fireMode = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
        spriteEvil.Visible = fireMode;
        sprite.Visible = !fireMode;

        #endregion
    }

    public override void Removed(Scene scene)
    {
        scene.Remove(pathRenderer);
        pathRenderer = null;
        base.Removed(scene);
    }

    #region bumper funcs

    public void OnChangeMode(Session.CoreModes coreMode)
    {
        fireMode = coreMode == Session.CoreModes.Hot;
        spriteEvil.Visible = fireMode;
        sprite.Visible = !fireMode;
    }
    public void UpdatePosition()
    {
        // Position = new Vector2((float)((double)anchor.X + (double)sine.Value * 3.0), (float)((double)anchor.Y + (double)sine.ValueOverTwo * 2.0));
    }

    public void OnPlayer(Player player)
    {
        Vector2 anchoredPos = Position + new Vector2(Width / 2, Height / 2);
        if (fireMode)
        {
            if (!SaveData.Instance.Assists.Invincible)
            {
                Vector2 vector = (player.Center - anchoredPos).SafeNormalize();
                hitDir = -vector;
                hitWiggler.Start();
                Audio.Play("event:/game/09_core/hotpinball_activate", anchoredPos);
                respawnTimer = 0.6f;
                player.Die(vector);
                //SceneAs<Level>().Particles.Emit(P_FireHit, 12, base.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
            }
        }
        else if (respawnTimer <= 0f)
        {
            if ((base.Scene as Level).Session.Area.ID == 9)
            {
                Audio.Play("event:/game/09_core/pinballbumper_hit", anchoredPos);
            }
            else
            {
                Audio.Play("event:/game/06_reflection/pinballbumper_hit", anchoredPos);
            }

            respawnTimer = 0.6f;
            Vector2 vector2 = player.ExplodeLaunch(anchoredPos, snapUp: false, sidesOnly: false);
            sprite.Play("hit", restart: true);
            spriteEvil.Play("hit", restart: true);
            light.Visible = false;
            bloom.Visible = false;
            SceneAs<Level>().DirectionalShake(vector2, 0.15f);
            SceneAs<Level>().Displacement.AddBurst(anchoredPos, 0.3f, 8f, 32f, 0.8f);
            //SceneAs<Level>().Particles.Emit(P_Launch, 12, base.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
        }
    }

    #endregion


    public override void Update()
    {

        base.Update();

        bloom.Visible = streetlight.CurrentAnimationFrame != 0;
        bloom.Y = customSkin ? streetlight.CurrentAnimationFrame * 3 : 9;

        #region bumper updates

        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                light.Visible = true;
                bloom.Visible = true;
                sprite.Play("on");
                spriteEvil.Play("on");
                if (!fireMode)
                {
                    Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                }
            }
        }
        else if (base.Scene.OnInterval(0.05f))
        {
            float num = Calc.Random.NextAngle();
            ParticleType type = (fireMode ? P_FireAmbience : P_Ambience);
            float direction = (fireMode ? (-(float)Math.PI / 2f) : num);
            float length = (fireMode ? 12 : 8);
            //SceneAs<Level>().Particles.Emit(type, 1, base.Center + Calc.AngleToVector(num, length), Vector2.One * 2f, direction);
        }

        UpdatePosition();

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
    }
}
