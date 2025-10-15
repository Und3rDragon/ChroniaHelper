using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public static partial class GeometryUtils
{
    // --- Normalize winding to clockwise using signed area ---
    public static Vector2[] NormalizeWindingToClockwise(Vector2[] points)
    {
        if (IsClockwise(points))
        {
            return points;
        }
        else
        {
            Vc2[] reverse = new Vc2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                reverse[i] = points[points.Length - 1 - i];
            }

            return reverse;
        }
    }

    // --- Determine winding using signed polygon area (Y-up coordinate system) ---
    public static bool IsClockwise(Vector2[] points)
    {
        float area = 0;
        int n = points.Length;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += points[i].X * points[j].Y;
            area -= points[j].X * points[i].Y;
        }
        // 在 Y 轴向下的屏幕坐标系中：
        // area > 0 表示点序为顺时针（从屏幕视角）
        return area > 0;
    }

    // --- Compute centroid (not strictly needed for collision, but kept for consistency) ---
    public static Vector2 ComputeCentroid(Vector2[] points)
    {
        var pts = points.ToList();
        if (pts.Count > 0 && pts[0] != pts[pts.Count - 1])
            pts.Add(pts[0]);

        float cx = 0, cy = 0, area = 0;
        int n = pts.Count - 1;

        for (int i = 0; i < n; i++)
        {
            float cross = pts[i].X * pts[i + 1].Y - pts[i + 1].X * pts[i].Y;
            area += cross;
            cx += (pts[i].X + pts[i + 1].X) * cross;
            cy += (pts[i].Y + pts[i + 1].Y) * cross;
        }

        area *= 0.5f;
        if (Math.Abs(area) < 1e-6f) return pts[0];
        cx /= (6 * area);
        cy /= (6 * area);
        return new Vector2(cx, cy);
    }

    // --- Convexity test + AABB bounds ---
    public static bool IsConvexAndComputeBounds(Vector2[] points, float[] bounds)
    {
        int n = points.Length;
        bounds[0] = bounds[2] = float.MaxValue;
        bounds[1] = bounds[3] = float.MinValue;

        bool isConvex = true;
        bool? firstSign = null;

        for (int i = 0; i < n; i++)
        {
            Vector2 p = points[i];
            if (p.X < bounds[0]) bounds[0] = p.X;
            if (p.X > bounds[1]) bounds[1] = p.X;
            if (p.Y < bounds[2]) bounds[2] = p.Y;
            if (p.Y > bounds[3]) bounds[3] = p.Y;

            Vector2 v1 = points[(i + 1) % n] - p;
            Vector2 v2 = points[(i + 2) % n] - points[(i + 1) % n];
            float cross = v1.X * v2.Y - v1.Y * v2.X;

            bool currentSign = cross > 0;
            if (firstSign == null)
            {
                firstSign = currentSign;
            }
            else if (firstSign != currentSign)
            {
                isConvex = false;
            }
        }

        return isConvex;
    }
    
    public static bool ArbitaryCollide(string dataIndex, Vector2 point)
    {
        var points = dataIndex.GetPointsGroup();

        if (points.Count == 0)
            return false;

        if (points.Count == 1)
            return Vector2.Distance(point, points[0]) < 0.001f;

        if (points.Count == 2)
            return IsPointOnSegment(point, points[0], points[1]);

        if (points.Count == 3)
            return IsPointInTriangle(point, points[0], points[1], points[2]);

        // 当 count >= 4，判断是否在多边形内部或边上
        return IsPointInPolygon(point, points);
    }

    // 判断点是否在线段上（包括端点）
    public static bool IsPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        float cross = CrossProduct(b - a, p - a);
        if (Math.Abs(cross) > 0.001f) // 不共线
            return false;

        float dot = DotProduct(p - a, b - a);
        if (dot < 0) return false;

        float lenSq = DotProduct(b - a, b - a);
        return dot <= lenSq;
    }

    // 判断点是否在三角形内部或边上
    public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        bool b1 = Sign(p, a, b) <= 0;
        bool b2 = Sign(p, b, c) <= 0;
        bool b3 = Sign(p, c, a) <= 0;

        return (b1 && b2 && b3);
    }

    public static float Sign(Vector2 p, Vector2 a, Vector2 b)
    {
        return (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X);
    }

    // 判断点是否在多边形内（射线法 + 边上检测）
    public static bool IsPointInPolygon(Vector2 p, List<Vector2> polygon)
    {
        int n = polygon.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 vi = polygon[i];
            Vector2 vj = polygon[j];

            // 检查点是否在边上
            if (IsPointOnSegment(p, vi, vj))
                return true;

            if ((vi.Y > p.Y) != (vj.Y > p.Y))
            {
                float intersectX = vj.X - vj.X - vi.X;
                intersectX = vi.X + (p.Y - vi.Y) * (vj.X - vi.X) / (vj.Y - vi.Y);
                if (p.X < intersectX)
                    inside = !inside;
            }
        }

        return inside;
    }

    // 向量点积
    public static float DotProduct(Vector2 a, Vector2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    // 向量叉积
    public static float CrossProduct(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

}
