using System;
using System.Collections;
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
[CustomEntity("ChroniaHelper/BezierZipmover")]
public class BezierZipmover : Solid
{
    public class PathRenderer : Entity
    {
        public BezierZipmover ZipMover;
        
        public MTexture cog;
        
        public Vector2 from;
        
        public Vector2 to;
        
        public Vector2 sparkAdd;
        
        public float sparkDirFromA;
        
        public float sparkDirFromB;
        
        public float sparkDirToA;
        
        public float sparkDirToB;
        
        public PathRenderer(BezierZipmover zipMover, string cogPath = "objects/zipmover/cog")
        {
            base.Depth = 5000;
            ZipMover = zipMover;
            from = ZipMover.start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
            to = ZipMover.target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
            sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            sparkDirFromA = num + MathF.PI / 8f;
            sparkDirFromB = num - MathF.PI / 8f;
            sparkDirToA = num + MathF.PI - MathF.PI / 8f;
            sparkDirToB = num + MathF.PI + MathF.PI / 8f;

            cog = GFX.Game[cogPath];
        }

        public PathRenderer(BezierZipmover zipMover, Vc2 start, Vc2 target, string cogPath = "objects/zipmover/cog")
        {
            base.Depth = 5000;
            ZipMover = zipMover;
            from = start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
            to = target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
            sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            sparkDirFromA = num + MathF.PI / 8f;
            sparkDirFromB = num - MathF.PI / 8f;
            sparkDirToA = num + MathF.PI - MathF.PI / 8f;
            sparkDirToB = num + MathF.PI + MathF.PI / 8f;

            cog = GFX.Game[cogPath];
        }

        public void CreateSparks()
        {
            SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
            SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
            SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
            SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
        }
        
        public override void Render()
        {
            DrawCogs(Vector2.UnitY, Color.Black);
            DrawCogs(Vector2.Zero);
            if (ZipMover.drawBlackBorder)
            {
                Draw.Rect(new Rectangle((int)(ZipMover.X + ZipMover.Shake.X - 1f), (int)(ZipMover.Y + ZipMover.Shake.Y - 1f), (int)ZipMover.Width + 2, (int)ZipMover.Height + 2), Color.Black);
            }
        }
        
        public void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            Vector2 vector = (to - from).SafeNormalize();
            Vector2 vector2 = vector.Perpendicular() * 3f;
            Vector2 vector3 = -vector.Perpendicular() * 4f;
            float rotation = ZipMover.percent * MathF.PI * 2f;
            Draw.Line(from + vector2 + offset, to + vector2 + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeColor.Parsed());
            Draw.Line(from + vector3 + offset, to + vector3 + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeColor.Parsed());
            for (float num = 4f - ZipMover.percent * MathF.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
            {
                Vector2 vector4 = from + vector2 + vector.Perpendicular() + vector * num;
                Vector2 vector5 = to + vector3 - vector * num;
                Draw.Line(vector4 + offset, vector4 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeLightColor.Parsed());
                Draw.Line(vector5 + offset, vector5 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeLightColor.Parsed());
            }

            cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
            cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
        }
    }
    public PathRenderer pathRenderer;

    public static ParticleType P_Scrape = ZipMover.P_Scrape;

    public static ParticleType P_Sparks = ZipMover.P_Sparks;
    
    public MTexture[,] edges = new MTexture[3, 3];
    
    public Sprite streetlight;
    
    public BloomPoint bloom;
    
    public List<MTexture> innerCogs;
    
    public MTexture temp = new MTexture();
    
    public bool drawBlackBorder;
    
    public Vector2 start;
    
    public Vector2 target;
    
    public float percent;
    
    public SoundSource sfx = new SoundSource();

    //更改参数
    public Vc2[] nodes;
    public string folderPath = "objects/zipmover/";
    public string sound = "event:/game/01_forsaken_city/zip_mover"; // alternate: "event:/new_content/game/10_farewell/zip_mover"
    public BezierCurve curve;
    public float moveTime = 0.5f, returnTime = 2f;
    public Ease.Easer moveEase = Ease.SineIn, returnEase = Ease.SineIn;
    public CColor baseColor = new("000000"), ropeColor = new("663931"), ropeLightColor = new("9b6157");
    public int renderGap = 0;
    public enum RenderStyle { Vanilla = 0, Single = 1 };
    public int renderStyle = 0;
    public BezierZipmover(EntityData e, Vector2 p)
        : base(e.Position + p, e.Width, e.Height, safe: false)
    {
        nodes = e.NodesWithPosition(p);
        curve = new BezierCurve(nodes, 0f);

        moveTime = e.Float("moveTime", 0.5f);
        moveTime = moveTime <= Engine.DeltaTime ? Engine.DeltaTime : moveTime;
        returnTime = e.Float("returnTime", 2f);
        returnTime = returnTime <= Engine.DeltaTime ? Engine.DeltaTime : returnTime;
        moveEase = EaseUtils.StringToEase(e.Attr("moveEase", "sinein"), Ease.SineIn);
        returnEase = EaseUtils.StringToEase(e.Attr("returnEase", "sinein"), Ease.SineIn);
        baseColor = e.GetChroniaColor("baseColor", Color.Black);
        ropeColor = e.GetChroniaColor("ropeColor", "663931");
        renderGap = e.Int("renderGap", 0);
        renderStyle = e.Int("renderStyle", 0);
        
        base.Depth = e.Int("depth", -9999);
        start = Position;
        target = nodes[nodes.Length - 1];
        
        Add(new Coroutine(Sequence()));
        Add(new LightOcclude());

        folderPath = e.Attr("directory", "objects/zipmover/").TrimEnd('/') + "/";
        sound = e.Attr("sfx", "event:/game/01_forsaken_city/zip_mover");

        drawBlackBorder = e.Bool("drawBorder", true);
        
        innerCogs = GFX.Game.GetAtlasSubtextures(folderPath + "innercog");
        Add(streetlight = new Sprite(GFX.Game, folderPath + "light"));
        streetlight.Add("frames", "", 1f);
        streetlight.Play("frames");
        streetlight.Active = false;
        streetlight.SetAnimationFrame(1);
        streetlight.Position = new Vector2(base.Width / 2f - streetlight.Width / 2f, 0f);
        Add(bloom = new BloomPoint(1f, 6f));
        bloom.Position = new Vector2(base.Width / 2f, 4f);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                edges[i, j] = GFX.Game[folderPath + "block"].GetSubtexture(i * 8, j * 8, 8, 8);
            }
        }

        SurfaceSoundIndex = 7;
        sfx.Position = new Vector2(base.Width, base.Height) / 2f;
        Add(sfx);

        // assistive
        pathRenderer = new(this, nodes[0], nodes[nodes.Length - 1], folderPath + "cog");
    }
    
    public override void Added(Scene scene)
    {
        base.Added(scene);
        //for(int i = 0; i < nodes.Length - 1; i++)
        //{
        //    scene.Add(pathRenderer = new PathRenderer(this, nodes[i], nodes[i + 1]));
        //}
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        scene.Remove(pathRenderer);
        pathRenderer = null;
        base.Removed(scene);
    }
    
    public override void Update()
    {
        base.Update();
        bloom.Y = streetlight.CurrentAnimationFrame * 3;
    }
    
    public void DrawSegment(Vc2 from, Vc2 to, float percent, Vc2? _offset = null)
    {
        Vc2 offset = _offset ?? Vc2.Zero;
        
        Vector2 vector = (to - from).SafeNormalize();
        Vector2 vector2 = vector.Perpendicular() * 3f;
        Vector2 vector3 = -vector.Perpendicular() * 4f;
        float rotation = percent * MathF.PI * 2f;
        Draw.Line(from + vector2 + offset, to + vector2 + offset, ropeColor.Parsed());
        Draw.Line(from + vector3 + offset, to + vector3 + offset, ropeColor.Parsed());
        for (float num = 4f - percent * MathF.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
        {
            Vector2 vector4 = from + vector2 + vector.Perpendicular() + vector * num;
            Vector2 vector5 = to + vector3 - vector * num;
            Draw.Line(vector4 + offset, vector4 + vector * 2f + offset, ropeLightColor.Parsed());
            Draw.Line(vector5 + offset, vector5 - vector * 2f + offset, ropeLightColor.Parsed());
        }
    }
    
    public void SingleRendering()
    {
        curve.Render(50, ropeColor.Parsed(0.3f), 5f, new Vc2(base.Width / 2, base.Height / 2), renderGap);
        curve.Render(50, ropeColor.Parsed(0.7f), 2f, new Vc2(base.Width / 2, base.Height / 2), renderGap);
        Draw.Circle(nodes[0] + new Vc2(base.Width / 2, base.Height / 2), 3f, ropeColor.Parsed(0.4f), 200);
        Draw.Circle(nodes[0] + new Vc2(base.Width / 2, base.Height / 2), 2f, ropeColor.Parsed(0.8f), 200);
        Draw.Circle(nodes[nodes.Length - 1] + new Vc2(base.Width / 2, base.Height / 2), 3f, ropeColor.Parsed(0.4f), 200);
        Draw.Circle(nodes[nodes.Length - 1] + new Vc2(base.Width / 2, base.Height / 2), 2f, ropeColor.Parsed(0.8f), 200);

        // draw cogs only
        pathRenderer.cog.DrawCentered(nodes[0] + new Vc2(base.Width / 2, base.Height / 2), Color.White, 1f, percent * MathF.PI * 2f);
        pathRenderer.cog.DrawCentered(nodes[nodes.Length - 1] + new Vc2(base.Width / 2, base.Height / 2), Color.White, 1f, percent * MathF.PI * 2f);

    }

    public override void Render()
    {
        // Render Bezier Curve
        if(renderStyle == (int)RenderStyle.Single)
        {
            SingleRendering();
        }
        else
        {
            curve.OperateBezierPoints(nodes.Length * 4, (a, b, ai, bi) =>
            {
                if(renderGap != 0)
                {
                    if (ai % (renderGap * 2) >= renderGap)
                    {
                        return;
                    }
                }
                DrawSegment(a, b, percent, new Vc2(base.Width / 2, base.Height / 2));
            });

            pathRenderer.cog.DrawCentered(nodes[0] + new Vc2(base.Width / 2, base.Height / 2), Color.White, 1f, percent * MathF.PI * 2f);
            pathRenderer.cog.DrawCentered(nodes[nodes.Length - 1] + new Vc2(base.Width / 2, base.Height / 2), Color.White, 1f, percent * MathF.PI * 2f);
        }
        
        Vector2 position = Position;
        Position += base.Shake;
        Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, Color.Black);
        int num = 1;
        float num2 = 0f;
        int count = innerCogs.Count;
        for (int i = 4; (float)i <= base.Height - 4f; i += 8)
        {
            int num3 = num;
            for (int j = 4; (float)j <= base.Width - 4f; j += 8)
            {
                int index = (int)(mod((num2 + (float)num * percent * MathF.PI * 4f) / (MathF.PI / 2f), 1f) * (float)count);
                MTexture mTexture = innerCogs[index];
                Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
                Vector2 zero = Vector2.Zero;
                if (j <= 4)
                {
                    zero.X = 2f;
                    rectangle.X = 2;
                    rectangle.Width -= 2;
                }
                else if ((float)j >= base.Width - 4f)
                {
                    zero.X = -2f;
                    rectangle.Width -= 2;
                }

                if (i <= 4)
                {
                    zero.Y = 2f;
                    rectangle.Y = 2;
                    rectangle.Height -= 2;
                }
                else if ((float)i >= base.Height - 4f)
                {
                    zero.Y = -2f;
                    rectangle.Height -= 2;
                }

                mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                mTexture.DrawCentered(Position + new Vector2(j, i) + zero, Color.White * ((num < 0) ? 0.5f : 1f));
                num = -num;
                num2 += MathF.PI / 3f;
            }

            if (num3 == num)
            {
                num = -num;
            }
        }

        for (int k = 0; (float)k < base.Width / 8f; k++)
        {
            for (int l = 0; (float)l < base.Height / 8f; l++)
            {
                int num4 = ((k != 0) ? (((float)k != base.Width / 8f - 1f) ? 1 : 2) : 0);
                int num5 = ((l != 0) ? (((float)l != base.Height / 8f - 1f) ? 1 : 2) : 0);
                if (num4 != 1 || num5 != 1)
                {
                    edges[num4, num5].Draw(new Vector2(base.X + (float)(k * 8), base.Y + (float)(l * 8)));
                }
            }
        }

        base.Render();
        Position = position;
    }
    
    public void ScrapeParticlesCheck(Vector2 to)
    {
        if (!base.Scene.OnInterval(0.03f))
        {
            return;
        }

        bool flag = to.Y != base.ExactPosition.Y;
        bool flag2 = to.X != base.ExactPosition.X;
        if (flag && !flag2)
        {
            int num = Math.Sign(to.Y - base.ExactPosition.Y);
            Vector2 vector = ((num != 1) ? base.TopLeft : base.BottomLeft);
            int num2 = 4;
            if (num == 1)
            {
                num2 = Math.Min((int)base.Height - 12, 20);
            }

            int num3 = (int)base.Height;
            if (num == -1)
            {
                num3 = Math.Max(16, (int)base.Height - 16);
            }

            if (base.Scene.CollideCheck<Solid>(vector + new Vector2(-2f, num * -2)))
            {
                for (int i = num2; i < num3; i += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? (-MathF.PI / 4f) : (MathF.PI / 4f));
                }
            }

            if (base.Scene.CollideCheck<Solid>(vector + new Vector2(base.Width + 2f, num * -2)))
            {
                for (int j = num2; j < num3; j += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? (MathF.PI * -3f / 4f) : (MathF.PI * 3f / 4f));
                }
            }
        }
        else
        {
            if (!flag2 || flag)
            {
                return;
            }

            int num4 = Math.Sign(to.X - base.ExactPosition.X);
            Vector2 vector2 = ((num4 != 1) ? base.TopLeft : base.TopRight);
            int num5 = 4;
            if (num4 == 1)
            {
                num5 = Math.Min((int)base.Width - 12, 20);
            }

            int num6 = (int)base.Width;
            if (num4 == -1)
            {
                num6 = Math.Max(16, (int)base.Width - 16);
            }

            if (base.Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, -2f)))
            {
                for (int k = num5; k < num6; k += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? (MathF.PI * 3f / 4f) : (MathF.PI / 4f));
                }
            }

            if (base.Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, base.Height + 2f)))
            {
                for (int l = num5; l < num6; l += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? (MathF.PI * -3f / 4f) : (-MathF.PI / 4f));
                }
            }
        }
    }
    
    public IEnumerator Sequence()
    {
        Vector2 start = Position;
        while (true)
        {
            if (!HasPlayerRider())
            {
                yield return null;
                continue;
            }

            sfx.Play(sound);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            StartShaking(0.1f);
            yield return 0.1f;
            streetlight.SetAnimationFrame(3);
            StopPlayerRunIntoAnimation = false;
            
            // Start Moving
            float at2 = 0f;
            while (at2 < 1f)
            {
                yield return null;
                at2 = Calc.Approach(at2, 1f, 1 / moveTime * Engine.DeltaTime);
                percent = moveEase(at2);
                Vector2 vector = curve.GetBezierPoint(percent);
                ScrapeParticlesCheck(vector);
                //if (Scene.OnInterval(0.1f))
                //{
                //    pathRenderer?.CreateSparks();
                //}

                MoveTo(vector);
            }

            StartShaking(0.2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().Shake();
            StopPlayerRunIntoAnimation = true;
            yield return 0.5f;
            StopPlayerRunIntoAnimation = false;
            streetlight.SetAnimationFrame(2);
            
            at2 = 0f;
            while (at2 < 1f)
            {
                yield return null;
                at2 = Calc.Approach(at2, 1f, 1 / returnTime * Engine.DeltaTime);
                percent = 1f - returnEase(at2);
                Vector2 position = curve.GetBezierPoint(percent);
                MoveTo(position);
            }

            StopPlayerRunIntoAnimation = true;
            StartShaking(0.2f);
            streetlight.SetAnimationFrame(1);
            yield return 0.5f;
        }
    }
    
    public float mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
