using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChroniaHelper.Components.SwimmingFish;

public class FishMotion : BaseComponent
{
    // ==================== 公开字段 ====================
    
    public Vector2 InitialPosition;
    public Vector2 Position;
    public Func<List<Vector2>> GetInterferePoints;

    // ==================== 状态枚举 ====================
    
    public enum FishState
    {
        Idle,       // 常规随机游动
        Fright,     // 正在进入躲避状态（过渡）
        Dodging,    // 受惊吓躲避中
        Recover     // 正在恢复常规状态（过渡）
    }

    // ==================== 私有字段 ====================
    
    // 当前状态
    private FishState _state = FishState.Idle;
    private float _stateTimer = 0f;
    
    // 边界数据
    private List<Vector2> _polygonPoints;
    private List<Rectangle> _rectangles;
    private bool _isPolygonMode;
    private float _gapProtection;
    
    // 位置和速度
    private Vector2 _velocity;
    
    // 游动参数
    private float _maxSpeed = 30f;
    private float _acceleration = 150f;
    private float _damping = 0.94f;
    
    // 随机游动
    private float _noiseTimer;
    private float _noiseSpeed = 0.3f;
    private float _noiseAmplitude = 2.5f;
    
    // 躲避
    private float _fleeRadius = 80f;
    private float _fleeStrength = 400f;
    private float _frightDuration = 0.3f;   // 进入躲避的过渡时间
    private float _recoverDuration = 0.5f;  // 恢复的过渡时间
    private float _dodgeDuration = 1.5f;    // 躲避持续时间
    
    // 边界状态
    private float _boundarySafeZone = 35f;
    private List<GeometryUtils.Line> _edges;
    private List<Vector2> _vertices;
    private Vector2 _center;
    private float _boundsMinX, _boundsMaxX, _boundsMinY, _boundsMaxY;
    
    // 矩形组缓存
    private List<HashSet<Rectangle>> _rectangleGroups;
    private int _currentGroupIndex = -1;
    private float _groupSwitchCooldown = 0f;
    
    // 随机数
    private Random _random;

    // ==================== 构造函数 ====================
    
    /// <summary>
    /// 多边形边界模式
    /// </summary>
    public FishMotion(Vector2 startPosition, List<Vector2> borders, float gapProtection = 12f)
    {
        Initialize(startPosition, borders, null, gapProtection);
        _isPolygonMode = true;
    }
    
    /// <summary>
    /// 矩形边界模式（自动分组）
    /// </summary>
    public FishMotion(Vector2 startPosition, List<Rectangle> borders, float gapProtection = 4f)
    {
        _rectangles = borders;
        _rectangleGroups = GroupRectangles(borders);
        _currentGroupIndex = FindGroupContainingPoint(startPosition);
        
        if (_currentGroupIndex == -1 && _rectangleGroups.Count > 0)
            _currentGroupIndex = 0;
        
        if (_currentGroupIndex == -1)
        {
            _rectangleGroups = new List<HashSet<Rectangle>> { new HashSet<Rectangle> { new Rectangle(-200, -200, 400, 400) } };
            _currentGroupIndex = 0;
        }
        
        var polygon = RectanglesToPolygon(_rectangleGroups[_currentGroupIndex]);
        Initialize(startPosition, polygon, borders, gapProtection);
        _isPolygonMode = false;
    }

    // ==================== 初始化 ====================
    
    private void Initialize(Vector2 startPosition, List<Vector2> polygon, List<Rectangle> rectangles, float gapProtection)
    {
        _random = new Random();
        InitialPosition = startPosition;
        Position = startPosition;
        _velocity = Vector2.Zero;
        _polygonPoints = polygon;
        _rectangles = rectangles;
        _gapProtection = Math.Max(gapProtection, 4f);
        _noiseTimer = (float)_random.NextDouble() * 100f;
        
        BuildBoundaryCache();
        Position = ClampToBoundary(Position);
    }

    // ==================== 边界缓存 ====================
    
    private void BuildBoundaryCache()
    {
        if (_polygonPoints == null || _polygonPoints.Count < 3)
        {
            _polygonPoints = new List<Vector2>
            {
                new Vector2(-100, -100),
                new Vector2(100, -100),
                new Vector2(100, 100),
                new Vector2(-100, 100)
            };
        }
        
        _boundsMinX = _polygonPoints.Min(p => p.X) - _gapProtection;
        _boundsMaxX = _polygonPoints.Max(p => p.X) + _gapProtection;
        _boundsMinY = _polygonPoints.Min(p => p.Y) - _gapProtection;
        _boundsMaxY = _polygonPoints.Max(p => p.Y) + _gapProtection;
        
        _center = GeometryUtils.ComputeCentroid(_polygonPoints.ToArray());
        
        _edges = new List<GeometryUtils.Line>();
        for (int i = 0; i < _polygonPoints.Count; i++)
        {
            int next = (i + 1) % _polygonPoints.Count;
            _edges.Add(new GeometryUtils.Line(_polygonPoints[i], _polygonPoints[next]));
        }
        _vertices = new List<Vector2>(_polygonPoints);
    }
    
    private void RebuildBoundaryCache(List<Vector2> newPolygon)
    {
        _polygonPoints = newPolygon;
        BuildBoundaryCache();
    }

    // ==================== 矩形分组 ====================
    
    private List<HashSet<Rectangle>> GroupRectangles(List<Rectangle> rectangles)
    {
        if (rectangles == null || rectangles.Count == 0)
            return new List<HashSet<Rectangle>>();
        
        var groups = new List<HashSet<Rectangle>>();
        var used = new HashSet<Rectangle>();
        
        foreach (var rect in rectangles)
        {
            if (used.Contains(rect)) continue;
            
            var group = new HashSet<Rectangle> { rect };
            used.Add(rect);
            
            bool added;
            do
            {
                added = false;
                foreach (var other in rectangles)
                {
                    if (used.Contains(other)) continue;
                    foreach (var gr in group.ToList())
                    {
                        if (RectanglesTouch(gr, other, _gapProtection * 2f))
                        {
                            group.Add(other);
                            used.Add(other);
                            added = true;
                            break;
                        }
                    }
                }
            } while (added);
            
            groups.Add(group);
        }
        return groups;
    }
    
    private bool RectanglesTouch(Rectangle a, Rectangle b, float gap)
    {
        int g = (int)Math.Ceiling(gap);
        return new Rectangle(a.X - g, a.Y - g, a.Width + g * 2, a.Height + g * 2).Intersects(b);
    }
    
    private int FindGroupContainingPoint(Vector2 point)
    {
        for (int i = 0; i < _rectangleGroups.Count; i++)
        {
            foreach (var rect in _rectangleGroups[i])
            {
                if (point.X >= rect.X && point.X <= rect.X + rect.Width &&
                    point.Y >= rect.Y && point.Y <= rect.Y + rect.Height)
                    return i;
            }
        }
        return -1;
    }
    
    private List<Vector2> RectanglesToPolygon(HashSet<Rectangle> rects)
    {
        var pts = new List<Vector2>();
        float exp = _gapProtection * 0.3f;
        foreach (var r in rects)
        {
            pts.Add(new Vector2(r.X - exp, r.Y - exp));
            pts.Add(new Vector2(r.X + r.Width + exp, r.Y - exp));
            pts.Add(new Vector2(r.X + r.Width + exp, r.Y + r.Height + exp));
            pts.Add(new Vector2(r.X - exp, r.Y + r.Height + exp));
        }
        return ComputeConvexHull(pts);
    }
    
    private List<Vector2> ComputeConvexHull(List<Vector2> pts)
    {
        if (pts.Count <= 3) return pts.Distinct().ToList();
        pts = pts.Distinct().ToList();
        
        var pivot = pts.OrderBy(p => p.Y).ThenBy(p => p.X).First();
        var sorted = pts.Where(p => p != pivot)
            .OrderBy(p => Math.Atan2(p.Y - pivot.Y, p.X - pivot.X))
            .ThenBy(p => Vector2.Distance(pivot, p)).ToList();
        sorted.Insert(0, pivot);
        
        var hull = new List<Vector2>();
        foreach (var p in sorted)
        {
            while (hull.Count >= 2 && Cross(hull[^2], hull[^1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }
        return hull;
    }
    
    private float Cross(Vector2 o, Vector2 a, Vector2 b) 
        => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

    // ==================== 核心更新 ====================
    
    public override void Update()
    {
        float dt = Engine.DeltaTime;
        if (dt <= 0) return;
        dt = Math.Min(dt, 0.05f);
        
        // 1. 检查矩形组切换
        if (!_isPolygonMode && _rectangleGroups != null && _rectangleGroups.Count > 1)
        {
            CheckGroupSwitch(dt);
        }
        
        // 2. 获取干扰点
        var interferes = GetInterferePoints?.Invoke() ?? new List<Vector2>();
        float dangerLevel = GetDangerLevel(interferes);
        
        // 3. 状态更新
        UpdateState(dt, dangerLevel);
        
        // 4. 计算目标方向
        Vector2 targetDir = CalculateTargetDirection(interferes, dangerLevel);
        
        // 5. 应用运动
        ApplyMovement(dt, targetDir);
        
        // 6. 边界夹紧
        Position = ClampToBoundary(Position);
    }

    // ==================== 状态机 ====================
    
    private void UpdateState(float dt, float dangerLevel)
    {
        _stateTimer += dt;
        
        switch (_state)
        {
            case FishState.Idle:
                if (dangerLevel > 0.3f)
                {
                    _state = FishState.Fright;
                    _stateTimer = 0f;
                }
                break;
                
            case FishState.Fright:
                if (_stateTimer >= _frightDuration)
                {
                    _state = FishState.Dodging;
                    _stateTimer = 0f;
                }
                else if (dangerLevel < 0.1f)
                {
                    // 威胁消失，直接恢复
                    _state = FishState.Recover;
                    _stateTimer = 0f;
                }
                break;
                
            case FishState.Dodging:
                if (_stateTimer >= _dodgeDuration || dangerLevel < 0.1f)
                {
                    _state = FishState.Recover;
                    _stateTimer = 0f;
                }
                break;
                
            case FishState.Recover:
                if (_stateTimer >= _recoverDuration || dangerLevel > 0.5f)
                {
                    _state = dangerLevel > 0.5f ? FishState.Fright : FishState.Idle;
                    _stateTimer = 0f;
                }
                break;
        }
    }
    
    private float GetDangerLevel(List<Vector2> interferes)
    {
        if (interferes == null || interferes.Count == 0)
            return 0f;
        
        float maxDanger = 0f;
        foreach (var p in interferes)
        {
            float dist = Vector2.Distance(Position, p);
            if (dist < _fleeRadius)
            {
                float danger = 1f - dist / _fleeRadius;
                if (danger > maxDanger) maxDanger = danger;
            }
        }
        return maxDanger;
    }

    // ==================== 方向计算 ====================
    
    private Vector2 CalculateTargetDirection(List<Vector2> interferes, float dangerLevel)
    {
        Vector2 dir = Vector2.Zero;
        
        switch (_state)
        {
            case FishState.Idle:
                dir = GetIdleDirection();
                break;
                
            case FishState.Fright:
                // 过渡期：混合游动和躲避
                Vector2 idle = GetIdleDirection();
                Vector2 flee = GetFleeDirection(interferes);
                float mix = Math.Min(_stateTimer / _frightDuration, 1f);
                dir = Vector2.Lerp(idle, flee, mix);
                break;
                
            case FishState.Dodging:
                dir = GetFleeDirection(interferes);
                break;
                
            case FishState.Recover:
                // 恢复期：逐渐回到游动
                Vector2 idle2 = GetIdleDirection();
                Vector2 flee2 = GetFleeDirection(interferes);
                float mix2 = 1f - Math.Min(_stateTimer / _recoverDuration, 1f);
                dir = Vector2.Lerp(flee2, idle2, mix2);
                break;
        }
        
        if (dir.LengthSquared() < 0.01f)
            dir = new Vector2(1, 0);
        dir.Normalize();
        
        // 边界推力微调
        Vector2 boundaryForce = GetBoundaryPushForce();
        if (boundaryForce.LengthSquared() > 0.01f)
        {
            dir = Vector2.Lerp(dir, boundaryForce, 0.2f);
            dir.Normalize();
        }
        
        return dir;
    }
    
    private Vector2 GetIdleDirection()
    {
        _noiseTimer += Engine.DeltaTime * _noiseSpeed;
        float t = _noiseTimer;
        
        // 多频率正弦波生成平滑随机方向
        float angle = 0f;
        angle += (float)Math.Sin(t * 0.25f) * 1.2f;
        angle += (float)Math.Sin(t * 0.6f + 1.3f) * 0.8f;
        angle += (float)Math.Sin(t * 1.1f + 2.7f) * 0.5f;
        angle += (float)Math.Sin(t * 1.8f + 4.2f) * 0.3f;
        
        // 偶尔大转向
        if ((int)(t * 0.3f) % 7 == 0 && (int)(t * 8f) % 5 == 0)
            angle += (float)(_random.NextDouble() - 0.5) * 2.5f;
        
        // 边界附近减小随机幅度
        float amp = IsNearBoundary() ? _noiseAmplitude * 0.4f : _noiseAmplitude;
        
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }
    
    private Vector2 GetFleeDirection(List<Vector2> interferes)
    {
        Vector2 total = Vector2.Zero;
        float totalWeight = 0f;
        
        foreach (var p in interferes)
        {
            Vector2 toP = p - Position;
            float dist = toP.Length();
            if (dist > _fleeRadius || dist < 0.01f) continue;
            
            float strength = 1f - dist / _fleeRadius;
            strength = strength * strength * strength;
            
            Vector2 flee = Position - p;
            flee.Normalize();
            
            // 边界附近调整方向
            if (IsNearBoundary())
            {
                Vector2 bf = GetBoundaryPushForce();
                if (bf.LengthSquared() > 0.01f)
                {
                    flee = Vector2.Lerp(flee, bf, 0.3f);
                    flee.Normalize();
                }
            }
            
            float weight = 1f / (dist + 1f);
            total += flee * strength * weight;
            totalWeight += weight;
        }
        
        if (totalWeight > 0.01f)
            return total / totalWeight;
        return Vector2.Zero;
    }

    // ==================== 运动应用 ====================
    
    private void ApplyMovement(float dt, Vector2 targetDir)
    {
        // 根据状态调整速度和加速度
        float speed = _maxSpeed;
        float accel = _acceleration;
        float damp = _damping;
        
        switch (_state)
        {
            case FishState.Idle:
                // 正常游动
                break;
            case FishState.Fright:
                // 加速进入躲避
                accel *= 1.5f;
                damp *= 0.98f;
                break;
            case FishState.Dodging:
                // 全力躲避
                speed *= 1.3f;
                accel *= 2f;
                damp *= 0.95f;
                break;
            case FishState.Recover:
                // 逐渐恢复
                speed *= 0.8f;
                break;
        }
        
        // 加速度
        _velocity += targetDir * accel * dt;
        
        // 阻尼
        _velocity *= damp;
        
        // 限速
        float len = _velocity.Length();
        if (len > speed)
            _velocity = _velocity / len * speed;
        
        // 边界速度限制
        if (IsNearBoundary())
        {
            _velocity = ClampVelocityToBoundary(_velocity);
        }
        
        // 更新位置
        Position += _velocity * dt;
    }

    // ==================== 边界检测 ====================
    
    private bool IsNearBoundary()
    {
        float totalZone = _boundarySafeZone + _gapProtection;
        float margin = totalZone * 1.5f;
        return !(Position.X > _boundsMinX + margin && Position.X < _boundsMaxX - margin &&
                 Position.Y > _boundsMinY + margin && Position.Y < _boundsMaxY - margin);
    }
    
    private Vector2 GetBoundaryPushForce()
    {
        float totalZone = _boundarySafeZone + _gapProtection;
        float margin = totalZone * 1.5f;
        if (Position.X > _boundsMinX + margin && Position.X < _boundsMaxX - margin &&
            Position.Y > _boundsMinY + margin && Position.Y < _boundsMaxY - margin)
            return Vector2.Zero;
        
        float minDist = float.MaxValue;
        Vector2 normal = Vector2.Zero;
        
        foreach (var edge in _edges)
        {
            float dist = edge.GetDistance(Position);
            if (dist < minDist)
            {
                minDist = dist;
                Vector2 dir = edge.B - edge.A;
                float len = dir.Length();
                if (len > 0.001f)
                {
                    normal = new Vector2(-dir.Y / len, dir.X / len);
                    Vector2 _toCenter = _center - edge.A;
                    if (Vector2.Dot(_toCenter, normal) < 0) normal = -normal;
                }
            }
        }
        
        foreach (var v in _vertices)
        {
            float dist = Vector2.Distance(Position, v);
            if (dist < minDist)
            {
                minDist = dist;
                normal = Position - v;
                if (normal.LengthSquared() > 0.001f)
                    normal.Normalize();
                else
                    normal = _center - v;
                if (normal.LengthSquared() > 0.001f) normal.Normalize();
            }
        }
        
        if (minDist > totalZone * 1.5f || normal.LengthSquared() < 0.01f)
            return Vector2.Zero;
        
        float strength = 1f - minDist / (totalZone * 1.5f);
        strength = MathHelper.Clamp(strength, 0f, 1f);
        strength = strength * strength * (3f - 2f * strength);
        
        Vector2 toCenter = _center - Position;
        if (toCenter.LengthSquared() > 0.01f && Vector2.Dot(normal, toCenter) < 0)
            normal = -normal;
        
        return normal * strength;
    }
    
    private Vector2 ClampVelocityToBoundary(Vector2 vel)
    {
        if (vel.LengthSquared() < 0.01f) return vel;
        Vector2 dir = Vector2.Normalize(vel);
        Vector2 test = Position + dir * 10f;
        
        if (!IsPointInBoundary(test))
        {
            Vector2 push = GetBoundaryPushForce();
            if (push.LengthSquared() > 0.01f)
            {
                Vector2 inward = Vector2.Normalize(push);
                float proj = Vector2.Dot(vel, inward);
                if (proj < 0) return -vel * 0.3f;
                return inward * proj * 0.6f;
            }
            return vel * 0.2f;
        }
        return vel;
    }
    
    private bool IsPointInBoundary(Vector2 point)
    {
        if (point.X < _boundsMinX || point.X > _boundsMaxX ||
            point.Y < _boundsMinY || point.Y > _boundsMaxY)
            return false;
        return GeometryUtils.IsPointInPolygon(point, _vertices);
    }
    
    private Vector2 ClampToBoundary(Vector2 point)
    {
        if (IsPointInBoundary(point)) return point;
        
        Vector2 nearest = point;
        float minDist = float.MaxValue;
        
        foreach (var edge in _edges)
        {
            Vector2 closest = edge.GetFootPointD(point);
            float dist = Vector2.Distance(point, closest);
            if (dist < minDist) { minDist = dist; nearest = closest; }
        }
        foreach (var v in _vertices)
        {
            float dist = Vector2.Distance(point, v);
            if (dist < minDist) { minDist = dist; nearest = v; }
        }
        
        Vector2 toCenter = _center - nearest;
        if (toCenter.LengthSquared() > 0.01f)
        {
            toCenter.Normalize();
            Vector2 test = nearest + toCenter * 4f;
            if (IsPointInBoundary(test)) return test;
        }
        
        for (int i = 0; i < 12; i++)
        {
            float ang = i * MathHelper.Pi / 6f;
            Vector2 dir = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang));
            Vector2 test = nearest + dir * 5f;
            if (IsPointInBoundary(test)) return test;
        }
        
        return _center;
    }

    // ==================== 矩形组切换 ====================
    
    private void CheckGroupSwitch(float dt)
    {
        if (_groupSwitchCooldown > 0)
        {
            _groupSwitchCooldown -= dt;
            return;
        }
        
        bool stillIn = false;
        float expand = _gapProtection * 2f;
        foreach (var rect in _rectangleGroups[_currentGroupIndex])
        {
            var r = new Rectangle(
                rect.X - (int)expand, rect.Y - (int)expand,
                rect.Width + (int)(expand * 2), rect.Height + (int)(expand * 2)
            );
            if (Position.X >= r.X && Position.X <= r.X + r.Width &&
                Position.Y >= r.Y && Position.Y <= r.Y + r.Height)
            { stillIn = true; break; }
        }
        
        if (!stillIn)
        {
            int newGroup = FindGroupContainingPoint(Position);
            if (newGroup != -1 && newGroup != _currentGroupIndex)
            {
                _currentGroupIndex = newGroup;
                var poly = RectanglesToPolygon(_rectangleGroups[newGroup]);
                RebuildBoundaryCache(poly);
                _groupSwitchCooldown = 0.5f;
                Position = ClampToBoundary(Position);
            }
        }
    }

    // ==================== 公共方法 ====================
    
    public FishState GetState() => _state;
}