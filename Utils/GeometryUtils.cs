using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Modules;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public static class GeometryUtils
{
    public static void EnterRectangleGroup(string groupName, Rectangle rectangle)
    {
        bool c = Md.Session.Geometry_Rectangles.ContainsKey(groupName);
        if (c)
        {
            Md.Session.Geometry_Rectangles[groupName].Enter(
                new KeyValuePair<Vector2, Vector2>(
                        new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom)
                    )
                );
        }
        else
        {
            List<KeyValuePair<Vector2, Vector2>> k = new();
            KeyValuePair<Vector2, Vector2> v = new KeyValuePair<Vector2, Vector2>(new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom));
            k.Enter(v);
            Md.Session.Geometry_Rectangles.Enter(groupName, k);
        }
    }

    public static void EnterRectangleGroup(string groupName, Vector2 point1, Vector2 point2)
    {
        bool c = Md.Session.Geometry_Rectangles.ContainsKey(groupName);
        if (c)
        {
            Md.Session.Geometry_Rectangles[groupName].Enter(
                new KeyValuePair<Vector2, Vector2>(point1, point2)
                );
        }
        else
        {
            List<KeyValuePair<Vector2, Vector2>> k = new();
            KeyValuePair<Vector2, Vector2> v = new KeyValuePair<Vector2, Vector2>(point1, point2);
            k.Enter(v);
            Md.Session.Geometry_Rectangles.Enter(groupName, k);
        }
    }

    public static List<KeyValuePair<Vector2, Vector2>> GetRectangleGroup(string index)
    {
        bool c = Md.Session.Geometry_Rectangles.ContainsKey(index);

        if (c)
        {
            return Md.Session.Geometry_Rectangles[index];
        }
        else
        {
            return new List<KeyValuePair<Vector2, Vector2>>();
        }
    }

    public static bool RectangleGroupCollide(this Vector2 point, string groupIndex)
    {
        var r = GetRectangleGroup(groupIndex);

        foreach (var item in r)
        {
            float x1 = item.Key.X, x2 = item.Value.X, y1 = item.Key.Y, y2 = item.Value.Y;
            bool flag1 = point.X >= float.Min(x1, x2) && point.X <= float.Max(x1, x2),
                flag2 = point.Y >= float.Min(y1, y2) && point.Y <= float.Max(y1, y2);
            if(flag1 && flag2)
            {
                return true;
            }
        }

        return false;
    }

    public static List<Vector2> GetPointsGroup(this string index)
    {
        bool c = Md.Session.Geometry_Points.ContainsKey(index);

        if (c)
        {
            return Md.Session.Geometry_Points[index];
        }
        else
        {
            return new List<Vector2>();
        }
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
    private static bool IsPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
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
    private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        bool b1 = Sign(p, a, b) <= 0;
        bool b2 = Sign(p, b, c) <= 0;
        bool b3 = Sign(p, c, a) <= 0;

        return (b1 && b2 && b3);
    }

    private static float Sign(Vector2 p, Vector2 a, Vector2 b)
    {
        return (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X);
    }

    // 判断点是否在多边形内（射线法 + 边上检测）
    private static bool IsPointInPolygon(Vector2 p, List<Vector2> polygon)
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
    private static float DotProduct(Vector2 a, Vector2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    // 向量叉积
    private static float CrossProduct(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    /// <summary>
    /// "x,y,width,height" => Rectangle
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static Rectangle ParseRectangle(this string attribute)
    {
        string[] p = attribute.Split(",", StringSplitOptions.TrimEntries);
        Rectangle rec = new Rectangle(0,0,16,16);
        if (p.Length >= 4)
        {
            int w, h, x, y;
            int.TryParse(p[2], out w);
            int.TryParse(p[3], out h);
            int.TryParse(p[0], out x);
            int.TryParse(p[1], out y);

            rec = new Rectangle(x, y, w, h);
        }

        return rec;
    }

    public static Rectangle ParseRectangle(this string attribute, Rectangle defaultRectangle)
    {
        string[] p = attribute.Split(",", StringSplitOptions.TrimEntries);
        Rectangle rec = defaultRectangle;
        if (p.Length >= 4)
        {
            int w, h, x, y;
            int.TryParse(p[2], out w);
            int.TryParse(p[3], out h);
            int.TryParse(p[0], out x);
            int.TryParse(p[1], out y);

            rec = new Rectangle(x, y, w, h);
        }

        return rec;
    }

    /// <summary>
    /// "r,x,y" => Circle
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static Circle ParseCircle(this string attribute)
    {
        string[] p = attribute.Split(',', StringSplitOptions.TrimEntries);
        Circle cir = new Circle(16f);
        if (p.Length >= 1)
        {
            float r, x = 0, y = 0;
            float.TryParse(p[0], out r);
            if (p.Length >= 2)
            {
                float.TryParse(p[1], out x);
            }
            if (p.Length >= 3)
            {
                float.TryParse(p[2], out y);
            }
            if (r > 0) { cir = new Circle(r, x, y); }
        }

        return cir;
    }

    public static Circle ParseCircle(this string attribute, Circle defaultCircle)
    {
        string[] p = attribute.Split(',', StringSplitOptions.TrimEntries);
        Circle cir = defaultCircle;
        if (p.Length >= 1)
        {
            float r, x = 0, y = 0;
            float.TryParse(p[0], out r);
            if (p.Length >= 2)
            {
                float.TryParse(p[1], out x);
            }
            if (p.Length >= 3)
            {
                float.TryParse(p[2], out y);
            }
            if (r > 0) { cir = new Circle(r, x, y); }
        }

        return cir;
    }
}
