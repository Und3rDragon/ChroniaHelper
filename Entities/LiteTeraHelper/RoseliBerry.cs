using Celeste.Mod.Entities;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/roseliBerry")]
public class RoseliBerry : Actor
{
    public Vector2 Speed;
    public Holdable Hold;
    private Level Level;
    private Collision onCollideH;
    private Collision onCollideV;
    private float noGravityTimer;
    private Vector2 prevLiftSpeed;
    public TeraType tera;
    private Image image;
    public RoseliBerry(EntityData e, Vector2 offset) : base(e.Position + offset)
    {
        var position = e.Position + offset;
        Depth = 100;
        Collider = new Hitbox(8f, 10f, -4f, -10f);
        Add(image = new Image(GFX.Game["ChroniaHelper/objects/tera/Goomy/berry"]));
        image.CenterOrigin();
        image.Position = new Vector2(0f, -10f);
        Add(Hold = new Holdable(0.1f));
        Hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
        Hold.SlowFall = false;
        Hold.SlowRun = false;
        Hold.OnPickup = OnPickup;
        Hold.OnRelease = OnRelease;
        Hold.OnHitSpring = HitSpring;
        Hold.SpeedGetter = () => Speed;
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        LiftSpeedGraceTime = 0.1f;
        Add(new VertexLight(Collider.Center, Color.White, 1f, 32, 64));
        //Tag = Tags.TransitionUpdate;
        Add(new MirrorReflection());
        Hold.SpeedSetter = delegate (Vector2 speed)
        {
            Speed = speed;
        };
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level = SceneAs<Level>();
    }
    public override void Update()
    {
        base.Update();
        Depth = 100;
        if (Hold.IsHeld)
            prevLiftSpeed = Vector2.Zero;
        else
        {
            if (OnGround())
            {
                float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                Vector2 liftSpeed = LiftSpeed;
                if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                {
                    Speed = prevLiftSpeed;
                    prevLiftSpeed = Vector2.Zero;
                    Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                    if (Speed.X != 0f && Speed.Y == 0f)
                    {
                        Speed.Y = -60f;
                    }

                    if (Speed.Y < 0f)
                    {
                        noGravityTimer = 0.15f;
                    }
                }
                else
                {
                    prevLiftSpeed = liftSpeed;
                    if (liftSpeed.Y < 0f && Speed.Y < 0f)
                    {
                        Speed.Y = 0f;
                    }
                }
            }
            else if (Hold.ShouldHaveGravity)
            {
                float num = 800f;
                if (Math.Abs(Speed.Y) <= 30f)
                {
                    num *= 0.5f;
                }
                float num2 = 350f;
                if (Speed.Y < 0f)
                {
                    num2 *= 0.5f;
                }
                Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                if (noGravityTimer > 0f)
                {
                    noGravityTimer -= Engine.DeltaTime;
                }
                else
                {
                    Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                }
            }
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            if (base.Center.X > (float)Level.Bounds.Right)
            {
                MoveH(32f * Engine.DeltaTime);
                if (base.Left - 8f > (float)Level.Bounds.Right)
                {
                    RemoveSelf();
                }
            }
            else if (base.Center.X < (float)Level.Bounds.Left)
            {
                MoveH(-32f * Engine.DeltaTime);
                if (base.Right + 8f < (float)Level.Bounds.Left)
                {
                    RemoveSelf();
                }
            }
            else if (base.Top < (float)(Level.Bounds.Top - 4))
            {
                base.Top = Level.Bounds.Top + 4;
                Speed.Y = 0f;
            }
            else if (base.Top > (float)Level.Bounds.Bottom)
            {
                RemoveSelf();
            }
        }
        Hold.CheckAgainstColliders();
    }
    public bool HitSpring(Spring spring)
    {
        if (!Hold.IsHeld)
        {
            if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
            {
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                noGravityTimer = 0.15f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = 220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = -220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
        }
        return false;
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        }
        if (Speed.X < 0f)
        {
            Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", Position);
        }
        else
        {
            Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);
        }
        if (Math.Abs(Speed.X) > 100f)
        {
            ImpactParticles(data.Direction);
        }
        Speed.X *= -0.4f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        }
        if (Speed.Y > 0f)
        {
            Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
        }
        if (Speed.Y > 160f)
        {
            ImpactParticles(data.Direction);
        }
        Speed.Y = 0f;
    }

    private void ImpactParticles(Vector2 dir)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        if (dir.X > 0f)
        {
            direction = (float)Math.PI;
            position = new Vector2(base.Right, base.Y - 4f);
            positionRange = Vector2.UnitY * 6f;
        }
        else if (dir.X < 0f)
        {
            direction = 0f;
            position = new Vector2(base.Left, base.Y - 4f);
            positionRange = Vector2.UnitY * 6f;
        }
        else if (dir.Y > 0f)
        {
            direction = -(float)Math.PI / 2f;
            position = new Vector2(base.X, base.Bottom);
            positionRange = Vector2.UnitX * 6f;
        }
        else
        {
            direction = (float)Math.PI / 2f;
            position = new Vector2(base.X, base.Top);
            positionRange = Vector2.UnitX * 6f;
        }

        Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
    }
    public override bool IsRiding(Solid solid)
    {
        if (Speed.Y == 0f)
        {
            return base.IsRiding(solid);
        }
        return false;
    }
    private void OnPickup()
    {
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
    }
    private void OnRelease(Vector2 force)
    {
        RemoveTag(Tags.Persistent);
        if (force.X != 0f && force.Y == 0f)
        {
            force.Y = -0.4f;
        }
        Speed = force * 200f;
        if (Speed != Vector2.Zero)
        {
            noGravityTimer = 0.1f;
        }
    }
}