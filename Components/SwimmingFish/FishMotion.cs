using ChroniaHelper.Cores;

namespace ChroniaHelper.Components.SwimmingFish;

public class FishMotion : BaseComponent
{
    // Position and Speed
    public Vector2 Position { get; set; }
    public Vector2 Speed { get; private set; }
    
    // 边界系统（统一接口）
    private IFishBoundary _boundary;
    private float _gapProtection;
    
    // Motion params
    private float _maxSpeed;
    private float _acceleration;
    private float _turnSpeed;
    private float _inertia;
    private float _boundaryForce;
    
    // Random swimming params
    private float _randomChangeInterval;
    private float _randomTimer;
    
    // Fleeing
    private float _fleeRadius;
    private float _fleeStrength;
    private float _fleeInfluence;
    
    // 原有的矩形边界构造函数（保持兼容）
    public FishMotion(
        Vector2 startPosition,
        Vector2 topLeft,
        Vector2 bottomRight,
        float gapProtection = 4f,
        float maxSpeed = 24f,
        float acceleration = 200f,
        float turnSpeed = 180f,
        float inertia = 0.95f,
        float boundaryForce = 150f,
        float randomChangeInterval = 0.5f,
        float fleeRadius = 120f,
        float fleeStrength = 400f,
        float fleeInfluence = 0.8f)
        : this(startPosition, new RectangleBoundary(topLeft, bottomRight, gapProtection), gapProtection,
               maxSpeed, acceleration, turnSpeed, inertia, boundaryForce, 
               randomChangeInterval, fleeRadius, fleeStrength, fleeInfluence)
    {
    }
    
    // 多边形边界构造函数
    public FishMotion(
        Vector2 startPosition,
        Vector2[] polygonPoints,
        float gapProtection = 4f,
        float maxSpeed = 24f,
        float acceleration = 200f,
        float turnSpeed = 180f,
        float inertia = 0.95f,
        float boundaryForce = 150f,
        float randomChangeInterval = 0.5f,
        float fleeRadius = 120f,
        float fleeStrength = 400f,
        float fleeInfluence = 0.8f)
        : this(startPosition, new PolygonBoundary(polygonPoints, gapProtection), gapProtection,
               maxSpeed, acceleration, turnSpeed, inertia, boundaryForce,
               randomChangeInterval, fleeRadius, fleeStrength, fleeInfluence)
    {
    }
    
    // 复合矩形边界构造函数
    public FishMotion(
        Vector2 startPosition,
        Rectangle[] rectangles,
        float gapProtection = 4f,
        float maxSpeed = 24f,
        float acceleration = 200f,
        float turnSpeed = 180f,
        float inertia = 0.95f,
        float boundaryForce = 150f,
        float randomChangeInterval = 0.5f,
        float fleeRadius = 120f,
        float fleeStrength = 400f,
        float fleeInfluence = 0.8f)
        : this(startPosition, new MultiRectangleBoundary(rectangles, gapProtection), gapProtection,
               maxSpeed, acceleration, turnSpeed, inertia, boundaryForce,
               randomChangeInterval, fleeRadius, fleeStrength, fleeInfluence)
    {
    }
    
    // 核心构造函数（接受任意边界）
    private FishMotion(
        Vector2 startPosition,
        IFishBoundary boundary,
        float gapProtection,
        float maxSpeed,
        float acceleration,
        float turnSpeed,
        float inertia,
        float boundaryForce,
        float randomChangeInterval,
        float fleeRadius,
        float fleeStrength,
        float fleeInfluence)
    {
        Position = startPosition;
        Speed = Vector2.Zero;
        _boundary = boundary;
        _gapProtection = gapProtection;
        _maxSpeed = maxSpeed;
        _acceleration = acceleration;
        _turnSpeed = turnSpeed;
        _inertia = MathHelper.Clamp(inertia, 0f, 1f);
        _boundaryForce = boundaryForce;
        _randomChangeInterval = randomChangeInterval;
        _fleeRadius = fleeRadius;
        _fleeStrength = fleeStrength;
        _fleeInfluence = MathHelper.Clamp(fleeInfluence, 0f, 1f);
        _randomTimer = 0f;
        
        // 确保起始位置在边界内
        Position = _boundary.ClampToBoundary(Position);
    }
    
    public override void Update()
    {
        float deltaTime = Engine.DeltaTime;
        if (deltaTime <= 0) return;
        
        // 获取干扰点（玩家位置）
        Vector2? interfere = null;
        if (PUt.TryGetPlayer(out Player player))
        {
            interfere = player.Center;
        }
        
        // Random swimming
        Vector2 desiredDirection = GetRandomDirection(deltaTime);
        
        // Boundary force（使用统一的边界接口）
        Vector2 boundaryForceDir = _boundary.GetBoundaryForce(Position, 40f);
        desiredDirection += boundaryForceDir * _boundaryForce;
        
        // Fleeing
        if (interfere != null)
        {
            Vector2 fleeDir = GetFleeDirection(interfere.Value, _fleeRadius);
            desiredDirection += fleeDir * _fleeStrength;
        }
        
        // Calculate final direction
        if (desiredDirection.Length() > 0.01f)
            desiredDirection.Normalize();
        
        // Turning
        if (Speed.Length() > 0.01f && desiredDirection.Length() > 0.01f)
        {
            float currentAngle = (float)Math.Atan2(Speed.Y, Speed.X);
            float targetAngle = (float)Math.Atan2(desiredDirection.Y, desiredDirection.X);
            float angleDiff = AngleDifference(currentAngle, targetAngle);
            float maxTurn = _turnSpeed * deltaTime;
            float newAngle = currentAngle + MathHelper.Clamp(angleDiff, -maxTurn, maxTurn);
            desiredDirection = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));
        }
        
        // Change speed
        Vector2 accelerationVec = desiredDirection * _acceleration * deltaTime;
        Speed += accelerationVec;
        
        // Inertia
        Speed *= _inertia;
        
        // Limit speed
        if (Speed.Length() > _maxSpeed)
            Speed = Vector2.Normalize(Speed) * _maxSpeed;
        
        // Update position
        Position += Speed * deltaTime;
        
        // Clamp to boundary（使用统一的夹紧方法）
        Position = _boundary.ClampToBoundary(Position);
    }
    
    private Vector2 GetRandomDirection(float deltaTime)
    {
        _randomTimer += deltaTime;
        
        if (_randomTimer >= _randomChangeInterval)
        {
            _randomTimer = 0f;
            
            float currentAngle = Speed.Length() > 0.01f
                ? (float)Math.Atan2(Speed.Y, Speed.X)
                : 0f;
            
            float angleChange = (float)((Utils.RandomUtils.Random.NextDouble() - 0.5) * Math.PI / 1.5);
            float newAngle = currentAngle + angleChange;
            
            return new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));
        }
        
        if (Speed.Length() > 0.01f)
            return Vector2.Normalize(Speed);
        
        float randomAngle = (float)(Utils.RandomUtils.Random.NextDouble() * Math.PI * 2);
        return new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
    }
    
    private Vector2 GetFleeDirection(Vector2 interfere, float radius)
    {
        Vector2 toInterfere = interfere - Position;
        float distance = toInterfere.Length();
        
        if (distance > radius || distance < 0.01f)
            return Vector2.Zero;
        
        float strength = 1f - (distance / radius);
        Vector2 fleeDir = Position - interfere;
        fleeDir.Normalize();
        
        if (Speed.Length() > 0.01f)
        {
            Vector2 currentDir = Vector2.Normalize(Speed);
            fleeDir = Vector2.Lerp(fleeDir, currentDir, _fleeInfluence);
            fleeDir.Normalize();
        }
        
        return fleeDir * strength;
    }
    
    private float AngleDifference(float from, float to)
    {
        float diff = to - from;
        while (diff > Math.PI) diff -= MathHelper.TwoPi;
        while (diff < -Math.PI) diff += MathHelper.TwoPi;
        return diff;
    }
}