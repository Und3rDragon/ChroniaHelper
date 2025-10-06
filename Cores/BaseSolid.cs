using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Monocle;


namespace ChroniaHelper.Cores;

public class BaseSolid : Solid
{
    public Level level;
    public BaseSolid(Vector2 position, EntityData data) : base(position, data.Width, data.Height, true)
    {
        
    }
    public PlayerCollider playerCollider;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }
    public int GetPlayerTouch()
    {
        foreach (Player player in level.Tracker.GetEntities<Player>())
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

    public float topKillTimer;

    public float bottomKillTimer;

    public float leftKillTimer;

    public float rightKillTimer;

    public float currentKillTimer;

    public void TimedKill()
    {
        int touch = GetPlayerTouch();
        if (touch > 0)
        {
            if (topKillTimer == 0 && touch == 1)
            {
                Player player = level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (bottomKillTimer == 0 && touch == 2)
            {
                Player player = level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (leftKillTimer == 0 && touch == 3)
            {
                Player player = level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (rightKillTimer == 0 && touch == 4)
            {
                Player player = level.Tracker.GetEntity<Player>();
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
                        Player player = level.Tracker.GetEntity<Player>();
                        if (player == null)
                        {
                            return;
                        }
                        player.Die((player.Position - Position).SafeNormalize());
                    }
                }
                else
                {
                    currentKillTimer = touch switch
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

    public void RenderDangerBorder()
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
    
    public void PlayerCollide(Player player)
    {

    }

    public bool onTouchEffective = false;
    public void OnTouch(Vector2 dir)
    {
        // Up left is dir positive
        if (onTouchEffective) { return; }

        if (PUt.TryGetAlivePlayer(out Player player))
        {
            if (dir == -Vector2.UnitX)
            {
                player.Speed = Vector2.Zero;
                player.SideBounce(1, base.Right, base.CenterY);
            }
            else if (dir == Vector2.UnitX)
            {
                player.Speed = Vector2.Zero;
                player.SideBounce(-1, base.Left, base.CenterY);
            }
            else if (dir == Vector2.UnitY)
            {
                player.Speed = Vector2.Zero;
                SideBounceY(player, 1, base.CenterX, base.Top);
            }
            else if (dir == -Vector2.UnitY)
            {
                player.Speed = Vector2.Zero;
                SideBounceY(player, -1, base.CenterX, base.Bottom);
            }
        }
        else { return; }

        onTouchEffective = true;
    }

    /// <summary>
    /// dir: 1 is from top, -1 is from bottom
    /// </summary>
    /// <param name="player"></param>
    /// <param name="dir"></param>
    /// <param name="fromX"></param>
    /// <param name="fromY"></param>
    /// <returns></returns>
    public bool SideBounceY(Player player, int dir, float fromX, float fromY)
    {
        if (Math.Abs(player.Speed.Y) > 185f && Math.Sign(player.Speed.Y) == dir)
        {
            return false;
        }

        Collider collider = player.Collider;
        player.Collider = player.normalHitbox;
        if (dir > 0)
        {
            player.MoveV(fromY - base.Top - 2f);
        }
        else if (dir < 0)
        {
            player.MoveV(fromY - base.Bottom + 2f);
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
        level.DirectionalShake(Vector2.UnitY * dir, 0.1f);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        player.Sprite.Scale = new Vector2(0.5f, 1.5f);
        player.Collider = collider;
        return true;
    }
}
