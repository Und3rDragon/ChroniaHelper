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
    public static void EnterRectangleGroup(string groupName, ICollection<Rectangle> rectangles)
    {
        foreach(var r in rectangles)
        {
            EnterRectangleGroup(groupName, r);
        }
    }

    public static void EnterRectangleGroup(string groupName, Rectangle rectangle)
    {
        bool c = Md.Session.Geometry_Rectangles.ContainsKey(groupName);
        if (c)
        {
            Md.Session.Geometry_Rectangles[groupName].Enter(rectangle);
        }
        else
        {
            HashSet<Rectangle> r = new();
            r.Add(rectangle);
            Md.Session.Geometry_Rectangles.Enter(groupName, r);
        }
    }

    public static void EnterRectangleGroup(string groupName, Vector2 point1, Vector2 point2)
    {
        bool c = Md.Session.Geometry_Rectangles.ContainsKey(groupName);
        if (c)
        {
            Md.Session.Geometry_Rectangles[groupName].Enter(ParseRectangle(point1, point2));
        }
        else
        {
            HashSet<Rectangle> r = new();
            r.Add(ParseRectangle(point1, point2));
            Md.Session.Geometry_Rectangles.Enter(groupName, r);
        }
    }

    public static HashSet<Rectangle> GetRectangleGroup(string index)
    {
        bool c = Md.Session.Geometry_Rectangles.ContainsKey(index);

        if (c)
        {
            return Md.Session.Geometry_Rectangles[index];
        }
        else
        {
            return new HashSet<Rectangle>();
        }
    }

    public static bool RectangleGroupCollide(this Vector2 point, string groupIndex)
    {
        var r = GetRectangleGroup(groupIndex);

        foreach (var item in r)
        {
            float x1 = item.Left, x2 = item.Right, y1 = item.Top, y2 = item.Bottom;
            bool flag1 = point.X >= float.Min(x1, x2) && point.X <= float.Max(x1, x2),
                flag2 = point.Y >= float.Min(y1, y2) && point.Y <= float.Max(y1, y2);
            if(flag1 && flag2)
            {
                return true;
            }
        }

        return false;
    }

    public static void EnterFreeRectangleGroup(this string groupName, Vector2 point1, Vector2 point2)
    {
        if (point1 == point2) { return; }
        if (point1.IsNull() || point2.IsNull()) { return; }
        if (Md.Session.Geometry_FreeRectangles.ContainsKey(groupName))
        {
            Md.Session.Geometry_FreeRectangles[groupName].Enter(
                new(point1.X, point1.Y, point2.X, point2.Y)
                );
        }
        else
        {
            Md.Session.Geometry_FreeRectangles.Enter(groupName, new());
            Md.Session.Geometry_FreeRectangles[groupName].Enter(
                new(point1.X, point1.Y, point2.X, point2.Y)
                );
        }
    }

    public static void EnterFreeRectangleGroup(this string groupName, Vector2 topLeft, float width, float height)
    {
        EnterFreeRectangleGroup(groupName, topLeft, topLeft + new Vector2(width, height));
    }

    public static HashSet<Vector4> GetFreeRectangleGroup(this string groupName)
    {
        if (Md.Session.Geometry_FreeRectangles.ContainsKey(groupName))
        {
            return Md.Session.Geometry_FreeRectangles[groupName];
        }

        return new HashSet<Vector4>();
    }

    public static bool FreeRectangleGroupCollide(this Vector2 point, string groupIndex)
    {
        var r = GetFreeRectangleGroup(groupIndex);

        foreach (var item in r)
        {
            Vector2 a = new(Calc.Min(item.X, item.Z), Calc.Min(item.Y, item.W)), 
                b = new(Calc.Max(item.X, item.Z), Calc.Max(item.Y, item.W));
            if (point.X >= a.X && point.X <= b.X && point.Y >= a.Y && point.Y <= b.Y)
            {
                return true;
            }
        }

        return false;
    }

    public static bool FreeRectangleGroupCollide(this Player player, string groupIndex)
    {
        var r = GetFreeRectangleGroup(groupIndex);

        foreach (var item in r)
        {
            Vector2 a = new(Calc.Min(item.X, item.Z), Calc.Min(item.Y, item.W)),
                b = new(Calc.Max(item.X, item.Z), Calc.Max(item.Y, item.W));

            if (player.BottomRight.X >= a.X && player.BottomRight.X <= b.X && player.BottomRight.Y >= a.Y && player.BottomRight.Y <= b.Y)
            {
                return true;
            }
            if (player.TopLeft.X >= a.X && player.TopLeft.X <= b.X && player.TopLeft.Y >= a.Y && player.TopLeft.Y <= b.Y)
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

    public static Rectangle ParseRectangle(this Vector2 a, Vector2 b)
    {
        if (a == b) { return new Rectangle(-8,-8,16,16); }

        return new((int)Calc.Min(a.X, b.X), (int)Calc.Min(a.Y, b.Y), (int)(a.X - b.X).GetAbs(), (int)(a.Y - b.Y).GetAbs());
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

    public struct Line
    {
        public Vector2 A;
        public Vector2 B;

        public Line(Vector2 A, Vector2 B)
        {
            this.A = A;
            this.B = B;
        }

        public float Length()
        {
            Vector2 AB = B - A;
            return AB.Length();
        }

        public float GetDistance(Vector2 C)
        {
            Vector2 AB = B - A;
            Vector2 AC = C - A;

            // 计算叉积 (AB × AC) 的模（在 2D 中是标量）
            float crossProduct = AB.X * AC.Y - AB.Y * AC.X;

            // 计算向量 AB 的长度
            float lengthAB = AB.Length();

            // 避免除以零（A 和 B 不能重合）
            if (lengthAB == 0f)
                return AC.Length(); // A 和 B 重合，退化为点到点距离

            // 距离 = |叉积| / |AB|
            return Math.Abs(crossProduct) / lengthAB;
        }

        /// <summary>
        /// 计算点 C 到直线 AB 的垂足 D，并返回 AD 向量
        /// </summary>
        public Vector2 GetADVector(Vector2 C)
        {
            Vector2 AB = B - A;
            Vector2 AC = C - A;

            float abLengthSquared = AB.LengthSquared();

            // 防止 A 和 B 重合
            if (abLengthSquared == 0f)
                return Vector2.Zero; // 或 throw

            // 计算投影系数 t
            float t = Vector2.Dot(AC, AB) / abLengthSquared;

            // AD 向量 = t * AB
            Vector2 AD = t * AB;

            return AD;
        }

        /// <summary>
        /// 返回垂足 D 的坐标
        /// </summary>
        public Vector2 GetFootPointD(Vector2 C)
        {
            Vector2 AB = B - A;
            Vector2 AC = C - A;

            float abLengthSquared = AB.LengthSquared();

            if (abLengthSquared == 0f)
                return A; // A 和 B 重合

            float t = Vector2.Dot(AC, AB) / abLengthSquared;

            return A + t * AB;
        }

        /// <summary>
        /// 返回 AD 的长度（带符号，表示方向）
        /// </summary>
        public float GetADLengthSigned(Vector2 C)
        {
            Vector2 AB = B - A;
            Vector2 AC = C - A;

            float abLengthSquared = AB.LengthSquared();

            if (abLengthSquared == 0f)
                return 0f;

            float t = Vector2.Dot(AC, AB) / abLengthSquared;

            // 有符号长度：t * |AB|
            return t * AB.Length();
        }
    }
}
