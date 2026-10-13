using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/GroupedKillerBlock", "ChroniaHelper/SpringBlockGrouped")]
class GroupedKillerBlock : GroupedBaseSolid
{
    public float FallDelay;
    public float shakeTime;
    public bool springBlockOverride;
    public bool HasStartedFalling { get; private set; }

    private bool dashRebound, dashReboundRefill;
    private float cooldown = 0.15f;
    private float defaultCooldown = 0.15f;

    public GroupedKillerBlock(EntityData data, Vc2 offset)
        : base(data, offset, data.Bool("safe", true))
    {
        climbFall = data.Bool("climbFall", true);
        tileType = data.Char("tiletype", '3');
        topKillTimer = data.Float("topKillTimer", -1);
        bottomKillTimer = data.Float("bottomKillTimer", -1);
        leftKillTimer = data.Float("leftKillTimer", -1);
        rightKillTimer = data.Float("rightKillTimer", -1);
        standFall = data.Bool("standFall", true);
        blendIn = data.Bool("blendIn");

        if (data.Bool("behind")) //old
        {
            Depth = 5000;
        }
        else
        {
            Depth = 9000;
        }
        Depth = data.Int("depth", Depth);

        Add(new LightOcclude());
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];

        shakeTime = data.Float("fallDelay");

        canTrigger = data.Bool("canTrigger");

        // 冲刺碰撞反弹
        dashRebound = data.Bool("dashRebound", false);
        dashReboundRefill = data.Bool("dashReboundRefill", false);
        OnDashCollide = OnDashed;

        springBlockOverride = data.Bool("springBlockOverride", false);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (!MasterOfGroup)
        {
            return;
        }

        Add(new Coroutine(Sequence()));
    }

    public override void Update()
    {
        base.Update();

        if (cooldown > 0f)
        {
            cooldown -= Engine.DeltaTime;
        }

        OnAfterUpdate();
    }

    /// <summary>
    /// 每帧在基类更新之后执行的计时击杀与弹簧块触碰判定。
    /// </summary>
    private void OnAfterUpdate()
    {
        TimedKill();

        if (springBlockOverride)
        {
            PlayerTouch = GetPlayerTouch();

            if (!Input.Grab.Check)
            {
                if (PlayerTouch == 1)
                {
                    OnTouch(Vc2.UnitY);
                }
                else if (PlayerTouch == 2)
                {
                    OnTouch(-Vc2.UnitY);
                }
                else if (PlayerTouch == 3)
                {
                    OnTouch(Vc2.UnitX);
                }
                else if (PlayerTouch == 4)
                {
                    OnTouch(-Vc2.UnitX);
                }
            }

            if (PlayerTouch != 0)
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

    /// <summary>
    /// 玩家冲刺撞击时按配置决定是否反弹。
    /// </summary>
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

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        if (MasterOfGroup)
        {
            Triggered = true;
        }
        else
        {
            master.Triggered = true;
        }
    }

    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        return other is GroupedKillerBlock k && k.tileType == tileType;
    }

    public override void PostGroupping()
    {
        base.PostGroupping();

        SyncGroup<GroupedKillerBlock>(k =>
        {
            k.climbFall = climbFall;
            k.standFall = standFall;
            k.tileType = tileType;
            k.shakeTime = shakeTime;
            k.FallDelay = FallDelay;
        });

        if (MasterOfGroup)
        {
            SortGroupByMoveDirection(Vc2.UnitY);
        }
    }

    private IEnumerator Sequence()
    {
        while (!Triggered && !PlayerFallCheck())
        {
            yield return null;
        }

        while (!canTrigger)
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
            yield return 0.2f;

            float timer = shakeTime;
            while (timer > 0f && PlayerWaitCheck())
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }

            StopShaking();

            foreach (GroupedKillerBlock block in Group)
            {
                for (int i = 2; i < Width; i += 4)
                {
                    if (block.CollideCheck<Solid>(block.TopLeft + new Vector2(i, -2f)))
                    {
                        SceneAs<Level>().Particles.Emit(FallDustA, 2, new Vector2(block.X + i, block.Y), Vector2.One * 4f, MathF.PI / 2f);
                    }
                    SceneAs<Level>().Particles.Emit(FallDustB, 2, new Vector2(block.X + i, block.Y), Vector2.One * 4f);
                }
            }

            float speed = 0f;
            float maxSpeed = 160f;

            while (true)
            {
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);

                // 骑乘玩家的携带由基类移动逻辑统一处理。
                bool hit = false;
                List<GroupedKillerBlock> hitters = new();

                foreach (GroupedKillerBlock block in Group)
                {
                    if (block.MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                    {
                        hit = true;
                        hitters.Add(block);
                    }
                }

                foreach (JumpThru jp in Jumpthrus)
                {
                    jp.MoveV(speed * Engine.DeltaTime);
                }

                if (hit)
                {
                    foreach (GroupedKillerBlock block in Group)
                    {
                        if (!hitters.Contains(block))
                        {
                            block.MoveV(-speed * Engine.DeltaTime);
                        }
                    }

                    foreach (JumpThru jp in Jumpthrus)
                    {
                        jp.MoveV(-speed * Engine.DeltaTime);
                    }

                    break;
                }

                if (GroupBoundsMin.Y > level.Bounds.Bottom + 16
                    || (GroupBoundsMin.Y > level.Bounds.Bottom - 1 && CollideCheck<Solid>(new Vc2(GroupBoundsMin.X, GroupBoundsMin.Y) + new Vector2(0f, 1f))))
                {
                    SetGroupVisibility(false);
                    yield return 0.2f;

                    if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                    {
                        yield return 0.2f;
                        SceneAs<Level>().Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    }

                    foreach (GroupedKillerBlock block in Group)
                    {
                        block.RemoveSelf();
                        block.DestroyStaticMovers();
                    }

                    foreach (JumpThru jp in Jumpthrus)
                    {
                        jp.RemoveSelf();
                        jp.DestroyStaticMovers();
                    }
                    yield break;
                }

                yield return null;
            }

            ImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
            StartShaking();
            LandParticles();
            yield return 0.2f;
            StopShaking();

            bool collideSolidTiles = false;
            foreach (GroupedKillerBlock block in Group)
            {
                if (block.CollideCheck<SolidTiles>(block.Position + new Vector2(0f, 1f)))
                {
                    collideSolidTiles = true;
                    break;
                }
            }
            if (collideSolidTiles) break;

            bool collidePlatforms;
            do
            {
                collidePlatforms = false;
                foreach (GroupedKillerBlock block in Group)
                {
                    foreach (Platform platform in Scene.Tracker.GetEntities<Platform>())
                    {
                        if (platform is GroupedKillerBlock && Group.Contains(platform as GroupedKillerBlock))
                        {
                            continue;
                        }
                        if (block.CollideCheck(platform, block.Position + new Vector2(0f, 1f)))
                        {
                            collidePlatforms = true;
                            break;
                        }
                    }
                    if (collidePlatforms) break;
                }
                if (collidePlatforms) yield return 0.1f;
            }
            while (collidePlatforms);
        }

        Safe = true;
    }

    /// <summary>
    /// 依据玩家触碰方向绘制危险警示线，并随计时击杀的进度渐显。
    /// </summary>
    private void RenderDangerBorder()
    {
        if (GetPlayerTouch() != 0)
        {
            float alpha = 0;
            if (GetPlayerTouch() == 1)
            {
                if (topKillTimer > 0 && currentKillTimer > 0)
                {
                    alpha = 1 - currentKillTimer / topKillTimer;
                }
                Draw.Line(Position, Position + new Vector2(Width, 0), Color.Red * alpha);
            }
            else if (GetPlayerTouch() == 2)
            {
                if (bottomKillTimer > 0 && currentKillTimer > 0)
                {
                    alpha = 1 - currentKillTimer / bottomKillTimer;
                }
                Draw.Line(Position + new Vector2(0, Height), Position + new Vector2(Width, Height), Color.Red * alpha);
            }
            else if (GetPlayerTouch() == 3)
            {
                if (leftKillTimer > 0 && currentKillTimer > 0)
                {
                    alpha = 1 - currentKillTimer / leftKillTimer;
                }
                Draw.Line(Position, Position + new Vector2(0, Height), Color.Red * alpha);
            }
            else if (GetPlayerTouch() == 4)
            {
                if (rightKillTimer > 0 && currentKillTimer > 0)
                {
                    alpha = 1 - currentKillTimer / rightKillTimer;
                }
                Draw.Line(Position + new Vector2(Width, 0), Position + new Vector2(Width, Height), Color.Red * alpha);
            }
        }
    }

    /// <summary>
    /// 触碰方向的玩家会被弹开，并消耗一次冷却。
    /// </summary>
    private void OnTouch(Vector2 dir)
    {
        // 方向以「上」与「左」为正
        if (cooldown > 0) { return; }

        if (PUt.TryGetAlivePlayer(out Player player))
        {
            if (dir == -Vector2.UnitX)
            {
                player.Speed = Vector2.Zero;
                player.SideBounce(1, Right, CenterY);
            }
            else if (dir == Vector2.UnitX)
            {
                player.Speed = Vector2.Zero;
                player.SideBounce(-1, Left, CenterY);
            }
            else if (dir == Vector2.UnitY)
            {
                player.Speed = Vector2.Zero;
                SideBounceY(player, 1, CenterX, Top);
            }
            else if (dir == -Vector2.UnitY)
            {
                player.Speed = Vector2.Zero;
                SideBounceY(player, -1, CenterX, Bottom);
            }
        }

        cooldown = defaultCooldown;
    }

    /// <summary>
    /// 沿竖直方向把玩家弹开：dir 为 1 表示从上方弹开，为 -1 表示从下方弹开。
    /// </summary>
    private bool SideBounceY(Player player, int dir, float fromX, float fromY)
    {
        if (Math.Abs(player.Speed.Y) > 185f && Math.Sign(player.Speed.Y) == dir)
        {
            return false;
        }

        Collider collider = player.Collider;
        player.Collider = player.normalHitbox;
        if (dir > 0)
        {
            player.MoveV(fromY - Top - 2f);
        }
        else if (dir < 0)
        {
            player.MoveV(fromY - Bottom + 2f);
        }

        if (!player.Inventory.NoRefills)
        {
            player.RefillDash();
        }

        player.RefillStamina();
        player.StateMachine.State = 0;
        if (dir > 0)
        {
            player.jumpGraceTimer = 0f;
            player.varJumpTimer = 0.2f;
            player.AutoJump = true;
            player.AutoJumpTimer = 0f;
        }
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
        player.launched = false;
        player.Speed.Y = 185f * (float)dir;
        SceneAs<Level>().DirectionalShake(Vector2.UnitY * dir, 0.1f);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        player.Sprite.Scale = new Vector2(0.5f, 1.5f);
        player.Collider = collider;
        return true;
    }
}
