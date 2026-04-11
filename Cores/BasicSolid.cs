using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

[Tracked(true)]
public class BasicSolid : Solid
{
    public BasicSolid(EntityData data, Vc2 offset, bool safe = false) : base(data.Position + offset,
        data.Width, data.Height, safe)
    {
        Nodes = data.NodesWithPosition(offset);
    }
    public int PlayerTouch;
    public Vc2[] Nodes;

    #region Touch Getter and Killer Setups
    public float topKillTimer = -1f;
    public float bottomKillTimer = -1f;
    public float leftKillTimer = -1f;
    public float rightKillTimer = -1f;
    public float currentKillTimer = -1f;
    public int GetPlayerTouch()
    {
        foreach (Player player in MaP.level.Tracker.GetEntities<Player>())
        {
            if (CollideCheck(player, Position - Vector2.UnitY))
            {
                return 1; // up
            }
            if (CollideCheck(player, Position + Vector2.UnitY))
            {
                return 2; // down
            }
            if (player.Facing == Facings.Right && CollideCheck(player, Position - Vector2.UnitX))
            {
                return 3; // left
            }
            if (player.Facing == Facings.Left && CollideCheck(player, Position + Vector2.UnitX))
            {
                return 4; // right
            }
        }
        return 0;
    }

    public void TimedKill()
    {
        PlayerTouch = GetPlayerTouch();
        if (PlayerTouch > 0)
        {
            if (topKillTimer == 0 && PlayerTouch == 1)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (bottomKillTimer == 0 && PlayerTouch == 2)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (leftKillTimer == 0 && PlayerTouch == 3)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (rightKillTimer == 0 && PlayerTouch == 4)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else
            {
                if (currentKillTimer > 0)
                {
                    currentKillTimer -= Engine.DeltaTime;
                    if (currentKillTimer <= 0)
                    {
                        Player player = MaP.level.Tracker.GetEntity<Player>();
                        if (player == null)
                        {
                            return;
                        }
                        player.Die((player.Position - Position).SafeNormalize());
                    }
                }
                else
                {
                    currentKillTimer = PlayerTouch switch
                    {
                        1 => topKillTimer,
                        2 => bottomKillTimer,
                        3 => leftKillTimer,
                        4 => rightKillTimer,
                        _ => -1
                    };
                }
            }
        }
        else
        {
            currentKillTimer = -1;
        }
    }
    #endregion
}
