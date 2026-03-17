using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.TrackSpinner;
using static ChroniaHelper.Utils.PlayerFacing;

namespace ChroniaHelper.WIPs.Entities;

[WorkingInProgress]
[CustomEntity("ChroniaHelper/CustomShield")]
public class CustomShield : BaseEntity
{
    public BloomPoint bloom;
    public VertexLight light;
    public Wiggler moveWiggle;
    public Wiggler shieldRadiusWiggle;
    public SineWave sine;
    public Vector2 moveWiggleDir;

    public CustomShield(EntityData data, Vc2 offset) : base(data, offset)
    {
        radius = data.Attr("radius", "10");
        square = data.Bool("square", false);
        ChroniaCollider.ColliderBuilder.ColliderType builderType = 
            square ? 
            ChroniaCollider.ColliderBuilder.ColliderType.Hitbox
            : ChroniaCollider.ColliderBuilder.ColliderType.Circle;
        List<string> builderParams = square ?
            new() { $"({radius}) * 2", $"({radius}) * 2", $"-({radius})", $"-({radius})" }
            : new() { radius };
        ChroniaCollider.ColliderBuilder builder = new(builderType, builderParams);
        dynCollider = new(builder);
        Add(dynCollider);

        Add(new PlayerCollider(OnPlayer));

        bloomAlpha = data.Slider("bloomAlpha", 0.5f, new(0f, 1f));
        bloomRadius = data.Slider("bloomRadius", 20f);
        Add(bloom = new BloomPoint(bloomAlpha.Value, bloomRadius.Value.GetAbs()));

        lightFlag = data.Attr("lightFlag", "lightOn");
        lightColor = data.GetChroniaColor("lightColor", Color.White);
        lightAlpha = data.Slider("lightAlpha", 1f, new(0f, 1f));
        lightStartFade = data.Slider("lightStartFade", 16f);
        lightEndFade = data.Slider("lightEndFade", 48f);
        Add(light = new VertexLight(lightColor.Parsed(), 
            lightFlag.GetFlag() ? lightAlpha.Value : 0f, 
            (int)(lightStartFade.Value.GetAbs()), 
            (int)(lightEndFade.Value.GetAbs()))
            );
        Add(sine = new SineWave(0.6f, 0.0f).Randomize());

        shieldRadiusWiggle = Wiggler.Create(0.5f, 4f);
        Add(shieldRadiusWiggle);
        moveWiggle = Wiggler.Create(0.8f, 2f);
        moveWiggle.StartZero = true;
        Add(moveWiggle);

        color = data.GetChroniaColor("color", Color.White);
    }
    private string radius;
    private bool square;
    private ChroniaCollider dynCollider;
    private string lightFlag;
    private SelectiveSlider bloomAlpha, bloomRadius;
    private ChroniaColor lightColor;
    private SelectiveSlider lightAlpha, lightStartFade, lightEndFade;
    private ChroniaColor color;

    public override void Update()
    {
        base.Update();

        light.Alpha = Calc.Approach(light.Alpha, lightFlag.GetFlag() ? 1f : 0.0f, 4f * Engine.DeltaTime);
        light.StartRadius = lightStartFade.Value.GetAbs();
        light.EndRadius = lightEndFade.Value.GetAbs();

        bloom.Radius = bloomRadius.Value.GetAbs();
        bloom.Alpha = Calc.Approach(bloom.Alpha, lightFlag.GetFlag() ? bloomAlpha.Value.GetAbs() : 0.0f , 4f * Engine.DeltaTime);
    }

    public override void Render()
    {
        base.Render();

        if (!square)
        {
            float r = (float)(radius.ParseMathExpression() - shieldRadiusWiggle.Value * 2.0);
            Draw.Circle(Position, r, color.Parsed(), 3);
        }
        else
        {
            float r = (float)(radius.ParseMathExpression() - shieldRadiusWiggle.Value * 1.0);
            Draw.HollowRect(Position - new Vc2(r, r), 2 * r, 2 * r, color.Parsed());
        }
    }

    private void OnPlayer(Player player)
    {
        Vc2 dP = player.Center - Center;

        if (!square)
        {
            PointBounce(player, Center);
        }
        else
        {
            Vc2 d = player.Center - Center;
            float a = d.Angle();
            if(a >= float.Pi * 0.25f && a < float.Pi * 0.75f)
            {
                PointBounce(player, player.Center - Vc2.UnitY);
                dP = -Vc2.UnitY;
            }

            else if(a >= float.Pi * 0.75f || a < float.Pi * -0.75f)
            {
                PointBounce(player, player.Center + Vc2.UnitX);
                dP = Vc2.UnitX;
            }

            else if(a > float.Pi * -0.25f && a < float.Pi * 0.25f)
            {
                PointBounce(player, player.Center - Vc2.UnitX);
                dP = -Vc2.UnitX;
            }

            else
            {
                PointBounce(player, player.Center + Vc2.UnitY);
                dP = Vc2.UnitY;
            }
        }

        if (Input.MoveX.Value == Math.Sign(player.Speed.X))
        {
            player.Speed.X *= 1.2f;
        }

        moveWiggle.Start();
        shieldRadiusWiggle.Start();
        moveWiggleDir = (-dP).SafeNormalize(Vector2.UnitY);
        Audio.Play("event:/game/06_reflection/feather_bubble_bounce", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        level.DirectionalShake((dP).SafeNormalize(), 0.15f);
    }

    public void PointBounce(Player player, Vector2 from)
    {
        if (player.StateMachine.State == 2)
        {
            player.StateMachine.State = 0;
        }

        if (player.StateMachine.State == 4 && player.CurrentBooster != null)
        {
            player.CurrentBooster.PlayerReleased();
        }

        player.RefillDash();
        player.RefillStamina();
        Vector2 vector = (player.Center - from).SafeNormalize();
        if (vector.Y > -0.2f && vector.Y <= 0.4f)
        {
            vector.Y = -0.2f;
        }

        player.Speed = vector * 220f;
        player.Speed.X *= 1.5f;
        if (Math.Abs(player.Speed.X) < 100f)
        {
            if (player.Speed.X == 0f)
            {
                player.Speed.X = square ? 0 : (float)(0 - player.Facing) * 100f;
            }
            else
            {
                player.Speed.X = (float)Math.Sign(player.Speed.X) * 100f;
            }
        }
    }
}
