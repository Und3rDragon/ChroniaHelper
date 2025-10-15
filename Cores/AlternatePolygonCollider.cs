using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;

namespace ChroniaHelper.Cores;

public class AlternatePolygonCollider : Collider
{
    public Vector2[] RelativePoints { get; private set; }
    public Vector2 Offset { get; private set; }

    private Vector2[] _triangulatedPoints;
    private int[] _indices;
    private bool _isConvex;
    private Rectangle _aabb; // AABB in local space (relative to Entity.Position + Position)

    // --- 构造函数 ---
    public AlternatePolygonCollider(Vector2[] relativePoints, Vector2 offset)
    {
        if (relativePoints == null || relativePoints.Length < 3)
            throw new ArgumentException("Polygon must have at least 3 points.");

        RelativePoints = (Vector2[])relativePoints.Clone();
        Offset = offset;

        // Normalize winding to clockwise for triangulation
        Vector2[] normalizedPoints = NormalizeWindingToClockwise(RelativePoints);

        // Compute bounds and convexity
        float[] bounds = new float[4]; // minX, maxX, minY, maxY
        _isConvex = IsConvexAndComputeBounds(normalizedPoints, bounds);

        // Build AABB in local space: (RelativePoints + Offset) defines the shape relative to (Entity.Position + Position)
        int left = (int)Math.Floor(bounds[0] + Offset.X);
        int top = (int)Math.Floor(bounds[2] + Offset.Y);
        int width = Math.Max(1, (int)Math.Ceiling(bounds[1] - bounds[0]));
        int height = Math.Max(1, (int)Math.Ceiling(bounds[3] - bounds[2]));
        _aabb = new Rectangle(left, top, width, height);

        // Triangulate (must be clockwise)
        Triangulator.Triangulator.Triangulate(
            normalizedPoints,
            Triangulator.WindingOrder.Clockwise,
            out _triangulatedPoints,
            out _indices
        );

        RelativePoints = normalizedPoints; // store normalized version
    }

    // --- Normalize winding to clockwise using signed area ---
    private static Vector2[] NormalizeWindingToClockwise(Vector2[] points)
    {
        if (IsClockwise(points))
        { 
            return points; 
        }
        else
        {
            Vc2[] reverse = new Vc2[points.Length];
            for(int i = 0; i < points.Length; i++)
            {
                reverse[i] = points[points.Length - 1 - i];
            }

            return reverse;
        }
    }

    // --- Determine winding using signed polygon area (Y-up coordinate system) ---
    private static bool IsClockwise(Vector2[] points)
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
    private static Vector2 ComputeCentroid(Vector2[] points)
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
    private static bool IsConvexAndComputeBounds(Vector2[] points, float[] bounds)
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

    // --- Helper: get absolute world points ---
    private Vector2[] GetAbsolutePoints()
    {
        Vector2 basePos = (Entity?.Position ?? Vector2.Zero) + Position + Offset;
        Vector2[] abs = new Vector2[RelativePoints.Length];
        for (int i = 0; i < RelativePoints.Length; i++)
        {
            abs[i] = basePos + RelativePoints[i];
        }
        return abs;
    }

    // --- Collider properties with full get/set support ---
    public override float Width
    {
        get => _aabb.Width;
        set { /* Width is fixed for polygon; assignment ignored */ }
    }

    public override float Height
    {
        get => _aabb.Height;
        set { /* Height is fixed for polygon; assignment ignored */ }
    }

    public override float Left
    {
        get => (Entity?.Position.X ?? 0f) + Position.X + _aabb.Left;
        set => Position = new Vector2(value - (Entity?.Position.X ?? 0f) - _aabb.Left, Position.Y);
    }

    public override float Right
    {
        get => (Entity?.Position.X ?? 0f) + Position.X + _aabb.Right;
        set => Position = new Vector2(value - (Entity?.Position.X ?? 0f) - _aabb.Right, Position.Y);
    }

    public override float Top
    {
        get => (Entity?.Position.Y ?? 0f) + Position.Y + _aabb.Top;
        set => Position = new Vector2(Position.X, value - (Entity?.Position.Y ?? 0f) - _aabb.Top);
    }

    public override float Bottom
    {
        get => (Entity?.Position.Y ?? 0f) + Position.Y + _aabb.Bottom;
        set => Position = new Vector2(Position.X, value - (Entity?.Position.Y ?? 0f) - _aabb.Bottom);
    }

    // --- Point-in-polygon ---
    private bool PointInPolygon(Vector2 worldPoint)
    {
        if (_isConvex)
        {
            // Ray casting (works for convex and concave)
            bool inside = false;
            int n = RelativePoints.Length;
            Vector2 origin = (Entity?.Position ?? Vector2.Zero) + Position + Offset;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                Vector2 vi = origin + RelativePoints[i];
                Vector2 vj = origin + RelativePoints[j];
                if (((vi.Y > worldPoint.Y) != (vj.Y > worldPoint.Y)) &&
                    (worldPoint.X < (vj.X - vi.X) * (worldPoint.Y - vi.Y) / (vj.Y - vi.Y) + vi.X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }
        else
        {
            // Use triangulation
            Vector2 origin = (Entity?.Position ?? Vector2.Zero) + Position + Offset;
            for (int i = 0; i < _indices.Length; i += 3)
            {
                Vector2 a = origin + _triangulatedPoints[_indices[i]];
                Vector2 b = origin + _triangulatedPoints[_indices[i + 1]];
                Vector2 c = origin + _triangulatedPoints[_indices[i + 2]];
                if (PointInTriangle(a, b, c, worldPoint))
                    return true;
            }
            return false;
        }
    }

    private static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        float area = Math.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y));
        float a1 = Math.Abs((a.X - p.X) * (b.Y - p.Y) - (b.X - p.X) * (a.Y - p.Y));
        float a2 = Math.Abs((b.X - p.X) * (c.Y - p.Y) - (c.X - p.X) * (b.Y - p.Y));
        float a3 = Math.Abs((c.X - p.X) * (a.Y - p.Y) - (a.X - p.X) * (c.Y - p.Y));
        return Math.Abs(a1 + a2 + a3 - area) < 0.001f;
    }

    // --- Collision implementations ---
    public override bool Collide(Vector2 point)
    {
        Vector2 local = point - (Entity?.Position ?? Vector2.Zero) - Position;
        if (!Monocle.Collide.RectToPoint(_aabb, local)) return false;
        return PointInPolygon(point);
    }

    public override bool Collide(Rectangle rect)
    {
        Vector2[] abs = GetAbsolutePoints();
        for (int i = 0; i < abs.Length; i++)
        {
            Vector2 a = abs[i];
            Vector2 b = abs[(i + 1) % abs.Length];
            if (Monocle.Collide.RectToLine(rect.Left, rect.Top, rect.Width, rect.Height, a, b))
                return true;
        }
        return PointInPolygon(new Vector2(rect.Center.X, rect.Center.Y));
    }

    public override bool Collide(Vector2 from, Vector2 to)
    {
        Vector2[] abs = GetAbsolutePoints();
        for (int i = 0; i < abs.Length; i++)
        {
            Vector2 a = abs[i];
            Vector2 b = abs[(i + 1) % abs.Length];
            if (Monocle.Collide.LineCheck(a, b, from, to))
                return true;
        }
        return PointInPolygon(from);
    }

    public override bool Collide(Hitbox hitbox) => Collide(hitbox.Bounds);

    public override bool Collide(Circle circle)
    {
        Vector2[] abs = GetAbsolutePoints();
        for (int i = 0; i < abs.Length; i++)
        {
            Vector2 a = abs[i];
            Vector2 b = abs[(i + 1) % abs.Length];
            if (Monocle.Collide.CircleToLine(circle.Center, circle.Radius, a, b))
                return true;
        }
        return PointInPolygon(circle.Center);
    }

    public override bool Collide(Grid grid)
    {
        Rectangle testRect = _aabb;
        testRect.Inflate(7, 7); // safety margin

        Vector2 entityOffset = (Entity?.Position ?? Vector2.Zero) + Position;
        int x0 = (int)Math.Ceiling((testRect.Left + entityOffset.X) / 8f);
        int x1 = (int)Math.Floor((testRect.Right + entityOffset.X) / 8f);
        int y0 = (int)Math.Ceiling((testRect.Top + entityOffset.Y) / 8f);
        int y1 = (int)Math.Floor((testRect.Bottom + entityOffset.Y) / 8f);

        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                if (grid[x, y] && Collide(new Rectangle(x * 8, y * 8, 8, 8)))
                    return true;
            }
        }
        return false;
    }

    public override bool Collide(ColliderList list)
    {
        foreach (Collider c in list.colliders)
            if (Collide(c)) return true;
        return false;
    }

    // --- Clone and Render ---
    public override Collider Clone()
    {
        var clone = new AlternatePolygonCollider(RelativePoints, Offset);
        clone.Position = Position;
        return clone;
    }

    public override void Render(Camera camera, Color color)
    {
        Vector2[] abs = GetAbsolutePoints();
        for (int i = 0; i < abs.Length; i++)
        {
            Vector2 a = abs[i];
            Vector2 b = abs[(i + 1) % abs.Length];
            if (Monocle.Collide.RectToLine(camera.Left, camera.Top, 320 * camera.Zoom, 180 * camera.Zoom, a, b))
                Draw.Line(a, b, color);
        }
    }
}