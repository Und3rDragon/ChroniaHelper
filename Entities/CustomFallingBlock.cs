using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.References;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

/// <summary>
/// 可配置的下落方块：按指定方向加速下落，撞击后停止；支持分组。
/// </summary>
[CustomEntity("ChroniaHelper/CustomFallingBlock"), Tracked]
[ExA.Credits("Viv for VivHelper CustomFallingBlock codes")]
public class CustomFallingBlock : GroupedBaseSolid
{
    /// <summary>
    /// 下落方向对应的角度（0=下, 1=左, 2=上, 3=右）。
    /// </summary>
    private static float DirectionToAngle(int direction)
    {
        return direction switch
        {
            1 => MathF.PI,
            2 => MathF.PI / 2f * 3f,
            3 => 0f,
            _ => MathF.PI / 2f,
        };
    }

    /// <summary>
    /// 材质对应的撞击面中心（按下落方向取对应的一侧）。
    /// </summary>
    private Vc2 ImpactCenter
    {
        get
        {
            return angle switch
            {
                0f => TopLeft + new Vc2(Width, Height / 2f),
                MathF.PI / 2f => TopLeft + new Vc2(Width / 2f, Height),
                MathF.PI => TopLeft + new Vc2(0, Height / 2f),
                MathF.PI / 2f * 3f => TopLeft + new Vc2(Width / 2f, 0),
                _ => TopLeft + new Vc2(Width / 2f, Height),
            };
        }
    }

    public bool metaTriggered;
    public bool IsTriggeredByCrushBlock;
    public bool legacy, disableEndShake;
    public bool HasStartedFalling { get; private set; }
    public bool EndShaking { get; private set; }

    private TileGrid highlight;
    private bool finalBoss;
    private bool dashBlock;
    private string flagTrigger;
    private string flagOnFall;
    private string flagOnGround;
    private bool flagOnly;
    private string shakeSfx;
    private string impactSfx;
    private int direction;
    private float angle;
    private float accel;
    private float maxSpeed;
    private float fallDelay;

    public CustomFallingBlock(EntityData data, Vector2 offset, EntityID id)
        : base(data, offset, false)
    {
        accel = data.Float("accel", 500f);
        maxSpeed = data.Float("maxSpeed", 160f);
        shakeSfx = data.Attr("shakeSfx", "event:/game/general/fallblock_shake");
        impactSfx = data.Attr("impactSfx", "event:/game/general/fallblock_impact");
        direction = data.Int("direction", 0);
        angle = DirectionToAngle(direction);
        flagOnFall = data.Attr("flagOnFall", "");
        flagTrigger = data.Attr("flagTrigger", "");
        flagOnGround = data.Attr("flagOnGround", "");
        flagOnly = data.Bool("flagOnly");
        legacy = !data.Bool("legacy", true);
        dashBlock = data.Bool("breakDashBlocks", true);
        disableEndShake = data.Bool("shakeForAllFalls", false);
        fallDelay = data.Float("fallDelay");

        tileType = data.Char("tiletype", '3');
        climbFall = data.Bool("bufferClimbFall", false) ? true : data.Bool("climbFall", true);
        standFall = false;
        IsTriggeredByCrushBlock = data.Bool("crushBlockTrigger", false);
        finalBoss = data.Bool("finalBoss", false);

        blendIn = data.Bool("blendIn", false);
        bgTexture = data.Bool("bgTexture", false);

        Depth = data.Int("depth", data.Bool("behind") ? 5000 : -12999);

        if (climbFall)
        {
            this.AddOrAddToSolidModifierComponent(new SolidModifierComponent(0, true, false));
        }

        Add(new LightOcclude());

        Ldm.LoadHook(typeof(BounceHelperUtils));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        Level level = scene as Level;

        if (flagOnGround != "" && level.Session.GetFlag(flagOnGround))
            level.Session.SetFlag(flagOnGround, false);
        if (flagOnFall != "" && level.Session.GetFlag(flagOnFall))
            level.Session.SetFlag(flagOnFall, false);
        if (flagTrigger != "" && level.Session.GetFlag(flagTrigger))
            level.Session.SetFlag(flagTrigger, false);

        if (!MasterOfGroup)
        {
            return;
        }

        if (finalBoss)
        {
            Add(highlight = GFX.FGAutotiler.GenerateBox('G', (int)Width / 8, (int)Height / 8).TileGrid);
            highlight.Alpha = 0f;
        }

        Add(new Coroutine(Sequence()));
    }

    public override void Update()
    {
        base.Update();

        Level level = SceneAs<Level>();

        if (flagTrigger != "" && level.Session.GetFlag(flagTrigger) && !Triggered && !metaTriggered)
        {
            if (legacy)
                TriggerAll();
            else
                metaTriggered = true;
        }

        if (Triggered && flagOnFall != "" && !level.Session.GetFlag(flagOnFall))
            level.Session.SetFlag(flagOnFall, true);
    }

    public override void OnShake(Vector2 amount)
    {
        base.OnShake(amount);
        if (highlight != null)
            highlight.Position += amount;
    }

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        if (!finalBoss)
        {
            TriggerAll();
            metaTriggered = true;
        }
    }

    /// <summary>
    /// 玩家是否触发了下落（由分组内的块共同判定）。
    /// </summary>
    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        if (other is not CustomFallingBlock f)
            return false;

        return f.direction == direction
            && f.accel == accel
            && f.maxSpeed == maxSpeed
            && f.tileType == tileType
            && f.climbFall == climbFall
            && f.finalBoss == finalBoss
            && f.dashBlock == dashBlock
            && f.legacy == legacy
            && f.disableEndShake == disableEndShake
            && f.Depth == Depth;
    }

    public override void PostGroupping()
    {
        base.PostGroupping();

        foreach (CustomFallingBlock f in Group)
        {
            f.direction = (master as CustomFallingBlock).direction;
            f.angle = (master as CustomFallingBlock).angle;
            f.accel = (master as CustomFallingBlock).accel;
            f.maxSpeed = (master as CustomFallingBlock).maxSpeed;
            f.tileType = (master as CustomFallingBlock).tileType;
            f.climbFall = (master as CustomFallingBlock).climbFall;
            f.finalBoss = (master as CustomFallingBlock).finalBoss;
            f.dashBlock = (master as CustomFallingBlock).dashBlock;
            f.legacy = (master as CustomFallingBlock).legacy;
            f.disableEndShake = (master as CustomFallingBlock).disableEndShake;
            f.impactSfx = (master as CustomFallingBlock).impactSfx;
            f.shakeSfx = (master as CustomFallingBlock).shakeSfx;
        }
    }

    /// <summary>
    /// 玩家是否触发了下落：按 climbFall 选择站立或攀爬判定，组内任一成员满足即可。
    /// </summary>
    public override bool PlayerFallCheck()
    {
        if (metaTriggered && !legacy)
            return true;

        if (flagOnly)
            return false;

        foreach (CustomFallingBlock block in Group)
        {
            if (climbFall ? block.HasPlayerRider() : block.HasPlayerOnTop())
                return true;
        }

        return false;
    }

    /// <summary>
    /// 下落前是否要继续等待玩家离开。
    /// </summary>
    public override bool PlayerWaitCheck()
    {
        if (legacy ? metaTriggered : Triggered)
            return true;

        if (PlayerFallCheck())
            return true;

        if (climbFall)
        {
            foreach (CustomFallingBlock block in Group)
            {
                if (!block.CollideCheck<Player>(block.Position - Vc2.UnitX))
                {
                    if (block.CollideCheck<Player>(block.Position + Vc2.UnitX))
                        return true;
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerator Sequence()
    {
        while (!Triggered && !EndShaking && (finalBoss || !PlayerFallCheck()))
        {
            yield return null;
        }

        TriggerAll();
        SetGroupFlag(ref flagOnFall, true);

        while (!EndShaking && fallDelay > 0f)
        {
            fallDelay -= Engine.DeltaTime;
            yield return null;
        }

        SetGroupHasStartedFalling(true);

        while (true)
        {
            if (!EndShaking)
            {
                PlayShakeSfx();
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

                if (finalBoss)
                    Add(new Coroutine(HighlightFade(1f)));

                yield return 0.2f;

                float timer = finalBoss ? 0.2f : 0.4f;
                while (timer > 0f && PlayerWaitCheck())
                {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }

                StopShaking();
                SetGroupEndShaking(true);
            }

            foreach (CustomFallingBlock block in Group)
            {
                for (int i = 2; (float)i < block.Width; i += 4)
                {
                    if (Scene.CollideCheck<Solid>(block.TopLeft + new Vc2(i, -2f)))
                        SceneAs<Level>().Particles.Emit(FallDustA, 2, new Vc2(block.X + i, block.Y), Vc2.One * 4f, MathF.PI / 2f);

                    SceneAs<Level>().Particles.Emit(FallDustB, 2, new Vc2(block.X + i, block.Y), Vc2.One * 4f);
                }
            }

            float speed = 0f;
            while (true)
            {
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, accel * Engine.DeltaTime);

                float moveV = MathF.Sin(angle) * speed * Engine.DeltaTime;
                float moveH = MathF.Cos(angle) * speed * Engine.DeltaTime;

                // 任一组员撞到实体则整组停下：撞到的不回退（它已贴到障碍），
                // 其余组员回退本帧位移，保证全组位置一致
                bool hit = false;
                List<CustomFallingBlock> hittersV = new();
                List<CustomFallingBlock> hittersH = new();

                foreach (CustomFallingBlock block in Group)
                {
                    if (moveV != 0f && block.MoveVCollideSolids(moveV, thruDashBlocks: dashBlock))
                    {
                        hit = true;
                        hittersV.Add(block);
                    }

                    if (moveH != 0f && block.MoveHCollideSolids(moveH, thruDashBlocks: dashBlock))
                    {
                        hit = true;
                        hittersH.Add(block);
                    }
                }

                if (hit)
                {
                    foreach (CustomFallingBlock block in Group)
                    {
                        if (!hittersV.Contains(block))
                            block.MoveV(-moveV);

                        if (!hittersH.Contains(block))
                            block.MoveH(-moveH);
                    }
                    SetGroupFlag(ref flagOnGround, true);
                    break;
                }

                if (GroupBoundsMin.Y > level.Bounds.Bottom + 16
                    || (GroupBoundsMin.Y > level.Bounds.Bottom - 1 && CollideCheckGroupSolid()))
                {
                    SetGroupVisibility(false);
                    yield return 0.2f;

                    if (level.Session.MapData.CanTransitionTo(level, new Vc2(GetGroupCenter().X, GroupBoundsMax.Y + 12f)))
                    {
                        yield return 0.2f;
                        SceneAs<Level>().Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    }

                    foreach (CustomFallingBlock block in Group)
                    {
                        block.RemoveSelf();
                        block.DestroyStaticMovers();
                    }
                    yield break;
                }

                yield return null;
            }

            PlayImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().DirectionalShake(Vc2.UnitX.Rotate(angle), finalBoss ? 0.2f : 0.3f);

            if (finalBoss)
                Add(new Coroutine(HighlightFade(0f)));

            StartShaking();
            LandParticles();
            yield return 0.2f;
            StopShaking();

            if (!disableEndShake)
                SetGroupEndShaking(false);

            if (CollideGroupSolidTiles())
                break;

            while (CollideGroupPlatform())
                yield return 0.1f;
        }

        Safe = true;
    }

    /// <summary>
    /// 组内是否任一块的法线方向上是实心砖块。
    /// </summary>
    private bool CollideGroupSolidTiles()
    {
        foreach (CustomFallingBlock block in Group)
        {
            if (block.CollideCheck<SolidTiles>(block.Position + Vc2.UnitX.Rotate(angle)))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 组内是否任一块的法线方向上是可站立平台。
    /// </summary>
    private bool CollideGroupPlatform()
    {
        foreach (CustomFallingBlock block in Group)
        {
            foreach (Platform platform in Scene.Tracker.GetEntities<Platform>())
            {
                if (platform is CustomFallingBlock && Group.Contains(platform as CustomFallingBlock))
                    continue;

                if (block.CollideCheck(platform, block.Position + Vc2.UnitX.Rotate(angle)))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 组内是否任一块的撞击面碰到实心砖块。
    /// </summary>
    private bool CollideCheckGroupSolid()
    {
        foreach (CustomFallingBlock block in Group)
        {
            if (block.CollideCheck<Solid>(block.ImpactCenter))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 统一设置组内所有成员的状态。
    /// </summary>
    private void SetGroupFlag(ref string flag, bool value)
    {
        if (string.IsNullOrEmpty(flag))
            return;

        SceneAs<Level>().Session.SetFlag(flag, value);
    }

    private void SetGroupEndShaking(bool value)
    {
        foreach (CustomFallingBlock block in Group)
            block.EndShaking = value;
    }

    private void SetGroupHasStartedFalling(bool value)
    {
        foreach (CustomFallingBlock block in Group)
            block.HasStartedFalling = value;
    }

    private void SetGroupVisibility(bool value)
    {
        foreach (CustomFallingBlock block in Group)
            block.Collidable = block.Visible = value;
    }

    private IEnumerator HighlightFade(float to)
    {
        float from = highlight.Alpha;

        for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f)
        {
            highlight.Alpha = MathHelper.Lerp(from, to, Ease.CubeInOut(p));
            grid.Alpha = 1f - highlight.Alpha;
            yield return null;
        }

        highlight.Alpha = to;
        grid.Alpha = 1f - to;
    }

    /// <summary>
    /// 按配置的音效名播放下落前的摇晃音效。
    /// </summary>
    private void PlayShakeSfx()
    {
        Audio.Play(shakeSfx, GetGroupCenter());
    }

    /// <summary>
    /// 按配置的音效名播放落地音效。
    /// </summary>
    private void PlayImpactSfx()
    {
        Audio.Play(impactSfx, ImpactCenter);
    }
}
