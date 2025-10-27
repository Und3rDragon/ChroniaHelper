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

    // 新增：预计算的曲线点
    private Vc2[] _precomputedPoints = null;
    private int _precomputedResolution = 0;
    private float _precomputedLength = 0f;

    public BezierCurve() { }

    public BezierCurve(Vector2[] points, float lerp = 0f, float offsetX = 0f, float offsetY = 0f, int estimationResolution = 100)
    {
        this.points = new Vc2[points.Length];
        this.points = points;
        this.lerp = lerp.Clamp(0f, 1f);
        this.offset = new Vc2(offsetX, offsetY);

        // 预计算曲线点
        PrecomputeCurve(estimationResolution);
    }

    /// <summary>
    /// 预计算曲线点
    /// </summary>
    /// <param name="resolution">分辨率</param>
    public void PrecomputeCurve(int resolution)
    {
        if (resolution < 1) resolution = 1;

        _precomputedResolution = resolution;
        _precomputedPoints = new Vc2[resolution + 1];

        // 计算曲线点并存储
        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            _precomputedPoints[i] = GetBezierPointRaw(points, t);
        }

        // 预计算曲线长度
        _precomputedLength = 0f;
        for (int i = 1; i < _precomputedPoints.Length; i++)
        {
            _precomputedLength += Vc2.Distance(_precomputedPoints[i - 1], _precomputedPoints[i]);
        }
    }

    /// <summary>
    /// 原始贝塞尔计算（不依赖预计算）
    /// </summary>
    private Vc2 GetBezierPointRaw(Vc2[] points, float t)
    {
        if (points == null || points.Length == 0) return Vc2.Zero;
        if (points.Length == 1) return points[0];
        if (points.Length == 2)
            return t.LerpValue(0f, 1f, points[0], points[1]);

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

        // 高阶：使用非递归迭代版 de Casteljau
        int n = points.Length;
        Vc2[] temp = new Vc2[n];
        Array.Copy(points, temp, n);

        for (int i = 1; i < n; i++)
        {
            for (int j = 0; j < n - i; j++)
            {
                temp[j] = t.LerpValue(0f, 1f, temp[j], temp[j + 1]);
            }
        }

        return temp[0];
    }

    public Vc2 GetBezierPoint()
    {
        if (_precomputedPoints != null && _precomputedResolution > 0)
        {
            // 使用预计算的点进行插值
            int index = (int)(lerp * _precomputedResolution);
            index = index.Clamp(0, _precomputedResolution - 1);

            // 线性插值获得更精确的结果
            float segmentT = lerp * _precomputedResolution - index;
            segmentT = segmentT.Clamp(0f, 1f);

            if (index < _precomputedPoints.Length - 1)
            {
                return segmentT.LerpValue(0f, 1f, _precomputedPoints[index], _precomputedPoints[index + 1]);
            }
            else
            {
                return _precomputedPoints[_precomputedPoints.Length - 1];
            }
        }

        // 回退到原始计算
        return GetBezierPointRaw(points, lerp);
    }

    public Vc2 GetBezierPoint(float overrideLerp)
    {
        if (_precomputedPoints != null && _precomputedResolution > 0)
        {
            // 使用预计算的点进行插值
            int index = (int)(overrideLerp * _precomputedResolution);
            index = index.Clamp(0, _precomputedResolution - 1);

            float segmentT = overrideLerp * _precomputedResolution - index;
            segmentT = segmentT.Clamp(0f, 1f);

            if (index < _precomputedPoints.Length - 1)
            {
                return segmentT.LerpValue(0f, 1f, _precomputedPoints[index], _precomputedPoints[index + 1]);
            }
            else
            {
                return _precomputedPoints[_precomputedPoints.Length - 1];
            }
        }

        return GetBezierPointRaw(points, overrideLerp);
    }

    public Vc2 GetBezierPoint(Vc2[] points, float overrideLerp)
    {
        // 如果传入的是不同的点数组，使用原始计算
        if (points != this.points)
        {
            return GetBezierPointRaw(points, overrideLerp);
        }

        return GetBezierPoint(overrideLerp);
    }

    public Vc2[] GetBezierPoints(int resolution, Vc2? offsets = null)
    {
        // 如果请求的分辨率与预计算相同，直接返回预计算结果
        if (_precomputedPoints != null && resolution == _precomputedResolution)
        {
            if (offsets == null || offsets == Vc2.Zero)
            {
                return _precomputedPoints.ToArray(); // 返回副本
            }
            else
            {
                var result = new Vc2[_precomputedPoints.Length];
                for (int i = 0; i < _precomputedPoints.Length; i++)
                {
                    result[i] = _precomputedPoints[i] + offsets.Value;
                }
                return result;
            }
        }

        // 否则使用原始计算
        int res = resolution.ClampMin(1);
        Vc2[] points = new Vc2[res + 1];
        for (int i = 0; i < res + 1; i++)
        {
            points[i] = GetBezierPointRaw(this.points, (float)i / res) + (offsets ?? offset);
        }
        return points;
    }

    public float GetBezierCurveLength(int resolution, Vc2? offset = null)
    {
        // 使用预计算的长度（如果可用且分辨率匹配）
        if (_precomputedPoints != null && resolution == _precomputedResolution && (offset == null || offset == Vc2.Zero))
        {
            return _precomputedLength;
        }

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

    // 其他方法保持不变，因为它们都依赖于上述方法
    public void OperateBezierPoints(int resolution, Action<Vc2, Vc2> operation, Vc2? pointOffset = null)
    {
        Vc2[] points = GetBezierPoints(resolution, pointOffset);

        for (int i = 0; i < points.Length - 1; i++)
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

        lerp = lerp.Clamp(0f, 1f);

        // 特殊情况：线性贝塞尔（两点）
        if (points.Length == 2)
        {
            return lerp.LerpValue(0f, 1f, points[0], points[1]);
        }

        // 如果使用预计算且分辨率匹配
        if (_precomputedPoints != null && resolution == _precomputedResolution && points == this.points)
        {
            float precomputedLength = lerp * _precomputedLength;

            // 在预计算点中查找
            float accumulated = 0f;
            for (int i = 1; i < _precomputedPoints.Length; i++)
            {
                float segmentLength = Vc2.Distance(_precomputedPoints[i - 1], _precomputedPoints[i]);
                if (accumulated + segmentLength >= precomputedLength)
                {
                    float ratio = (precomputedLength - accumulated) / segmentLength;
                    ratio = ratio.Clamp(0f, 1f);
                    return ratio.LerpValue(0f, 1f, _precomputedPoints[i - 1], _precomputedPoints[i]);
                }
                accumulated += segmentLength;
            }
            return _precomputedPoints[_precomputedPoints.Length - 1];
        }

        // 原始计算方式
        int numSegments = resolution;
        Vc2[] samples = new Vc2[numSegments + 1];
        float[] cumulativeLengths = new float[numSegments + 1];

        samples[0] = points[0];
        cumulativeLengths[0] = 0f;

        float totalLength = 0f;
        for (int i = 1; i <= numSegments; i++)
        {
            float t = (float)i / numSegments;
            samples[i] = GetBezierPointRaw(points, t);
            float segmentLength = Vc2.Distance(samples[i - 1], samples[i]);
            totalLength += segmentLength;
            cumulativeLengths[i] = totalLength;
        }

        if (totalLength <= 0f)
            return points[0];

        float targetLength = lerp * totalLength;
        int index = 0;
        for (int i = 1; i <= numSegments; i++)
        {
            if (cumulativeLengths[i] >= targetLength)
            {
                index = i;
                break;
            }
        }

        if (index == 0) return samples[0];
        if (index >= numSegments) return samples[numSegments];

        float prevLen = cumulativeLengths[index - 1];
        float nextLen = cumulativeLengths[index];
        float segmentRatio = (targetLength - prevLen) / (nextLen - prevLen);
        segmentRatio = segmentRatio.Clamp(0f, 1f);

        float t0 = (float)(index - 1) / numSegments;
        float t1 = (float)index / numSegments;
        float interpolatedT = t0 + segmentRatio * (t1 - t0);

        return GetBezierPointRaw(points, interpolatedT);
    }

    public void Render(int? resolution = null, Color? lineColor = null, float? thickness = null,
        Vc2? offset = null, int? gaps = null)
    {
        Render(resolution ?? 100, lineColor ?? new CColor("663931").Parsed(), thickness ?? 1f,
            offset ?? Vc2.Zero, gaps ?? 0);
    }

    public void Render(int resolution, Color lineColor, float thickness, Vc2 offset, int gaps)
    {
        // 使用预计算的点进行渲染（如果分辨率匹配）
        Vc2[] renderPoints;
        if (_precomputedPoints != null && resolution == _precomputedResolution && offset == Vc2.Zero)
        {
            renderPoints = _precomputedPoints;
        }
        else
        {
            renderPoints = GetBezierPoints(resolution, offset);
        }

        int res = resolution < 1 ? 1 : resolution;
        int gap = gaps.GetAbs();

        HashSet<int> byPassIndexes = new();
        if (gap != 0)
        {
            for (int i = 0; i < renderPoints.Length; i++)
            {
                if (i % (gap * 2) >= gap)
                {
                    byPassIndexes.Add(i);
                }
            }
        }

        for (int i = 0; i < renderPoints.Length - 1; i++)
        {
            if (byPassIndexes.Contains(i)) { continue; }

            Draw.Line(renderPoints[i], renderPoints[i + 1], lineColor, thickness);
        }
    }

    /// <summary>
    /// 清除预计算数据（当控制点改变时调用）
    /// </summary>
    public void ClearPrecomputedData()
    {
        _precomputedPoints = null;
        _precomputedResolution = 0;
        _precomputedLength = 0f;
    }

    /// <summary>
    /// 更新控制点并重新预计算
    /// </summary>
    public void UpdatePoints(Vc2[] newPoints, int estimationResolution = 100)
    {
        this.points = newPoints;
        PrecomputeCurve(estimationResolution);
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
