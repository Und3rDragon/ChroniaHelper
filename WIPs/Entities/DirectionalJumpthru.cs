using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.WIPs.Entities;

[Tracked(true)]
public class DirectionalJumpThru : Platform
{
    public enum Direction
    {
        Up,    // 原版 JumpThru：从下穿，上站
        Down,  // 从上穿，下站（天花板）
        Left,  // 从右穿，左站（左墙）
        Right  // 从左穿，右站（右墙）
    }

    public Direction Facing { get; private set; }

    // 构造函数
    public DirectionalJumpThru(Vector2 position, int width, int height, Direction facing, bool safe = false)
        : base(position, safe)
    {
        Facing = facing;
        // 根据方向设置碰撞体尺寸和锚点（可选，用于渲染对齐）
        base.Collider = new Hitbox(width, height);
        base.Depth = -9000;
    }

    // 判断 Actor 是否“贴附”在平台的承载面上（且未试图穿透）
    private bool IsAttached(Actor actor)
    {
        if (!Collidable || actor == null) return false;

        Vector2 platformPos = Position;
        Vector2 actorPos = actor.Position;
        Hitbox platHit = Hitbox;
        Hitbox actorHit = actor.Collider as Hitbox;

        if (actorHit == null) return false;

        const float tolerance = 2f; // 像素容差

        switch (Facing)
        {
            case Direction.Up:
                // 承载面：平台顶部
                // 穿透方向：从下往上（Y+）
                return actor.Bottom >= platHit.Top - tolerance &&
                       actor.Bottom <= platHit.Top + tolerance &&
                       actor.Right > platHit.Left &&
                       actor.Left < platHit.Right &&
                       actor.Speed.Y >= 0; // 下落或静止（非向上穿）

            case Direction.Down:
                // 承载面：平台底部（天花板）
                // 穿透方向：从上往下（Y-）
                return actor.Top <= platHit.Bottom + tolerance &&
                       actor.Top >= platHit.Bottom - tolerance &&
                       actor.Right > platHit.Left &&
                       actor.Left < platHit.Right &&
                       actor.Speed.Y <= 0; // 上升或静止（非向下穿）

            case Direction.Left:
                // 承载面：平台左边缘
                // 穿透方向：从右往左（X-）
                return actor.Right <= platHit.Left + tolerance &&
                       actor.Right >= platHit.Left - tolerance &&
                       actor.Bottom > platHit.Top &&
                       actor.Top < platHit.Bottom &&
                       actor.Speed.X <= 0; // 向左移动或静止（非向右穿）

            case Direction.Right:
                // 承载面：平台右边缘
                // 穿透方向：从左往右（X+）
                return actor.Left >= platHit.Right - tolerance &&
                       actor.Left <= platHit.Right + tolerance &&
                       actor.Bottom > platHit.Top &&
                       actor.Top < platHit.Bottom &&
                       actor.Speed.X >= 0; // 向右移动或静止（非向左穿）

            default:
                return false;
        }
    }

    // 获取所有当前“贴附”的 Actor（包括 Player）
    private List<Actor> GetAttachedActors()
    {
        var attached = new List<Actor>();
        foreach (Actor actor in Scene.Tracker.GetEntities<Actor>())
        {
            if (IsAttached(actor))
            {
                attached.Add(actor);
            }
        }
        return attached;
    }

    // ===== 重写 Awake：绑定 StaticMover（可选，按需） =====
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        // 注意：StaticMover 通常只用于垂直/水平移动物体，方向性需额外处理
        // 此处省略，或按类似 IsAttached 逻辑绑定
    }

    // ===== 重写 MoveHExact / MoveVExact =====
    public override void MoveHExact(int move)
    {
        // 先移动平台自身
        base.X += move;

        // 再带动贴附的 Actor
        foreach (Actor actor in GetAttachedActors())
        {
            if (actor.TreatNaive)
                actor.NaiveMove(Vector2.UnitX * move);
            else
                actor.MoveHExact(move);
        }

        // 移动 StaticMover
        MoveStaticMovers(Vector2.UnitX * move);
    }

    public override void MoveVExact(int move)
    {
        base.Y += move;

        foreach (Actor actor in GetAttachedActors())
        {
            if (actor.TreatNaive)
                actor.NaiveMove(Vector2.UnitY * move);
            else
                actor.MoveVExact(move);
        }

        MoveStaticMovers(Vector2.UnitY * move);
    }
}
