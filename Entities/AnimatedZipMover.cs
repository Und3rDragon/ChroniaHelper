using Celeste.Mod.Entities;
using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/AnimatedZipMover")]
public class AnimatedZipMover : Solid
{
    public enum State
    {
        Idle,
        Active,
        Return,
        Unlit
    }

    public static ParticleType P_Scrape = ZipMover.P_Scrape;
    public static ParticleType P_Sparks = ZipMover.P_Sparks;

    private string spritePath;
    private Sprite innerSprite;
    private List<MTexture> idleSprites;
    private List<MTexture> activeSprites;
    private List<MTexture> returnSprites;
    private List<MTexture> unlitSprites;
    private Dictionary<string, MTexture[,]> subTextureCache = new Dictionary<string, MTexture[,]>();
    private MTexture[,] currentFrameTexture;
    private int currentAnimationFrame;

    private Color ropeColor;
    private Color ropeLightColor;

    private Vector2 start;
    private Vector2 target;
    private float percent;
    private bool noReturn;
    private bool firstDirection = true;
    private State currentState;
    private State lastState;

    private ZipMoverPathRenderer pathRenderer;
    //private BloomPoint bloom;
    private SoundSource sfx = new SoundSource();

    private class ZipMoverPathRenderer : Entity
    {
        public AnimatedZipMover ZipMover;

        private readonly MTexture cog;
        private readonly float sparkDirFromA;
        private readonly float sparkDirFromB;
        private readonly float sparkDirToA;
        private readonly float sparkDirToB;
        private Vector2 from;
        private Vector2 to;
        private Vector2 sparkAdd;

        public ZipMoverPathRenderer(AnimatedZipMover zipMover, string spritePath) : base()
        {
            cog = GFX.Game[spritePath + "/cog"];
            Depth = 5000;
            ZipMover = zipMover;
            from = ZipMover.start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
            to = ZipMover.target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
            sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            sparkDirFromA = num + 0.3926991f;
            sparkDirFromB = num - 0.3926991f;
            sparkDirToA = num + 3.14159274f - 0.3926991f;
            sparkDirToB = num + 3.14159274f + 0.3926991f;
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
            //DrawCogs(Vector2.UnitY, new Color?(Color.Black));
            DrawCogs(Vector2.UnitY, null);
            DrawCogs(Vector2.Zero, null);
        }

        private void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            Vector2 vector = (to - from).SafeNormalize();
            Vector2 value = vector.Perpendicular() * 3f;
            Vector2 value2 = -vector.Perpendicular() * 4f;
            float rotation = ZipMover.percent * 3.14159274f * 2f;
            Draw.Line(from + value + offset, to + value + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeColor);
            Draw.Line(from + value2 + offset, to + value2 + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeColor);
            for (float num = 4f - (ZipMover.percent * 3.14159274f * 8f % 4f); num < (to - from).Length(); num += 4f)
            {
                Vector2 value3 = from + value + vector.Perpendicular() + (vector * num);
                Vector2 value4 = to + value2 - (vector * num);
                Draw.Line(value3 + offset, value3 + (vector * 2f) + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeLightColor);
                Draw.Line(value4 + offset, value4 - (vector * 2f) + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeLightColor);
            }

            cog.DrawCentered(from + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
            cog.DrawCentered(to + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
        }
    }

    public AnimatedZipMover(Vector2 position, Vector2 size, Vector2 target, string spritePath, bool noReturn) 
        : base(position, size.X, size.Y, false)
    {
        Depth = -9999;
        start = position;
        this.target = target;
        this.spritePath = spritePath;
        this.noReturn = noReturn;
        lastState = currentState = State.Idle;
        currentAnimationFrame = 0;

        P_Sparks.Color = Calc.HexToColor("a2a2a2");

        if (noReturn)
        {
            Add(new Coroutine(NoReturnSequence()));
        }
        else
        {
            Add(new Coroutine(NormalSequence()));
        }

        //InnerSprite

        /*
        
        Add(innerSprite = ChroniaHelperModule.CustomSpriteBank.Create("customZipMoverInnerSprite"));
        innerSprite.Position = new Vector2(Width / 2f , Height / 2f);
        innerSprite.Play("idle");
        
        
        //Edge
        edgeSprites = new Sprite[4];
        for (int i = 0; i < edgeSprites.Length; i++)
        {
            Add(edgeSprites[i] = FFFFFFModule.CustomSpriteBank.Create("customZipMoverEdge"));
            edgeSprites[i].Play("idle");
        }

        edgeSprites[0].Position = new Vector2(4f, 4f);
        edgeSprites[1].Position = new Vector2(Width - 4f, Height - 4f);
        edgeSprites[2].Position = new Vector2(Width - 4f, 4f);
        edgeSprites[3].Position = new Vector2(4f, Height - 4f);

        edgeSprites[0].Rotation = 0f;
        edgeSprites[1].Rotation = (float)Math.PI;
        edgeSprites[2].Rotation = (float)Math.PI / 2f;
        edgeSprites[3].Rotation = -(float)Math.PI / 2f;

        //Side
        int sideWidth = (((int)Width / 8) - 2) < 0 ? 0 : (((int)Width / 8) - 2);
        int sideHeight = (((int)Height / 8) - 2) < 0 ? 0 : (((int)Height / 8) - 2);
        sideSprites = new Sprite[sideWidth > sideHeight ? sideWidth : sideHeight, 4];
        int v1 = ((int)Width / 8) - 2;
        int v2 = ((int)Height / 8) - 2;
        Console.WriteLine($"{sideWidth} {sideHeight} {v1} {v2}");

        for (int i = 0; i< sideWidth; i++) //up
        {
            Add(sideSprites[i,0] = FFFFFFModule.CustomSpriteBank.Create("customZipMoverSide"));
            sideSprites[i, 0].Play("idle");
            sideSprites[i, 0].Position = new Vector2(12f + i * 8, 4f);
            sideSprites[i, 0].Rotation = 0f;
        }

        for (int i = 0; i < sideWidth; i++) //down
        {
            Add(sideSprites[i, 0] = FFFFFFModule.CustomSpriteBank.Create("customZipMoverSide"));
            sideSprites[i, 0].Play("idle");
            sideSprites[i, 0].Position = new Vector2(12f + i * 8, Height - 4f);
            sideSprites[i, 0].Rotation = (float)Math.PI;
        }

        for (int i = 0; i < sideHeight; i++) //left
        {
            Add(sideSprites[i, 2] = FFFFFFModule.CustomSpriteBank.Create("customZipMoverSide"));
            sideSprites[i, 2].Play("idle");
            sideSprites[i, 2].Position = new Vector2(4f, 12f + i * 8);
            sideSprites[i, 2].Rotation = -(float)Math.PI / 2;
        }

        for (int i = 0; i < sideHeight; i++) //right
        {
            Add(sideSprites[i, 3] = FFFFFFModule.CustomSpriteBank.Create("customZipMoverSide"));
            sideSprites[i, 3].Play("idle");
            sideSprites[i, 3].Position = new Vector2(Width - 4f, 12f + i * 8);
            sideSprites[i, 3].Rotation = (float)Math.PI / 2f;
        }*/

        Add(innerSprite = new Sprite(GFX.Game, spritePath));
        
        innerSprite.Add("idle", "/innerSprite_idle", 0.08f);
        innerSprite.Add("active", "/innerSprite_active", 0.08f);
        innerSprite.Add("return", "/innerSprite_return", 0.08f);
        innerSprite.Add("unlit", "/innerSprite_unlit", 0.08f);
        innerSprite.Position = new Vector2(Width / 2f, Height / 2f);
        innerSprite.Justify = new Vector2(0.5f, 0.5f);
        Add(innerSprite);
        innerSprite.Play("idle");

        idleSprites = GFX.Game.GetAtlasSubtextures(spritePath + "/idle");
        activeSprites = GFX.Game.GetAtlasSubtextures(spritePath + "/active");
        returnSprites = GFX.Game.GetAtlasSubtextures(spritePath + "/return");
        unlitSprites = GFX.Game.GetAtlasSubtextures(spritePath + "/unlit");

        GenerateSubTextureCache("idle", idleSprites);
        GenerateSubTextureCache("active", activeSprites);
        GenerateSubTextureCache("return", returnSprites);
        GenerateSubTextureCache("unlit", unlitSprites);

        currentFrameTexture = subTextureCache["idle0"];

/*        Console.WriteLine(idleSprites.Count);
        foreach (var item in idleSprites)
        {
            Console.WriteLine(item.AtlasPath);
        }
        Console.WriteLine(activeSprites.Count);
        foreach (var item in activeSprites)
        {
            Console.WriteLine(item.AtlasPath);
        }
        Console.WriteLine(returnSprites.Count);
        foreach (var item in returnSprites)
        {
            Console.WriteLine(item.AtlasPath);
        }
        Console.WriteLine(unlitSprites.Count);
        foreach (var item in unlitSprites)
        {
            Console.WriteLine(item.AtlasPath);
        }*/

        SurfaceSoundIndex = 7;
        sfx.Position = new Vector2(base.Width, base.Height) / 2f;
        Add(new LightOcclude());
        Add(sfx);
    }

    public AnimatedZipMover(EntityData data, Vector2 offset) : 
        this(data.Position + offset, new Vector2(data.Width, data.Height), data.Nodes[0] + offset, data.Attr("spritePath"), data.Bool("noReturn"))
    {
        ropeColor = Calc.HexToColor(data.Attr("ropeColor"));
        ropeLightColor = Calc.HexToColor(data.Attr("ropeLightColor"));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(pathRenderer = new ZipMoverPathRenderer(this, spritePath));
    }

    public override void Removed(Scene scene)
    {
        scene.Remove(pathRenderer);
        pathRenderer = null;
        base.Removed(scene);
    }

    public override void Update()
    {
        if (!Scene.OnInterval(0.08f))
        {
            base.Update();
            return;
        }

        List<MTexture> textureSource;

        textureSource = currentState switch
        {
            State.Idle => idleSprites,
            State.Active => activeSprites,
            State.Return => returnSprites,
            State.Unlit => unlitSprites,
            _ => throw new Exception("Switch expression not match!"),
        };

        if (currentState != lastState || currentAnimationFrame == textureSource.Count - 1)
        {
            currentAnimationFrame = 0;
            lastState = currentState;
        }
        else
            currentAnimationFrame++;

        currentFrameTexture = currentState switch
        {
            State.Idle => subTextureCache["idle" + currentAnimationFrame.ToString()],
            State.Active => subTextureCache["active" + currentAnimationFrame.ToString()],
            State.Return => subTextureCache["return" + currentAnimationFrame.ToString()],
            State.Unlit => subTextureCache["unlit" + currentAnimationFrame.ToString()],
            _ => throw new Exception("Switch expression not match!"),
        };

        //Console.WriteLine($"{currentAnimationFrame} {textureSource.Count} {currentState}");    
        base.Update();
    }

    public override void Render()
    {
        Draw.Rect(new Rectangle((int)(X), (int)(Y), (int)Width, (int)Height), Calc.HexToColor("e8e8e8"));

        for (int k = 0; k < Width / 8f; k++)
        {
            for (int l = 0; l < Height / 8f; l++)
            {
                int num1 = ((k != 0) ? ((k != Width / 8f - 1f) ? 1 : 2) : 0);
                int num2 = ((l != 0) ? ((l != Height / 8f - 1f) ? 1 : 2) : 0);
                if (num1 != 1 || num2 != 1)
                {
                    currentFrameTexture[num1, num2].Draw(new Vector2(X + k * 8, Y + l * 8));
                }
            }
        }

        base.Render();
    }

    private IEnumerator NormalSequence()
    {
        Vector2 start = Position;
        while (true)
        {
            if (!HasPlayerRider())
            {
                yield return null;
                continue;
            }

            sfx.Play("event:/game/01_forsaken_city/zip_mover");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            StartShaking(0.1f);
            yield return 0.1f;

            currentState = State.Active;
            innerSprite.Play("active");

            StopPlayerRunIntoAnimation = false;
            float at2 = 0f;
            while (at2 < 1f)
            {
                yield return null;
                at2 = Calc.Approach(at2, 1f, 2f * Engine.DeltaTime);
                percent = Ease.SineIn(at2);
                Vector2 vector = Vector2.Lerp(start, target, percent);
                ScrapeParticlesCheck(vector);
                if (Scene.OnInterval(0.1f))
                {
                    pathRenderer.CreateSparks();
                }
                MoveTo(vector);
            }
            StartShaking(0.2f);

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().Shake();
            StopPlayerRunIntoAnimation = true;

            currentState = State.Unlit;
            innerSprite.Play("unlit");

            yield return 0.5f;
            StopPlayerRunIntoAnimation = false;

            currentState = State.Return;
            innerSprite.Play("return");

            at2 = 0f;
            while (at2 < 1f)
            {
                yield return null;
                at2 = Calc.Approach(at2, 1f, 0.5f * Engine.DeltaTime);
                percent = 1f - Ease.SineIn(at2);
                Vector2 position = Vector2.Lerp(target, start, Ease.SineIn(at2));
                MoveTo(position);
            }
            StopPlayerRunIntoAnimation = true;
            StartShaking(0.2f);

            currentState = State.Idle;
            innerSprite.Play("idle");

            yield return 0.5f;
        }
    }

    private IEnumerator NoReturnSequence()
    {
        Vector2 start = Position;
        while (true)
        {
            if (!HasPlayerRider())
            {
                yield return null;
                continue;
            }

            sfx.Play("event:/game/01_forsaken_city/zip_mover");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            StartShaking(0.1f);
            yield return 0.1f;

            currentState = State.Active;
            innerSprite.Play("active");

            StopPlayerRunIntoAnimation = false;
            float at = 0f;
            while (at < 1f)
            {
                yield return null;
                at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime);
                percent = Ease.SineIn(at);
                Vector2 to = firstDirection ? Vector2.Lerp(start, target, percent) : Vector2.Lerp(target, start, percent);
                ScrapeParticlesCheck(to);
                bool flag = Scene.OnInterval(0.1f);
                if (flag)
                {
                    pathRenderer.CreateSparks();
                }

                MoveTo(to);
            }

            StartShaking(0.2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().Shake(0.3f);
            StopPlayerRunIntoAnimation = true;

            currentState = State.Idle;
            innerSprite.Play("idle");

            yield return 0.5f;
            sfx.Stop();
            StopPlayerRunIntoAnimation = false;



            firstDirection = !firstDirection;
        }
    }

    private void ScrapeParticlesCheck(Vector2 to)
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
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? (-(float)Math.PI / 4f) : ((float)Math.PI / 4f));
                }
            }
            if (base.Scene.CollideCheck<Solid>(vector + new Vector2(base.Width + 2f, num * -2)))
            {
                for (int j = num2; j < num3; j += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? ((float)Math.PI * -3f / 4f) : ((float)Math.PI * 3f / 4f));
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
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? ((float)Math.PI * 3f / 4f) : ((float)Math.PI / 4f));
                }
            }
            if (base.Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, base.Height + 2f)))
            {
                for (int l = num5; l < num6; l += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? ((float)Math.PI * -3f / 4f) : (-(float)Math.PI / 4f));
                }
            }
        }
    }

    private void GenerateSubTextureCache(string key, List<MTexture> textures)
    {
        for (int index = 0; index <= textures.Count - 1; index++)
        {
            MTexture[,] subTexture = new MTexture[3, 3];
            //Console.WriteLine($"{index} {textures.Count} {key + index.ToString()}");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    subTexture[i, j] = textures[index].GetSubtexture(i * 8, j * 8, 8, 8);
                }
            }
            subTextureCache[key + index.ToString()] = subTexture;
        }
    }
}
