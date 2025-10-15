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


    public Vc2 GetBezierPoint(Vc2[] points, float overrideLerp)
    {
        if (points == null || points.Length == 0) return Vc2.Zero;
        if (points.Length == 1) return points[0];
        if (points.Length == 2)
            return overrideLerp.LerpValue(0f, 1f, points[0], points[1]);

        float t = overrideLerp;
        float u = 1 - t;

        // 二次贝塞尔：3个点
        if (points.Length == 3)
        {
            float uu = u * u;
            float tt = t * t;
            return uu * points[0] + 2 * u * t * points[1] + tt * points[2];
        }

        // 三次贝塞尔：4个点
        if (points.Length == 4)
        {
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = tt * t;
            return uuu * points[0]
                 + 3 * uu * t * points[1]
                 + 3 * u * tt * points[2]
                 + ttt * points[3];
        }

        // 高阶：使用非递归迭代版 de Casteljau（避免递归 + new[]）
        int n = points.Length;
        Vc2[] temp = new Vc2[n];
        Array.Copy(points, temp, n);

        for (int i = 1; i < n; i++)
        {
            for (int j = 0; j < n - i; j++)
            {
                temp[j] = overrideLerp.LerpValue(0f, 1f, temp[j], temp[j + 1]);
            }
        }

        return temp[0];
    }

    public Vc2[] GetBezierPoints(int resolution, Vc2? offsets = null)
    {
        int res = resolution.ClampMin(1);
        
        Vc2[] points = new Vc2[res + 1];
        for (int i = 0; i < res + 1; i++)
        {
            points[i] = GetBezierPoint((float)i / res) + (offsets?? offset);
        }

        return points;
    }

    public float GetBezierCurveLength(int resolution, Vc2? offset = null)
    {
        Vc2[] bezierPoints = GetBezierPoints(resolution, offset);

        float length = 0f;
        for (int i = 1; i < bezierPoints.Length; i++)
        {
            length += Vc2.Distance(bezierPoints[i - 1], bezierPoints[i]);
        }

        return length;
    }
    
    public float GetBezierCurveLength(int resolution, out Vc2[] bezierPoints, Vc2? offset = null)
    {
        bezierPoints = GetBezierPoints(resolution, offset);

        float length = 0f;
        for (int i = 1; i < bezierPoints.Length; i++)
        {
            length += Vc2.Distance(bezierPoints[i - 1], bezierPoints[i]);
        }

        return length;
    }

    public void OperateBezierPoints(int resolution, Action<Vc2, Vc2> operation, Vc2? pointOffset = null)
    {
        Vc2[] points = GetBezierPoints(resolution, pointOffset);
        
        for(int i = 0; i < points.Length - 1; i++)
        {
            operation(points[i], points[i + 1]);
        }
    }

    public void OperateBezierPoints(int resolution, Action<Vc2, Vc2, int, int> operation, Vc2? pointOffset = null)
    {
        Vc2[] points = GetBezierPoints(resolution, pointOffset);

        for (int i = 0; i < points.Length - 1; i++)
        {
            operation(points[i], points[i + 1], i, i + 1);
        }
    }

    public void OperateBezierPointsBasedOnLength(int resolution, Action<float, Vc2, Vc2> operation, Vc2? pointOffset = null)
    {
        float length = GetBezierCurveLength(resolution, out Vc2[] points, pointOffset);

        for (int i = 0; i < points.Length - 1; i++)
        {
            operation(length, points[i], points[i + 1]);
        }
    }

    public void OperateBezierPointsBasedOnLength(int resolution, Action<float, Vc2, Vc2, int, int> operation, Vc2? pointOffset = null)
    {
        float length = GetBezierCurveLength(resolution, out Vc2[] points, pointOffset);

        for (int i = 0; i < points.Length - 1; i++)
        {
            operation(length, points[i], points[i + 1], i, i + 1);
        }
    }

    /// <summary>
    /// 获取曲线上距离起点为 lerp * 总长度 的点（等弧长采样）
    /// </summary>
    /// <param name="points">贝塞尔控制点</param>
    /// <param name="lerp">归一化弧长比例 [0,1]</param>
    /// <param name="resolution">采样分辨率（越高越准，默认 100）</param>
    /// <returns>对应位置的点</returns>
    public Vc2 GetEqualDistancePoint(float lerp)
    {
        return GetEqualDistancePoint(points, lerp);
    }

    public Vc2 GetEqualDistancePoint()
    {
        return GetEqualDistancePoint(points, lerp);
    }
    
    public Vc2 GetEqualDistancePoint(Vc2[] points, float lerp, int resolution = 100)
    {
        if (points == null || points.Length == 0) return Vc2.Zero;
        if (points.Length == 1) return points[0];
        if (resolution < 1) resolution = 1;

        lerp = lerp.ClampWhole(0f, 1f);

        // 特殊情况：线性贝塞尔（两点）
        if (points.Length == 2)
        {
            return lerp.LerpValue(0f, 1f, points[0], points[1]);
        }

        // 1. 高分辨率采样，计算累积弧长
        int numSegments = resolution;
        Vc2[] samples = new Vc2[numSegments + 1];
        float[] cumulativeLengths = new float[numSegments + 1];

        samples[0] = points[0];
        cumulativeLengths[0] = 0f;

        float totalLength = 0f;
        for (int i = 1; i <= numSegments; i++)
        {
            float t = (float)i / numSegments;
            samples[i] = GetBezierPoint(points, t);
            float segmentLength = Vc2.Distance(samples[i - 1], samples[i]);
            totalLength += segmentLength;
            cumulativeLengths[i] = totalLength;
        }

        if (totalLength <= 0f)
            return points[0];

        // 2. 目标弧长
        float targetLength = lerp * totalLength;

        // 3. 在 cumulativeLengths 中查找区间 [i-1, i] 使得:
        //    cumulativeLengths[i-1] <= targetLength <= cumulativeLengths[i]
        int index = 0;
        for (int i = 1; i <= numSegments; i++)
        {
            if (cumulativeLengths[i] >= targetLength)
            {
                index = i;
                break;
            }
        }

        // 边界处理
        if (index == 0) return samples[0];
        if (index >= numSegments) return samples[numSegments];

        // 4. 线性插值估算精确的 t
        float prevLen = cumulativeLengths[index - 1];
        float nextLen = cumulativeLengths[index];
        float segmentRatio = (targetLength - prevLen) / (nextLen - prevLen);
        segmentRatio = segmentRatio.ClampWhole(0f, 1f);

        float t0 = (float)(index - 1) / numSegments;
        float t1 = (float)index / numSegments;
        float interpolatedT = t0 + segmentRatio * (t1 - t0);

        // 5. 用精确的 t 计算点（可选：再用 de Casteljau 精确计算）
        return GetBezierPoint(points, interpolatedT);
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
        
        HashSet<int> byPassIndexes = new();
        if(gap != 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if(i % (gap * 2) >= gap)
                {
                    byPassIndexes.Add(i);
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
