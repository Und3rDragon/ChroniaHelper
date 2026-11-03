using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/ZaggingLine")]
public class ZaggingLine : Entity
{
    private Vector2 start, end, direction;
    private float length;

    private CColor bgLineColor;
    private List<CColor> lineColors = new(), nodeColors = new();
    private MTexture nodeTexture;

    private bool showBgLine, showNodes;

    private float thickness, dashLength, gapLength, speed, fadeLength;
    private int resolution;

    private float dashOffset;
    private int dashRotation;

    public ZaggingLine(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Vector2[] nodes = data.NodesWithPosition(offset);

        start = Position;
        // nodes[0] is the entity position, nodes[1] is the actual node
        end = nodes.Length > 1 ? nodes[1] : Position;

        Vector2 delta = end - start;
        length = delta.Length();
        direction = length > 0f ? delta / length : Vector2.Zero;

        CColor color = data.PackColor("color", "4da6ffff");
        lineColors = ParseColors(data.Attr("lineColors", ""), color);

        showBgLine = data.Bool("showBgLine", false);
        bgLineColor = data.PackColor("bgLineColor", "ffffffff");

        showNodes = data.Bool("showNodes", true);
        nodeColors = ParseColors(data.Attr("nodeColors", "ffffffff"), CColor.White);
        nodeTexture = GFX.Game[data.Attr("nodeTexture", "ChroniaHelper/ZaggingLine/node00")];

        thickness = data.Float("thickness", 2f).GetAbs();
        dashLength = data.Float("dashLength", 8f).GetAbs();
        if (dashLength < 1f) { dashLength = 1f; }
        gapLength = data.Float("gapLength", 6f).GetAbs();
        speed = data.Float("speed", 40f);
        fadeLength = data.Float("fadeLength", 3f).GetAbs();
        resolution = data.Int("resolution", 10).ClampMin(1);

        Depth = data.Int("depth", 9500);
    }

    public override void Update()
    {
        base.Update();

        float period = dashLength + gapLength;
        if (speed == 0f || period <= 0f) { return; }

        dashOffset += speed * Engine.DeltaTime;

        while (dashOffset >= period) { dashOffset -= period; dashRotation++; }
        while (dashOffset < 0f) { dashOffset += period; dashRotation--; }
    }

    public override void Render()
    {
        base.Render();

        if (length <= 0f) { return; }

        if (showBgLine)
        {
            Draw.Line(start, end, bgLineColor.Parsed(), thickness);
        }

        if (showNodes)
        {
            nodeTexture.DrawCentered(start, PickColor(nodeColors, 0).Parsed());
            nodeTexture.DrawCentered(end, PickColor(nodeColors, 1).Parsed());
        }

        float period = dashLength + gapLength;
        if (period <= 0f) { return; }

        int first = (int)Math.Floor((-dashLength - dashOffset) / period);
        int last = (int)Math.Ceiling((length - dashOffset) / period);

        for (int i = first; i <= last; i++)
        {
            DrawDash(dashOffset + i * period, PickColor(lineColors, i + dashRotation));
        }
    }

    private void DrawDash(float from, CColor lineColor)
    {
        float fadeRatio = Math.Clamp(fadeLength / dashLength, 0f, 0.5f);

        for (int i = 0; i < resolution; i++)
        {
            float ratioA = i / (float)resolution, ratioB = (i + 1) / (float)resolution;
            float distanceA = from + ratioA * dashLength, distanceB = from + ratioB * dashLength;

            if (distanceB <= 0f || distanceA >= length) { continue; }

            float alpha = FadeAlpha(ratioA, ratioB, fadeRatio);

            Draw.Line(start + direction * Math.Clamp(distanceA, 0f, length),
                start + direction * Math.Clamp(distanceB, 0f, length),
                lineColor.Parsed(alpha), thickness);
        }
    }

    private static float FadeAlpha(float ratioA, float ratioB, float fadeRatio)
    {
        if (fadeRatio <= 0f) { return 1f; }

        float ratio = (ratioA + ratioB) * 0.5f;
        return Math.Clamp(Math.Min(ratio / fadeRatio, (1f - ratio) / fadeRatio), 0f, 1f);
    }

    private static CColor PickColor(List<CColor> colors, int index)
    {
        return colors[NumberUtils.Mod(index, colors.Count)];
    }

    private static List<CColor> ParseColors(string attribute, CColor fallback)
    {
        List<CColor> result = new();

        foreach (string entry in attribute.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            result.Add(new CColor(entry));
        }

        if (result.Count == 0) { result.Add(fallback); }

        return result;
    }
}
