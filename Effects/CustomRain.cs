using Celeste;
using Celeste.Mod.Backdrops;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Components;

namespace ChroniaHelper.Effects;

[ExA.Credits("Viv for open source code")]
[CustomBackdrop("ChroniaHelper/CustomRain")]
public class CustomRain : Backdrop {
    
    public struct Particle {
        public Vector2 Position;

        public Vector2 Speed;

        public float Rotation;

        public Vector2 Scale;
        public Color color;

        public void Init(float rot, float speedMult, Color color, float extX, float extY) {
            Position = new Vector2(-32f + Rd.Random.NextFloat(384f + extX), -32f + Rd.Random.NextFloat(244f + extY));
            Rotation = rot;
            Speed = Calc.AngleToVector(Rotation, Rd.Random.Range(200f * speedMult, 600f * speedMult));
            Scale = new Vector2(4f + (Speed.Length() - 200f) * speedMult / 33.33333f, 1f);
            this.color = color;
        }
    }

    public float speedMult, alpha;
    public List<Particle> particles = new();
    public List<Particle> currentParticles = new();
    public SelectiveCounter count;
    public int? trueCount = null;

    private float visibleFade = 1f;

    private float linearFade = 1f;

    public float windStrength = 0f;

    /// <summary>
    /// 本帧实际参与更新与绘制的粒子数，逐帧按计数器刷新一次。
    /// </summary>
    private int activeCount = 0;

    public CustomRain(BinaryPacker.Element child)
        :this(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), 
             child.AttrFloat("angle", 270f), child.AttrFloat("angleDiff", 3f), child.AttrFloat("speedMult", 1f), 
             child.Attr("Amount", "240"), child.Attr("Colors", "161933"), child.AttrFloat("alpha", 1f),
             child.AttrFloat("extendedBorderX", 0f), child.AttrFloat("extendedBorderY", 0f),
             child.Attr("fadingX"), child.Attr("fadingY"), child.AttrFloat("windStrength")
             )
    { }

    public CustomRain(Vector2 scroll, float angle, float angleDiff, float speedMult, string count, string colors, float alpha,
        float extX, float extY, string fadeX, string fadeY, float windStrength
        ) 
    {
        this.Scroll = scroll;
        this.windStrength = windStrength;
        this.count = count;
        this.speedMult = speedMult;
        if(count.TryParse(out int n))
        {
            trueCount = n;
        }

        particles = new();

        _colors = new List<Color>();
        if (string.IsNullOrEmpty(colors))
        {
            _colors = new List<Color>() { Color.White };
        }
        else if (colors.StartsWith("§"))
        {
            string[] colorParams = colors.Substring(1).Split(',', StringSplitOptions.TrimEntries);
            foreach (string p in colorParams) {
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
        _angle = angle * -Calc.DegToRad;
        _angleDiff = Math.Abs(angleDiff * Calc.DegToRad);
        this.extX = extX;
        this.extY = extY;
        // Prepare initial counts
        int initialCount = trueCount ?? (count.GetCounter() <= 0 ? 100 : count.GetCounter());
        for (int i = 0; i < initialCount; i++) {
            Particle p = new();
            p.Init(_angle + Rd.Random.Range(-_angleDiff, _angleDiff), speedMult, Rd.Random.Choose<Color>(_colors), extX, extY);
            particles.Add(p);
        }
        this.alpha = alpha;

        if (!fadeX.IsNullOrEmpty())
        {
            FadeX = new();
            string[] x = fadeX.Split(';', StringSplitOptions.TrimEntries);
            for(int i = 0; i < x.Length; i++)
            {
                string[] s = x[i].Split(',', StringSplitOptions.TrimEntries);
                if (s.Length < 4) { continue; }

                FadeX.Add(s[0].Parse(0f), s[1].Parse(0f), s[2].Parse(1f).Clamp(0f,1f), s[3].Parse(1f).Clamp(0f, 1f));
            }
        }

        if (!fadeY.IsNullOrEmpty())
        {
            FadeY = new();
            string[] y = fadeY.Split(';', StringSplitOptions.TrimEntries);
            for (int i = 0; i < y.Length; i++)
            {
                string[] s = y[i].Split(',', StringSplitOptions.TrimEntries);
                if (s.Length < 4) { continue; }

                FadeY.Add(s[0].Parse(0f), s[1].Parse(0f), s[2].Parse(1f).Clamp(0f, 1f), s[3].Parse(1f).Clamp(0f, 1f));
            }
        }
    }

    private float _angle, _angleDiff, extX, extY;
    private List<Color> _colors = new();
    public override void Update(Scene scene) {
        base.Update(scene);
        Level level = scene as Level;
        bool flag = (level.Raining = IsVisible(level));
        visibleFade = Calc.Approach(visibleFade, flag ? 1 : 0, Engine.DeltaTime * (flag ? 10f : 0.25f));
        if (FadeX != null) {
            linearFade = FadeX.Value(PUt.player?.Position.X ?? 0);
        }
        if (FadeY.IsNotNull())
        {
            float v = FadeY.Value(PUt.player?.Position.Y ?? 0);
            linearFade = linearFade.Clamp(0f, v);
        }

        // Update particle counts (expand)
        activeCount = trueCount ?? ((int)count).ClampMin(0);

        if (particles.Count < activeCount)
        {
            for (int i = particles.Count; i < activeCount; i++)
            {
                Particle p = new();
                p.Init(_angle + Rd.Random.Range(-_angleDiff, _angleDiff), speedMult, Rd.Random.Choose<Color>(_colors), extX, extY);
                particles.Add(p);
            }
        }

        Vector2 windOffset = level.Wind * windStrength;

        for (int i = 0; i < activeCount; i++) {
            Particle p = particles[i];
            p.Position += (p.Speed + windOffset) * Engine.DeltaTime;
            particles[i] = p;
        }
    }

    [ExA.Credits("karl2883 for optimizing Render performance")]
    public override void Render(Scene scene) {
        if (alpha > 0f && visibleFade > 0f && linearFade > 0f) {
            Level level = scene as Level;
            Camera camera = level.Camera;
            Vector2 screenSize = Miscs.Screen.Size;
            Vector2 windOffset = level.Wind * windStrength;
            float colorFactor = alpha * linearFade * visibleFade;

            int getCount = activeCount.ClampMax(particles.Count - 1);
            for (int i = 0; i < getCount; i++) {
                Particle p = particles[i];

                Vector2 position = new Vector2(
                    NumberUtils.Mod(p.Position.X - camera.X * Scroll.X, screenSize.X),
                    NumberUtils.Mod(p.Position.Y - camera.Y * Scroll.Y, screenSize.Y));

                Draw.Pixel.DrawCentered(position,
                    p.color * colorFactor,
                    p.Scale,
                    Calc.Angle(p.Speed + windOffset)
                    );
            }
        }
    }

}
