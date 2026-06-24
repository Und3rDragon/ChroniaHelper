using ChroniaHelper.Utils;

namespace ChroniaHelper.Components.SwimmingFish;

/// <summary>
/// 复合矩形边界 - 由多个矩形合并而成
/// </summary>
public class MultiRectangleBoundary : IFishBoundary
{
    private Rectangle[] _rectangles;
    private float _gapProtection;
    private List<Rectangle> _expandedRectangles;
    private float _minX, _maxX, _minY, _maxY;
    
    public MultiRectangleBoundary(Rectangle[] rectangles, float gapProtection = 20f)
    {
        if (rectangles == null || rectangles.Length == 0)
            throw new ArgumentException("至少需要一个矩形");
            
        _rectangles = rectangles;
        _gapProtection = gapProtection;
        
        // 扩展所有矩形（用于快速判断）
        _expandedRectangles = new List<Rectangle>();
        foreach (var rect in rectangles)
        {
            int expand = (int)Math.Ceiling(gapProtection);
            var expanded = new Rectangle(
                rect.X - expand,
                rect.Y - expand,
                rect.Width + expand * 2,
                rect.Height + expand * 2
            );
            _expandedRectangles.Add(expanded);
        }
        
        // 计算总边界框
        _minX = rectangles.Min(r => r.X - gapProtection);
        _maxX = rectangles.Max(r => r.X + r.Width + gapProtection);
        _minY = rectangles.Min(r => r.Y - gapProtection);
        _maxY = rectangles.Max(r => r.Y + r.Height + gapProtection);
    }
    
    public bool Contains(Vector2 point)
    {
        // 快速排除
        if (point.X < _minX || point.X > _maxX || point.Y < _minY || point.Y > _maxY)
            return false;
        
        // 检查是否在任何扩展矩形内
        foreach (var rect in _expandedRectangles)
        {
            if (rect.Contains((int)point.X, (int)point.Y))
                return true;
        }
        
        // 检查是否在保护距离内
        return IsPointInGapProtection(point);
    }
    
    private bool IsPointInGapProtection(Vector2 point)
    {
        foreach (var rect in _rectangles)
        {
            // 检查点到矩形边界的距离
            float dist = DistanceToRectangle(point, rect);
            if (dist <= _gapProtection)
                return true;
        }
        return false;
    }
    
    private float DistanceToRectangle(Vector2 point, Rectangle rect)
    {
        // 计算点到矩形的最小距离
        float dx = 0, dy = 0;
        
        if (point.X < rect.X)
            dx = rect.X - point.X;
        else if (point.X > rect.X + rect.Width)
            dx = point.X - (rect.X + rect.Width);
        
        if (point.Y < rect.Y)
            dy = rect.Y - point.Y;
        else if (point.Y > rect.Y + rect.Height)
            dy = point.Y - (rect.Y + rect.Height);
        
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    
    public Vector2 ClampToBoundary(Vector2 point)
    {
        if (Contains(point))
            return point;
        
        // 找最近的矩形，将点夹到最近的矩形内（考虑保护距离）
        float minDist = float.MaxValue;
        Vector2 result = point;
        
        foreach (var rect in _rectangles)
        {
            Vector2 clamped = ClampToRectangle(point, rect, _gapProtection);
            float dist = Vector2.Distance(point, clamped);
            if (dist < minDist)
            {
                minDist = dist;
                result = clamped;
            }
        }
        
        return result;
    }
    
    private Vector2 ClampToRectangle(Vector2 point, Rectangle rect, float gap)
    {
        // 将点夹到矩形边界内，并考虑保护距离
        float x = point.X;
        float y = point.Y;
        
        // 计算最近的边界
        float leftDist = Math.Abs(point.X - rect.X);
        float rightDist = Math.Abs(point.X - (rect.X + rect.Width));
        float topDist = Math.Abs(point.Y - rect.Y);
        float bottomDist = Math.Abs(point.Y - (rect.Y + rect.Height));
        
        float minDist = Math.Min(Math.Min(leftDist, rightDist), Math.Min(topDist, bottomDist));
        
        if (minDist <= gap)
        {
            // 在保护范围内，推向矩形
            if (point.X < rect.X)
                x = rect.X - gap;
            else if (point.X > rect.X + rect.Width)
                x = rect.X + rect.Width + gap;
            else
                x = point.X;
                
            if (point.Y < rect.Y)
                y = rect.Y - gap;
            else if (point.Y > rect.Y + rect.Height)
                y = rect.Y + rect.Height + gap;
            else
                y = point.Y;
        }
        else
        {
            // 直接夹到矩形边界内
            x = MathHelper.Clamp(point.X, rect.X + gap, rect.X + rect.Width - gap);
            y = MathHelper.Clamp(point.Y, rect.Y + gap, rect.Y + rect.Height - gap);
        }
        
        return new Vector2(x, y);
    }
    
    public Vector2 GetBoundaryForce(Vector2 position, float safeZone)
    {
        Vector2 force = Vector2.Zero;
        
        foreach (var rect in _rectangles)
        {
            Vector2 rectForce = GetRectangleBoundaryForce(position, rect, safeZone + _gapProtection);
            force += rectForce;
        }
        
        // 限制最大推力
        if (force.Length() > 1f)
            force.Normalize();
            
        return force;
    }
    
    private Vector2 GetRectangleBoundaryForce(Vector2 position, Rectangle rect, float safeZone)
    {
        Vector2 force = Vector2.Zero;
        
        // 计算到矩形四条边的距离
        float leftDist = position.X - rect.X;
        float rightDist = rect.X + rect.Width - position.X;
        float topDist = position.Y - rect.Y;
        float bottomDist = rect.Y + rect.Height - position.Y;
        
        // 如果点在矩形内
        if (leftDist >= 0 && rightDist >= 0 && topDist >= 0 && bottomDist >= 0)
        {
            // 靠近边界时产生推力
            float minDist = Math.Min(Math.Min(leftDist, rightDist), Math.Min(topDist, bottomDist));
            if (minDist < safeZone)
            {
                float strength = 1f - minDist / safeZone;
                
                // 找到最近的边界方向
                if (leftDist <= rightDist && leftDist <= topDist && leftDist <= bottomDist)
                    force.X = -strength;
                else if (rightDist <= leftDist && rightDist <= topDist && rightDist <= bottomDist)
                    force.X = strength;
                else if (topDist <= leftDist && topDist <= rightDist && topDist <= bottomDist)
                    force.Y = -strength;
                else
                    force.Y = strength;
            }
        }
        else
        {
            // 点在矩形外，推向矩形
            Vector2 toRect = rect.Center.ToVector2() - position;
            float dist = toRect.Length();
            if (dist < safeZone)
            {
                float strength = 1f - dist / safeZone;
                if (dist > 0.01f)
                    force = Vector2.Normalize(toRect) * strength;
                else
                    force = new Vector2(0, -strength);
            }
        }
        
        return force;
    }
}