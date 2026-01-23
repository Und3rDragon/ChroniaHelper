using Microsoft.Xna.Framework;
using Monocle;

using ChroniaHelper.WIPs.Entities;

[Tracked(true)]
public class DirectionalJumpThru : Platform
{
    public enum Direction
    {
        Up,    // 从下穿，上站（原版 JumpThru）
        Down,  // 从上穿，下站（天花板）
        Left,  // 从右穿，左站（左墙）
        Right  // 从左穿，右站（右墙）
    }

    public Direction Facing { get; private set; }

    public DirectionalJumpThru(Vector2 position, int width, int height, Direction facing, bool safe = false)
        : base(position, safe)
    {
        Facing = facing;
        Collider = new Hitbox(width, height);
        Depth = -9000;
    }

    private bool IsAttached(Solid solid)
    {
        if (!Collidable || solid == null)
            return false;

        Hitbox platHit = Collider as Hitbox;
        Hitbox actorHit = solid.Collider as Hitbox;

        if (platHit == null || actorHit == null)
            return false;

        const float tolerance = 2f;
        Vector2 speed = solid.Speed;

        switch (Facing)
        {
            case Direction.Up:
                return solid.Bottom >= platHit.Top - tolerance &&
                       solid.Bottom <= platHit.Top + tolerance &&
                       solid.Right > platHit.Left &&
                       solid.Left < platHit.Right &&
                       speed.Y >= 0;

            case Direction.Down:
                return solid.Top <= platHit.Bottom + tolerance &&
                       solid.Top >= platHit.Bottom - tolerance &&
                       solid.Right > platHit.Left &&
                       solid.Left < platHit.Right &&
                       speed.Y <= 0;

            case Direction.Left:
                return solid.Right <= platHit.Left + tolerance &&
                       solid.Right >= platHit.Left - tolerance &&
                       solid.Bottom > platHit.Top &&
                       solid.Top < platHit.Bottom &&
                       speed.X <= 0;

            case Direction.Right:
                return solid.Left >= platHit.Right - tolerance &&
                       solid.Left <= platHit.Right + tolerance &&
                       solid.Bottom > platHit.Top &&
                       solid.Top < platHit.Bottom &&
                       speed.X >= 0;

            default:
                return false;
        }
    }

    private List<Solid> GetAttachedSolids()
    {
        var attached = new List<Solid>();
        foreach (Solid solid in Scene.Tracker.GetEntities<Solid>())
        {
            if (IsAttached(solid))
            {
                attached.Add(solid);
            }
        }
        return attached;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        // 绑定 StaticMover（使用普通碰撞，与原版一致）
        foreach (StaticMover mover in Scene.Tracker.GetComponents<StaticMover>())
        {
            if (mover.Entity is Solid solid && CollideCheck(solid))
            {
                mover.Platform = this;
            }
        }
    }

    public override void MoveHExact(int move)
    {
        X += move;

        // 带动所有贴附的 Solid（使用它们自己的 MoveHExact）
        foreach (Solid solid in GetAttachedSolids())
        {
            solid.MoveHExact(move);
        }

        MoveStaticMovers(Vector2.UnitX * move);
    }

    public override void MoveVExact(int move)
    {
        Y += move;

        foreach (Solid solid in GetAttachedSolids())
        {
            solid.MoveVExact(move);
        }

        MoveStaticMovers(Vector2.UnitY * move);
    }
}