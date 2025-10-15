using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using ChroniaHelper.Utils;

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
        Vector2[] normalizedPoints = GeometryUtils.NormalizeWindingToClockwise(RelativePoints);

        // Compute bounds and convexity
        float[] bounds = new float[4]; // minX, maxX, minY, maxY
        _isConvex = GeometryUtils.IsConvexAndComputeBounds(normalizedPoints, bounds);

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
                if (GeometryUtils.IsPointInTriangle(worldPoint, a, b, c))
                    return true;
            }
            return false;
        }
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