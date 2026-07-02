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
        Fright,     // 正在进入躲避状态
        Dodging,    // 受惊吓躲避中
        Recover     // 正在恢复常规状态
    }

    // ==================== 私有字段 ====================
    
    // 当前状态
    private FishState _state = FishState.Idle;
    private float _stateTimer = 0f;
    
    // ========== 点集边界（核心） ==========
    private HashSet<(int x, int y)> _boundaryPoints;
    private int _minX, _maxX, _minY, _maxY;
    private Vector2 _center;
    
    // 位置和速度
    private Vector2 _velocity;
    
    // 游动参数
    private float _maxSpeed = 30f;
    private float _acceleration = 250f;
    private float _damping = 0.96f;
    
    // ========== 动态正弦波（替代原有噪声） ==========
    private DynamicSineWave _xWave;   // X方向运动
    private DynamicSineWave _yWave;   // Y方向运动
    
    // 动态波形参数范围
    private (float Min, float Max) _ampRange = (0.8f, 2.5f);
    private (float Min, float Max) _freqRange = (0.8f, 1.2f);
    private (float Min, float Max) _phaseRange = (0f, 2 * Single.Pi);
    
    // 躲避
    private float _fleeRadius = 24f;
    private float _fleeStrength = 400f;
    private float _frightDuration = 0.3f;
    private float _recoverDuration = 0.5f;
    private float _dodgeDuration = 1.5f;
    
    // 边界安全距离
    private float _gapProtection = 2f;
    private float _boundarySafeZone = 4f;
    
    // 中心吸引力
    private float _centerAttractionStrength = 0.3f;
    private float _centerAttractionRadius = 60f;

    // ==================== 构造函数 ====================
    
    public FishMotion(Vector2 startPosition, List<Vector2> borders, float gapProtection = 2f)
    {
        _gapProtection = Math.Max(gapProtection, 0f);
        InitialPosition = startPosition;
        Position = startPosition;
        _velocity = Vector2.Zero;
        
        // 初始化动态波形
        InitializeWaves();
        
        BuildBoundaryPointSet(borders);
        Position = ClampToBoundary(Position);
    }
    
    public FishMotion(Vector2 startPosition, List<Rectangle> borders, float gapProtection = 2f)
    {
        _gapProtection = Math.Max(gapProtection, 0f);
        InitialPosition = startPosition;
        Position = startPosition;
        _velocity = Vector2.Zero;
        
        // 初始化动态波形
        InitializeWaves();
        
        var points = RectanglesToPoints(borders);
        BuildBoundaryPointSet(points);
        Position = ClampToBoundary(Position);
    }

    // ==================== 动态波形初始化 ====================
    
    private void InitializeWaves()
    {
        // X方向波形
        _xWave = new DynamicSineWave(
            ampRange: _ampRange,
            freqRange: _freqRange,
            phaseRange: _phaseRange,
            useSystemTime: false
        );
        
        // Y方向波形（独立参数，产生不同的运动模式）
        _yWave = new DynamicSineWave(
            ampRange: _ampRange,
            freqRange: _freqRange,
            phaseRange: _phaseRange,
            useSystemTime: false
        );
    }

    // ==================== 几何工具函数（自包含） ====================
    
    private float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ap = point - a;
        Vector2 ab = b - a;
        float abSq = ab.X * ab.X + ab.Y * ab.Y;
        
        if (abSq < 0.0001f)
            return Vector2.Distance(point, a);
        
        float t = (ap.X * ab.X + ap.Y * ab.Y) / abSq;
        t = MathHelper.Clamp(t, 0f, 1f);
        
        Vector2 closest = a + ab * t;
        return Vector2.Distance(point, closest);
    }
    
    private bool IsPointInPolygon(Vector2 p, List<Vector2> polygon)
    {
        int n = polygon.Count;
        bool inside = false;
        
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 vi = polygon[i];
            Vector2 vj = polygon[j];
            
            if (IsPointOnSegment(p, vi, vj))
                return true;
            
            if ((vi.Y > p.Y) != (vj.Y > p.Y))
            {
                float intersectX = vi.X + (p.Y - vi.Y) * (vj.X - vi.X) / (vj.Y - vi.Y);
                if (p.X < intersectX)
                    inside = !inside;
            }
        }
        
        return inside;
    }
    
    private bool IsPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float cross = ab.X * ap.Y - ab.Y * ap.X;
        if (Math.Abs(cross) > 0.001f)
            return false;
        
        float dot = ap.X * ab.X + ap.Y * ab.Y;
        if (dot < 0) return false;
        
        float abSq = ab.X * ab.X + ab.Y * ab.Y;
        return dot <= abSq;
    }

    // ==================== 核心：点集边界构建 ====================
    
    private void BuildBoundaryPointSet(List<Vector2> polygon)
    {
        if (polygon == null || polygon.Count == 0)
        {
            polygon = new List<Vector2>
            {
                new Vector2(-100, -100),
                new Vector2(100, -100),
                new Vector2(100, 100),
                new Vector2(-100, 100)
            };
        }
        
        if (polygon.Count == 1)
        {
            BuildCircleBoundary(polygon[0], _gapProtection);
            return;
        }
        
        if (polygon.Count == 2)
        {
            BuildCapsuleBoundary(polygon[0], polygon[1], _gapProtection);
            return;
        }
        
        _minX = (int)Math.Floor(polygon.Min(p => p.X) - _gapProtection);
        _maxX = (int)Math.Ceiling(polygon.Max(p => p.X) + _gapProtection);
        _minY = (int)Math.Floor(polygon.Min(p => p.Y) - _gapProtection);
        _maxY = (int)Math.Ceiling(polygon.Max(p => p.Y) + _gapProtection);
        
        _center = ComputeCentroid(polygon);
        
        _boundaryPoints = new HashSet<(int x, int y)>();
        
        for (int x = _minX; x <= _maxX; x++)
        {
            for (int y = _minY; y <= _maxY; y++)
            {
                Vector2 point = new Vector2(x, y);
                
                if (IsPointInPolygon(point, polygon))
                {
                    _boundaryPoints.Add((x, y));
                    continue;
                }
                
                if (IsPointWithinGap(point, polygon))
                {
                    _boundaryPoints.Add((x, y));
                }
            }
        }
    }
    
    private void BuildCircleBoundary(Vector2 center, float radius)
    {
        int r = (int)Math.Ceiling(radius);
        _minX = (int)Math.Floor(center.X - r);
        _maxX = (int)Math.Ceiling(center.X + r);
        _minY = (int)Math.Floor(center.Y - r);
        _maxY = (int)Math.Ceiling(center.Y + r);
        _center = center;
        
        _boundaryPoints = new HashSet<(int x, int y)>();
        float rSq = radius * radius;
        
        for (int x = _minX; x <= _maxX; x++)
        {
            for (int y = _minY; y <= _maxY; y++)
            {
                float dx = x - center.X;
                float dy = y - center.Y;
                if (dx * dx + dy * dy <= rSq)
                {
                    _boundaryPoints.Add((x, y));
                }
            }
        }
    }
    
    private void BuildCapsuleBoundary(Vector2 a, Vector2 b, float radius)
    {
        _center = (a + b) / 2f;
        float r = radius;
        
        _minX = (int)Math.Floor(Math.Min(a.X, b.X) - r);
        _maxX = (int)Math.Ceiling(Math.Max(a.X, b.X) + r);
        _minY = (int)Math.Floor(Math.Min(a.Y, b.Y) - r);
        _maxY = (int)Math.Ceiling(Math.Max(a.Y, b.Y) + r);
        
        _boundaryPoints = new HashSet<(int x, int y)>();
        float rSq = r * r;
        
        for (int x = _minX; x <= _maxX; x++)
        {
            for (int y = _minY; y <= _maxY; y++)
            {
                Vector2 p = new Vector2(x, y);
                float dist = DistanceToSegment(p, a, b);
                if (dist <= r)
                {
                    _boundaryPoints.Add((x, y));
                }
            }
        }
    }
    
    private bool IsPointWithinGap(Vector2 point, List<Vector2> polygon)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            int next = (i + 1) % polygon.Count;
            if (DistanceToSegment(point, polygon[i], polygon[next]) <= _gapProtection)
                return true;
        }
        
        foreach (var v in polygon)
        {
            if (Vector2.Distance(point, v) <= _gapProtection)
                return true;
        }
        
        return false;
    }
    
    private Vector2 ComputeCentroid(List<Vector2> points)
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
    
    private List<Vector2> RectanglesToPoints(List<Rectangle> rectangles)
    {
        if (rectangles == null || rectangles.Count == 0)
        {
            return new List<Vector2>
            {
                new Vector2(-100, -100),
                new Vector2(100, -100),
                new Vector2(100, 100),
                new Vector2(-100, 100)
            };
        }
        
        if (rectangles.Count == 1)
        {
            var r = rectangles[0];
            float _exp = _gapProtection * 0.3f;
            return new List<Vector2>
            {
                new Vector2(r.X - _exp, r.Y - _exp),
                new Vector2(r.X + r.Width + _exp, r.Y - _exp),
                new Vector2(r.X + r.Width + _exp, r.Y + r.Height + _exp),
                new Vector2(r.X - _exp, r.Y + r.Height + _exp)
            };
        }
        
        var allPoints = new List<Vector2>();
        float exp = _gapProtection * 0.3f;
        foreach (var r in rectangles)
        {
            allPoints.Add(new Vector2(r.X - exp, r.Y - exp));
            allPoints.Add(new Vector2(r.X + r.Width + exp, r.Y - exp));
            allPoints.Add(new Vector2(r.X + r.Width + exp, r.Y + r.Height + exp));
            allPoints.Add(new Vector2(r.X - exp, r.Y + r.Height + exp));
        }
        
        return ComputeConvexHull(allPoints);
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
            while (hull.Count >= 2)
            {
                Vector2 a = hull[hull.Count - 2];
                Vector2 b = hull[hull.Count - 1];
                float cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
                if (cross > 0) break;
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(p);
        }
        return hull;
    }

    // ==================== 点集边界查询 ====================
    
    private bool IsPointInBoundary(Vector2 point)
    {
        int x = (int)Math.Round(point.X);
        int y = (int)Math.Round(point.Y);
        
        if (x < _minX || x > _maxX || y < _minY || y > _maxY)
            return false;
        
        return _boundaryPoints.Contains((x, y));
    }
    
    private Vector2 ClampToBoundary(Vector2 point)
    {
        if (IsPointInBoundary(point))
            return point;
        
        int startX = (int)Math.Round(point.X);
        int startY = (int)Math.Round(point.Y);
        
        for (int radius = 1; radius <= 30; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                        continue;
                    
                    int testX = startX + dx;
                    int testY = startY + dy;
                    
                    if (testX < _minX || testX > _maxX || testY < _minY || testY > _maxY)
                        continue;
                    
                    if (_boundaryPoints.Contains((testX, testY)))
                    {
                        return new Vector2(testX, testY);
                    }
                }
            }
        }
        
        return _center;
    }
    
    private bool IsNearBoundary()
    {
        int x = (int)Math.Round(Position.X);
        int y = (int)Math.Round(Position.Y);
        
        int checkRadius = (int)Math.Ceiling(_boundarySafeZone + _gapProtection);
        
        for (int dx = -checkRadius; dx <= checkRadius; dx++)
        {
            for (int dy = -checkRadius; dy <= checkRadius; dy++)
            {
                int tx = x + dx;
                int ty = y + dy;
                if (tx < _minX || tx > _maxX || ty < _minY || ty > _maxY)
                    return true;
                
                if (!_boundaryPoints.Contains((tx, ty)))
                {
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist < _boundarySafeZone + _gapProtection)
                        return true;
                }
            }
        }
        
        return false;
    }
    
    private Vector2 GetBoundaryPushForce()
    {
        int x = (int)Math.Round(Position.X);
        int y = (int)Math.Round(Position.Y);
        
        if (!_boundaryPoints.Contains((x, y)))
        {
            Vector2 toCenter = _center - Position;
            if (toCenter.LengthSquared() > 0.01f)
                toCenter.Normalize();
            return toCenter;
        }
        
        int checkRadius = (int)Math.Ceiling(_boundarySafeZone + _gapProtection);
        Vector2 pushForce = Vector2.Zero;
        int count = 0;
        
        for (int dx = -checkRadius; dx <= checkRadius; dx++)
        {
            for (int dy = -checkRadius; dy <= checkRadius; dy++)
            {
                int tx = x + dx;
                int ty = y + dy;
                if (tx < _minX || tx > _maxX || ty < _minY || ty > _maxY)
                {
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist < 0.01f) continue;
                    
                    float strength = 1f - dist / checkRadius;
                    strength = Math.Max(strength, 0f);
                    
                    Vector2 dir = new Vector2(-dx / dist, -dy / dist);
                    pushForce += dir * strength;
                    count++;
                }
                else if (!_boundaryPoints.Contains((tx, ty)))
                {
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist < 0.01f) continue;
                    
                    float strength = 1f - dist / checkRadius;
                    strength = Math.Max(strength, 0f);
                    
                    Vector2 dir = new Vector2(-dx / dist, -dy / dist);
                    pushForce += dir * strength;
                    count++;
                }
            }
        }
        
        if (count > 0)
        {
            pushForce /= count;
            if (pushForce.LengthSquared() > 0.01f)
            {
                pushForce.Normalize();
                return pushForce;
            }
        }
        
        Vector2 toCenter2 = _center - Position;
        if (toCenter2.LengthSquared() > 0.01f)
        {
            toCenter2.Normalize();
            return toCenter2;
        }
        
        return Vector2.Zero;
    }

    // ==================== 中心吸引力 ====================
    
    private Vector2 GetCenterAttraction()
    {
        Vector2 toCenter = _center - Position;
        float dist = toCenter.Length();
        
        if (dist < 0.01f || dist > _centerAttractionRadius)
            return Vector2.Zero;
        
        // 距离越远吸引力越大（平方曲线）
        float normalizedDist = dist / _centerAttractionRadius;
        float strength = normalizedDist * normalizedDist * _centerAttractionStrength;
        
        return Vector2.Normalize(toCenter) * strength;
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

    // ==================== 方向计算（使用动态波形） ====================
    
    private Vector2 CalculateTargetDirection(List<Vector2> interferes, float dangerLevel)
    {
        Vector2 dir = Vector2.Zero;
        
        // 更新动态波形
        _xWave.Update();
        _yWave.Update();
        
        // 从动态波形获取方向
        float waveX = (float)_xWave.GetValue();
        float waveY = (float)_yWave.GetValue();
        
        // 边界附近减小波形幅度
        float ampScale = IsNearBoundary() ? 0.5f : 1f;
        Vector2 waveDir = new Vector2(waveX * ampScale, waveY * ampScale);
        
        // 如果波形方向太小，使用随机方向兜底
        if (waveDir.LengthSquared() < 0.01f)
        {
            float angle = (float)(Rd.Random.NextDouble() * Math.PI * 2);
            waveDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
        waveDir.Normalize();
        
        // 计算其他力
        Vector2 fleeDir = GetFleeDirection(interferes);
        Vector2 boundaryForce = GetBoundaryPushForce();
        Vector2 centerAttraction = GetCenterAttraction();
        
        bool nearBoundary = IsNearBoundary();
        
        switch (_state)
        {
            case FishState.Idle:
                dir = waveDir;
                
                // 中心吸引力微调
                if (centerAttraction.LengthSquared() > 0.01f)
                {
                    float blend = nearBoundary ? 0.4f : 0.1f;
                    dir = Vector2.Lerp(dir, centerAttraction, blend);
                }
                break;
                
            case FishState.Fright:
                float mix = Math.Min(_stateTimer / _frightDuration, 1f);
                dir = Vector2.Lerp(waveDir, fleeDir, mix);
                
                if (centerAttraction.LengthSquared() > 0.01f)
                {
                    float blend = nearBoundary ? 0.2f : 0.05f;
                    dir = Vector2.Lerp(dir, centerAttraction, blend);
                }
                break;
                
            case FishState.Dodging:
                dir = fleeDir;
                break;
                
            case FishState.Recover:
                float mix2 = 1f - Math.Min(_stateTimer / _recoverDuration, 1f);
                dir = Vector2.Lerp(fleeDir, waveDir, mix2);
                
                if (centerAttraction.LengthSquared() > 0.01f)
                {
                    float blend = nearBoundary ? 0.3f : 0.08f;
                    dir = Vector2.Lerp(dir, centerAttraction, blend);
                }
                break;
        }
        
        if (dir.LengthSquared() < 0.01f)
            dir = new Vector2(1, 0);
        dir.Normalize();
        
        // 边界推力作为最后保障
        if (boundaryForce.LengthSquared() > 0.01f && nearBoundary)
        {
            dir = Vector2.Lerp(dir, boundaryForce, 0.3f);
            dir.Normalize();
        }
        
        return dir;
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
            
            if (IsNearBoundary())
            {
                Vector2 bf = GetBoundaryPushForce();
                if (bf.LengthSquared() > 0.01f)
                {
                    flee = Vector2.Lerp(flee, bf, 0.2f);
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
        float speed = _maxSpeed;
        float accel = _acceleration;
        float damp = _damping;
        
        switch (_state)
        {
            case FishState.Idle:
                break;
            case FishState.Fright:
                accel *= 1.5f;
                damp *= 0.98f;
                break;
            case FishState.Dodging:
                speed *= 1.3f;
                accel *= 2f;
                damp *= 0.95f;
                break;
            case FishState.Recover:
                speed *= 0.8f;
                break;
        }
        
        _velocity += targetDir * accel * dt;
        _velocity *= damp;
        
        float len = _velocity.Length();
        if (len > speed)
            _velocity = _velocity / len * speed;
        
        Vector2 newPos = Position + _velocity * dt;
        if (!IsPointInBoundary(newPos))
        {
            Vector2 boundaryForce = GetBoundaryPushForce();
            if (boundaryForce.LengthSquared() > 0.01f)
            {
                float proj = _velocity.X * boundaryForce.X + _velocity.Y * boundaryForce.Y;
                if (proj < 0)
                {
                    _velocity = boundaryForce * Math.Abs(proj) * 0.3f;
                }
            }
            else
            {
                _velocity *= 0.3f;
            }
        }
        
        Position += _velocity * dt;
        Position = ClampToBoundary(Position);
    }

    // ==================== 公共方法 ====================
    
    public override void Update()
    {
        float dt = Engine.DeltaTime;
        if (dt <= 0) return;
        dt = Math.Min(dt, 0.05f);
        
        var interferes = GetInterferePoints?.Invoke() ?? new List<Vector2>();
        float dangerLevel = GetDangerLevel(interferes);
        
        UpdateState(dt, dangerLevel);
        Vector2 targetDir = CalculateTargetDirection(interferes, dangerLevel);
        ApplyMovement(dt, targetDir);
        Position = ClampToBoundary(Position);
    }
    
    public FishState GetState() => _state;
    public int GetBoundaryPointCount() => _boundaryPoints?.Count ?? 0;
    
    /// <summary>
    /// 设置动态波形参数范围
    /// </summary>
    public void SetWaveParameters(
        (float Min, float Max)? ampRange = null,
        (float Min, float Max)? freqRange = null,
        (float Min, float Max)? phaseRange = null)
    {
        if (ampRange.HasValue)
            _ampRange = ampRange.Value;
        if (freqRange.HasValue)
            _freqRange = freqRange.Value;
        if (phaseRange.HasValue)
            _phaseRange = phaseRange.Value;
        
        // 重新初始化波形
        InitializeWaves();
    }
    
    /// <summary>
    /// 设置中心吸引力参数
    /// </summary>
    public void SetCenterAttraction(float strength, float radius)
    {
        _centerAttractionStrength = MathHelper.Clamp(strength, 0f, 1f);
        _centerAttractionRadius = Math.Max(radius, 1f);
    }
}