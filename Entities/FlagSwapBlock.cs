using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/FlagSwapBlock")]
public class FlagSwapBlock : BaseSolid2
{
    public class PathRenderer : Entity
    {
        public FlagSwapBlock block;

        public MTexture pathTexture;

        public MTexture clipTexture = new MTexture();

        public float timer;

        public PathRenderer(FlagSwapBlock block)
            : base(block.Position)
        {
            this.block = block;
            base.Depth = 8999;
            pathTexture = GFX.Game["objects/swapblock/path" + ((block.start.X == block.end.X) ? "V" : "H")];
            timer = Calc.Random.NextFloat();
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime * 4f;
        }

        public override void Render()
        {
            //if (block.Theme != Themes.Moon)
            //{
            //    for (int i = block.moveRect.Left; i < block.moveRect.Right; i += pathTexture.Width)
            //    {
            //        for (int j = block.moveRect.Top; j < block.moveRect.Bottom; j += pathTexture.Height)
            //        {
            //            pathTexture.GetSubtexture(0, 0, Math.Min(pathTexture.Width, block.moveRect.Right - i), Math.Min(pathTexture.Height, block.moveRect.Bottom - j), clipTexture);
            //            clipTexture.DrawCentered(new Vector2(i + clipTexture.Width / 2, j + clipTexture.Height / 2), Color.White);
            //        }
            //    }
            //}

            float num = 0.5f * (0.5f + ((float)Math.Sin(timer) + 1f) * 0.25f);
            block.DrawBlockStyle(new Vector2(block.moveRect.X, block.moveRect.Y), block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, Color.White * num);
        }
    }

    public static ParticleType P_Move = new(SwapBlock.P_Move);

    public const float ReturnTime = 0.8f;

    public Vector2 Direction;

    public bool Swapping;

    public Vector2 start;

    public Vector2 end;

    public float lerp;

    public int target;

    public Rectangle moveRect;

    public float speed;

    public SelectiveSlider forwardSpeed;

    public SelectiveSlider backwardSpeed;

    public float returnTimer;

    public float redAlpha = 1f;

    public MTexture[,] nineSliceGreen;

    public MTexture[,] nineSliceRed;

    public MTexture[,] nineSliceTarget;

    public Sprite middleGreen;

    public Sprite middleRed;

    public PathRenderer path;

    public EventInstance moveSfx;

    public EventInstance returnSfx;

    public DisplacementRenderer.Burst burst;

    public float particlesRemainder;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public FlagSwapBlock(EntityData d, Vc2 o) : base(o, d)
    {
        //Theme = Themes.Moon;
        start = Position;
        end = nodes[1];
        forwardSpeed = d.Slider("forwardSpeed", 360f);
        backwardSpeed = d.Slider("backwardSpeed", 360 * 0.4f);
        Direction.X = Math.Sign(end.X - start.X);
        Direction.Y = Math.Sign(end.Y - start.Y);
        Add(flag = new FlagListener(d.Attr("flag", "swap"))
        {
            onEnable = () => OnArgument(true),
            onDisable = () => OnArgument(false)
        });

        int num = (int)MathHelper.Min(base.X, nodes[1].X);
        int num2 = (int)MathHelper.Min(base.Y, nodes[1].Y);
        int num3 = (int)MathHelper.Max(base.X + base.Width, nodes[1].X + base.Width);
        int num4 = (int)MathHelper.Max(base.Y + base.Height, nodes[1].Y + base.Height);
        moveRect = new Rectangle(num, num2, num3 - num, num4 - num2);

        MTexture mTexture = GFX.Game[d.Attr("blockTexture", "objects /swapblock/moon/block")];
        MTexture mTexture2 = GFX.Game[d.Attr("blockRedTexture", "objects/swapblock/moon/blockRed")];
        MTexture mTexture3 = GFX.Game[d.Attr("blockTargetIndicator", "objects/swapblock/moon/target")];

        nineSliceGreen = new MTexture[3, 3];
        nineSliceRed = new MTexture[3, 3];
        nineSliceTarget = new MTexture[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSliceGreen[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceRed[i, j] = mTexture2.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceTarget[i, j] = mTexture3.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }

        // Create the light sprites
        Add(middleGreen = GFX.SpriteBank.Create(d.Attr("middleLightXML", "swapBlockLightMoon")));
        Add(middleRed = GFX.SpriteBank.Create(d.Attr("middleLightRedXML", "swapBlockLightRedMoon")));

        Add(new LightOcclude(d.Slider("lightOcclude", 0.2f, new(0f, 1f)).Value));
        base.Depth = d.Int("depth", -9999);
    }
    private FlagListener flag;
    private SelectiveSlider lightOcclude;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        scene.Add(path = new PathRenderer(this));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Audio.Stop(moveSfx);
        Audio.Stop(returnSfx);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Audio.Stop(moveSfx);
        Audio.Stop(returnSfx);
    }

    public void OnArgument(bool enabled)
    {
        Swapping = lerp < 1f;
        target = enabled ? 1 : 0;
        returnTimer = 0.8f;
        burst = (base.Scene as Level).Displacement.AddBurst(base.Center, 0.2f, 0f, 16f);
        //if (lerp >= 0.2f)
        //{
        //    speed = forwardSpeed;
        //}
        //else
        //{
        //    speed = MathHelper.Lerp(forwardSpeed * 0.333f, forwardSpeed, lerp / 0.2f);
        //}
        speed = enabled ? forwardSpeed.Value.ClampMin(0f) : backwardSpeed.Value.ClampMin(0f);

        Audio.Stop(returnSfx);
        Audio.Stop(moveSfx);
        if (!Swapping)
        {
            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
        }
        else
        {
            moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", base.Center);
        }
    }

    public override void Update()
    {
        base.Update();
        //if (returnTimer > 0f)
        //{
        //    returnTimer -= Engine.DeltaTime;
        //    if (returnTimer <= 0f)
        //    {
        //        target = 0;
        //        speed = 0f;
        //        returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", base.Center);
        //    }
        //}

        if (burst != null)
        {
            burst.Position = base.Center;
        }

        redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
        if (target == 0 && lerp == 0f)
        {
            middleRed.SetAnimationFrame(0);
            middleGreen.SetAnimationFrame(0);
        }

        //if (target == 1)
        //{
        //    speed = Calc.Approach(speed, forwardSpeed, forwardSpeed / 0.2f * Engine.DeltaTime);
        //}
        //else
        //{
        //    speed = Calc.Approach(speed, backwardSpeed, backwardSpeed / 1.5f * Engine.DeltaTime);
        //}

        float num = lerp;
        lerp = Calc.Approach(lerp, target, (speed / (end - start).Length().ClampMin(1e-6f)) * Engine.DeltaTime);
        if (lerp != num)
        {
            Vector2 liftSpeed = (end - start) * speed;
            Vector2 position = Position;
            //if (target == 1)
            //{
            //    liftSpeed = (end - start) * forwardSpeed;
            //}

            if (lerp < num)
            {
                liftSpeed *= -1f;
            }

            if (target == 1 && base.Scene.OnInterval(0.02f))
            {
                MoveParticles(end - start);
            }

            MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
            if (position != Position)
            {
                Audio.Position(moveSfx, base.Center);
                Audio.Position(returnSfx, base.Center);
                if (Position == start && target == 0)
                {
                    Audio.SetParameter(returnSfx, "end", 1f);
                    Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", base.Center);
                }
                else if (Position == end && target == 1)
                {
                    Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
                }
            }
        }

        if (Swapping && lerp >= 1f)
        {
            Swapping = false;
        }

        StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
    }

    public void MoveParticles(Vector2 normal)
    {
        Vector2 position;
        Vector2 positionRange;
        float direction;
        float num;
        if (normal.X > 0f)
        {
            position = base.CenterLeft;
            positionRange = Vector2.UnitY * (base.Height - 6f);
            direction = MathF.PI;
            num = Math.Max(2f, base.Height / 14f);
        }
        else if (normal.X < 0f)
        {
            position = base.CenterRight;
            positionRange = Vector2.UnitY * (base.Height - 6f);
            direction = 0f;
            num = Math.Max(2f, base.Height / 14f);
        }
        else if (normal.Y > 0f)
        {
            position = base.TopCenter;
            positionRange = Vector2.UnitX * (base.Width - 6f);
            direction = -MathF.PI / 2f;
            num = Math.Max(2f, base.Width / 14f);
        }
        else
        {
            position = base.BottomCenter;
            positionRange = Vector2.UnitX * (base.Width - 6f);
            direction = MathF.PI / 2f;
            num = Math.Max(2f, base.Width / 14f);
        }

        particlesRemainder += num;
        int num2 = (int)particlesRemainder;
        particlesRemainder -= num2;
        positionRange *= 0.5f;
        SceneAs<Level>().Particles.Emit(P_Move, num2, position, positionRange, direction);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        Vector2 vector = Position + base.Shake;
        if (lerp != (float)target && speed > 0f)
        {
            Vector2 vector2 = (end - start).SafeNormalize();
            if (target == 1)
            {
                vector2 *= -1f;
            }

            //float num = speed / forwardSpeed;
            float num = 1f;
            float num2 = 16f * num;
            for (int i = 2; (float)i < num2; i += 2)
            {
                DrawBlockStyle(vector + vector2 * i, base.Width, base.Height, nineSliceGreen, middleGreen, Color.White * (1f - (float)i / num2));
            }
        }

        if (redAlpha < 1f)
        {
            DrawBlockStyle(vector, base.Width, base.Height, nineSliceGreen, middleGreen, Color.White);
        }

        if (redAlpha > 0f)
        {
            DrawBlockStyle(vector, base.Width, base.Height, nineSliceRed, middleRed, Color.White * redAlpha);
        }
    }

    public void DrawBlockStyle(Vector2 pos, float width, float height, MTexture[,] ninSlice, Sprite middle, Color color)
    {
        int num = (int)(width / 8f);
        int num2 = (int)(height / 8f);
        ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
        ninSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
        ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
        for (int i = 1; i < num - 1; i++)
        {
            ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
            ninSlice[1, 2].Draw(pos + new Vector2(i * 8, height - 8f), Vector2.Zero, color);
        }

        for (int j = 1; j < num2 - 1; j++)
        {
            ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
            ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, j * 8), Vector2.Zero, color);
        }

        for (int k = 1; k < num - 1; k++)
        {
            for (int l = 1; l < num2 - 1; l++)
            {
                ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
            }
        }

        if (middle != null)
        {
            middle.Color = color;
            middle.RenderPosition = pos + new Vector2(width / 2f, height / 2f);
            middle.Render();
        }
    }
}
