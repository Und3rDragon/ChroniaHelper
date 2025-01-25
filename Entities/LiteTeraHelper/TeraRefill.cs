using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using Celeste;
using ChroniaHelper.Cores.LiteTeraHelper;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/teraRefill")]
public class TeraRefill : Entity
{
    private Image image;
    private Image outline;
    private BloomPoint bloom;
    private VertexLight light;
    private Level level;
    private SineWave sine;
    private bool oneUse;
    private bool shield;
    private TeraType tera;
    private ParticleType p_shatter;
    private ParticleType p_regen;
    private ParticleType p_glow;
    private float respawnTimer;

    private readonly Wiggler moveWiggle;
    private Vector2 moveWiggleDir;

    public TeraRefill(Vector2 position, bool oneUse, bool shield, TeraType type)
        : base(position)
    {
        this.oneUse = oneUse;
        this.shield = shield;
        if (shield)
        {
            Collider = new Circle(8f);
        }
        else
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
        }
        Add(new PlayerCollider(OnPlayer));
        p_shatter = Celeste.Refill.P_Shatter;
        p_regen = Celeste.Refill.P_Regen;
        p_glow = Celeste.Refill.P_Glow;
        tera = type;
        Add(outline = new Image(GFX.Game[TeraUtil.GetImagePath(TeraType.Any)]));
        outline.CenterOrigin();
        outline.Visible = false;
        Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
        image.CenterOrigin();
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.1f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        moveWiggle = Wiggler.Create(0.8f, 2f);
        moveWiggle.StartZero = true;
        Add(moveWiggle);
        UpdateY();
        Depth = -100;
    }

    public TeraRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("oneUse"), data.Bool("shield"), data.Enum("tera", TeraType.Normal))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
        }

        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, image.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        //bloom.Alpha = light.Alpha * 0.8f;
    }

    private void Respawn()
    {
        if (!Collidable)
        {
            Collidable = true;
            image.Visible = true;
            outline.Visible = false;
            Depth = -100;
            Audio.Play("event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
        }
    }

    private void UpdateY()
    {
        image.X = 0f;
        image.Y = bloom.Y = sine.Value * 2f;
        image.Position = image.Position + moveWiggleDir * moveWiggle.Value * -8f;
    }

    public override void Render()
    {
        if (image.Visible)
        {
            image.DrawOutline();
        }
        base.Render();
        if (shield && !outline.Visible)
            Draw.Circle(Position + image.Position, 8f, Color.White, 3);
    }

    private void OnPlayer(Player player)
    {
        var effect = TeraUtil.GetEffect(player.GetTera(), tera);
        if (shield)
        {
            if (effect == TeraEffect.None)
                return;
            if (effect == TeraEffect.Bad || (effect == TeraEffect.Normal && !player.DashAttacking))
            {
                //keep this as a feature
                //if (player.StateMachine.State == 5)
                //    player.StateMachine.State = 0;
                player.PointBounce(Center);
                moveWiggle.Start();
                moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
                Audio.Play("event:/game/06_reflection/feather_bubble_bounce", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                return;
            }
        }
        bool useRefill = player.UseRefill(false);
        bool changeTera = player.ChangeTera(tera);
        if (useRefill || changeTera)
        {
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }

    private IEnumerator RefillRoutine(Player player)
    {
        Celeste.Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        image.Visible = false;
        if (!oneUse)
        {
            outline.Visible = true;
        }

        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }
}