using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public class BezierCurve
{
    /// <summary>
    /// points[0] is the starting point, points[Length - 1] is the last point
    /// </summary>
    public Vc2[] points = new Vc2[2] { Vc2.Zero, Vc2.Zero };
    public float lerp = 0f;
    public Vc2 offset = Vc2.Zero;

    public BezierCurve() { }

    public BezierCurve(Vector2[] points, float lerp = 0f, float offsetX = 0f, float offsetY = 0f)
    {
        this.points = new Vc2[points.Length];
        this.points = points;
        this.lerp = lerp.ClampWhole(0f, 1f);
        this.offset = new Vc2(offsetX, offsetY);
    }

    public Vc2 GetBezierPoint()
    {
        return GetBezierPoint(points, lerp);
    }

    public Vc2 GetBezierPoint(float overrideLerp)
    {
        return GetBezierPoint(points, overrideLerp);
    }


    public Vc2 GetBezierPoint(Vc2[]points, float overrideLerp)
    {
        if (points.IsNull()) { return Vc2.Zero; }
        if (points.Length == 0) { return Vc2.Zero; }
        if (points.Length == 1) { return points[0]; }

        Vc2[] dump = new Vc2[points.Length - 1];
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vc2 a = points[i], b = points[i + 1];

            dump[i] = overrideLerp.LerpValue(0f, 1f, a, b);
        }

        return GetBezierPoint(dump, overrideLerp);
    }

    public void Render(int? resolution = null, Color? lineColor = null, float? thickness = null,
        Vc2? offset = null, int? gaps = null)
    {
        Render(resolution?? 100, lineColor?? new CColor("663931").Parsed(), thickness?? 1f,
            offset?? Vc2.Zero, gaps?? 0);
    }
    public void Render(int resolution, Color lineColor, float thickness, Vc2 offset, int gaps)
    {
        int res = resolution < 1 ? 1 : resolution;
        int gap = gaps.GetAbs();

        Vc2[] points = new Vc2[res + 1];
        for (int i = 0; i < res + 1; i++)
        {
            points[i] = GetBezierPoint((float)i / res) + offset;
        }
        
        int[] byPassIndexes = new int[points.Length];
        if(gap != 0)
        {
            for (int i = 0, j = 0; i < points.Length; i++)
            {
                if(i % (gap * 2) >= gap)
                {
                    byPassIndexes[j] = i;
                    j++;
                }
            }
        }
        
        for (int i = 0; i < points.Length - 1; i++)
        {
            if (byPassIndexes.Contains(i)) { continue; }
            
            Draw.Line(points[i], points[i + 1], lineColor, thickness);
        }
    }
}

public class BezierGroup
{
    public Vc2[] points = new Vc2[2] { Vc2.Zero, Vc2.Zero };
    /// <summary>
    /// Define how many points there should be in each group
    /// For example, if there are points 1, 2, 3 and 4
    /// Divider = 2 means there'll be 3 groups: {1, 2}, {2, 3}, {3, 4}
    /// Divider = 3 means there'll be only 1 group: {1, 2, 3} Because normally the next group should be {3, 4, 5} but there is no point 5
    /// </summary>
    public int divider = 2;
    public int renderResolution;
    public Color renderColor = Color.White;
    public float thickness = 1.2f;
    public Vc2 offset = Vc2.Zero;
    public int lineGap = 0;

    public BezierCurve[] members = Array.Empty<BezierCurve>();

    public BezierGroup()
    {
        GenerateMembers();
    }

    public BezierGroup(Vc2[] points, int divider = 2, int renderResolution = 20, float offsetX = 0, float offsetY = 0, int renderGaps = 0)
    {
        this.points = new Vc2[points.Length];
        this.points = points;
        this.divider = divider < 2 ? 2 : divider;
        this.renderResolution = renderResolution.GetAbs() < 1 ? 1 : renderResolution.GetAbs();

        offset = new Vc2(offsetX, offsetY);
        lineGap = renderGaps;

        GenerateMembers();
    }

    public void GenerateMembers()
    {
        List<BezierCurve> _members = new();
        for (int i = 0; i < points.Length && i + divider - 1 < points.Length; i += divider - 1)
        {
            Vc2[] dump = new Vc2[divider];
            for (int j = 0; j < divider; j++)
            {
                dump[j] = points[i + j];
            }

            BezierCurve bezier = new BezierCurve(dump, offsetX: offset.X, offsetY: offset.Y);
            _members.Add(bezier);
        }

        members = _members.ToArray();
    }

    public void Render()
    {
        for (int i = 0; i < members.Length; i++)
        {
            members[i].Render(resolution: renderResolution, 
                lineColor: renderColor, thickness: thickness, offset: offset, gaps: lineGap);
        }
    }

}
