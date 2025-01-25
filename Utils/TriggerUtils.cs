using System.Collections.Generic;

namespace ChroniaHelper.Utils;

public static class TriggerUtils
{

    public static ColliderSide CheckEnterColliderSide(Entity entity, Player player)
    {
        return TriggerUtils.DetermineColliderSide(GetEnterColliderSides(entity, player));
    }

    public static ColliderSide CheckLeaveColliderSide(Entity entity, Player player)
    {
        return TriggerUtils.DetermineColliderSide(GetLeaveColliderSides(entity, player));
    }

    private static ColliderSide DetermineColliderSide(bool[] colliderSides)
    {
        if (colliderSides[0] && colliderSides[1])
        {
            return ColliderSide.TopRight;
        }
        if (colliderSides[0] && colliderSides[3])
        {
            return ColliderSide.TopLeft;
        }
        if (colliderSides[2] && colliderSides[1])
        {
            return ColliderSide.BottomRight;
        }
        if (colliderSides[2] && colliderSides[3])
        {
            return ColliderSide.BottomLeft;
        }
        if (colliderSides[0])
        {
            return ColliderSide.Top;
        }
        if (colliderSides[1])
        {
            return ColliderSide.Right;
        }
        if (colliderSides[2])
        {
            return ColliderSide.Bottom;
        }
        if (colliderSides[3])
        {
            return ColliderSide.Left;
        }
        return ColliderSide.None;
    }

    private static bool[] GetEnterColliderSides(Entity entity, Player player)
    {
        return
        [
            (entity.Top <= player.Bottom && entity.Top >= player.Bottom - player.Speed.Y),
            (entity.Right >= player.Left && entity.Right <= player.Left - player.Speed.X),
            (entity.Bottom >= player.Top && entity.Bottom <= player.Top - player.Speed.Y),
            (entity.Left <= player.Right && entity.Left >= player.Right - player.Speed.X)
        ];
    }

    private static bool[] GetLeaveColliderSides(Entity entity, Player player)
    {
        return
        [
            (entity.Top >= player.Bottom && entity.Top <= player.Bottom - player.Speed.Y),
            (entity.Right <= player.Left && entity.Right >= player.Left - player.Speed.X),
            (entity.Bottom <= player.Top && entity.Bottom >= player.Top - player.Speed.Y),
            (entity.Left >= player.Right && entity.Left <= player.Right - player.Speed.X)
        ];
    }

    private delegate bool ColliderSideOperation(ColliderSide colliderSide);

    private static Dictionary<TriggerEnterMode, ColliderSideOperation> EnterColliderSideDictionary = new Dictionary<TriggerEnterMode, ColliderSideOperation>
    {
        { TriggerEnterMode.Top, (colliderSide) => colliderSide == ColliderSide.Top },
        { TriggerEnterMode.Right, (colliderSide) => colliderSide == ColliderSide.Right },
        { TriggerEnterMode.Bottom, (colliderSide) => colliderSide == ColliderSide.Bottom },
        { TriggerEnterMode.Left, (colliderSide) => colliderSide == ColliderSide.Left },
        { TriggerEnterMode.TopLeft, (colliderSide) => colliderSide == ColliderSide.TopLeft },
        { TriggerEnterMode.TopRight, (colliderSide) => colliderSide == ColliderSide.TopRight },
        { TriggerEnterMode.BottomLeft, (colliderSide) => colliderSide == ColliderSide.BottomLeft },
        { TriggerEnterMode.BottomRight, (colliderSide) => colliderSide == ColliderSide.BottomRight },
        { TriggerEnterMode.Horizontal, (colliderSide) => colliderSide == ColliderSide.Left || colliderSide == ColliderSide.Right },
        { TriggerEnterMode.Vertical, (colliderSide) => colliderSide == ColliderSide.Top || colliderSide == ColliderSide.Bottom },
        { TriggerEnterMode.Coordinate, (colliderSide) =>colliderSide == ColliderSide.Top || colliderSide == ColliderSide.Right || colliderSide == ColliderSide.Bottom || colliderSide == ColliderSide.Left },
        { TriggerEnterMode.ForwardSlash, (colliderSide) => colliderSide == ColliderSide.TopRight || colliderSide == ColliderSide.BottomLeft },
        { TriggerEnterMode.BackSlash, (colliderSide) => colliderSide == ColliderSide.TopLeft || colliderSide == ColliderSide.BottomRight },
        { TriggerEnterMode.Slash, (colliderSide) => colliderSide == ColliderSide.TopLeft || colliderSide == ColliderSide.TopRight || colliderSide == ColliderSide.BottomLeft || colliderSide == ColliderSide.BottomRight }
    };

    public static bool IsCorrectEnterMode(Entity entity, TriggerEnterMode enterMode, Player player)
    {
        if (enterMode == TriggerEnterMode.None)
        {
            return false;
        }
        if (enterMode != TriggerEnterMode.Any)
        {
            ColliderSide colliderSide = TriggerUtils.CheckEnterColliderSide(entity, player);
            if (!TriggerUtils.EnterColliderSideDictionary[enterMode](colliderSide))
            {
                return false;
            }
        }
        return true;
    }

    private static Dictionary<TriggerLeaveMode, ColliderSideOperation> LeaveColliderSideDictionary = new Dictionary<TriggerLeaveMode, ColliderSideOperation>
    {
        { TriggerLeaveMode.Top, (colliderSide) => colliderSide == ColliderSide.Top },
        { TriggerLeaveMode.Right, (colliderSide) => colliderSide == ColliderSide.Right },
        { TriggerLeaveMode.Bottom, (colliderSide) => colliderSide == ColliderSide.Bottom },
        { TriggerLeaveMode.Left, (colliderSide) => colliderSide == ColliderSide.Left },
        { TriggerLeaveMode.TopLeft, (colliderSide) => colliderSide == ColliderSide.TopLeft },
        { TriggerLeaveMode.TopRight, (colliderSide) => colliderSide == ColliderSide.TopRight },
        { TriggerLeaveMode.BottomLeft, (colliderSide) => colliderSide == ColliderSide.BottomLeft },
        { TriggerLeaveMode.BottomRight, (colliderSide) => colliderSide == ColliderSide.BottomRight },
        { TriggerLeaveMode.Horizontal, (colliderSide) => colliderSide == ColliderSide.Left || colliderSide == ColliderSide.Right },
        { TriggerLeaveMode.Vertical, (colliderSide) => colliderSide == ColliderSide.Top || colliderSide == ColliderSide.Bottom },
        { TriggerLeaveMode.Coordinate, (colliderSide) =>colliderSide == ColliderSide.Top || colliderSide == ColliderSide.Right || colliderSide == ColliderSide.Bottom || colliderSide == ColliderSide.Left },
        { TriggerLeaveMode.ForwardSlash, (colliderSide) => colliderSide == ColliderSide.TopRight || colliderSide == ColliderSide.BottomLeft },
        { TriggerLeaveMode.BackSlash, (colliderSide) => colliderSide == ColliderSide.TopLeft || colliderSide == ColliderSide.BottomRight },
        { TriggerLeaveMode.Slash, (colliderSide) => colliderSide == ColliderSide.TopLeft || colliderSide == ColliderSide.TopRight || colliderSide == ColliderSide.BottomLeft || colliderSide == ColliderSide.BottomRight }
    };

    public static bool IsCorrectLeaveMode(Entity entity, TriggerLeaveMode leaveMode, Player player)
    {
        if (leaveMode == TriggerLeaveMode.None)
        {
            return false;
        }
        if (leaveMode != TriggerLeaveMode.Any)
        {
            ColliderSide colliderSide = TriggerUtils.CheckLeaveColliderSide(entity, player);
            if (!TriggerUtils.LeaveColliderSideDictionary[leaveMode](colliderSide))
            {
                return false;
            }
        }
        return true;
    }

    private delegate bool RelationalOperation(int currentDeath, int deathCount);

    private static Dictionary<RelationalOperator, RelationalOperation> RelationalDictionary = new Dictionary<RelationalOperator, RelationalOperation>()
    {
        { RelationalOperator.Equal, (currentDeath, deathCount) => currentDeath == deathCount },
        { RelationalOperator.NotEqual, (currentDeath, deathCount) => currentDeath != deathCount },
        { RelationalOperator.LessThan, (currentDeath, deathCount) => currentDeath < deathCount },
        { RelationalOperator.LessThanOrEqual, (currentDeath, deathCount) => currentDeath <= deathCount },
        { RelationalOperator.GreaterThan, (currentDeath, deathCount) => currentDeath > deathCount },
        { RelationalOperator.GreaterThanOrEqual, (currentDeath, deathCount) => currentDeath >= deathCount }
    };

    public static bool IsDeathCount(Level level, RelationalOperator levelDeathMode, int levelDeathCount, RelationalOperator totalDeathMode, int totalDeathCount)
    {
        return (levelDeathCount < 0 || TriggerUtils.RelationalDictionary[levelDeathMode](level.Session.DeathsInCurrentLevel, levelDeathCount)) && (totalDeathCount < 0 || TriggerUtils.RelationalDictionary[totalDeathMode](level.Session.Deaths, totalDeathCount));
    }

}
