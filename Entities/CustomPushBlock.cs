using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Linq;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

/// <summary>
/// 可冲刺推动的分组方块：冲刺撞到可推的一面时整组沿该方向滑动，滑动中被挡住即停下；
/// 支持吸附面（该面贴住实体时不受重力）与下方失去支撑时自动滑落，
/// 材质按所在房间的 tileset 自动拼接绘制。
/// </summary>
[CustomEntity("ChroniaHelper/CustomPushBlock"), Tracked]
[ExA.Credits("CanyonHelper for PushBlock codes")]
public class CustomPushBlock : GroupedBaseSolid
{
    /// <summary>被撞后获得的速度。</summary>
    private float PushSpeed = 320f;

    /// <summary>滑动后每帧的减速。</summary>
    private const float Friction = 960f;

    /// <summary>悬空时的重力加速度与最大下落速度。</summary>
    private const float GravityAccel = 480f;

    private const float MaxFallSpeed = 160f;

    /// <summary>下方只靠一侧支撑时被挤向另一侧的速度。</summary>
    private const float EdgeSlideSpeed = 150f;

    /// <summary>同一次撞击中音效与碎屑的最短间隔。</summary>
    private const float ImpactInterval = 0.1f;

    /// <summary>被推动时贴到的开关，随推动一并触发。</summary>
    private DashSwitch[] dashSwitches;

    private Level level;

    private float speedX, speedY;

    private float gravity;

    private bool applyGravity;

    private bool lastTouchingGround = true;

    private float impactTimer;

    /// <summary>两个方向各自的亚像素余量。</summary>
    private float remainX, remainY;

    private bool stickyTop, stickyBottom, stickyLeft, stickyRight;

    private char debrisTileset;

    public CustomPushBlock(EntityData data, Vector2 offset) : base(data, offset)
    {
        tileType = data.Char("tiletype", '3');
        bgTexture = data.Bool("bgTexture", false);
        Depth = data.Int("depth", -12999);

        stickyTop = data.Bool("stickyTop", false);
        stickyBottom = data.Bool("stickyBottom", false);
        stickyLeft = data.Bool("stickyLeft", false);
        stickyRight = data.Bool("stickyRight", false);
        debrisTileset = data.Char("debrisTileset", '5');

        Add(new LightOcclude());
        OnDashCollide = OnDashed;

        PushSpeed = data.Float("pushSpeed", 320f);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;
        dashSwitches = scene.Entities.OfType<DashSwitch>().ToArray();
    }

    public override void Update()
    {
        base.Update();

        if (!MasterOfGroup)
        {
            return;
        }

        ApplyGravityCheck();

        impactTimer = Calc.Approach(impactTimer, 0f, Engine.DeltaTime);

        gravity = applyGravity
            ? Calc.Approach(gravity, MaxFallSpeed, GravityAccel * Engine.DeltaTime)
            : 0f;

        if (Scene.OnInterval(0.03f))
        {
            ScrapeParticles();
        }

        MoveHorizontally();

        MoveVertically();

        if (speedX != 0f || applyGravity)
        {
            return;
        }

        TrySlideOffEdge();
    }

    /// <summary>
    /// 水平滑动：按亚像素余量换算成整数像素后整组平移，结束时把速度减回零。
    /// </summary>
    private void MoveHorizontally()
    {
        remainX += speedX * Engine.DeltaTime;
        int pixels = (int)MathF.Round(remainX);
        remainX -= pixels;

        if (pixels != 0)
        {
            MoveGroupBy(pixels, false);
        }

        if (speedX == 0f)
        {
            return;
        }

        TriggerTouchedSwitches(Vc2.UnitX * MathF.Sign(speedX));

        speedX = Calc.Approach(speedX, 0f, Friction * Engine.DeltaTime);

        (Vc2 min, Vc2 max) = GetGroupBounds();
        if (min.X < level.Bounds.Left || max.X > level.Bounds.Right)
        {
            speedX = 0f;
        }
    }

    /// <summary>
    /// 竖直滑动：重力与推动速度合并计算，滑出房间下方时整组消失。
    /// </summary>
    private void MoveVertically()
    {
        remainY += (speedY + gravity) * Engine.DeltaTime;
        int pixels = (int)MathF.Round(remainY);
        remainY -= pixels;

        if (pixels != 0)
        {
            MoveGroupBy(pixels, true);
        }

        if (speedY == 0f)
        {
            return;
        }

        (Vc2 min, Vc2 max) = GetGroupBounds();
        if (min.Y < level.Bounds.Top)
        {
            speedY = 0f;
        }

        if (max.Y > level.Bounds.Bottom)
        {
            DestroyGroup();
            return;
        }

        TriggerTouchedSwitches(Vc2.UnitY * MathF.Sign(speedY));

        speedY = Calc.Approach(speedY, 0f, Friction * Engine.DeltaTime);
    }

    /// <summary>
    /// 整组刚性平移：先求全组都能通过的像素数，再让每块移动同样的距离，
    /// 这样整组始终保持原有的相对位置。
    /// </summary>
    private void MoveGroupBy(int pixels, bool vertical)
    {
        Vc2 axis = vertical ? Vc2.UnitY : Vc2.UnitX;
        int sign = Math.Sign(pixels);
        int allowed = 0;

        while (allowed != pixels)
        {
            if (GroupBlockedAt(axis * (allowed + sign)))
            {
                break;
            }

            allowed += sign;
        }

        if (allowed == 0)
        {
            return;
        }

        foreach (CustomPushBlock block in Group)
        {
            if (vertical)
            {
                block.MoveVExact(allowed);
            }
            else
            {
                block.MoveHExact(allowed);
            }
        }
    }

    /// <summary>
    /// 判断整组是否受重力：脚下有支撑、或有吸附面贴住实体时不受重力。
    /// </summary>
    private void ApplyGravityCheck()
    {
        bool touchingGround = GroupBlockedAt(Vc2.UnitY);

        if (touchingGround && !lastTouchingGround)
        {
            Audio.Play("event:/game/general/fallblock_impact", GetGroupBottomCenter());
            StartShaking(0.2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(Vc2.UnitY, 0.3f);
            LandParticles();
        }

        lastTouchingGround = touchingGround;

        applyGravity = !(touchingGround
            || (stickyTop && GroupBlockedAt(-Vc2.UnitY))
            || (stickyLeft && GroupBlockedAt(-Vc2.UnitX))
            || (stickyRight && GroupBlockedAt(Vc2.UnitX)));
    }

    /// <summary>
    /// 下方只剩一侧支撑时，整组朝失去支撑的那一侧滑落。
    /// </summary>
    private void TrySlideOffEdge()
    {
        (Vc2 min, Vc2 max) = GetGroupBounds();

        int unsupported = 0;
        if (!BlockedAtPoint(new Vc2(max.X, max.Y)))
        {
            unsupported++;
        }

        if (!BlockedAtPoint(new Vc2(min.X, max.Y)))
        {
            unsupported++;
        }

        if (unsupported != 1)
        {
            return;
        }

        if (min.X < level.Bounds.Left || max.X > level.Bounds.Right)
        {
            return;
        }

        if (BlockedAtPoint(new Vc2(min.X, max.Y)) && !BlockedAtPoint(new Vc2(min.X + 4f, max.Y + 1f)))
        {
            speedX = EdgeSlideSpeed;
        }
        else if (BlockedAtPoint(new Vc2(max.X, max.Y)) && !BlockedAtPoint(new Vc2(max.X - 4f, max.Y + 1f)))
        {
            speedX = -EdgeSlideSpeed;
        }
    }

    /// <summary>
    /// 玩家冲刺撞到方块：撞到可推的一面时整组获得该方向的速度并把玩家弹开，
    /// 否则按普通碰撞处理。
    /// </summary>
    private DashCollisionResults OnDashed(Player player, Vc2 direction)
    {
        bool fromSide = player.Left + 1f > Right || player.Right - 1f < Left;

        bool pushed = fromSide ? PushHorizontally(player.Center) : PushVertically(player.Center);

        return pushed ? DashCollisionResults.Rebound : DashCollisionResults.NormalCollision;
    }

    private bool PushHorizontally(Vc2 from)
    {
        bool pushRight = from.X < Position.X;

        if (GroupBlockedAt(Vc2.UnitX * (pushRight ? 1f : -1f)))
        {
            return false;
        }

        if (impactTimer <= 0f)
        {
            Audio.Play("event:/game/general/wall_break_stone", Position);
        }

        if (Left + 1f < level.Bounds.Left || Right + 1f > level.Bounds.Right)
        {
            return true;
        }

        Owner.speedX = pushRight ? PushSpeed : -PushSpeed;

        if (impactTimer <= 0f)
        {
            for (int i = 1; i < 7; i++)
            {
                Vc2 debrisAt = pushRight
                    ? TopLeft + new Vc2(-1f, i * 4f)
                    : TopRight + new Vc2(1f, i * 4f);

                Scene.Add(Engine.Pooler.Create<Debris>().Init(debrisAt, debrisTileset, true).BlastFrom(from));
            }

            impactTimer = ImpactInterval;
        }

        return true;
    }

    private bool PushVertically(Vc2 from)
    {
        bool pushDown = from.Y < Position.Y;

        if (GroupBlockedAt(Vc2.UnitY * (pushDown ? 1f : -1f)))
        {
            return false;
        }

        if (impactTimer <= 0f)
        {
            Audio.Play("event:/game/general/wall_break_stone", Position);
        }

        Owner.speedY = pushDown ? PushSpeed : -PushSpeed;

        if (impactTimer <= 0f)
        {
            for (int i = 1; i < 7; i++)
            {
                Vc2 debrisAt = pushDown
                    ? TopLeft + new Vc2(i * 4f, -2f)
                    : BottomLeft + new Vc2(i * 4f, 2f);

                Vc2 blastFrom = pushDown
                    ? TopCenter - Vc2.UnitY
                    : BottomCenter + Vc2.UnitY;

                Scene.Add(Engine.Pooler.Create<Debris>().Init(debrisAt, debrisTileset, true).BlastFrom(blastFrom));
            }

            impactTimer = ImpactInterval;
        }

        return true;
    }

    /// <summary>
    /// 推动中被方块贴住的开关随之触发。
    /// </summary>
    private void TriggerTouchedSwitches(Vc2 direction)
    {
        foreach (CustomPushBlock block in Group)
        {
            foreach (DashSwitch dashSwitch in dashSwitches)
            {
                if (block.CollideCheck(dashSwitch))
                {
                    dashSwitch.OnDashCollide?.Invoke(null, direction);
                }
            }
        }
    }

    /// <summary>
    /// 移动时在贴住实体的边缘持续冒出摩擦粒子。
    /// </summary>
    private void ScrapeParticles()
    {
        foreach (CustomPushBlock block in Group)
        {
            if (speedX != 0f)
            {
                for (int x = 0; x < block.Width; x += 8)
                {
                    EmitScrapePoint(new Vc2(block.Left + x, block.Bottom + 1f));
                    EmitScrapePoint(new Vc2(block.Left + x, block.Top - 1f));
                }
            }

            if (speedY != 0f)
            {
                for (int y = 0; y < block.Height; y += 8)
                {
                    EmitScrapePoint(new Vc2(block.Left - 1f, block.Top + y));
                    EmitScrapePoint(new Vc2(block.Right + 1f, block.Top + y));
                }
            }
        }
    }

    private void EmitScrapePoint(Vc2 at)
    {
        if (BlockedAtPoint(at))
        {
            level.ParticlesFG.Emit(ZipMover.P_Scrape, at);
        }
    }

    private void DestroyGroup()
    {
        SetGroupVisibility(false);

        foreach (CustomPushBlock block in Group)
        {
            block.RemoveSelf();
            block.DestroyStaticMovers();
        }
    }

    /// <summary>
    /// 整组当前占据的包围盒。
    /// </summary>
    private (Vc2 min, Vc2 max) GetGroupBounds()
    {
        Vc2 min = new(float.MaxValue, float.MaxValue);
        Vc2 max = new(float.MinValue, float.MinValue);

        foreach (CustomPushBlock block in Group)
        {
            min.X = MathF.Min(min.X, block.Left);
            min.Y = MathF.Min(min.Y, block.Top);
            max.X = MathF.Max(max.X, block.Right);
            max.Y = MathF.Max(max.Y, block.Bottom);
        }

        return (min, max);
    }

    /// <summary>
    /// 组内任一成员按指定偏移平移后是否会撞到实体；同组成员之间互不算阻挡。
    /// </summary>
    private bool GroupBlockedAt(Vc2 offset)
    {
        foreach (CustomPushBlock block in Group)
        {
            Platform hit = block.CollideFirst<Platform>(block.Position + offset);

            if (hit == null || IsGroupMember(hit))
            {
                continue;
            }

            // 向上移动可以穿过单向平台。
            if (offset.Y < 0f && hit is JumpThru)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 指定点是否落在实心砖块上；同组成员之间互不算。
    /// </summary>
    private bool BlockedAtPoint(Vc2 point)
    {
        foreach (Solid solid in level.Tracker.GetEntities<Solid>())
        {
            if (!solid.Collidable || IsGroupMember(solid))
            {
                continue;
            }

            if (solid.Collider.Collide(point))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsGroupMember(Entity entity)
    {
        return entity is GroupedBaseSolid solid && Group.Contains(solid);
    }

    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        if (other is not CustomPushBlock block)
        {
            return false;
        }

        return block.stickyTop == stickyTop
            && block.stickyBottom == stickyBottom
            && block.stickyLeft == stickyLeft
            && block.stickyRight == stickyRight
            && block.Depth == Depth
            && block.PushSpeed == PushSpeed;
    }

    public override void PostGroupping()
    {
        base.PostGroupping();

        SyncGroup<CustomPushBlock>(block =>
        {
            block.stickyTop = stickyTop;
            block.stickyBottom = stickyBottom;
            block.stickyLeft = stickyLeft;
            block.stickyRight = stickyRight;
            block.debrisTileset = debrisTileset;
            block.PushSpeed = PushSpeed;
        });
    }

    /// <summary>
    /// 组内由主块承担物理运算，其余成员只跟随主块移动。
    /// </summary>
    private CustomPushBlock Owner => MasterOfGroup ? this : master as CustomPushBlock;
}
