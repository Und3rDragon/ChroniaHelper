using System.Reflection;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/DrivableCar")]
[Credits("Modified from Pandora's Box by Curor")]
[Note("This version is simply a reskinnable version of original Drivable Car" +
      "If the original version updated, this is suggested to be removed")]
public class DrivableCar : Actor
{
    private TalkComponent talker;
    private bool currently_driving;

    private Monocle.Image bodySprite;
    private Monocle.Image wheelsSprite;

    public int id;

    private Vector2 speed;
    private float acceleration;
    private float deceleration;
    private float maxSpeed;
    private float breakingDuration;
    private bool keepCarSpeedOnExit;

    private float nitroAcceleration;
    private float nitroMaxDuration;
    private float nitroDuration;
    private float nitroRegenMultiplier;
    private bool nitroActive;

    private bool crashed;
    private bool brokenDoor;

    private Collision onCollideH;
    private Collision onCollideV;

    private double exhaustAcc;

    private int facing;
    private Vector2 prevLiftSpeed;

    private MethodInfo springMethod = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance);

    private double springDelay;
    private double enterDelay;

    private double springGrace;
    private double enterGrace;

    public Player driver;

    private Scene scene;
    private Level level;

    public DrivableCar(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        // TODO - Fix this, needs a entity per depthed image
        this.Depth = 1;
        this.Add(this.bodySprite = new Monocle.Image(GFX.Game[data.Attr("bodyTexture")]));
        this.bodySprite.Origin = new Vector2(this.bodySprite.Width / 2f, this.bodySprite.Height);

        this.Depth = 3;
        this.Add(this.wheelsSprite = new Monocle.Image(GFX.Game[data.Attr("wheelsTexture")]));
        this.wheelsSprite.Origin = new Vector2(this.wheelsSprite.Width / 2f, this.wheelsSprite.Height);

        base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer)));
        this.Collider = new Hitbox(40, 18, -20, -18);

        this.Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), this.OnTalk, null));
        this.talker.Enabled = false;

        this.acceleration = float.Parse(data.Attr("acceleration", "256"));
        this.deceleration = float.Parse(data.Attr("deceleration", "384"));
        this.maxSpeed = float.Parse(data.Attr("maxSpeed", "384"));

        this.brokenDoor = bool.Parse(data.Attr("brokenDoor", "false"));
        this.keepCarSpeedOnExit = bool.Parse(data.Attr("keepCarSpeedOnExit", "false"));

        this.nitroAcceleration = float.Parse(data.Attr("nitroAcceleration", "448"));
        this.nitroMaxDuration = float.Parse(data.Attr("nitroMaxDuration", "3"));
        this.nitroRegenMultiplier = float.Parse(data.Attr("nitroRegenMultiplier", "0.2"));
        this.nitroDuration = this.nitroMaxDuration;

        this.nitroActive = false;

        this.breakingDuration = 0;
        this.facing = int.Parse(data.Attr("facing", "1"));

        this.onCollideH = new Collision(this.OnCollideH);
        this.onCollideV = new Collision(this.OnCollideV);

        this.prevLiftSpeed = Vector2.Zero;

        this.exhaustAcc = 0.0;

        this.springGrace = 0.3;
        this.enterGrace = 0.5;

        this.id = data.ID;

        this.AddTag(Tags.TransitionUpdate);
    }

    // TODO - Hack this together again
    private void HitSpring(Spring spring)
    {
        if (this.springDelay == 0) {
            this.speed.Y -= spring.Orientation == Spring.Orientations.Floor ? 240f : 140f;
            this.speed.X += spring.Orientation == Spring.Orientations.WallRight ? -180f : (spring.Orientation == Spring.Orientations.WallLeft ? 180f : 0);

            this.facing = Math.Sign(this.speed.X);

            this.springDelay = this.springGrace;
            this.springMethod.Invoke(spring, null);
        }
    }

    private void OnTalk(Player player)
    {
        if (!this.crashed)
        {
            OnEnterCar(player);
        }

        this.talker.Visible = false;
    }

    private void OnPlayer(Player player)
    {
        if (!currently_driving)
        {
            talker.Enabled = true;
        }
    }

    private void OnEnterCar(Player player)
    {
        this.currently_driving = true;
        this.talker.Enabled = false;
        this.driver = player;
        this.enterDelay = this.enterGrace;

        this.AddTag(Tags.Persistent);

        player.Visible = false;
        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        player.ForceCameraUpdate = true;
    }

    private void OnExitCar(Player player)
    {
        this.currently_driving = false;
        this.driver = null;
        this.nitroActive = false;

        this.RemoveTag(Tags.Persistent);

        player.Visible = true;
        player.Position = this.Position;

        if (keepCarSpeedOnExit) {
            player.Speed = this.speed;

            // Launch the player upwards a bit if the car is driving fast
            if (Math.Abs(speed.X) > this.maxSpeed * 0.85)
            {
                player.Speed.Y -= 180f;
            }
        }

        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        player.ForceCameraUpdate = false;
    }

    private bool alreadyAdded
    {
        get
        {
            return Scene.Tracker.GetEntities<DrivableCar>().Cast<DrivableCar>().Any(other => (other.id == this.id && this.driver == null && other.driver != null));
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.scene = scene;
        this.level = scene as Level;

        if (alreadyAdded) {
            base.RemoveSelf();
        }
    }

    public override void Update()
    {
        if (level.Transitioning)
        {
            if (this.driver != null)
            {
                this.driver.StateMachine.State = Player.StNormal;
                this.Position = this.driver.Position;
            }

            return;
        }


        this.MoveH(this.speed.X * Engine.DeltaTime, this.onCollideH);
        this.MoveV(this.speed.Y * Engine.DeltaTime, this.onCollideV);

        this.bodySprite.Scale.X = facing;
        this.wheelsSprite.Scale.X = facing;

        this.springDelay = Math.Max(0, this.springDelay - Engine.DeltaTime);
        this.enterDelay = Math.Max(0, this.enterDelay - Engine.DeltaTime);
            
        // Make sure player is in dummy state, springs can reset it
        if (this.driver != null && this.currently_driving && !this.driver.Dead) {
            this.driver.Position = this.Position;
            this.driver.StateMachine.State = Player.StDummy;
            this.driver.DummyGravity = false;
            this.driver.DummyAutoAnimate = false;

            Vector2 center = new Vector2(Calc.Clamp(this.driver.X - this.level.Camera.X, 120f, 200f), Calc.Clamp(this.driver.Y - this.level.Camera.Y, 60f, 120f));

            level.EnforceBounds(driver);
        }

        if (this.currently_driving && Input.Grab && !this.brokenDoor)
        {
            OnExitCar(this.driver);
        }

        foreach (Spring spring in scene.Entities.Where(e => e is Spring))
        {
            if (this.CollideCheck(spring))
            {
                Audio.Play("event:/game/general/spring", this.BottomCenter);
                HitSpring(spring);
            }
        }

        updateExhaust();
        updateSpeed();
        updateLiftSpeed();

        base.Update();
    }

    private void updateLiftSpeed()
    {
        if (this.LiftSpeed == Vector2.Zero && this.prevLiftSpeed != Vector2.Zero)
        {
            this.speed = this.prevLiftSpeed;
            this.prevLiftSpeed = Vector2.Zero;
        }

        this.prevLiftSpeed = this.LiftSpeed;
    }

    private void updateExhaust()
    {
        if (currently_driving)
        {
            bool nitroParticles = this.nitroActive && this.speed.X != 0;
            double threshold = nitroParticles ? 0.05 : 0.5;

            if (this.exhaustAcc > threshold) {
                ParticleType particle = nitroParticles ? Player.P_DashB : ParticleTypes.Chimney;
                int amount = (int) Math.Floor(Math.Abs(this.speed.X) / this.maxSpeed * 3) + 1;
                amount *= nitroParticles ? 5 : 1;

                this.level.ParticlesFG.Emit(particle, amount, this.Center + new Vector2(20f * -this.facing, 0f), new Vector2(-this.facing * 2, -6));

                this.exhaustAcc -= threshold;
            }

            this.exhaustAcc += Engine.DeltaTime;
        }
    }

    private void updateSpeed()
    {
        this.speed.Y = Calc.Approach(this.speed.Y, 200f, 400f * Engine.DeltaTime);

        // Madeline is not "in" the car yet
        if (this.enterDelay > 0)
        {
            return;
        }

        this.nitroActive = driver != null && !driver.Dead && this.currently_driving && Input.Dash && this.nitroDuration > Engine.DeltaTime;

        // Nitro is allowed in air, but can only go forward
        if (this.nitroActive)
        {
            float newSpeed = this.speed.X + this.facing * this.nitroAcceleration * Engine.DeltaTime;
            this.speed.X = MathHelper.Clamp(newSpeed, -this.maxSpeed, this.maxSpeed);
        }

         // No magic movement in air
        if (!(this.speed.Y >= 0 && this.speed.Y <= Calc.Approach(0f, 200f, 400f * Engine.DeltaTime)))
        {
            return;
        }

        // Crashed, no player controll
        if (this.crashed)
        {
            return;
        }

        if (Input.MoveX != 0 && this.driver != null)
        {
            this.breakingDuration = 0;
            this.facing = Math.Sign(Input.MoveX);

            float newSpeed = 0;

            if (!this.nitroActive)
            { 
                newSpeed = this.speed.X + Input.MoveX * this.acceleration * Engine.DeltaTime;

                this.nitroDuration = Math.Min(this.nitroMaxDuration, this.nitroDuration + this.nitroRegenMultiplier * Engine.DeltaTime);
                this.nitroActive = false;

                this.speed.X = MathHelper.Clamp(newSpeed, -this.maxSpeed, this.maxSpeed);
            }
        }
        else
        {
            if (this.speed.X != 0)
            {
                float delta = Math.Sign(this.speed.X) * this.deceleration * (float)Math.Pow(this.breakingDuration, 2) * (float)(Input.MoveY > 0 ? 2.5 : 1);
                this.breakingDuration += Engine.DeltaTime;

                this.speed.X = (Math.Abs(delta) > Math.Abs(this.speed.X) ? 0 : this.speed.X - delta);
            }
        }
    }

    private void OnCollideH(CollisionData data)
    {
        if (Math.Abs(speed.X) > this.maxSpeed * 0.85)
        {
            this.crashed = true;
            this.talker.Visible = false;

            if (driver != null && !driver.Dead)
            {
                driver.Die(Vector2.Zero);
            }

            Level scene = this.Scene as Level;
            ParticleEmitter particleEmitter = new ParticleEmitter(scene.ParticlesFG, ParticleTypes.Chimney, new Vector2(20f * this.facing, -8f), new Vector2(4f, 1f), -1.570796f, 5, 0.2f);
            this.Add(particleEmitter);
            particleEmitter.SimulateCycle();

            Audio.Play("event:/game/06_reflection/crushblock_activate", this.Position).setVolume(1f);
        }

        this.speed.X = 0;
    }

    public override void OnSquish(CollisionData data)
    {
        if (this.TrySquishWiggle(data) || SaveData.Instance.AssistMode && SaveData.Instance.Assists.Invincible)
        {
            return;
        }

        die();
    }

    private void die()
    {
        if (driver != null && this.currently_driving)
        {
            driver.Die(Vector2.Zero);
        }

        this.Visible = false;
        this.Collidable = false;
        this.talker.Visible = false;
        this.crashed = true;
    }

    private void OnCollideV(CollisionData data)
    {
        this.speed.Y = 0f;
    }
}
