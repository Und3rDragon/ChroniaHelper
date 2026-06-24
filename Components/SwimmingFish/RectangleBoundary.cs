namespace ChroniaHelper.Components.SwimmingFish;

/// <summary>
/// 矩形边界 - 原有的长方形边界
/// </summary>
public class RectangleBoundary : IFishBoundary
{
    private Vector2 _topLeft;
    private Vector2 _bottomRight;
    private float _gapProtection;
    
    public RectangleBoundary(Vector2 topLeft, Vector2 bottomRight, float gapProtection = 20f)
    {
        _topLeft = topLeft;
        _bottomRight = bottomRight;
        _gapProtection = gapProtection;
    }
    
    public bool Contains(Vector2 point)
    {
        // 考虑保护距离
        return point.X >= _topLeft.X - _gapProtection && point.X <= _bottomRight.X + _gapProtection &&
               point.Y >= _topLeft.Y - _gapProtection && point.Y <= _bottomRight.Y + _gapProtection;
    }
    
    public Vector2 ClampToBoundary(Vector2 point)
    {
        // 夹紧时保留保护距离
        float x = MathHelper.Clamp(point.X, _topLeft.X - _gapProtection, _bottomRight.X + _gapProtection);
        float y = MathHelper.Clamp(point.Y, _topLeft.Y - _gapProtection, _bottomRight.Y + _gapProtection);
        return new Vector2(x, y);
    }
    
    public Vector2 GetBoundaryForce(Vector2 position, float safeZone)
    {
        Vector2 force = Vector2.Zero;
        float effectiveZone = safeZone + _gapProtection;
        
        // X方向
        if (position.X < _topLeft.X + effectiveZone)
        {
            float strength = 1f - (position.X - _topLeft.X) / effectiveZone;
            force.X = -strength;
        }
        else if (position.X > _bottomRight.X - effectiveZone)
        {
            float strength = 1f - (_bottomRight.X - position.X) / effectiveZone;
            force.X = strength;
        }
        
        // Y方向
        if (position.Y < _topLeft.Y + effectiveZone)
        {
            float strength = 1f - (position.Y - _topLeft.Y) / effectiveZone;
            force.Y = -strength;
        }
        else if (position.Y > _bottomRight.Y - effectiveZone)
        {
            float strength = 1f - (_bottomRight.Y - position.Y) / effectiveZone;
            force.Y = strength;
        }
        
        return force;
    }
}