using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.Cores.SampleJumpThrough;

/// <summary>
/// 跳穿板共用的辅助方法。
/// </summary>
public static class SampleJumpThroughUtils
{
    /// <summary>
    /// 场景中所有实体所骑乘的平台集合。
    /// </summary>
    private static readonly HashSet<Actor> solidRiders =
        (HashSet<Actor>) typeof(Solid).GetField("riders", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

    /// <summary>
    /// 板体随所粘附的实体移动时的处理：推挤挡在前方的玩家，并携带正攀附板面的玩家一同移动。
    /// </summary>
    /// <param name="platform">板体本身。</param>
    /// <param name="playerInteractingSolid">承担玩家碰撞的临时实体。</param>
    /// <param name="left">板体是否朝左。</param>
    /// <param name="move">本次移动量。</param>
    public static void SidewaysJumpthruOnMove(Entity platform, Solid playerInteractingSolid, bool left, Vector2 move)
    {
        if (platform.Scene == null)
        {
            // 尚未进入场景时不做碰撞检测，直接移动
            platform.Position += move;
            playerInteractingSolid.MoveHNaive(move.X);
            playerInteractingSolid.MoveVNaive(move.Y);
            return;
        }

        bool playerHasToMove = false;

        // 板体朝玩家所在方向推挤时，交由临时实体去推动玩家
        if (platform.CollideCheckOutside<Player>(platform.Position + move) && (Math.Sign(move.X) == (left ? -1 : 1)))
        {
            playerHasToMove = true;
        }

        // 玩家正攀附板面时，临时实体需承载玩家一同移动
        if (platform.Collidable && GetPlayerClimbing(platform, left) != null)
        {
            playerHasToMove = true;
        }

        platform.Position += move;

        // 备份骑乘者，避免移动过程中被其他实体的移动干扰
        HashSet<Actor> ridersBackup = new HashSet<Actor>(solidRiders);
        solidRiders.Clear();

        playerInteractingSolid.Collidable = playerHasToMove;

        // 记录此刻骑乘在临时实体上的对象，稍后需排除以免重复移动
        List<Actor> platformRiders = new List<Actor>();
        if (playerInteractingSolid.Collidable)
        {
            foreach (Actor entity in platform.Scene.Tracker.GetEntities<Actor>())
            {
                if (entity.IsRiding(playerInteractingSolid))
                {
                    platformRiders.Add(entity);
                }
            }
        }

        Vector2 liftSpeed = playerInteractingSolid.LiftSpeed;
        playerInteractingSolid.MoveH(move.X, liftSpeed.X);
        playerInteractingSolid.MoveV(move.Y, liftSpeed.Y);
        playerInteractingSolid.Collidable = false;

        solidRiders.Clear();

        foreach (Actor rider in ridersBackup)
        {
            if (!platformRiders.Contains(rider))
            {
                solidRiders.Add(rider);
            }
        }
    }

    /// <summary>
    /// 取正攀附在板面上的玩家，同时校验玩家所处的一侧与板体朝向是否相符。
    /// </summary>
    /// <param name="platform">板体本身。</param>
    /// <param name="left">板体是否朝左。</param>
    public static Player GetPlayerClimbing(Entity platform, bool left)
    {
        if (platform.Scene == null)
        {
            return null;
        }

        foreach (Player player in platform.Scene.Tracker.GetEntities<Player>())
        {
            if (player.StateMachine.State == Player.StClimb)
            {
                if (!left && player.Facing == Facings.Left
                    && platform.CollideCheckOutside(player, platform.Position + Vector2.UnitX))
                {
                    return player;
                }

                if (left && player.Facing == Facings.Right
                    && platform.CollideCheckOutside(player, platform.Position - Vector2.UnitX))
                {
                    return player;
                }
            }
        }

        return null;
    }
}
