using System;
using System.Collections;
using Celeste.Mod.Entities;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/FallingBlock")]
public class FallingBlock : Solid
{

    public bool Triggered;

    public float FallDelay;

    private char TileType;

    private TileGrid tiles;

    private TileGrid highlight;

    private bool finalBoss;

    private bool climbFall;

    public bool HasStartedFalling { get; private set; }

    private float topKillTimer;

    private float bottomKillTimer;

    private float leftKillTimer;

    private float rightKillTimer;

    private float currentKillTimer;

    private bool standFall;

    private Level level;

    public FallingBlock(Vector2 position, EntityData data) : base(position, data.Width, data.Height, false)
    {
        char tile = data.Char("tiletype", '3');
        this.topKillTimer = data.Float("topKillTimer", -1);
        if (this.topKillTimer == 0)
        {
            this.topKillTimer = Engine.DeltaTime;
        }
        this.bottomKillTimer = data.Float("bottomKillTimer", -1);
        if (this.bottomKillTimer == 0)
        {
            this.bottomKillTimer = Engine.DeltaTime;
        }
        this.leftKillTimer = data.Float("leftKillTimer", -1);
        if (this.leftKillTimer == 0)
        {
            this.leftKillTimer = Engine.DeltaTime;
        }
        this.rightKillTimer = data.Float("rightKillTimer", -1);
        if (this.rightKillTimer == 0)
        {
            this.rightKillTimer = Engine.DeltaTime;
        }
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
        if (data.Bool("behind", false))
        {
            base.Depth = 5000;
        }
    }

    public FallingBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    public static FallingBlock CreateFinalBossBlock(EntityData data, Vector2 offset)
    {
        return new FallingBlock(data.Position + offset, data);
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
            if (!this.standFall && base.HasPlayerOnTop())
            {
                return false;
            }
            return base.HasPlayerRider();
        }
        if (this.standFall)
        {
            return base.HasPlayerOnTop();
        }
        return false;
    }

    private bool PlayerWaitCheck()
    {
        if (Triggered)
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
            float timer = 0.4f;
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

            float speed = 0f;
            float maxSpeed = (finalBoss ? 130f : 160f);
            while (true)
            {
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                {
                    break;
                }

                if (Top > (float) (level.Bounds.Bottom + 16) || (Top > (float) (level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
                {
                    FallingBlock fallingBlock = this;
                    FallingBlock fallingBlock2 = this;
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

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
        int touch = this.GetPlayerTouch();
        if (touch > 0)
        {
            if (this.currentKillTimer > 0)
            {
                this.currentKillTimer -= Engine.DeltaTime;
                if (this.currentKillTimer <= 0)
                {
                    Player player = this.level.Tracker.GetEntity<Player>();
                    if (player == null)
                    {
                        return;
                    }
                    player.Die((player.Position - base.Position).SafeNormalize());
                }
            }
            else
            {
                this.currentKillTimer = (touch) switch
                {
                    1 => this.topKillTimer,
                    2 => this.bottomKillTimer,
                    3 => this.leftKillTimer,
                    4 => this.rightKillTimer,
                    _ => -1
                };
            }
        }
        else
        {
            this.currentKillTimer = 0;
        }
    }

    private int GetPlayerTouch()
    {
        foreach (Player player in this.level.Tracker.GetEntities<Player>())
        {
            if (base.CollideCheck(player, base.Position - Vector2.UnitY))
            {
                return 1;
            }
            if (base.CollideCheck(player, base.Position + Vector2.UnitY))
            {
                return 2;
            }
            if (player.Facing == Facings.Right && base.CollideCheck(player, base.Position - Vector2.UnitX))
            {
                return 3;
            }
            if (player.Facing == Facings.Left && base.CollideCheck(player, base.Position + Vector2.UnitX))
            {
                return 4;
            }
        }
        return 0;
    }

}
