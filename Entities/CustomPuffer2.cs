using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[Note("Forked a CustomPuffer from Crystalline Helper" +
    "But then found that CrystallineHelper one isn't delegated from the original" +
    "Which means some actor cannot be affected by the puffer" +
    "So this one comes in handy by delegating from vanilla")]
[Tracked]
[CustomEntity("ChroniaHelper/CustomPuffer2")]
[Obsoleted("Bug found, kept class file for furture study purposes")]
public class CustomPuffer2 : Puffer
{
    public CustomPuffer2(EntityData data, Vc2 offset): base(data, offset)
    {
        Collider = data.Attr("colliders", "r,12,10,-6,-5").ParseColliderList(new ColliderList(new Hitbox(12f, 10f, -6f, -5f)));

        // Erasing all components before re-initializing
        Components.components.Clear();

        Add(new PlayerCollider(OnPlayer, data.Attr("playerColliders", "r,14,12,-7,-7").ParseColliderList(new ColliderList(new Hitbox(14f, 12f, -7f, -7f)))));
        Add(sprite = GFX.SpriteBank.Create(data.Attr("spriteXMLTag", "pufferFish")));
        sprite.Play("idle");
        if (!data.Bool("right", false))
        {
            facing.X = -1f;
        }

        idleSine = new SineWave(data.Float("waveFrequency", 0.5f), data.Float("waveOffset", 0f));
        idleSine.Randomize();
        Add(idleSine);

        // Re-initialize position
        Position = data.Position + offset;

        anchorPosition = Position;
        Position += new Vector2(idleSine.Value * 3f, idleSine.ValueOverTwo * 2f);
        state = States.Idle;
        startPosition = (lastSinePosition = (lastSpeedPosition = Position));
        pushRadius = new Circle(data.Float("radiusPush", 40f));
        detectRadius = new Circle(data.Float("radiusDetect", 32f));
        breakWallsRadius = new Circle(data.Float("radiusBreakWalls", 16f));
        onCollideV = OnCollideV;
        onCollideH = OnCollideH;
        scale = data.Vector2("scaleX", "scaleY", Vc2.One);
        bounceWiggler = Wiggler.Create(data.Float("bounceWiggleDuration", 0.6f), data.Float("bounceWiggleFrquency", 2.5f), delegate (float v) {
            sprite.Rotation = v * 20f * ((float)Math.PI / 180f);
        });
        Add(bounceWiggler);
        inflateWiggler = Wiggler.Create(data.Float("inflateWiggleDuration", 0.6f), data.Float("inflateWiggleFrequency", 2f));
        Add(inflateWiggler);

        overrideOutline = data.Int("overrideOutline", 0);
        indicatorColor = data.GetChroniaColor("indicatorColor", Color.White);

        Depth = data.Int("depth", 1);
    }
    /// <summary>
    /// Default = 0 <br/>
    /// True = 1 <br/>
    /// False = 2
    /// </summary>
    private int overrideOutline = 0;
    private CColor indicatorColor = CColor.White;

    public override void Render()
    {
        sprite.Scale = scale * (1f + inflateWiggler.Value * 0.4f);
        sprite.Scale *= facing;
        bool flag = false;
        if (sprite.CurrentAnimationID != "hidden" && sprite.CurrentAnimationID != "explode" && sprite.CurrentAnimationID != "recover")
        {
            flag = true;
        }
        else if (sprite.CurrentAnimationID == "explode" && sprite.CurrentAnimationFrame <= 1)
        {
            flag = true;
        }
        else if (sprite.CurrentAnimationID == "recover" && sprite.CurrentAnimationFrame >= 4)
        {
            flag = true;
        }

        if (overrideOutline == 0 ? flag : overrideOutline == 1)
        {
            sprite.DrawSimpleOutline();
        }

        float num = playerAliveFade * Calc.ClampedMap((Position - lastPlayerPos).Length(), 128f, 96f);
        if (num > 0f && state != States.Gone)
        {
            bool flag2 = false;
            Vector2 vector = lastPlayerPos;
            if (vector.Y < base.Y)
            {
                vector.Y = base.Y - (vector.Y - base.Y) * 0.5f;
                vector.X += vector.X - base.X;
                flag2 = true;
            }

            float radiansB = (vector - Position).Angle();
            for (int i = 0; i < 28; i++)
            {
                float num2 = (float)Math.Sin(base.Scene.TimeActive * 0.5f) * 0.02f;
                float num3 = Calc.Map((float)i / 28f + num2, 0f, 1f, -MathF.PI / 30f, 3.24631262f);
                num3 += bounceWiggler.Value * 20f * (MathF.PI / 180f);
                Vector2 vector2 = Calc.AngleToVector(num3, 1f);
                Vector2 vector3 = Position + vector2 * 32f;
                float t = Calc.ClampedMap(Calc.AbsAngleDiff(num3, radiansB), MathF.PI / 2f, 0.17453292f);
                t = Ease.CubeOut(t) * 0.8f * num;
                if (!(t > 0f))
                {
                    continue;
                }

                if (i == 0 || i == 27)
                {
                    Draw.Line(vector3, vector3 - vector2 * 10f, indicatorColor.Parsed(t));
                    continue;
                }

                Vector2 vector4 = vector2 * (float)Math.Sin(base.Scene.TimeActive * 2f + (float)i * 0.6f);
                if (i % 2 == 0)
                {
                    vector4 *= -1f;
                }

                vector3 += vector4;
                if (!flag2 && Calc.AbsAngleDiff(num3, radiansB) <= 0.17453292f)
                {
                    Draw.Line(vector3, vector3 - vector2 * 3f, indicatorColor.Parsed(t));
                }
                else
                {
                    Draw.Point(vector3, indicatorColor.Parsed(t));
                }
            }
        }
        
        // Replacing base.Render()
        Components.Render();

        if (sprite.CurrentAnimationID == "alerted")
        {
            Vector2 vector5 = Position + new Vector2(3f, (facing.X < 0f) ? (-5) : (-4)) * sprite.Scale;
            Vector2 to = lastPlayerPos + new Vector2(0f, -4f);
            Vector2 vector6 = Calc.AngleToVector(Calc.Angle(vector5, to) + eyeSpin * (MathF.PI * 2f) * 2f, 1f);
            Vector2 vector7 = vector5 + new Vector2((float)Math.Round(vector6.X), (float)Math.Round(Calc.ClampedMap(vector6.Y, -1f, 1f, -1f, 2f)));
            Draw.Rect(vector7.X, vector7.Y, 1f, 1f, Color.Black);
        }

        sprite.Scale /= facing;
    }
}
