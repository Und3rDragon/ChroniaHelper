using System.Collections.Generic;
using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

public static class TriggerUtils
{

    public static bool CheckCorrectEnterDirection(Entity entity, Player player, TriggerDirections enterDirection)
    {
        return TriggerUtils.CheckCorrectDirection(entity, player, enterDirection, true);
    }

    public static bool CheckCorrectLeaveDirection(Entity entity, Player player, TriggerDirections leaveDirection)
    {
        return TriggerUtils.CheckCorrectDirection(entity, player, leaveDirection, false);
    }

    private static bool CheckCorrectDirection(Entity entity, Player player, TriggerDirections direction, bool isEnter)
    {
        if ((ObjectUtils.IsNull(entity)) || (ObjectUtils.IsNull(player)) || (direction == TriggerDirections.None))
        {
            return false;
        }
        if (direction != TriggerDirections.Any)
        {
            ColliderSides colliderSide = TriggerUtils.GetColliderSide((isEnter ? TriggerUtils.GetEnterDirectionState(entity, player) : TriggerUtils.GetLeaveDirectionState(entity, player)));
            if (!TriggerUtils.CheckColliderDirection[direction](colliderSide))
            {
                return false;
            }
        }
        return true;
    }

    private static bool[] GetEnterDirectionState(Entity entity, Player player)
    {
        return
        [
            ((entity.Top <= player.Bottom) && (entity.Top >= (player.Bottom - player.Speed.Y))),
            ((entity.Right >= player.Left) && (entity.Right <= (player.Left - player.Speed.X))),
            ((entity.Bottom >= player.Top) && (entity.Bottom <= (player.Top - player.Speed.Y))),
            ((entity.Left <= player.Right) && (entity.Left >= (player.Right - player.Speed.X)))
        ];
    }

    private static bool[] GetLeaveDirectionState(Entity entity, Player player)
    {
        return
        [
            ((entity.Top >= player.Bottom) && (entity.Top <= (player.Bottom - player.Speed.Y))),
            ((entity.Right <= player.Left) && (entity.Right >= (player.Left - player.Speed.X))),
            ((entity.Bottom <= player.Top) && (entity.Bottom >= (player.Top - player.Speed.Y))),
            ((entity.Left >= player.Right) && (entity.Left <= (player.Right - player.Speed.X)))
        ];
    }

    private static ColliderSides GetColliderSide(bool[] colliderSides)
    {
        if (colliderSides[0])
        {
            if (colliderSides[1])
            {
                return ColliderSides.TopRight;
            }
            if (colliderSides[3])
            {
                return ColliderSides.TopLeft;
            }
            return ColliderSides.Top;
        }
        if (colliderSides[1])
        {
            return ColliderSides.Right;
        }
        if (colliderSides[2])
        {
            if (colliderSides[1])
            {
                return ColliderSides.BottomRight;
            }
            if (colliderSides[3])
            {
                return ColliderSides.BottomLeft;
            }
            return ColliderSides.Bottom;
        }
        if (colliderSides[3])
        {
            return ColliderSides.Left;
        }
        return ColliderSides.None;
    }

    private delegate bool ColliderSideOperation(ColliderSides colliderSide);

    private static Dictionary<TriggerDirections, ColliderSideOperation> CheckColliderDirection = new Dictionary<TriggerDirections, ColliderSideOperation>
    {
        { TriggerDirections.Top, (colliderSide) => (colliderSide == ColliderSides.Top) },
        { TriggerDirections.Right, (colliderSide) => (colliderSide == ColliderSides.Right) },
        { TriggerDirections.Bottom, (colliderSide) => (colliderSide == ColliderSides.Bottom) },
        { TriggerDirections.Left, (colliderSide) => (colliderSide == ColliderSides.Left) },
        { TriggerDirections.TopLeft, (colliderSide) => (colliderSide == ColliderSides.TopLeft) },
        { TriggerDirections.TopRight, (colliderSide) => (colliderSide == ColliderSides.TopRight) },
        { TriggerDirections.BottomLeft, (colliderSide) => (colliderSide == ColliderSides.BottomLeft) },
        { TriggerDirections.BottomRight, (colliderSide) => (colliderSide == ColliderSides.BottomRight) },
        { TriggerDirections.Horizontal, (colliderSide) => ((colliderSide == ColliderSides.Left) || (colliderSide == ColliderSides.Right)) },
        { TriggerDirections.Vertical, (colliderSide) => ((colliderSide == ColliderSides.Top) || (colliderSide == ColliderSides.Bottom)) },
        { TriggerDirections.Orientational, (colliderSide) => ((colliderSide == ColliderSides.Top) || (colliderSide == ColliderSides.Right) || (colliderSide == ColliderSides.Bottom) || (colliderSide == ColliderSides.Left)) },
        { TriggerDirections.TopRightOrBottomLeft, (colliderSide) => ((colliderSide == ColliderSides.TopRight) || (colliderSide == ColliderSides.BottomLeft)) },
        { TriggerDirections.TopLeftOrBottomRight, (colliderSide) => ((colliderSide == ColliderSides.TopLeft) || (colliderSide == ColliderSides.BottomRight)) },
        { TriggerDirections.Diagonal, (colliderSide) => ((colliderSide == ColliderSides.TopLeft) || (colliderSide == ColliderSides.TopRight) || (colliderSide == ColliderSides.BottomLeft) || (colliderSide == ColliderSides.BottomRight)) }
    };

}
