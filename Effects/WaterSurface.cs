using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Backdrops;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ChroniaHelper.Effects;

[CustomBackdrop("ChroniaHelper/WaterSurface")]
public class WaterSurface : Backdrop
{

    public struct Particle
    {
        public Vector2 Position;

        public Vector2 Speed;

        public float Rotation;

        public Vector2 Scale;
        public Color color;

        public void Init(Vector2 position, float rot, float speed, Vector2 scale, Color color, float extX, float extY)
        {
            Position = position;
            Rotation = rot;
            Speed = Calc.AngleToVector(Rotation, speed);
            Scale = scale;
            this.color = color;
        }
    }

    public Particle[] particles, backParticles;
    public int count, backCount;

    public float extX, extY;

    public WaterSurface(BinaryPacker.Element child)
        : this(new Vector2(child.AttrFloat("scrollXFar"), child.AttrFloat("scrollYFar")),
              new Vector2(child.AttrFloat("scrollXNear"), child.AttrFloat("scrollYNear")),
              new Vector2(0f, child.AttrFloat("yFar")),
              new Vector2(0f, child.AttrFloat("yNear")),
             child.AttrInt("particleCount", 50), child.Attr("particleColors", "161933"),
             child.AttrFloat("alphaFar", 0.5f), child.AttrFloat("alphaNear", 1f),
             child.AttrFloat("particleScaleFar", 2f), child.AttrFloat("particleScaleNear", 6f),
             child.AttrFloat("waterSpeedFar", 100f), child.AttrFloat("waterSpeedNear", 120f),
             child.AttrFloat("extendedBorderX", 0f), child.AttrFloat("extendedBorderY", 0f),
             child.AttrBool("hasFarLine", false), child.AttrBool("hasCloseLine", true),
             child.Attr("surfaceColor", "ffffff"), child.Attr("farLineColor", "ffffff"),
             child.Attr("nearLineColor", "ffffff"), child.AttrFloat("surfaceAlpha", 0.1f),
             child.AttrFloat("farLineAlpha", 1f), child.AttrFloat("nearLineAlpha", 1f),
             child.Attr("surfaceColorBack", "ffffff"), child.AttrFloat("surfaceBackAlpha", 0.1f),
             child.Attr("backParticleColors", "ffffff"), child.AttrInt("backParticleCount", 50),
             child.AttrFloat("backAlphaNear", 1f), child.AttrFloat("backAlphaFar", 0.5f)
             )
    { }

    private Vector2 scroll1, scroll2;
    private float waterSpeed1, waterSpeed2, alpha1, alpha2, ps1, ps2, backAlphaFar, backAlphaNear;

    private bool farLine, closeLine;

    private Color surfaceColor, surfaceColorBack, farLineColor, nearLineColor;
    private float surfaceAlpha, surfaceBackAlpha, farLineAlpha, nearLineAlpha;
    public WaterSurface(Vector2 scroll1, Vector2 scroll2, Vector2 pos1, Vector2 pos2, 
        int count, string colors, float alpha1, float alpha2,
        float particleScale1, float particleScale2,
        float waterspeed1, float waterspeed2, float extX, float extY,
        bool farLine, bool closeLine,
        string surfaceColor, string farLineColor, string nearLineColor,
        float surfaceAlpha, float farLineAlpha, float nearLineAlpha,
        string surfaceColorBack, float surfaceBackAlpha,
        string backParticleColor, int backParticleCount,
        float backAlphaNear, float backAlphaFar
        )
    {
        //this.Scroll = Vector2.Zero;
        base.LoopX = false;
        base.LoopY = false;

        baseRenderY1 = pos1.Y; baseRenderY2 = pos2.Y;
        renderY1 = baseRenderY1; renderY2 = baseRenderY2;
        this.scroll1 = scroll1; this.scroll2 = scroll2;
        this.waterSpeed1 = waterspeed1; this.waterSpeed2 = waterspeed2;

        this.farLine = farLine;
        this.closeLine = closeLine;
        this.alpha1 = alpha1; this.alpha2 = alpha2;
        ps1 = particleScale1; ps2 = particleScale2;
        this.backAlphaFar = backAlphaFar;
        this.backAlphaNear = backAlphaNear;
        this.surfaceAlpha = surfaceAlpha;
        this.surfaceBackAlpha = surfaceBackAlpha;
        this.farLineAlpha = farLineAlpha;
        this.nearLineAlpha = nearLineAlpha;
        this.surfaceColor = Calc.HexToColor(surfaceColor);
        this.farLineColor = Calc.HexToColor(farLineColor);
        this.nearLineColor = Calc.HexToColor(nearLineColor);
        this.surfaceColorBack = Calc.HexToColor(surfaceColorBack);

        // Setting up particles
        this.count = count;
        particles = new Particle[count];
        List<Color> _colors = new List<Color>();
        if (string.IsNullOrEmpty(colors))
        {
            _colors = new List<Color>() { Color.White };
        }
        else if (colors.StartsWith("§"))
        {
            string[] colorParams = colors.Substring(1).Split(',', StringSplitOptions.TrimEntries);
            foreach (string p in colorParams)
            {
                _colors.Add(Calc.HexToColor(p.Trim().TrimStart('#')));
            }
        }
        else
        {
            string[] colorParams = colors.Split(',', StringSplitOptions.TrimEntries);
            foreach (string p in colorParams)
            {
                _colors.Add(Calc.HexToColor(p.Trim().TrimStart('#')));
            }
        }
        var _angle = 0f * -Calc.DegToRad; //90f = angle
        for (int i = 0; i < count; i++)
        {
            float x = Calc.Random.Range(-16f, 344f + extX);
            float y = Calc.Random.Range(renderY1, renderY2);
            particles[i].Init(
                new Vector2(x, y), _angle, 
                FadeUtils.LerpValue(y, renderY1, renderY2, this.waterSpeed1, this.waterSpeed2),
                new Vector2(FadeUtils.LerpValue(y, renderY1, renderY2, particleScale1, particleScale2), 1f),
                Calc.Random.Choose<Color>(_colors), 
                extX, extY
                );
        }

        // Setting up back particles
        this.backCount = backParticleCount;
        backParticles = new Particle[backParticleCount];
        List<Color> _colorsBG = new List<Color>();
        if (string.IsNullOrEmpty(backParticleColor))
        {
            _colorsBG = new List<Color>() { Color.White };
        }
        else if (backParticleColor.StartsWith("§"))
        {
            string[] colorParamsBG = backParticleColor.Substring(1).Split(',', StringSplitOptions.TrimEntries);
            foreach (string p in colorParamsBG)
            {
                _colorsBG.Add(Calc.HexToColor(p.Trim().TrimStart('#')));
            }
        }
        else
        {
            string[] colorParamsBG = backParticleColor.Split(',', StringSplitOptions.TrimEntries);
            foreach (string p in colorParamsBG)
            {
                _colorsBG.Add(Calc.HexToColor(p.Trim().TrimStart('#')));
            }
        }
        var _angleBG = 0f * -Calc.DegToRad; //90f = angle
        for (int i = 0; i < backParticleCount; i++)
        {
            float x = Calc.Random.Range(-16f, 344f + extX);
            float y = Calc.Random.Range(renderY1, renderY2);
            backParticles[i].Init(
                new Vector2(x, y), _angleBG,
                FadeUtils.LerpValue(y, renderY1, renderY2, this.waterSpeed1, this.waterSpeed2),
                new Vector2(FadeUtils.LerpValue(y, renderY1, renderY2, particleScale1, particleScale2), 1f),
                Calc.Random.Choose<Color>(_colorsBG),
                extX, extY
                );
        }

        // extended render options
        this.extX = extX;
        this.extY = extY;
    }

    private float baseRenderY1, baseRenderY2, renderY1, renderY2;

    public override void Update(Scene scene)
    {
        base.Update(scene);

        renderY1 = baseRenderY1 - MapProcessor.level.Camera.Position.Y * scroll1.Y;
        renderY2 = baseRenderY2 - MapProcessor.level.Camera.Position.Y * scroll2.Y;

        for (int i = 0; i < count; i++)
        {
            particles[i].Position += particles[i].Speed * Engine.DeltaTime;
        }
        for (int i = 0; i < backCount; i++)
        {
            backParticles[i].Position += backParticles[i].Speed * Engine.DeltaTime;
        }
    }

    public override void Render(Scene scene)
    {
        Draw.Rect(0f, Calc.Min(renderY1, renderY2),
                320f + extX, (renderY1 - renderY2).GetAbs(),
                renderY1 < renderY2 ? surfaceColor * surfaceAlpha : surfaceColorBack * surfaceBackAlpha);
        
        if (farLine)
        {
            Draw.Line(0f, renderY1, 320f + extX, renderY1, farLineColor * farLineAlpha);
        }
        if (closeLine)
        {
            Draw.Line(0f, renderY2, 320f + extX, renderY2, nearLineColor * nearLineAlpha);
        }
        Camera camera = (scene as Level).Camera;
        
        for (int i = 0; i < particles.Length; i++)
        {
            if (renderY1 >= renderY2) { break; }
            float posX = particles[i].Position.X - camera.X * FadeUtils.LerpValue(particles[i].Position.Y, baseRenderY1, baseRenderY2, scroll1.X, scroll2.X);
            float posY = particles[i].Position.Y - camera.Y * FadeUtils.LerpValue(particles[i].Position.Y, baseRenderY1, baseRenderY2, scroll1.Y, scroll2.Y);
            float alpha = FadeUtils.LerpValue(particles[i].Position.Y, baseRenderY1, baseRenderY2, alpha1, alpha2);
            //Vector2 position = new Vector2(NumberUtils.Mod(posX, 320f + extX), NumberUtils.Mod(posY, 180f + extY));
            Vector2 position = new Vector2(NumberUtils.Mod(posX, 320f + extX), posY);
            Draw.Pixel.DrawCentered(position,
                particles[i].color * alpha,
                particles[i].Scale,
                particles[i].Rotation);

        }

        for (int i = 0; i < backParticles.Length; i++)
        {
            if (renderY1 < renderY2) { break; }
            float posX = backParticles[i].Position.X - camera.X * FadeUtils.LerpValue(backParticles[i].Position.Y, baseRenderY1, baseRenderY2, scroll1.X, scroll2.X);
            float posY = backParticles[i].Position.Y - camera.Y * FadeUtils.LerpValue(backParticles[i].Position.Y, baseRenderY1, baseRenderY2, scroll1.Y, scroll2.Y);
            float alpha = FadeUtils.LerpValue(backParticles[i].Position.Y, baseRenderY1, baseRenderY2, backAlphaFar, backAlphaNear);
            Vector2 position = new Vector2(NumberUtils.Mod(posX, 320f + extX), posY);
            Draw.Pixel.DrawCentered(position,
                backParticles[i].color * alpha,
                backParticles[i].Scale,
                backParticles[i].Rotation);

        }
    }

}
