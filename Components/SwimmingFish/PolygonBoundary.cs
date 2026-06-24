using System;
using System.Collections.Generic;
using System.Linq;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Components.SwimmingFish;

/// <summary>
/// 多边形边界 - 支持任意形状的多边形，以及点和线段的特殊情况
/// </summary>
public class PolygonBoundary : IFishBoundary
{
    private Vector2[] _points;
    private float _gapProtection;
    private bool _isPoint;
    private bool _isLine;
    private Vector2 _lineStart;
    private Vector2 _lineEnd;
    
    // 缓存的边界数据
    private List<Vector2> _expandedPoints;
    private List<GeometryUtils.Line> _segments;
    private float _minX, _maxX, _minY, _maxY;
    
    /// <summary>
    /// 构造多边形边界
    /// </summary>
    /// <param name="points">多边形顶点（至少1个）</param>
    /// <param name="gapProtection">边界保护距离</param>
    public PolygonBoundary(Vector2[] points, float gapProtection = 20f)
    {
        if (points == null || points.Length == 0)
            throw new ArgumentException("多边形至少需要1个顶点");
            
        _points = points;
        _gapProtection = gapProtection;
        
        // 判断特殊类型
        _isPoint = points.Length == 1;
        _isLine = points.Length == 2;
        
        if (_isLine)
        {
            _lineStart = points[0];
            _lineEnd = points[1];
        }
        
        // 预计算边界数据
        InitializeBoundaryData();
    }
    
    private void InitializeBoundaryData()
    {
        // 计算边界框（用于快速排除）
        _minX = _points.Min(p => p.X);
        _maxX = _points.Max(p => p.X);
        _minY = _points.Min(p => p.Y);
        _maxY = _points.Max(p => p.Y);
        
        // 预计算线段
        _segments = new List<GeometryUtils.Line>();
        for (int i = 0; i < _points.Length; i++)
        {
            int next = (i + 1) % _points.Length;
            _segments.Add(new GeometryUtils.Line(_points[i], _points[next]));
        }
    }
    
    public bool Contains(Vector2 point)
    {
        // 使用射线法判断点是否在多边形内（考虑保护距离）
        // 对于点和线段，使用距离判断
        
        if (_isPoint)
        {
            return Vector2.Distance(point, _points[0]) <= _gapProtection;
        }
        
        if (_isLine)
        {
            return IsPointNearLineSegment(point, _lineStart, _lineEnd, _gapProtection);
        }
        
        // 快速排除：超出边界框的不可能在多边形内（但可能在保护范围内）
        if (point.X < _minX - _gapProtection || point.X > _maxX + _gapProtection ||
            point.Y < _minY - _gapProtection || point.Y > _maxY + _gapProtection)
        {
            return false;
        }
        
        // 标准射线法判断
        bool inside = false;
        for (int i = 0, j = _points.Length - 1; i < _points.Length; j = i++)
        {
            Vector2 pi = _points[i];
            Vector2 pj = _points[j];
            
            if (((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
            {
                inside = !inside;
            }
        }
        
        // 如果点在多边形内，返回true
        if (inside)
            return true;
        
        // 否则检查是否在保护距离内
        return IsPointWithinGap(point);
    }
    
    private bool IsPointNearLineSegment(Vector2 point, Vector2 start, Vector2 end, float radius)
    {
        Vector2 startToPoint = point - start;
        Vector2 startToEnd = end - start;
        float lineLength = startToEnd.Length();
        
        if (lineLength < 0.001f)
            return Vector2.Distance(point, start) <= radius;
        
        // 计算投影比例
        float t = Vector2.Dot(startToPoint, startToEnd) / (lineLength * lineLength);
        t = MathHelper.Clamp(t, 0f, 1f);
        
        // 最近点
        Vector2 closestPoint = start + startToEnd * t;
        float distance = Vector2.Distance(point, closestPoint);
        
        return distance <= radius;
    }
    
    private bool IsPointWithinGap(Vector2 point)
    {
        // 检查点是否在多边形边界外但在保护距离内
        foreach (var segment in _segments)
        {
            float distance = DistanceToSegment(point, segment.A, segment.B);
            if (distance <= _gapProtection)
                return true;
        }
        
        // 检查顶点保护范围
        foreach (var p in _points)
        {
            if (Vector2.Distance(point, p) <= _gapProtection)
                return true;
        }
        
        return false;
    }
    
    private float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 startToPoint = point - start;
        Vector2 startToEnd = end - start;
        float lineLength = startToEnd.Length();
        
        if (lineLength < 0.001f)
            return Vector2.Distance(point, start);
        
        float t = Vector2.Dot(startToPoint, startToEnd) / (lineLength * lineLength);
        t = MathHelper.Clamp(t, 0f, 1f);
        
        Vector2 closestPoint = start + startToEnd * t;
        return Vector2.Distance(point, closestPoint);
    }
    
    public Vector2 ClampToBoundary(Vector2 point)
    {
        if (Contains(point))
            return point;
        
        // 对于点和线段，直接返回最近的合法点
        if (_isPoint)
        {
            Vector2 dir = point - _points[0];
            if (dir.Length() > _gapProtection)
                dir = Vector2.Normalize(dir) * _gapProtection;
            return _points[0] + dir;
        }
        
        if (_isLine)
        {
            // 找线段上最近的点
            Vector2 closest = ClosestPointOnSegment(point, _lineStart, _lineEnd);
            Vector2 dir = point - closest;
            if (dir.Length() > _gapProtection)
                dir = Vector2.Normalize(dir) * _gapProtection;
            return closest + dir;
        }
        
        // 对于多边形：尝试向多边形中心方向推
        Vector2 center = GetCenter();
        Vector2 direction = center - point;
        if (direction.Length() < 0.01f)
            direction = new Vector2(0, -1);
        else
            direction.Normalize();
        
        // 逐步推进，直到进入边界
        Vector2 testPoint = point;
        float step = 2f;
        for (int i = 0; i < 50; i++) // 限制迭代次数
        {
            testPoint += direction * step;
            if (Contains(testPoint))
                return testPoint;
        }
        
        // 如果失败，返回中心点
        return center;
    }
    
    private Vector2 ClosestPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 startToPoint = point - start;
        Vector2 startToEnd = end - start;
        float lineLength = startToEnd.Length();
        
        if (lineLength < 0.001f)
            return start;
        
        float t = Vector2.Dot(startToPoint, startToEnd) / (lineLength * lineLength);
        t = MathHelper.Clamp(t, 0f, 1f);
        return start + startToEnd * t;
    }
    
    public Vector2 GetBoundaryForce(Vector2 position, float safeZone)
    {
        if (_isPoint || _isLine)
            return GetBoundaryForceForSimpleShape(position, safeZone);
        
        // 检查是否在边界附近
        Vector2 force = Vector2.Zero;
        Vector2 center = GetCenter();
        Vector2 toCenter = center - position;
        float distanceToCenter = toCenter.Length();
        
        if (distanceToCenter < 0.01f)
            return Vector2.Zero;
        
        // 检查边界上的最近点
        Vector2 nearestPoint = GetNearestBoundaryPoint(position);
        float distance = Vector2.Distance(position, nearestPoint);
        
        if (distance < safeZone + _gapProtection)
        {
            float strength = 1f - distance / (safeZone + _gapProtection);
            Vector2 dir = position - nearestPoint;
            if (dir.Length() < 0.01f)
                dir = toCenter / distanceToCenter;
            else
                dir.Normalize();
            
            force = dir * strength;
        }
        
        return force;
    }
    
    private Vector2 GetBoundaryForceForSimpleShape(Vector2 position, float safeZone)
    {
        Vector2 center = GetCenter();
        Vector2 toCenter = center - position;
        float distance = toCenter.Length();
        
        float effectiveRadius = _isPoint ? _gapProtection : 
            Vector2.Distance(_lineStart, _lineEnd) / 2f + _gapProtection;
        
        if (distance < effectiveRadius + safeZone)
        {
            float strength = 1f - distance / (effectiveRadius + safeZone);
            Vector2 dir = toCenter;
            if (dir.Length() < 0.01f)
                dir = new Vector2(0, -1);
            else
                dir.Normalize();
            return dir * strength;
        }
        
        return Vector2.Zero;
    }
    
    private Vector2 GetCenter()
    {
        if (_isPoint)
            return _points[0];
        if (_isLine)
            return (_lineStart + _lineEnd) / 2f;
        
        // 计算多边形质心
        Vector2 center = Vector2.Zero;
        foreach (var p in _points)
            center += p;
        return center / _points.Length;
    }
    
    private Vector2 GetNearestBoundaryPoint(Vector2 point)
    {
        Vector2 nearest = _points[0];
        float minDist = float.MaxValue;
        
        foreach (var segment in _segments)
        {
            Vector2 closest = ClosestPointOnSegment(point, segment.A, segment.B);
            float dist = Vector2.Distance(point, closest);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = closest;
            }
        }
        
        return nearest;
    }
}