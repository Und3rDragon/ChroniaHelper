using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Modules;
using VivHelper.Entities;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public static partial class GeometryUtils
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
                new Vector4(point1.X, point1.Y, point2.X, point2.Y)
                );
        }
        else
        {
            Md.Session.Geometry_FreeRectangles.Enter(groupName, new());
            Md.Session.Geometry_FreeRectangles[groupName].Enter(
                new Vector4(point1.X, point1.Y, point2.X, point2.Y)
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

    public static List<Vector2> GenerateRandomPoints(Rectangle bounds, int pointCount)
    {
        List<Vector2> points = new List<Vector2>();
        Random random = new Random();
        for (int i = 0; i < pointCount; i++)
        {
            float x = random.Next(bounds.Left, bounds.Right);
            float y = random.Next(bounds.Top, bounds.Bottom);
            points.Add(new Vector2(x, y));
        }
        return points;
    }
    
    public static Vc2 Floor(this Vc2 vector)
    {
        return new Vc2(float.Floor(vector.X), float.Floor(vector.Y));
    }

    public static Vc2 Ceiling(this Vc2 vector)
    {
        return new Vc2(float.Ceiling(vector.X), float.Ceiling(vector.Y));
    }

    public static Vc2 Round(this Vc2 vector)
    {
        return new Vc2(float.Round(vector.X), float.Round(vector.Y));
    }

    public static Vc3 Floor(this Vc3 vector)
    {
        return new Vc3(float.Floor(vector.X), float.Floor(vector.Y), float.Floor(vector.Z));
    }

    public static Vc3 Ceiling(this Vc3 vector)
    {
        return new Vc3(float.Ceiling(vector.X), float.Ceiling(vector.Y), float.Floor(vector.Z));
    }

    public static Vc3 Round(this Vc3 vector)
    {
        return new Vc3(float.Round(vector.X), float.Round(vector.Y), float.Floor(vector.Z));
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

    /// <summary>
    /// 判断两个矩形是否相交。
    /// 如果它们仅在边或角点接触（即交集面积为0），则返回 abstractConditionValue；
    /// 如果有实际重叠区域（面积 > 0），则返回 true；
    /// 如果完全不相交，返回 false。
    /// </summary>
    public static bool Crossover(this Rectangle orig, Rectangle target, bool abstractConditionValue = false)
    {
        // 计算交集区域的边界
        int left = Math.Max(orig.Left, target.Left);
        int right = Math.Min(orig.Right, target.Right);
        int top = Math.Max(orig.Top, target.Top);
        int bottom = Math.Min(orig.Bottom, target.Bottom);

        // 检查是否有交集
        if (left >= right || top >= bottom)
        {
            // 完全无交集
            return false;
        }

        // 此时存在交集区域
        int width = right - left;
        int height = bottom - top;

        if (width == 0 || height == 0)
        {
            // 理论上由于上面的 >= 判断，这里不会发生，
            // 但为了逻辑清晰：如果交集退化为线或点（面积为0）
            return abstractConditionValue;
        }

        // 有正面积的重叠区域
        return true;
    }
}
