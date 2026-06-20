using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities.SwimmingFish;

public class FishMotion : BaseComponent
{
    // Position and Speed

    public Vector2 Position { get; set; }
    public Vector2 Speed { get; private set; }

    // Border area

    private Vector2 _topLeft;
    private Vector2 _bottomRight;

    // Motion params

    private float _maxSpeed;           
    private float _acceleration;
    /// <summary>
    /// Defines how sharp the turning could be
    /// </summary>
    private float _turnSpeed;
    /// <summary>
    /// The speed kept from original speed under affection
    /// </summary>
    private float _inertia;
    private float _boundaryForce;

    // Random swimming params

    private float _randomChangeInterval;
    private float _randomTimer;

    // Fleeing

    private float _fleeRadius;
    /// <summary>
    /// How sharp the fleeing could affect the fish turning
    /// </summary>
    private float _fleeStrength;
    /// <summary>
    /// The parameters that defines the flee affection strength
    /// </summary>
    private float _fleeInfluence;

    public FishMotion(
        Vector2 startPosition,
        Vector2 topLeft,
        Vector2 bottomRight,
        float maxSpeed = 24f,
        float acceleration = 200f,
        float turnSpeed = 180f,
        float inertia = 0.95f,
        float boundaryForce = 150f,
        float randomChangeInterval = 0.5f,
        float fleeRadius = 120f,
        float fleeStrength = 400f,
        float fleeInfluence = 0.8f)
    {
        Position = startPosition;
        Speed = Vector2.Zero;
        _topLeft = topLeft;
        _bottomRight = bottomRight;
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
    }

    public override void Update()
    {
        float deltaTime = Engine.DeltaTime;
        if (deltaTime <= 0) return;

        Vector2? interfere = null;
        if (PUt.TryGetPlayer(out Player player))
        {
            interfere = player.Center;
        }

        // Random swimming
        Vector2 desiredDirection = GetRandomDirection(deltaTime);

        // Avoiding border areas
        Vector2 boundaryForceDir = GetBoundaryForce();
        desiredDirection += boundaryForceDir * _boundaryForce;

        // If possible, avoiding interferes
        if (interfere != null)
        {
            float radius = _fleeRadius;
            Vector2 fleeDir = GetFleeDirection(interfere.Value, radius);
            desiredDirection += fleeDir * _fleeStrength;
        }

        // Calculate final direction
        if (desiredDirection.Length() > 0.01f)
            desiredDirection.Normalize();

        // Turning towards the target directions
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

        // Calculate inertia
        Speed *= _inertia;

        // Do remind to limit the maximum speed
        if (Speed.Length() > _maxSpeed)
            Speed = Vector2.Normalize(Speed) * _maxSpeed;

        // Then update the position
        Position += Speed * deltaTime;

        // Restrain the boundaries again
        Position = ClampToBounds(Position);
    }

    /// <summary>
    /// Get random sirection for swimming
    /// </summary>
    private Vector2 GetRandomDirection(float deltaTime)
    {
        _randomTimer += deltaTime;

        if (_randomTimer >= _randomChangeInterval)
        {
            _randomTimer = 0f;

            // Generate random swimming angle changes
            float currentAngle = Speed.Length() > 0.01f
                ? (float)Math.Atan2(Speed.Y, Speed.X)
                : 0f;

            // Angle changes: -60 to 60 degrees
            float angleChange = (float)((Utils.RandomUtils.Random.NextDouble() - 0.5) * Math.PI / 1.5);
            float newAngle = currentAngle + angleChange;

            return new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));
        }

        // If no changes, keep the current speed
        if (Speed.Length() > 0.01f)
            return Vector2.Normalize(Speed);

        // If the speed is back to zero, change another direction
        float randomAngle = (float)(Utils.RandomUtils.Random.NextDouble() * Math.PI * 2);
        return new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
    }

    private Vector2 GetBoundaryForce()
    {
        Vector2 force = Vector2.Zero;
        float marginX = Math.Min(Position.X - _topLeft.X, _bottomRight.X - Position.X);
        float marginY = Math.Min(Position.Y - _topLeft.Y, _bottomRight.Y - Position.Y);

        float safeZone = 40f;

        if (marginX < safeZone)
        {
            float strength = (1f - marginX / safeZone);
            if (Position.X < (_topLeft.X + _bottomRight.X) / 2)
                force.X = -strength;
            else
                force.X = strength;
        }

        if (marginY < safeZone)
        {
            float strength = (1f - marginY / safeZone);
            if (Position.Y < (_topLeft.Y + _bottomRight.Y) / 2)
                force.Y = -strength;
            else
                force.Y = strength;
        }

        return force;
    }

    private Vector2 GetFleeDirection(Vector2 interfere, float radius)
    {
        Vector2 toInterfere = interfere - Position;
        float distance = toInterfere.Length();

        if (distance > radius || distance < 0.01f)
            return Vector2.Zero;

        // The flee force should be bigger when interfere approaches
        float strength = 1f - (distance / radius);
        Vector2 fleeDir = Position - interfere;
        fleeDir.Normalize();

        // Mix the current speed with it so the dodging seems more natural
        if (Speed.Length() > 0.01f)
        {
            Vector2 currentDir = Vector2.Normalize(Speed);
            fleeDir = Vector2.Lerp(fleeDir, currentDir, _fleeInfluence);
            fleeDir.Normalize();
        }

        return fleeDir * strength;
    }

    private Vector2 ClampToBounds(Vector2 pos)
    {
        pos.X = MathHelper.Clamp(pos.X, _topLeft.X, _bottomRight.X);
        pos.Y = MathHelper.Clamp(pos.Y, _topLeft.Y, _bottomRight.Y);
        return pos;
    }

    private float AngleDifference(float from, float to)
    {
        float diff = to - from;
        while (diff > Math.PI) diff -= MathHelper.TwoPi;
        while (diff < -Math.PI) diff += MathHelper.TwoPi;
        return diff;
    }

    /// <summary>
    /// Change the swimming zones
    /// </summary>
    public void SetBounds(Vector2 topLeft, Vector2 bottomRight)
    {
        _topLeft = topLeft;
        _bottomRight = bottomRight;
        Position = ClampToBounds(Position);
    }

    public bool IsWithinBounds(Vector2 point)
    {
        return point.X >= _topLeft.X && point.X <= _bottomRight.X &&
               point.Y >= _topLeft.Y && point.Y <= _bottomRight.Y;
    }
}
