using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/LightningBreakerBox")]
public class LightningBreakerBox : Solid
{
    public static ParticleType P_Smash;

    public static ParticleType P_Sparks;

    public Sprite sprite;

    public SineWave sine;

    public Vector2 start;

    public float sink;

    public int health = 2;

    public bool flag;

    public float shakeCounter;

    public string music;

    public int musicProgress = -1;

    public bool musicStoreInSession;

    public Vector2 bounceDir;

    public Wiggler bounce;

    public Shaker shaker;

    public bool makeSparks;

    public bool smashParticles;

    public Coroutine pulseRoutine;

    public SoundSource firstHitSfx;

    public bool spikesLeft;

    public bool spikesRight;

    public bool spikesUp;

    public bool spikesDown;

    public LightningBreakerBox(Vector2 position, bool flipX)
        : base(position, 32f, 32f, safe: true)
    {
        SurfaceSoundIndex = 9;
        start = Position;
        sprite = GFX.SpriteBank.Create("breakerBox");
        Sprite obj = sprite;
        obj.OnLastFrame = (Action<string>)Delegate.Combine(obj.OnLastFrame, (Action<string>)([MethodImpl(MethodImplOptions.NoInlining)] (string anim) =>
        {
            if (anim == "break")
            {
                Visible = false;
            }
            else if (anim == "open")
            {
                makeSparks = true;
            }
        }));
        sprite.Position = new Vector2(base.Width, base.Height) / 2f;
        sprite.FlipX = flipX;
        Add(sprite);
        sine = new SineWave(0.5f, 0f);
        Add(sine);
        bounce = Wiggler.Create(1f, 0.5f);
        bounce.StartZero = false;
        Add(bounce);
        Add(shaker = new Shaker(on: false));
        OnDashCollide = Dashed;
    }

    public LightningBreakerBox(EntityData e, Vector2 levelOffset)
        : this(e.Position + levelOffset, e.Bool("flipX"))
    {
        flag = e.Bool("flag");
        music = e.Attr("music", null);
        musicProgress = e.Int("music_progress", -1);
        musicStoreInSession = e.Bool("music_session");

        vanillaLighting = e.Bool("vanillaLightingChanges", false);
    }
    private bool vanillaLighting;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        spikesUp = CollideCheck<Spikes>(Position - Vector2.UnitY);
        spikesDown = CollideCheck<Spikes>(Position + Vector2.UnitY);
        spikesLeft = CollideCheck<Spikes>(Position - Vector2.UnitX);
        spikesRight = CollideCheck<Spikes>(Position + Vector2.UnitX);
    }

    public DashCollisionResults Dashed(Player player, Vector2 dir)
    {
        if (!SaveData.Instance.Assists.Invincible)
        {
            if (dir == Vector2.UnitX && spikesLeft)
            {
                return DashCollisionResults.NormalCollision;
            }

            if (dir == -Vector2.UnitX && spikesRight)
            {
                return DashCollisionResults.NormalCollision;
            }

            if (dir == Vector2.UnitY && spikesUp)
            {
                return DashCollisionResults.NormalCollision;
            }

            if (dir == -Vector2.UnitY && spikesDown)
            {
                return DashCollisionResults.NormalCollision;
            }
        }

        (base.Scene as Level).DirectionalShake(dir);
        sprite.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
        health--;
        if (health > 0)
        {
            Add(firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
            Celeste.Celeste.Freeze(0.1f);
            shakeCounter = 0.2f;
            shaker.On = true;
            bounceDir = dir;
            bounce.Start();
            smashParticles = true;
            Pulse();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }
        else
        {
            if (firstHitSfx != null)
            {
                firstHitSfx.Stop();
            }

            Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
            Celeste.Celeste.Freeze(0.2f);
            player.RefillDash();
            Break();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            SmashParticles(dir.Perpendicular());
            SmashParticles(-dir.Perpendicular());
        }

        return DashCollisionResults.Rebound;
    }

    public void SmashParticles(Vector2 dir)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        int num;
        if (dir == Vector2.UnitX)
        {
            direction = 0f;
            position = base.CenterRight - Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
            num = (int)(base.Height / 8f) * 4;
        }
        else if (dir == -Vector2.UnitX)
        {
            direction = MathF.PI;
            position = base.CenterLeft + Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
            num = (int)(base.Height / 8f) * 4;
        }
        else if (dir == Vector2.UnitY)
        {
            direction = MathF.PI / 2f;
            position = base.BottomCenter - Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }
        else
        {
            direction = -MathF.PI / 2f;
            position = base.TopCenter + Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }

        num += 2;
        SceneAs<Level>().Particles.Emit(P_Smash, num, position, positionRange, direction);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (flag && (base.Scene as Level).Session.GetFlag("disable_lightning"))
        {
            RemoveSelf();
        }
    }

    public override void Update()
    {
        base.Update();
        if (makeSparks && base.Scene.OnInterval(0.03f))
        {
            SceneAs<Level>().ParticlesFG.Emit(P_Sparks, 1, base.Center, Vector2.One * 12f);
        }

        if (shakeCounter > 0f)
        {
            shakeCounter -= Engine.DeltaTime;
            if (shakeCounter <= 0f)
            {
                shaker.On = false;
                sprite.Scale = Vector2.One * 1.2f;
                sprite.Play("open");
            }
        }

        if (Collidable)
        {
            bool flag = HasPlayerRider();
            sink = Calc.Approach(sink, flag ? 1 : 0, 2f * Engine.DeltaTime);
            sine.Rate = MathHelper.Lerp(1f, 0.5f, sink);
            Vector2 vector = start;
            vector.Y += sink * 6f + sine.Value * MathHelper.Lerp(4f, 2f, sink);
            vector += bounce.Value * bounceDir * 12f;
            MoveToX(vector.X);
            MoveToY(vector.Y);
            if (smashParticles)
            {
                smashParticles = false;
                SmashParticles(bounceDir.Perpendicular());
                SmashParticles(-bounceDir.Perpendicular());
            }
        }

        sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
        sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
        LiftSpeed = Vector2.Zero;
    }

    public override void Render()
    {
        Vector2 position = sprite.Position;
        sprite.Position += shaker.Value;
        base.Render();
        sprite.Position = position;
    }

    public void Pulse()
    {
        pulseRoutine = new Coroutine(Lightning.PulseRoutine(SceneAs<Level>()));
        pulseRoutine.Active = vanillaLighting;
        Add(pulseRoutine);
    }

    public void Break()
    {
        Session session = (base.Scene as Level).Session;
        RumbleTrigger.ManuallyTrigger(base.Center.X, 1.2f);
        base.Tag = Tags.Persistent;
        shakeCounter = 0f;
        shaker.On = false;
        sprite.Play("break");
        Collidable = false;
        DestroyStaticMovers();
        if (flag)
        {
            session.SetFlag("disable_lightning");
        }

        if (musicStoreInSession)
        {
            if (!string.IsNullOrEmpty(music))
            {
                session.Audio.Music.Event = SFX.EventnameByHandle(music);
            }

            if (musicProgress >= 0)
            {
                session.Audio.Music.SetProgress(musicProgress);
            }

            session.Audio.Apply(forceSixteenthNoteHack: false);
        }
        else
        {
            if (!string.IsNullOrEmpty(music))
            {
                Audio.SetMusic(SFX.EventnameByHandle(music), startPlaying: false);
            }

            if (musicProgress >= 0)
            {
                Audio.SetMusicParam("progress", musicProgress);
            }

            if (!string.IsNullOrEmpty(music) && Audio.CurrentMusicEventInstance != null)
            {
                Audio.CurrentMusicEventInstance.start();
            }
        }

        if (pulseRoutine != null)
        {
            pulseRoutine.Active = false;
        }

        Add(new Coroutine(Lightning.RemoveRoutine(SceneAs<Level>(), base.RemoveSelf)));
    }
}
