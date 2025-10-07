using System;
using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using VivHelper.Entities;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/KillerBlock", "ChroniaHelper/SpringBlock")]
public class KillerBlock : BaseSolid
{

    public bool Triggered;

    public float FallDelay;

    private char TileType;

    private TileGrid tiles;

    private TileGrid highlight;

    private bool finalBoss;

    private bool climbFall;

    public bool HasStartedFalling { get; private set; }

    private bool standFall;

    private float shakeTime;

    private float x0, xm, a, xb;

    private bool canTrigger, dashRebound, dashReboundRefill;

    public KillerBlock(Vector2 position, EntityData data) : base(position, data)
    {
        char tile = data.Char("tiletype", '3');
        base.topKillTimer = data.Float("topKillTimer", -1);
        base.bottomKillTimer = data.Float("bottomKillTimer", -1);
        base.leftKillTimer = data.Float("leftKillTimer", -1);
        base.rightKillTimer = data.Float("rightKillTimer", -1);
        this.finalBoss = data.Bool("finalBoss", false);
        this.climbFall = data.Bool("climbFall", true);
        this.standFall = data.Bool("standFall", true);
        int newSeed = Calc.Random.Next();
        Calc.PushRandom(newSeed);
        Add(tiles = GFX.FGAutotiler.GenerateBox(tile, data.Width / 8, data.Height / 8).TileGrid);
        Calc.PopRandom();
        if (finalBoss)
        {
            Calc.PushRandom(newSeed);
            Add(highlight = GFX.FGAutotiler.GenerateBox('G', data.Width / 8, data.Height / 8).TileGrid);
            Calc.PopRandom();
            highlight.Alpha = 0f;
        }
        Add(new Coroutine(Sequence()));
        Add(new LightOcclude());
        Add(new TileInterceptor(tiles, highPriority: false));
        TileType = tile;
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
        if (data.Bool("behind")) //old
        {
            base.Depth = 5000;
        }
        else { base.Depth = 9000; }
        base.Depth = data.Int("depth");

        this.shakeTime = data.Float("fallDelay");
        if(this.shakeTime < 0f) { this.shakeTime = 0f; }

        //下落速度

        /*
        string[] speeds = data.Attr("movement").Split(',') ;
        this.x0 = (speeds[0] == null)||(float.Parse(speeds[0]) < 0f) ? 0f : float.Parse(speeds[0]);
        this.xm = (speeds[1] == null) || (float.Parse(speeds[1]) < 0f) ? 130f : float.Parse(speeds[1]);
        this.a = (speeds[2] == null)||(float.Parse(speeds[2]) < 0f) ? 500f : float.Parse(speeds[2]);
        this.xb = (speeds[3] == null) || (float.Parse(speeds[3]) < 0f) ? 160f : float.Parse(speeds[3]);
        if(this.x0 >= this.xm) { this.xm = this.x0; }
        if(this.xb <= this.xm) { this.xb = this.xm; }

        */
        this.x0 = 0f;
        this.xm = 130f;
        this.a = 500f;
        this.xb = 160f;

        this.canTrigger = data.Bool("canTrigger");

        // On dash
        dashRebound = data.Bool("dashRebound", false);
        dashReboundRefill = data.Bool("dashReboundRefill", false);
        OnDashCollide = OnDashed;

        springBlockOverride = data.Bool("springBlockOverride", false);
    }

    public KillerBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    public bool springBlockOverride;
    // On dashed
    public DashCollisionResults OnDashed(Player player, Vector2 dir)
    {
        if (dashRebound && !springBlockOverride)
        {
            Vector2 scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);

            if (dashReboundRefill)
            {
                player.RefillStamina();
                player.RefillDash();
            }
            Triggered = true;
            //Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_1", Center);
            // Was a test sound (for the smash vibe), cannot use because of never ending event with unrelated SFX.
            return DashCollisionResults.Rebound;
        }
        return DashCollisionResults.NormalCollision;
    }

    public static KillerBlock CreateFinalBossBlock(EntityData data, Vector2 offset)
    {
        return new KillerBlock(data.Position + offset, data);
    }

    public override void OnShake(Vector2 amount)
    {
        base.OnShake(amount);
        tiles.Position += amount;
        if (highlight != null)
        {
            highlight.Position += amount;
        }
    }

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        if (!finalBoss)
        {
            Triggered = true;
        }
    }

    private bool PlayerFallCheck()
    {
        if (this.climbFall)
        {
            if(!this.standFall && base.HasPlayerOnTop())
            {
                return false;
            }
            return HasPlayerRider(); //玩家抓住侧面或站在顶端
        }
        if (this.standFall)
        {
            return HasPlayerOnTop(); //玩家在顶端
        }
        return false;
    }

    private bool PlayerWaitCheck()
    {
        if (Triggered && this.canTrigger)
        {
            return true;
        }

        if (PlayerFallCheck())
        {
            return true;
        }

        if (climbFall)
        {
            if (!CollideCheck<Player>(Position - Vector2.UnitX))
            {
                return CollideCheck<Player>(Position + Vector2.UnitX);
            }

            return true;
        }

        if (this.standFall)
        {
            if (!base.CollideCheck<Player>(base.Position - Vector2.UnitY))
            {
                return base.CollideCheck<Player>(base.Position + Vector2.UnitY);
            }

            return true;
        }

        return false;
    }

    private IEnumerator Sequence()
    {
        
        while (!Triggered && (finalBoss || !PlayerFallCheck()))
        {
            yield return null;
        }

        while (!this.canTrigger)
        {
            yield return null;
        }

        while (FallDelay > 0f)
        {
            FallDelay -= Engine.DeltaTime;
            yield return null;
        }

        HasStartedFalling = true;
        while (true)
        {
            ShakeSfx();
            StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            if (finalBoss)
            {
                Add(new Coroutine(HighlightFade(1f)));
            }

            yield return 0.2f;
            float timer = this.shakeTime;
            if (finalBoss)
            {
                timer = 0.2f;
            }

            while (timer > 0f && PlayerWaitCheck())
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }

            StopShaking();
            for (int i = 2; (float) i < Width; i += 4)
            {
                if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                {
                    SceneAs<Level>().Particles.Emit(global::Celeste.FallingBlock.P_FallDustA, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f, (float) Math.PI / 2f);
                }

                SceneAs<Level>().Particles.Emit(global::Celeste.FallingBlock.P_FallDustB, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f);
            }

            //运动形式

            float speed = this.x0;
            float maxSpeed = (finalBoss ? this.xm : this.xb);
            while (true)
            {
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, this.a * Engine.DeltaTime);

                // >>更改运动参数
                if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                {
                    break;
                }

                if (Top > (float) (level.Bounds.Bottom + 16) || (Top > (float) (level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
                {
                    KillerBlock fallingBlock = this;
                    KillerBlock fallingBlock2 = this;
                    bool collidable = false;
                    fallingBlock2.Visible = false;
                    fallingBlock.Collidable = collidable;
                    yield return 0.2f;
                    if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                    {
                        yield return 0.2f;
                        SceneAs<Level>().Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    }

                    RemoveSelf();
                    DestroyStaticMovers();
                    yield break;
                }

                yield return null;
            }

            ImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().DirectionalShake(Vector2.UnitY, finalBoss ? 0.2f : 0.3f);
            if (finalBoss)
            {
                Add(new Coroutine(HighlightFade(0f)));
            }

            StartShaking();
            LandParticles();
            yield return 0.2f;
            StopShaking();
            if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
            {
                break;
            }

            while (CollideCheck<Platform>(Position + new Vector2(0f, 1f)))
            {
                yield return 0.1f;
            }
        }

        Safe = true;
    }

    private IEnumerator HighlightFade(float to)
    {
        float from = highlight.Alpha;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f)
        {
            highlight.Alpha = MathHelper.Lerp(from, to, Ease.CubeInOut(p));
            tiles.Alpha = 1f - highlight.Alpha;
            yield return null;
        }

        highlight.Alpha = to;
        tiles.Alpha = 1f - to;
    }

    private void LandParticles()
    {
        for (int i = 2; (float) i <= base.Width; i += 4)
        {
            if (base.Scene.CollideCheck<Solid>(base.BottomLeft + new Vector2(i, 3f)))
            {
                SceneAs<Level>().ParticlesFG.Emit(global::Celeste.FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float) i, base.Bottom), Vector2.One * 4f, -(float) Math.PI / 2f);
                float direction = ((!((float) i < base.Width / 2f)) ? 0f : ((float) Math.PI));
                SceneAs<Level>().ParticlesFG.Emit(global::Celeste.FallingBlock.P_LandDust, 1, new Vector2(base.X + (float) i, base.Bottom), Vector2.One * 4f, direction);
            }
        }
    }

    private void ShakeSfx()
    {
        if (TileType == '3')
        {
            Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", base.Center);
        }
        else if (TileType == '9')
        {
            Audio.Play("event:/game/03_resort/fallblock_wood_shake", base.Center);
        }
        else if (TileType == 'g')
        {
            Audio.Play("event:/game/06_reflection/fallblock_boss_shake", base.Center);
        }
        else
        {
            Audio.Play("event:/game/general/fallblock_shake", base.Center);
        }
    }

    private void ImpactSfx()
    {
        if (TileType == '3')
        {
            Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", base.BottomCenter);
        }
        else if (TileType == '9')
        {
            Audio.Play("event:/game/03_resort/fallblock_wood_impact", base.BottomCenter);
        }
        else if (TileType == 'g')
        {
            Audio.Play("event:/game/06_reflection/fallblock_boss_impact", base.BottomCenter);
        }
        else
        {
            Audio.Play("event:/game/general/fallblock_impact", base.BottomCenter);
        }
    }

    

    public override void AfterUpdate()
    {
        TimedKill();

        if (springBlockOverride)
        {
            playerTouch = GetPlayerTouch();

            if (!Input.Grab.Check)
            {
                if (playerTouch == 1)
                {
                    OnTouch(Vc2.UnitY);
                }
                else if (playerTouch == 2)
                {
                    OnTouch(-Vc2.UnitY);
                }
                else if (playerTouch == 3)
                {
                    OnTouch(Vc2.UnitX);
                }
                else if (playerTouch == 4)
                {
                    OnTouch(-Vc2.UnitX);
                }
            }

            if (playerTouch != 0)
            {
                Triggered = true;
            }
        }
    }

    public override void Render()
    {
        base.Render();
        RenderDangerBorder();
    }
}
