using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/WiggleLiner")]
public class WiggleLiner : BaseEntity
{
    public List<StandardWave2> wiggles = new();
    public List<Vector2> points = new();
    public List<CColor> color = new();
    private MTexture texture;

    public Vector2 frequency = 2.0f * Vector2.One;
    public Vector2 amplitude = 4.0f * Vector2.One;
    public Vector2 phase = Vector2.Zero;
    public string path = "ChroniaHelper/WiggleLine/dot";
    public float lineThickness = 2f;
    public int resolution = 10;
    public bool allowNodeAlpha = false;
    public WiggleLiner(EntityData data, Vector2 offset) : base(data, offset)
    {
        Depth = data.Int("Depth");

        string[] colors = data.Attr("Color", "ffffffff").Split(',', StringSplitOptions.TrimEntries);
        color = colors.Select((e) => new CColor(e)).ToList();

        frequency = data.Vector2("WiggleFrequencyX", "WiggleFrequencyY", 2.0f * Vector2.One);
        amplitude = data.Vector2("WiggleAmplifyX", "WiggleAmplifyY", 4.0f * Vector2.One);
        phase = data.Vector2("WigglePhaseX", "WigglePhaseY", Vector2.Zero);
        path = data.Attr("Path", "ChroniaHelper/WiggleLine/dot");
        lineThickness = data.Float("LineThickness", 2f).GetAbs();
        resolution = data.Int("ColorFadeResolution", 10).ClampMin(2);
        allowNodeAlpha = data.Bool("AllowNodeAlpha", false);

        texture = GFX.Game[path];

        points = nodes.ToList();

        Random ran = RandomUtils.Random;
        Vector2 frequencyModulation = new(ran.NextFloat(), ran.NextFloat());
        Vector2 amplitudeModulation = new(ran.NextFloat(), ran.NextFloat());
        Vector2 phaseModulation = new(ran.NextFloat(), ran.NextFloat());

        for (int i = 0; i < nodes.Length; i++)
        {
            frequencyModulation = new(ran.NextFloat(), ran.NextFloat());
            amplitudeModulation = new(ran.NextFloat(), ran.NextFloat());
            phaseModulation = new(ran.NextFloat(), ran.NextFloat());

            StandardWave2 wiggle = new(
                frequency * frequencyModulation,
                amplitude * amplitudeModulation,
                this.phase * phaseModulation);
            wiggles.Add(wiggle);
            Add(wiggle);
        }
    }

    public override void Update()
    {
        base.Update();

        for (int i = 0; i < nodes.Length; i++)
        {
            points[i] = nodes[i] + wiggles[i].Sin;
        }
    }

    public override void Render()
    {
        base.Render();

        List<Color> c = color.Select((i) => i.Parsed()).ToList();
        
        // Draw radative line?
        for (int i = 0; i < points.Count - 1; i++)
        {
            texture.DrawCentered(points[i], 
                allowNodeAlpha ? SafePick(c, i) : SafePick(color, i).color,
                Vector2.One, 0.0f);
            if(c.Count == 1)
            {
                Draw.Line(points[i], points[i + 1], c.First(), lineThickness);
            }
            else if(c.Count > 1)
            {
                Vector2 start = points[i];
                Vector2 delta = points[i + 1] - points[i];
                Vector2 va, vb;
                Color ca = SafePick(c, i), cb = SafePick(c, i + 1);
                
                for(int j = 0; j < resolution; j++)
                {
                    va = start + delta * j / resolution;
                    vb = start + delta * (j + 1) / resolution;
                    Draw.Line(va, vb,
                        Color.Lerp(ca, cb, j / (resolution - 1f)), lineThickness);
                }
            }
            else
            {
                Draw.Line(points[i], points[i + 1], Color.White, lineThickness);
            }
        }
        texture.DrawCentered(points.Last(),
            allowNodeAlpha ? SafePick(c, points.Count - 1) : SafePick(color, points.Count - 1).color,
            Vector2.One, 0.0f);
    }

    private T SafePick<T>(List<T> list, int index)
    {
        int count = list.Count();

        if(list.Count() > 0)
        {
            return list[NumberUtils.Mod(index, count)];
        }

        return default(T);
    }
}
