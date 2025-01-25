using Celeste.Mod.Entities;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/goomyGate")]
public class GoomyGate : Solid
{
    private MTexture[,] nineSlice;
    private Sprite icon;
    private Vector2 iconOffset;
    private Wiggler wiggler;
    private Vector2 node;
    private SoundSource openSfx;
    private Color inactiveColor = Calc.HexToColor("5fcde4");
    private Color activeColor = Color.White;
    private Color finishColor = Calc.HexToColor("f141df");
    private bool open;
    public GoomyGate(Vector2 position, float width, float height, Vector2 node, string spriteName)
        : base(position, width, height, safe: false)
    {
        this.node = node;
        open = false;
        Add(icon = new Sprite(GFX.Game, "ChroniaHelper/objects/tera/Goomy/gate"));
        icon.Add("spin", "", 0.1f, "spin");
        icon.Play("spin");
        icon.Rate = 0f;
        icon.Color = inactiveColor;
        icon.Position = (iconOffset = new Vector2(width / 2f, height / 2f));
        icon.CenterOrigin();
        Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
        {
            icon.Scale = Vector2.One * (1f + f);
        }));
        MTexture mTexture = GFX.Game["objects/switchgate/" + spriteName];
        nineSlice = new MTexture[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
        Add(openSfx = new SoundSource());
        Add(new LightOcclude(0.5f));
    }

    public GoomyGate(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Attr("sprite", "block"))
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Add(new Coroutine(Sequence(node)));
    }

    public override void Render()
    {
        float num = base.Collider.Width / 8f - 1f;
        float num2 = base.Collider.Height / 8f - 1f;
        for (int i = 0; (float)i <= num; i++)
        {
            for (int j = 0; (float)j <= num2; j++)
            {
                int num3 = (((float)i < num) ? Math.Min(i, 1) : 2);
                int num4 = (((float)j < num2) ? Math.Min(j, 1) : 2);
                nineSlice[num3, num4].Draw(Position + base.Shake + new Vector2(i * 8, j * 8));
            }
        }

        icon.Position = iconOffset + base.Shake;
        icon.DrawOutline();
        base.Render();
    }
    public void Open()
    {
        open = true;
        SoundEmitter.Play("event:/game/general/touchswitch_last_oneshot");
        Add(new SoundSource("event:/game/general/touchswitch_last_cutoff"));
    }

    private IEnumerator Sequence(Vector2 node)
    {
        Vector2 start = Position;
        while (!open)
        {
            yield return null;
        }

        yield return 0.1f;
        openSfx.Play("event:/game/general/touchswitch_gate_open");
        StartShaking(0.5f);
        while (icon.Rate < 1f)
        {
            icon.Color = Color.Lerp(inactiveColor, activeColor, icon.Rate);
            icon.Rate += Engine.DeltaTime * 2f;
            yield return null;
        }

        yield return 0.1f;
        int particleAt = 0;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 2f, start: true);
        tween.OnUpdate = delegate (Tween t)
        {
            MoveTo(Vector2.Lerp(start, node, t.Eased));
            if (Scene.OnInterval(0.1f))
            {
                particleAt++;
                particleAt %= 2;
                for (int n = 0; (float)n < Width / 8f; n++)
                {
                    for (int num2 = 0; (float)num2 < Height / 8f; num2++)
                    {
                        if ((n + num2) % 2 == particleAt)
                        {
                            SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind, Position + new Vector2(n * 8, num2 * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                        }
                    }
                }
            }
        };
        Add(tween);
        yield return 1.8f;
        bool collidable = Collidable;
        Collidable = false;
        if (node.X <= start.X)
        {
            Vector2 vector = new Vector2(0f, 2f);
            for (int i = 0; (float)i < Height / 8f; i++)
            {
                Vector2 vector2 = new Vector2(Left - 1f, Top + 4f + (float)(i * 8));
                Vector2 point = vector2 + Vector2.UnitX;
                if (Scene.CollideCheck<Solid>(vector2) && !Scene.CollideCheck<Solid>(point))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector2 + vector, (float)Math.PI);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector2 - vector, (float)Math.PI);
                }
            }
        }

        if (node.X >= start.X)
        {
            Vector2 vector3 = new Vector2(0f, 2f);
            for (int j = 0; (float)j < Height / 8f; j++)
            {
                Vector2 vector4 = new Vector2(Right + 1f, Top + 4f + (float)(j * 8));
                Vector2 point2 = vector4 - Vector2.UnitX * 2f;
                if (Scene.CollideCheck<Solid>(vector4) && !Scene.CollideCheck<Solid>(point2))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector4 + vector3, 0f);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector4 - vector3, 0f);
                }
            }
        }

        if (node.Y <= start.Y)
        {
            Vector2 vector5 = new Vector2(2f, 0f);
            for (int k = 0; (float)k < Width / 8f; k++)
            {
                Vector2 vector6 = new Vector2(Left + 4f + (float)(k * 8), Top - 1f);
                Vector2 point3 = vector6 + Vector2.UnitY;
                if (Scene.CollideCheck<Solid>(vector6) && !Scene.CollideCheck<Solid>(point3))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector6 + vector5, -(float)Math.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector6 - vector5, -(float)Math.PI / 2f);
                }
            }
        }

        if (node.Y >= start.Y)
        {
            Vector2 vector7 = new Vector2(2f, 0f);
            for (int l = 0; (float)l < Width / 8f; l++)
            {
                Vector2 vector8 = new Vector2(Left + 4f + (float)(l * 8), Bottom + 1f);
                Vector2 point4 = vector8 - Vector2.UnitY * 2f;
                if (Scene.CollideCheck<Solid>(vector8) && !Scene.CollideCheck<Solid>(point4))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector8 + vector7, (float)Math.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector8 - vector7, (float)Math.PI / 2f);
                }
            }
        }

        Collidable = collidable;
        Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
        StartShaking(0.2f);
        while (icon.Rate > 0f)
        {
            icon.Color = Color.Lerp(activeColor, finishColor, 1f - icon.Rate);
            icon.Rate -= Engine.DeltaTime * 4f;
            yield return null;
        }

        icon.Rate = 0f;
        icon.SetAnimationFrame(0);
        wiggler.Start();
        bool collidable2 = Collidable;
        Collidable = false;
        if (!Scene.CollideCheck<Solid>(Center))
        {
            for (int m = 0; m < 32; m++)
            {
                float num = Calc.Random.NextFloat((float)Math.PI * 2f);
                SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, Position + iconOffset + Calc.AngleToVector(num, 4f), num);
            }
        }

        Collidable = collidable2;
    }
}