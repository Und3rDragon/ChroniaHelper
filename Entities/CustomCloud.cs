using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System.Runtime.CompilerServices;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
[CustomEntity("ChroniaHelper/CustomCloud")]
public class CustomCloud : JumpThru
{
    public Sprite sprite;
    public Wiggler wiggler;
    public ParticleType particleType;
    public SoundSource sfx;
    public bool waiting = true;
    public float speed;
    public float startY;
    public float respawnTimer, respawnTime;
    public bool returning;
    public bool fragile;
    public float timer;
    public Vector2 scale, origScale, scaleOnPress;
    public bool canRumble;
    public bool? Small;
    public string sound, appearSound;
    public float liftSpeedMult = 1f, neutralLiftSpeed = -200f;
    public string spriteXML;
    public float bumperness;
    public CustomCloud(EntityData data, Vc2 offset)
        : base(data.Position + offset, data.Int("colliderWidth", 32).ClampMin(1), data.Bool("forceSafe", false))
    {
        this.fragile = data.Bool("fragile");
        startY = base.Y;
        base.Collider.Position.X = data.Float("colliderOffset", -16f);
        timer = Calc.Random.NextFloat() * 4f;
        Add(wiggler = Wiggler.Create(0.3f, 4f));
        particleType = (fragile ? new(Cloud.P_FragileCloud) : new(Cloud.P_Cloud));
        particleType.Color = data.GetChroniaColor("particleColor", particleType.Color).Parsed();
        SurfaceSoundIndex = data.Int("soundIndex", 4);
        Add(new LightOcclude(data.Float("lightOcclude", 0.2f)));
        origScale = scale = data.Vector2("scaleX", "scaleY", Vc2.One);
        scaleOnPress = data.Vector2("scaleXOnPress", "scaleYOnPress", new Vc2(1.3f, 0.7f));
        Add(sfx = new SoundSource());
        sound = data.Attr("cloudSound", "default");
        appearSound = data.Attr("cloudAppearSound", "default");
        respawnTime = data.Float("respawnTime", 2.5f);
        liftSpeedMult = data.Float("liftSpeedMultiplier", 1f);
        spriteXML = data.Attr("spriteXMLTag", "cloud");
        Depth = data.Int("depth", 0);
        neutralLiftSpeed = data.Float("neutralLiftSpeed", -200f);
        bumperness = data.Float("bumperness", 1f);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        string text = fragile ? (spriteXML + "Fragile") : spriteXML;
        //if (IsSmall((byte)SceneAs<Level>().Session.Area.Mode != 0))
        //{
        //    base.Collider.Position.X += 2f;
        //    base.Collider.Width -= 6f;
        //    text += "Remix";
        //}

        Add(sprite = GFX.SpriteBank.Create(text));
        sprite.Origin = new Vector2(sprite.Width / 2f, 8f);
        sprite.OnFrameChange = [MethodImpl(MethodImplOptions.NoInlining)] (string s) =>
        {
            if (s == "spawn" && sprite.CurrentAnimationFrame == sprite.CurrentAnimationTotalFrames - 1)
            {
                wiggler.Start();
            }
        };
    }

    public override void Update()
    {
        base.Update();

        scale.X = Calc.Approach(scale.X, origScale.X, 1f * Engine.DeltaTime);
        scale.Y = Calc.Approach(scale.Y, origScale.Y, 1f * Engine.DeltaTime);
        timer += Engine.DeltaTime;
        if (GetPlayerRider() != null)
        {
            sprite.Position = Vector2.Zero;
        }
        else
        {
            sprite.Position = Calc.Approach(sprite.Position, new Vector2(0f, (float)Math.Sin(timer * 2f)), Engine.DeltaTime * 4f);
        }

        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                waiting = true;
                base.Y = startY;
                speed = 0f;
                scale = origScale;
                Collidable = true;
                sprite.Play("spawn");
                if(appearSound.ToLower() == "default" || !appearSound.HasValidContent())
                {
                    sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
                }
                else
                {
                    sfx.Play(appearSound);
                }
            }

            return;
        }

        if (waiting)
        {
            Player playerRider = GetPlayerRider();
            if (playerRider != null && playerRider.Speed.Y >= 0f)
            {
                canRumble = true;
                speed = 180f;
                scale = scaleOnPress;
                waiting = false;
                if(sound.ToLower() == "default" || !sound.HasValidContent())
                {
                    if (fragile)
                    {
                        Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
                    }
                    else
                    {
                        Audio.Play("event:/game/04_cliffside/cloud_blue_boost", Position);
                    }
                }
                else
                {
                    Audio.Play(sound, Position);
                }
            }

            return;
        }

        if (returning)
        {
            speed = Calc.Approach(speed, 180f, 600f * Engine.DeltaTime);
            MoveTowardsY(startY, speed * Engine.DeltaTime);
            if (base.ExactPosition.Y == startY)
            {
                returning = false;
                waiting = true;
                speed = 0f;
            }

            return;
        }

        if (fragile && Collidable && !HasPlayerRider())
        {
            Collidable = false;
            sprite.Play("fade");
        }

        if (speed < 0f && canRumble)
        {
            canRumble = false;
            if (HasPlayerRider())
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        }

        if (speed < 0f && base.Scene.OnInterval(0.02f))
        {
            (base.Scene as Level).ParticlesBG.Emit(particleType, 1, Position + new Vector2(0f, 2f), new Vector2(base.Collider.Width / 2f, 1f), MathF.PI / 2f);
        }

        if (fragile && speed < 0f)
        {
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 0f, Engine.DeltaTime * 4f);
        }

        if (base.Y >= startY)
        {
            speed -= 1200f * Engine.DeltaTime;
        }
        else
        {
            speed += 1200f * Engine.DeltaTime;
            if (speed >= -100f)
            {
                Player playerRider2 = GetPlayerRider();
                if (playerRider2 != null && playerRider2.Speed.Y >= 0f)
                {
                    playerRider2.Speed.Y = neutralLiftSpeed;
                }

                if (fragile)
                {
                    Collidable = false;
                    sprite.Play("fade");
                    respawnTimer = respawnTime.GetAbs();
                }
                else
                {
                    scale = new Vector2(0.7f, 1.3f);
                    returning = true;
                }
            }
        }

        float num = speed;
        if (num < 0f)
        {
            num = -220f;
        }
        
        MoveV(speed * Engine.DeltaTime * bumperness, num * liftSpeedMult);
    }

    public override void Render()
    {
        Vector2 vector = scale;
        vector *= 1f + 0.1f * wiggler.Value;
        sprite.Scale = vector;
        base.Render();
    }

    //public bool IsSmall(bool value)
    //{
    //    return Small ?? value;
    //}
}
