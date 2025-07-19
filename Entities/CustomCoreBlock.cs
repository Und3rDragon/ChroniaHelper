using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomCoreBlock")]
public class CustomCoreBlock : Solid
{
    public enum States
    {
        Waiting,
        WindingUp,
        Bouncing,
        BounceEnd,
        Broken
    }
    public class RespawnDebris : Entity
    {
        public Image sprite;

        public Vector2 from;

        public Vector2 to;

        public float percent;

        public float duration;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public RespawnDebris Init(Vector2 from, Vector2 to, bool ice, float duration)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(ice ? "objects/BumpBlockNew/ice_rubble" : "objects/BumpBlockNew/fire_rubble");
            MTexture texture = Calc.Random.Choose(atlasSubtextures);
            if (sprite == null)
            {
                Add(sprite = new Image(texture));
                sprite.CenterOrigin();
            }
            else
            {
                sprite.Texture = texture;
            }

            Position = (this.from = from);
            percent = 0f;
            this.to = to;
            this.duration = duration;
            return this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            if (percent > 1f)
            {
                RemoveSelf();
                return;
            }

            percent += Engine.DeltaTime / duration;
            Position = Vector2.Lerp(from, to, Ease.CubeIn(percent));
            sprite.Color = Color.White * percent;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            sprite.DrawOutline(Color.Black);
            base.Render();
        }
    }

    public class BreakDebris : Entity
    {
        public Image sprite;

        public Vector2 speed;

        public float percent;

        public float duration;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public BreakDebris Init(Vector2 position, Vector2 direction, bool ice)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(ice ? "objects/BumpBlockNew/ice_rubble" : "objects/BumpBlockNew/fire_rubble");
            MTexture texture = Calc.Random.Choose(atlasSubtextures);
            if (sprite == null)
            {
                Add(sprite = new Image(texture));
                sprite.CenterOrigin();
            }
            else
            {
                sprite.Texture = texture;
            }

            Position = position;
            direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
            speed = direction * (ice ? Calc.Random.Range(20, 40) : Calc.Random.Range(120, 200));
            percent = 0f;
            duration = Calc.Random.Range(2, 3);
            return this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            if (percent >= 1f)
            {
                RemoveSelf();
                return;
            }

            Position += speed * Engine.DeltaTime;
            speed.X = Calc.Approach(speed.X, 0f, 180f * Engine.DeltaTime);
            speed.Y += 200f * Engine.DeltaTime;
            percent += Engine.DeltaTime / duration;
            sprite.Color = Color.White * (1f - percent);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            sprite.DrawOutline(Color.Black);
            base.Render();
        }
    }

    public static ParticleType P_Reform = BounceBlock.P_Reform;

    public static ParticleType P_FireBreak = BounceBlock.P_FireBreak;

    public static ParticleType P_IceBreak = BounceBlock.P_IceBreak;

    public const float WindUpDelay = 0f;

    public const float WindUpDist = 10f;

    public const float IceWindUpDist = 16f;

    public const float BounceDist = 24f;

    public const float LiftSpeedXMult = 0.75f;

    public const float RespawnTime = 1.6f;

    public const float WallPushTime = 0.1f;

    public const float BounceEndTime = 0.05f;

    public Vector2 bounceDir;

    public States state;

    public Vector2 startPos;

    public float moveSpeed;

    public float windUpStartTimer;

    public float windUpProgress;

    public bool iceMode;

    public bool iceModeNext;

    public float respawnTimer;

    public float bounceEndTimer;

    public Vector2 bounceLift;

    public float reappearFlash;

    public bool reformed = true;

    public Vector2 debrisDirection;

    public List<Image> hotImages;

    public List<Image> coldImages;

    public Sprite hotCenterSprite;

    public Sprite coldCenterSprite;

    private bool notCoreMode;

    private bool isIceBlock;

    public CustomCoreBlock(Vector2 position, float width, float height, EntityData data)
        : base(position, width, height, safe: false)
    {
        isIceBlock = data.Int("type", 0) == 1;
        state = States.Waiting;
        startPos = Position;
        hotImages = BuildSprite(GFX.Game[data.Attr("fireBlockTexture", "objects/BumpBlockNew/fire00")]);
        hotCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterFire");
        hotCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
        hotCenterSprite.Visible = false;
        Add(hotCenterSprite);
        coldImages = BuildSprite(GFX.Game[data.Attr("iceBlockTexture", "objects/BumpBlockNew/ice00")]);
        coldCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterIce");
        coldCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
        coldCenterSprite.Visible = false;
        Add(coldCenterSprite);
        Add(new CoreModeListener(OnChangeMode));
    }

    public CustomCoreBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data)
    {
        notCoreMode = data.Bool("notCoreMode");
    }

    public List<Image> BuildSprite(MTexture source)
    {
        List<Image> list = new List<Image>();
        int num = source.Width / 8;
        int num2 = source.Height / 8;
        for (int i = 0; (float)i < base.Width; i += 8)
        {
            for (int j = 0; (float)j < base.Height; j += 8)
            {
                int num3 = ((i != 0) ? ((!((float)i >= base.Width - 8f)) ? Calc.Random.Next(1, num - 1) : (num - 1)) : 0);
                int num4 = ((j != 0) ? ((!((float)j >= base.Height - 8f)) ? Calc.Random.Next(1, num2 - 1) : (num2 - 1)) : 0);
                Image image = new Image(source.GetSubtexture(num3 * 8, num4 * 8, 8, 8));
                image.Position = new Vector2(i, j);
                list.Add(image);
                Add(image);
            }
        }

        return list;
    }

    public void ToggleSprite()
    {
        hotCenterSprite.Visible = !iceMode;
        coldCenterSprite.Visible = iceMode;
        foreach (Image hotImage in hotImages)
        {
            hotImage.Visible = !iceMode;
        }

        foreach (Image coldImage in coldImages)
        {
            coldImage.Visible = iceMode;
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        iceModeNext = (iceMode = SceneAs<Level>().CoreMode == Session.CoreModes.Cold || (notCoreMode && isIceBlock));
        
        ToggleSprite();
    }

    public void OnChangeMode(Session.CoreModes coreMode)
    {
        iceModeNext = coreMode == Session.CoreModes.Cold;
    }

    public void CheckModeChange()
    {
        if (iceModeNext != iceMode)
        {
            iceMode = iceModeNext;
            ToggleSprite();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        Vector2 position = Position;
        Position += base.Shake;
        if (state != States.Broken && reformed)
        {
            base.Render();
        }

        if (reappearFlash > 0f)
        {
            float num = Ease.CubeOut(reappearFlash);
            float num2 = num * 2f;
            Draw.Rect(base.X - num2, base.Y - num2, base.Width + num2 * 2f, base.Height + num2 * 2f, Color.White * num);
        }

        Position = position;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();

        reappearFlash = Calc.Approach(reappearFlash, 0f, Engine.DeltaTime * 8f);
        if (state == States.Waiting)
        {
            CheckModeChange();
            moveSpeed = Calc.Approach(moveSpeed, 100f, 400f * Engine.DeltaTime);
            Vector2 vector = Calc.Approach(base.ExactPosition, startPos, moveSpeed * Engine.DeltaTime);
            Vector2 liftSpeed = (vector - base.ExactPosition).SafeNormalize(moveSpeed);
            liftSpeed.X *= 0.75f;
            MoveTo(vector, liftSpeed);
            windUpProgress = Calc.Approach(windUpProgress, 0f, 1f * Engine.DeltaTime);
            Player player = WindUpPlayerCheck();
            if (player != null)
            {
                moveSpeed = 80f;
                windUpStartTimer = 0f;
                if (iceMode)
                {
                    bounceDir = -Vector2.UnitY;
                }
                else
                {
                    bounceDir = (player.Center - base.Center).SafeNormalize();
                }

                state = States.WindingUp;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                if (iceMode)
                {
                    StartShaking(0.2f);
                    Audio.Play("event:/game/09_core/iceblock_touch", base.Center);
                }
                else
                {
                    Audio.Play("event:/game/09_core/bounceblock_touch", base.Center);
                }
            }
        }
        else if (state == States.WindingUp)
        {
            Player player2 = WindUpPlayerCheck();
            if (player2 != null)
            {
                if (iceMode)
                {
                    bounceDir = -Vector2.UnitY;
                }
                else
                {
                    bounceDir = (player2.Center - base.Center).SafeNormalize();
                }
            }

            if (windUpStartTimer > 0f)
            {
                windUpStartTimer -= Engine.DeltaTime;
                windUpProgress = Calc.Approach(windUpProgress, 0f, 1f * Engine.DeltaTime);
                return;
            }

            moveSpeed = Calc.Approach(moveSpeed, iceMode ? 35f : 40f, 600f * Engine.DeltaTime);
            float num = (iceMode ? 0.333f : 1f);
            Vector2 vector2 = startPos - bounceDir * (iceMode ? 16f : 10f);
            Vector2 vector3 = Calc.Approach(base.ExactPosition, vector2, moveSpeed * num * Engine.DeltaTime);
            Vector2 liftSpeed2 = (vector3 - base.ExactPosition).SafeNormalize(moveSpeed * num);
            liftSpeed2.X *= 0.75f;
            MoveTo(vector3, liftSpeed2);
            windUpProgress = Calc.ClampedMap(Vector2.Distance(base.ExactPosition, vector2), 16f, 2f);
            if (iceMode && Vector2.DistanceSquared(base.ExactPosition, vector2) <= 12f)
            {
                StartShaking(0.1f);
            }
            else if (!iceMode && windUpProgress >= 0.5f)
            {
                StartShaking(0.1f);
            }

            if (Vector2.DistanceSquared(base.ExactPosition, vector2) <= 2f)
            {
                if (iceMode)
                {
                    Break();
                }
                else
                {
                    state = States.Bouncing;
                }

                moveSpeed = 0f;
            }
        }
        else if (state == States.Bouncing)
        {
            moveSpeed = Calc.Approach(moveSpeed, 140f, 800f * Engine.DeltaTime);
            Vector2 vector4 = startPos + bounceDir * 24f;
            Vector2 vector5 = Calc.Approach(base.ExactPosition, vector4, moveSpeed * Engine.DeltaTime);
            bounceLift = (vector5 - base.ExactPosition).SafeNormalize(Math.Min(moveSpeed * 3f, 200f));
            bounceLift.X *= 0.75f;
            MoveTo(vector5, bounceLift);
            windUpProgress = 1f;
            if (base.ExactPosition == vector4 || (!iceMode && WindUpPlayerCheck() == null))
            {
                debrisDirection = (vector4 - startPos).SafeNormalize();
                state = States.BounceEnd;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                moveSpeed = 0f;
                bounceEndTimer = 0.05f;
                ShakeOffPlayer(bounceLift);
            }
        }
        else if (state == States.BounceEnd)
        {
            bounceEndTimer -= Engine.DeltaTime;
            if (bounceEndTimer <= 0f)
            {
                Break();
            }
        }
        else
        {
            if (state != States.Broken)
            {
                return;
            }

            base.Depth = 8990;
            reformed = false;
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                return;
            }

            Vector2 position = Position;
            Position = startPos;
            if (!CollideCheck<Actor>() && !CollideCheck<Solid>())
            {
                CheckModeChange();
                Audio.Play(iceMode ? "event:/game/09_core/iceblock_reappear" : "event:/game/09_core/bounceblock_reappear", base.Center);
                float duration = 0.35f;
                for (int i = 0; (float)i < base.Width; i += 8)
                {
                    for (int j = 0; (float)j < base.Height; j += 8)
                    {
                        Vector2 vector6 = new Vector2(base.X + (float)i + 4f, base.Y + (float)j + 4f);
                        base.Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(vector6 + (vector6 - base.Center).SafeNormalize() * 12f, vector6, iceMode, duration));
                    }
                }

                Alarm.Set(this, duration, [MethodImpl(MethodImplOptions.NoInlining)] () =>
                {
                    reformed = true;
                    reappearFlash = 0.6f;
                    EnableStaticMovers();
                    ReformParticles();
                });
                base.Depth = -9000;
                MoveStaticMovers(Position - position);
                Collidable = true;
                state = States.Waiting;
            }
            else
            {
                Position = position;
            }
        }
    }

    public void ReformParticles()
    {
        Level level = SceneAs<Level>();
        for (int i = 0; (float)i < base.Width; i += 4)
        {
            level.Particles.Emit(P_Reform, new Vector2(base.X + 2f + (float)i + (float)Calc.Random.Range(-1, 1), base.Y), -MathF.PI / 2f);
            level.Particles.Emit(P_Reform, new Vector2(base.X + 2f + (float)i + (float)Calc.Random.Range(-1, 1), base.Bottom - 1f), MathF.PI / 2f);
        }

        for (int j = 0; (float)j < base.Height; j += 4)
        {
            level.Particles.Emit(P_Reform, new Vector2(base.X, base.Y + 2f + (float)j + (float)Calc.Random.Range(-1, 1)), MathF.PI);
            level.Particles.Emit(P_Reform, new Vector2(base.Right - 1f, base.Y + 2f + (float)j + (float)Calc.Random.Range(-1, 1)), 0f);
        }
    }

    public Player WindUpPlayerCheck()
    {
        Player player = CollideFirst<Player>(Position - Vector2.UnitY);
        if (player != null && player.Speed.Y < 0f)
        {
            player = null;
        }

        if (player == null)
        {
            player = CollideFirst<Player>(Position + Vector2.UnitX);
            if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Left)
            {
                player = CollideFirst<Player>(Position - Vector2.UnitX);
                if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Right)
                {
                    player = null;
                }
            }
        }

        return player;
    }

    public void ShakeOffPlayer(Vector2 liftSpeed)
    {
        Player player = WindUpPlayerCheck();
        if (player != null)
        {
            player.StateMachine.State = 0;
            player.Speed = liftSpeed;
            player.StartJumpGraceTime();
        }
    }

    public void Break()
    {
        if (!iceMode)
        {
            Audio.Play("event:/game/09_core/bounceblock_break", base.Center);
        }

        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        state = States.Broken;
        Collidable = false;
        DisableStaticMovers();
        respawnTimer = 1.6f;
        Vector2 direction = new Vector2(0f, 1f);
        if (!iceMode)
        {
            direction = debrisDirection;
        }

        Vector2 center = base.Center;
        for (int i = 0; (float)i < base.Width; i += 8)
        {
            for (int j = 0; (float)j < base.Height; j += 8)
            {
                if (iceMode)
                {
                    direction = (new Vector2(base.X + (float)i + 4f, base.Y + (float)j + 4f) - center).SafeNormalize();
                }

                base.Scene.Add(Engine.Pooler.Create<BreakDebris>().Init(new Vector2(base.X + (float)i + 4f, base.Y + (float)j + 4f), direction, iceMode));
            }
        }

        float num = debrisDirection.Angle();
        Level level = SceneAs<Level>();
        for (int k = 0; (float)k < base.Width; k += 4)
        {
            for (int l = 0; (float)l < base.Height; l += 4)
            {
                Vector2 vector = Position + new Vector2(2 + k, 2 + l) + Calc.Random.Range(-Vector2.One, Vector2.One);
                float direction2 = (iceMode ? (vector - center).Angle() : num);
                level.Particles.Emit(iceMode ? P_IceBreak : P_FireBreak, vector, direction2);
            }
        }
    }
}
