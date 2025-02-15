﻿using System;
using Celeste.Mod.Entities;
using System.Collections;
using ChroniaHelper.Utils;
using ChroniaHelper;
using MonoMod.Cil;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/CustomGlider")]
public class CustomGlider : Actor
{
    
    public ParticleType P_Glide;

    public ParticleType P_GlideUp;

    public ParticleType P_Platform;

    public ParticleType P_Glow;

    public ParticleType P_Expand;

    private const float HighFrictionTime = 0.5f;

    public Vector2 Speed;

    public Holdable Hold;

    private Level level;

    private Collision onCollideH;

    private Collision onCollideV;

    private Vector2 prevLiftSpeed;

    private Vector2 startPos;

    private float noGravityTimer;

    private float highFrictionTimer;

    private bool bubble;

    private bool tutorial;

    private bool destroyed;

    private Sprite sprite;

    private Wiggler wiggler;

    private SineWave platformSine;

    private SoundSource fallingSfx;

    private BirdTutorialGui tutorialGui;

    private Color GlideC1, GlideC2, GlowC1, GlowC2;
    private string GlidePath, GlowPath;

    

    public string idlePath, heldPath, fallPath, fallLoopPath, deathPath, respawnPath;
    public float idleTime, heldTime, fallTime, fallLoopTime, deathTime, respawnAnimTime;

    private float hbw, hbh, hbx, hby, hdw, hdh, hdx, hdy;
    public CustomGlider(EntityData e, Vector2 offset)
        : base(e.Position + offset)
    {
        bubble = e.Bool("bubble");
        this.tutorial = e.Bool("tutorial");
        startPos = Position;
        bool gfxoverride = e.Bool("XMLOverride", false);

        // New data from 1.23.7
        var hitbox = e.Attr("hitboxParameters").Split(',',StringSplitOptions.TrimEntries);
        var holdableHitbox = e.Attr("holdableParameters").Split(',',StringSplitOptions.TrimEntries);
        int hitboxVars = hitbox.Length;
        int holdableVars = holdableHitbox.Length;
        hbw = 8f; hbh = 10f; hbx = -4f; hby = -10f;
        hdw = 20f; hdh = 22f; hdx = -10f; hdy = -16f;
        if (hitboxVars >= 1) { hbw = hitbox[0].ParseFloat(8f); }
        if (hitboxVars >= 2) { hbh = hitbox[1].ParseFloat(10f); }
        if (hitboxVars >= 3) { hbx = hitbox[2].ParseFloat(-4f); }
        if (hitboxVars >= 4) { hby = hitbox[3].ParseFloat(-10f); }
        if (holdableVars >= 1) { hdw = holdableHitbox[0].ParseFloat(20f); }
        if (holdableVars >= 2) { hdh = holdableHitbox[1].ParseFloat(22f); }
        if (holdableVars >= 3) { hdx = holdableHitbox[2].ParseFloat(-10f); }
        if (holdableVars >= 4) { hdy = holdableHitbox[3].ParseFloat(-16f); }

        // Old data before 1.23.7
        if (!string.IsNullOrEmpty(e.Attr("hitboxWidth"))){
            hbw = Math.Abs(e.Float("hitboxWidth", 8f));
        }
        if (!string.IsNullOrEmpty(e.Attr("hitboxHeight"))){
            hbh = Math.Abs(e.Float("hitboxHeight", 10f));
        }
        if (!string.IsNullOrEmpty(e.Attr("hitboxX")))
        {
            hbx = e.Float("hitboxX", -4f);
        }
        if (!string.IsNullOrEmpty(e.Attr("hitboxY")))
        {
            hby = e.Float("hitboxY", -10f);
        }
        
        
        base.Collider = new Hitbox(hbw, hbh, hbx, hby);
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;

        //自定义属性

        this.idlePath = "idle";
        this.heldPath = "held";
        this.fallPath = "fall";
        this.fallLoopPath = "fallLoop";
        this.deathPath = "death";
        this.respawnPath = "respawn";
        this.idleTime = e.Float("idleAnimInterval");
        this.heldTime = e.Float("heldAnimInterval");
        this.fallTime = e.Float("fallAnimInterval");
        this.fallLoopTime = e.Float("fallLoopAnimInterval");
        this.deathTime = e.Float("deathAnimInterval");
        this.respawnAnimTime = e.Float("respawnAnimInterval");

        //Sprite shenanigans
        if (!gfxoverride)
        {
            sprite = new Sprite(GFX.Game, e.Attr("Directory", "objects/glider").Trim('/') + "/");
            sprite.Justify = new Vector2(0.5f, 0.58f);

            sprite.AddLoop("idle", this.idlePath, this.idleTime);
            sprite.AddLoop("held", this.heldPath, this.heldTime);
            sprite.Add("fall", this.fallPath, this.fallTime, "fallLoop");
            sprite.AddLoop("fallLoop", this.fallLoopPath, this.fallLoopTime);
            sprite.Add("death", this.deathPath, this.deathTime);
            sprite.Add("respawn", this.respawnPath, this.respawnAnimTime, "idle");
        }
        else
        {
            string dir = e.Attr("Directory", "Custom_glider").Trim();
            sprite = GFX.SpriteBank.Create(dir);
        }

        Add(sprite);
        sprite.Play("idle");

        Add(wiggler = Wiggler.Create(0.25f, 4f));
        base.Depth = e.Int("Depth", -5);
        Add(Hold = new Holdable(0.3f));
        Hold.PickupCollider = new Hitbox(hdw, hdh, hdx, hdy);
        Hold.SlowFall = true;
        Hold.SlowRun = false;
        Hold.OnPickup = OnPickup;
        Hold.OnRelease = OnRelease;
        Hold.SpeedGetter = () => Speed;
        Hold.OnHitSpring = HitSpring;
        platformSine = new SineWave(0.3f, 0f);
        Add(platformSine);
        fallingSfx = new SoundSource();
        Add(fallingSfx);
        Add(new WindMover(WindMode));

        //Particle stuff
        GlidePath = e.Attr("GlidePath", "particles/rect");
        if (string.IsNullOrWhiteSpace(GlidePath))
            GlidePath = "particles/rect";
        GlowPath = e.Attr("GlowPath", "");
        if (string.IsNullOrWhiteSpace(GlowPath))
            GlowPath = "";
        GlideC1 = ColorUtils.ColorFix(e.Attr("GlideColor1", "4FFFF3"));
        GlideC2 = ColorUtils.ColorFix(e.Attr("GlideColor2", "FFF899"));
        GlowC1 = ColorUtils.ColorFix(e.Attr("GlowColor1", "B7F3FF"));
        GlowC2 = ColorUtils.ColorFix(e.Attr("GlowColor2", "F4FDFF"));

        P_Glide = new ParticleType
        {
            Acceleration = Vector2.UnitY * 60f,
            SpeedMin = 30f,
            SpeedMax = 40f,
            Direction = -(float)Math.PI / 2f,
            DirectionRange = (float)Math.PI / 2f,
            LifeMin = 0.6f,
            LifeMax = 1.2f,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            Color = GlideC1,
            Color2 = GlideC2,
            Size = 0.5f,
            SizeRange = 0.2f,
            RotationMode = ParticleType.RotationModes.SameAsDirection
        };
        if (!string.IsNullOrWhiteSpace(GlidePath))
            P_Glide.Source = GFX.Game[GlidePath];
        P_GlideUp = new ParticleType(Glider.P_Glide)
        {
            Acceleration = Vector2.UnitY * -10f,
            SpeedMin = 50f,
            SpeedMax = 60f
        };
        P_Glow = new ParticleType
        {
            SpeedMin = 8f,
            SpeedMax = 16f,
            DirectionRange = (float)Math.PI * 2f,
            LifeMin = 0.4f,
            LifeMax = 0.8f,
            Size = 1f,
            FadeMode = ParticleType.FadeModes.Late,
            Color = GlowC1,
            Color2 = GlowC2,
            ColorMode = ParticleType.ColorModes.Blink
        };
        if (!string.IsNullOrWhiteSpace(GlowPath))
            P_Glide.Source = GFX.Game[GlowPath];
        P_Expand = new ParticleType(Glider.P_Glow)
        {
            SpeedMin = 40f,
            SpeedMax = 80f,
            SpeedMultiplier = 0.2f,
            LifeMin = 0.6f,
            LifeMax = 1.2f,
            DirectionRange = (float)Math.PI * 3f / 4f
        };
        P_Platform = new ParticleType
        {
            Acceleration = Vector2.UnitY * 60f,
            SpeedMin = 5f,
            SpeedMax = 20f,
            Direction = -(float)Math.PI / 2f,
            LifeMin = 0.6f,
            LifeMax = 1.4f,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 1f
        };

        // New parameters
        float dsx = e.Float("droppingSpeedX", 0f), dsy = e.Float("droppingSpeedY", 30f);
        droppingSpeed = new Vector2(dsx, dsy);
        droppingSpeedXMultiplier = e.Bool("droppingSpeedXMultiplier", false);
        acc = new Vector2(Math.Abs(e.Float("accelerationX", 1f)),
                          Math.Abs(e.Float("accelerationY", 1f))
                          );
    }
    private Vector2 droppingSpeed;
    private bool droppingSpeedXMultiplier;
    private Vector2 acc;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        if (tutorial)
        {
            tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), Input.Grab);
            tutorialGui.Open = true;
            base.Scene.Add(tutorialGui);
        }
    }

    public override void Update()
    {
        if (base.Scene.OnInterval(0.05f))
        {
            level.Particles.Emit(P_Glow, 1, base.Center + Vector2.UnitY * -9f, new Vector2(10f, 4f));
        }
        float target = ((!Hold.IsHeld) ? 0f : ((!Hold.Holder.OnGround()) ? Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, (float)Math.PI / 3f, -(float)Math.PI / 3f) : Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 0.6981317f, -0.6981317f)));
        sprite.Rotation = Calc.Approach(sprite.Rotation, target, (float)Math.PI * Engine.DeltaTime);
        if (Hold.IsHeld && !Hold.Holder.OnGround() && (sprite.CurrentAnimationID == "fall" || sprite.CurrentAnimationID == "fallLoop"))
        {
            if (!fallingSfx.Playing)
            {
                Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
                fallingSfx.Play("event:/new_content/game/10_farewell/glider_movement");
            }
            Vector2 speed = Hold.Holder.Speed;
            Vector2 vector = new Vector2(speed.X * 0.5f, (speed.Y < 0f) ? (speed.Y * 2f) : speed.Y);
            float value = Calc.Map(vector.Length(), 0f, 120f, 0f, 0.7f);
            fallingSfx.Param("glider_speed", value);
        }
        else
        {
            fallingSfx.Stop();
        }
        base.Update();
        if (!destroyed)
        {
            foreach (SeekerBarrier entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
            {
                entity.Collidable = true;
                bool num = CollideCheck(entity);
                entity.Collidable = false;
                if (num)
                {
                    destroyed = true;
                    Collidable = false;
                    if (Hold.IsHeld)
                    {
                        Vector2 speed2 = Hold.Holder.Speed;
                        Hold.Holder.Drop();
                        Speed = speed2 * 0.333f;
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    }
                    Add(new Coroutine(DestroyAnimationRoutine()));
                    return;
                }
            }
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else if (!bubble)
            {
                if (highFrictionTimer > 0f)
                {
                    highFrictionTimer -= Engine.DeltaTime;
                }
                if (OnGround())
                {
                    float target2 = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                    Speed.X = Calc.Approach(Speed.X, target2, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
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
                    float num2 = 200f;
                    if (Speed.Y >= -30f)
                    {
                        num2 *= 0.5f;
                    }
                    float num3 = ((Speed.Y < 0f) ? 40f : ((!(highFrictionTimer <= 0f)) ? 10f : 40f));
                    // Floating Speed
                    Speed.X = Calc.Approach(Speed.X, 
                                            droppingSpeedXMultiplier? Speed.X * droppingSpeed.X : droppingSpeed.X, 
                                            num3 * Engine.DeltaTime * acc.X);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else if (level.Wind.Y < 0f)
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 0f, num2 * Engine.DeltaTime * acc.Y);
                    }
                    else
                    {
                        // Max Falling Speed
                        Speed.Y = Calc.Approach(Speed.Y, droppingSpeed.Y, num2 * Engine.DeltaTime * acc.Y);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                CollisionData data;
                if (base.Left < (float)level.Bounds.Left)
                {
                    base.Left = level.Bounds.Left;
                    data = new CollisionData
                    {
                        Direction = -Vector2.UnitX
                    };
                    OnCollideH(data);
                }
                else if (base.Right > (float)level.Bounds.Right)
                {
                    base.Right = level.Bounds.Right;
                    data = new CollisionData
                    {
                        Direction = Vector2.UnitX
                    };
                    OnCollideH(data);
                }
                if (base.Top < (float)level.Bounds.Top)
                {
                    base.Top = level.Bounds.Top;
                    data = new CollisionData
                    {
                        Direction = -Vector2.UnitY
                    };
                    OnCollideV(data);
                }
                else if (base.Top > (float)(level.Bounds.Bottom + 16))
                {
                    RemoveSelf();
                    return;
                }
                Hold.CheckAgainstColliders();
            }
            else
            {
                Position = startPos + Vector2.UnitY * platformSine.Value * 1f;
            }
            Vector2 one = Vector2.One;
            if (!Hold.IsHeld)
            {
                if (level.Wind.Y < 0f)
                {
                    PlayOpen();
                }
                else
                {
                    sprite.Play("idle");
                }
            }
            else if (Hold.Holder.Speed.Y > 20f || level.Wind.Y < 0f)
            {
                if (level.OnInterval(0.04f))
                {
                    if (level.Wind.Y < 0f)
                    {
                        level.ParticlesBG.Emit(P_GlideUp, 1, Position - Vector2.UnitY * 20f, new Vector2(6f, 4f));
                    }
                    else
                    {
                        level.ParticlesBG.Emit(P_Glide, 1, Position - Vector2.UnitY * 10f, new Vector2(6f, 4f));
                    }
                }
                PlayOpen();
                if (Input.GliderMoveY.Value > 0)
                {
                    one.X = 0.7f;
                    one.Y = 1.4f;
                }
                else if (Input.GliderMoveY.Value < 0)
                {
                    one.X = 1.2f;
                    one.Y = 0.8f;
                }
                Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
            }
            else
            {
                sprite.Play("held");
            }
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, one.Y, Engine.DeltaTime * 2f);
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, (float)Math.Sign(sprite.Scale.X) * one.X, Engine.DeltaTime * 2f);
            if (tutorialGui != null)
            {
                tutorialGui.Open = tutorial && !Hold.IsHeld && (OnGround(4) || bubble);
            }
        }
        else
        {
            Position += Speed * Engine.DeltaTime;
        }
    }

    private void PlayOpen()
    {
        if (sprite.CurrentAnimationID != "fall" && sprite.CurrentAnimationID != "fallLoop")
        {
            sprite.Play("fall");
            sprite.Scale = new Vector2(1.5f, 0.6f);
            level.Particles.Emit(P_Expand, 16, base.Center + (Vector2.UnitY * -12f).Rotate(sprite.Rotation), new Vector2(8f, 3f), -(float)Math.PI / 2f + sprite.Rotation);
            if (Hold.IsHeld)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            }
        }
    }

    public override void Render()
    {
        if (!destroyed)
        {
            sprite.DrawSimpleOutline();
        }
        base.Render();
        if (bubble)
        {
            for (int i = 0; i < 24; i++)
            {
                Draw.Point(Position + PlatformAdd(i), PlatformColor(i));
            }
        }
    }

    private void WindMode(Vector2 wind)
    {
        if (!Hold.IsHeld)
        {
            if (wind.X != 0f)
            {
                MoveH(wind.X * 0.5f);
            }
            if (wind.Y != 0f)
            {
                MoveV(wind.Y);
            }
        }
    }

    private Vector2 PlatformAdd(int num)
    {
        return new Vector2(-12 + num, -5 + (int)Math.Round(Math.Sin(base.Scene.TimeActive + (float)num * 0.2f) * 1.7999999523162842));
    }

    private Color PlatformColor(int num)
    {
        if (num <= 1 || num >= 22)
        {
            return Color.White * 0.4f;
        }
        return Color.White * 0.8f;
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
        Speed.X *= -1f;
        sprite.Scale = new Vector2(0.8f, 1.2f);
    }

    private void OnCollideV(CollisionData data)
    {
        if (Math.Abs(Speed.Y) > 8f)
        {
            sprite.Scale = new Vector2(1.2f, 0.8f);
            Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
        }
        if (Speed.Y < 0f)
        {
            Speed.Y *= -0.5f;
        }
        else
        {
            Speed.Y = 0f;
        }
    }

    private void OnPickup()
    {
        if (bubble)
        {
            for (int i = 0; i < 24; i++)
            {
                level.Particles.Emit(P_Platform, Position + PlatformAdd(i), PlatformColor(i));
            }
        }
        AllowPushing = false;
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
        highFrictionTimer = 0.5f;
        bubble = false;
        wiggler.Start();
        tutorial = false;
    }

    private void OnRelease(Vector2 force)
    {
        if (force.X == 0f)
        {
            Audio.Play("event:/new_content/char/madeline/glider_drop", Position);
        }
        AllowPushing = true;
        RemoveTag(Tags.Persistent);
        force.Y *= 0.5f;
        if (force.X != 0f && force.Y == 0f)
        {
            force.Y = -0.4f;
        }
        Speed = force * 100f;
        wiggler.Start();
    }

    public override void OnSquish(CollisionData data)
    {
        if (!TrySquishWiggle(data))
        {
            RemoveSelf();
        }
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
                wiggler.Start();
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = 160f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                wiggler.Start();
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = -160f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                wiggler.Start();
                return true;
            }
        }
        return false;
    }

    private IEnumerator DestroyAnimationRoutine()
    {
        Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
        sprite.Play("death");
        yield return 1f;
        RemoveSelf();
    }
}
