using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/BubbleField")]
public class BubblePushField : Entity
{
    public enum ActivationMode
    {
        Always, OnlyWhenFlagActive, OnlyWhenFlagInactive
    }

    public float Strength;

    private Dictionary<WindMover, float> WindMovers = new Dictionary<WindMover, float>();

    private Dictionary<WindMover, float> QueuedMovers = new Dictionary<WindMover, float>();

    private int FramesSinceSpawn = 0;

    private int SpawnFrame = 30;

    public PushDirection Direction;

    private ActivationMode activationMode;

    private string flag;

    private bool liftOffOfGround;

    private bool makeWind;

    private static FieldInfo playerWindTimeout;

    private static FieldInfo playerWindDirection;

    private static FieldInfo playerClimbNoMoveTimer;

    private static FieldInfo playerNoWindTimer;

    public BubblePushField(EntityData data, Vector2 offset) : this(
        data.Position + offset,
        data.Width,
        data.Height,
        data.Float("strength", 1f),
        data.Attr("direction", "right"),
        data.Enum("activationMode", ActivationMode.Always),
        data.Attr("flag", "bubble_push_field"),
        data.Bool("liftOffOfGround", false),
        data.Bool("force", true),
        data.Attr("particleDirs", "particles/ChroniaHelper/bubble_a,particles/ChroniaHelper/bubble_b"),
        data.Bool("noParticles"),
        data.Bool("instant", false)
        )
    { }

    public string[] particlePaths;
    public int pDirLength;
    private bool noParticles;
    private bool instant;
    public BubblePushField(Vector2 position, 
        int width, int height, float strength, 
        string direction, ActivationMode activationMode,
        string flag, bool liftOff, bool makeWind,
        string particleDirs, bool noParticles, bool instant)
    {
        Position = position;
        Strength = strength;
        this.activationMode = activationMode;
        this.flag = flag;
        this.liftOffOfGround = liftOff;
        this.makeWind = makeWind;

        Enum.TryParse(direction, out Direction);

        Collider = new Hitbox(width, height);

        if( noParticles || particleDirs.Trim() == "" ) { particleDirs = "particles/ChroniaHelper/none"; }
        particlePaths = particleDirs.Split(',');
        //for(int i = 0; i < particlePaths.Length; i++) { Utils.Log.Info(particlePaths[i]); }
        pDirLength = particlePaths.Length;
        for(int i = 0 ; i < pDirLength ; i++)
        {
            particlePaths[i] = particlePaths[i].Trim();
        }

        this.instant = instant;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }

    public override void Render()
    {
        base.Render();
    }

    public override void Update()
    {
        base.Update();

        Session session = SceneAs<Level>().Session;
        if ((activationMode == ActivationMode.OnlyWhenFlagActive && !session.GetFlag(flag))
            || (activationMode == ActivationMode.OnlyWhenFlagInactive && session.GetFlag(flag)))
        {

            // the bubble push field is currently turned off by a session flag.
            Collidable = false;
            return;
        }

        Collidable = true;

        FramesSinceSpawn++;
        if (Strength > 0)
        {
            if (FramesSinceSpawn == SpawnFrame)
            {
                FramesSinceSpawn = 0;
                SpawnFrame = Calc.Random.Next(2, 10);
                Add(new BubbleParticle(true, true));
            }

            if (this.makeWind)
            {
                foreach (WindMover mover in Scene.Tracker.GetComponents<WindMover>())
                {
                    if (mover.Entity.CollideCheck(this))
                    {
                        if (WindMovers.ContainsKey(mover))
                            WindMovers[mover] = Calc.Approach(WindMovers[mover], Strength, instant? Engine.DeltaTime * 10000f : Engine.DeltaTime / 0.6f);
                        else
                            WindMovers.Add(mover, 0f);
                    }
                    else
                    {
                        if (WindMovers.ContainsKey(mover))
                        {
                            WindMovers[mover] = Calc.Approach(WindMovers[mover], 0f, instant ? Engine.DeltaTime * 10000f : Engine.DeltaTime / 0.3f);
                            if (WindMovers[mover] == 0f)
                                WindMovers.Remove(mover);
                        }
                    }
                }

                QueuedMovers.Clear();
                foreach (WindMover mover in WindMovers.Keys)
                {
                    float windSpeed = Strength * 2f * Ease.CubeInOut(WindMovers[mover]);

                    if (mover != null && mover.Entity != null && mover.Entity.Scene != null)
                    {
                        Vector2 move = Vector2.Zero;
                        switch (Direction)
                        {
                            case PushDirection.Up:
                                move = new Vector2(0, -windSpeed);
                                break;

                            case PushDirection.Down:
                                move = new Vector2(0, windSpeed);
                                break;

                            case PushDirection.Left:
                                move = new Vector2(-windSpeed, 0);
                                break;

                            case PushDirection.Right:
                                move = new Vector2(windSpeed, 0);
                                break;
                        }

                        if (mover.Entity.GetType() == typeof(Player))
                        {
                            if (!MovePlayer((Player)mover.Entity, move))
                            {
                                QueuedMovers.Add(mover, 0f);
                            }
                        }
                        else
                        {
                            mover.Move(move);
                        }
                    }
                }

                if (QueuedMovers.Count > 0)
                {
                    foreach (KeyValuePair<WindMover, float> pair in QueuedMovers)
                    {
                        WindMovers[pair.Key] = pair.Value;
                    }
                }
            }
        }
    }
    [LoadHook]
    public static void Load()
    {
        playerWindTimeout = typeof(Player).GetField("windTimeout", BindingFlags.NonPublic | BindingFlags.Instance);
        playerWindDirection = typeof(Player).GetField("windDirection", BindingFlags.NonPublic | BindingFlags.Instance);
        playerClimbNoMoveTimer = typeof(Player).GetField("climbNoMoveTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        playerNoWindTimer = typeof(Player).GetField("noWindTimer", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private bool MovePlayer(Player player, Vector2 move)
    {
        Vector2 windDirection = (Vector2)playerWindDirection.GetValue(player);

        if (player.OnGround() && (player.Ducking))
        {
            playerWindDirection.SetValue(player, windDirection);
            return false;
        }

        if (!player.JustRespawned && (float)playerNoWindTimer.GetValue(player) <= 0f && player.InControl && player.StateMachine.State != 4 && player.StateMachine.State != 2 && player.StateMachine.State != 10)
        {
            if (move.X != 0f)
            {
                playerWindTimeout.SetValue(player, 0.2f);
                windDirection.X = (float)Math.Sign(move.X);
                if (liftOffOfGround || (!player.CollideCheck<Solid>(player.Position + Vector2.UnitX * (float)(-(float)Math.Sign(move.X)) * 3f) && player.StateMachine.State != 1))
                {
                    if (move.X < 0f)
                    {
                        move.X = Math.Max(move.X, (float)(SceneAs<Level>().Bounds.Left - (player.ExactPosition.X + player.Collider.Left)));
                    }
                    else
                    {
                        move.X = Math.Min(move.X, (float)(SceneAs<Level>().Bounds.Right - (player.ExactPosition.X + player.Collider.Right)));
                    }

                    player.MoveH(move.X);
                }
            }

            if (move.Y != 0f)
            {
                if (player.OnGround())
                {
                    if (liftOffOfGround)
                    {
                        player.MoveV(move.Y);
                    }

                    playerWindDirection.SetValue(player, windDirection);
                    return true;
                }

                playerWindTimeout.SetValue(player, 0.2f);
                windDirection.Y = (float)Math.Sign(move.Y);

                if (player.Bottom > (float)SceneAs<Level>().Bounds.Top)
                {
                    if (player.StateMachine.State == 1)
                    {
                        if (move.Y <= 0f || (float)playerClimbNoMoveTimer.GetValue(player) > 0f)
                        {
                            playerWindDirection.SetValue(player, windDirection);
                            return false;
                        }
                        move.Y *= 0.4f;
                    }
                    player.MoveV(move.Y);
                }
            }
        }

        playerWindDirection.SetValue(player, windDirection);
        return true;
    }
}

class BubbleParticle : Component
{
    public Vector2 Position = Vector2.Zero;

    public BubblePushField BubbleField;

    public MTexture Texture;

    private int FramesAlive = 0;

    private int FramesMaxAlive;

    private Vector2 Origin, End;

    private static readonly string[] TextureNames = new string[] { "a", "b" };

    public BubbleParticle(bool active, bool visible) : base(active, visible) { }

    public override void Added(Entity entity)
    {
        base.Added(entity);

        BubbleField = (BubblePushField)entity;
        Position = BubbleField.Position;

        int dirIndex = Calc.Random.Range(0, BubbleField.pDirLength);
        //Texture = GFX.Game["particles/ChroniaHelper/bubble_" + TextureNames[Calc.Random.Next(0, 2)]];
        Texture = GFX.Game[BubbleField.particlePaths[dirIndex]];
        //自定义贴图

        // Determine bubble spawn point

        //FramesMaxAlive决定气泡存活时长
        //Update()决定渲染时气泡移动速度

        switch (BubbleField.Direction)
        {
            case PushDirection.Up:
                Origin = new Vector2(Calc.Random.Range(BubbleField.BottomLeft.X, BubbleField.BottomRight.X), BubbleField.BottomCenter.Y);
                End = new Vector2(Calc.Random.Range(BubbleField.TopLeft.X, BubbleField.TopRight.X), BubbleField.TopCenter.Y);
                FramesMaxAlive = (int)Calc.Random.Range(20, BubbleField.Height / BubbleField.Strength * .5f);
                break;

            case PushDirection.Down:
                Origin = new Vector2(Calc.Random.Range(BubbleField.TopLeft.X, BubbleField.TopRight.X), BubbleField.TopCenter.Y);
                End = new Vector2(Calc.Random.Range(BubbleField.BottomLeft.X, BubbleField.BottomRight.X), BubbleField.BottomCenter.Y);
                FramesMaxAlive = (int)Calc.Random.Range(20, BubbleField.Height / BubbleField.Strength * .5f);
                break;

            case PushDirection.Right:
                Origin = new Vector2(BubbleField.CenterLeft.X, Calc.Random.Range(BubbleField.BottomLeft.Y, BubbleField.TopLeft.Y));
                End = new Vector2(BubbleField.CenterRight.X, Calc.Random.Range(BubbleField.BottomRight.Y, BubbleField.TopRight.Y));
                FramesMaxAlive = (int)Calc.Random.Range(20, BubbleField.Width / BubbleField.Strength * .5f);
                break;

            case PushDirection.Left:
                Origin = new Vector2(BubbleField.CenterRight.X, Calc.Random.Range(BubbleField.BottomRight.Y, BubbleField.TopRight.Y));
                End = new Vector2(BubbleField.CenterLeft.X, Calc.Random.Range(BubbleField.BottomLeft.Y, BubbleField.TopLeft.Y));
                FramesMaxAlive = (int)Calc.Random.Range(20, BubbleField.Width / BubbleField.Strength * .5f);
                break;
        }

        Position = Origin;
    }

    public override void Update()
    {
        base.Update();

        if (FramesAlive == FramesMaxAlive)
            RemoveSelf();

        
        FramesAlive++;

        if (BubbleField.Direction == PushDirection.Up || BubbleField.Direction == PushDirection.Down)
        {
            Position.X = Calc.Approach(Position.X, End.X, BubbleField.Strength / 5);
            Position.Y = Calc.Approach(Position.Y, End.Y, BubbleField.Strength * 2);
        }
        else
        {
            Position.X = Calc.Approach(Position.X, End.X, BubbleField.Strength * 2);
            Position.Y = Calc.Approach(Position.Y, End.Y, BubbleField.Strength / 5);
        }
    }

    public override void Render()
    {
        Texture.DrawCentered(Position);
    }
}

public enum PushDirection
{
    Left,
    Right,
    Up,
    Down
}
