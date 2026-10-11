using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.Entities;

//[CustomEntity("ChroniaHelper/LineConnector")]
[Tracked]
[ExA.Note("DrawingHelper import for testing")]
public class LineConnector : Entity
{
    public Vector2[] Nodes { get; }

    public Color LineColor { get; }

    public float Thickness { get; }

    public bool Dashed { get; }

    public float DashLength { get; }

    public float GapLength { get; }

    private Vector2 roomOffset;

    public LineConnector(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        roomOffset = offset;
        Nodes = data.Nodes;
        LineColor = data.HexColor("color", Calc.HexToColor("4DA6FF"));
        Thickness = data.Float("thickness", 2f);
        Dashed = data.Bool("dashed", false);
        DashLength = data.Float("dashLength", 8f);
        GapLength = data.Float("gapLength", 6f);
        Depth = data.Int("depth", 99);
    }

    public override void Render()
    {
        base.Render();

        if (Nodes == null || Nodes.Length == 0)
            return;

        Vector2 start = Position;
        foreach (Vector2 node in Nodes)
        {
            // 节点在 .bin 里是房间绝对坐标，仅需加房间偏移
            Vector2 end = node + roomOffset;
            if (Dashed)
                DrawDashedLine(start, end);
            else
                Draw.Line(start, end, LineColor, Thickness);
            start = end;
        }
    }

    private void DrawDashedLine(Vector2 start, Vector2 end)
    {
        Vector2 delta = end - start;
        float distance = delta.Length();
        if (distance <= 0.01f)
            return;

        Vector2 unit = delta / distance;
        float traveled = 0f;
        bool drawing = true;

        while (traveled < distance)
        {
            float step = drawing ? DashLength : GapLength;
            float next = Math.Min(traveled + step, distance);
            if (drawing)
            {
                Vector2 a = start + unit * traveled;
                Vector2 b = start + unit * next;
                Draw.Line(a, b, LineColor, Thickness);
            }
            traveled = next;
            drawing = !drawing;
        }
    }
}
