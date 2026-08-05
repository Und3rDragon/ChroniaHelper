using ChroniaHelper.Components.SwimmingFish;
using ChroniaHelper.Cores;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace ChroniaHelper.Components.SwimmingFish;

public class FishMotion : BaseComponent
{
    // 用户输入参数
    public List<Rectangle> Waters { get; set; }
    public List<Vector2> InterferePoints { get; set; }
    public int GroupTag { get; private set; }
    public bool IgnoreWater { get; set; } = false;
    public Rectangle RoomBounds { get; set; }

    // 可读取属性
    public Vector2 Position { get; set; }
    public Vector2 Speed { get; private set; }
    public bool Valid { get; private set; } = true;

    // 内部状态
    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private float changeTargetCooldown = 0f;
    private FishMotionManager manager;

    // 可调节参数
    public float TargetReachedThreshold { get; set; } = 4f;
    public float MinSpeed { get; set; } = 30f;
    public float MaxSpeed { get; set; } = 80f;
    public float ChangeTargetInterval { get; set; } = 2f;
    public float InterfereRadius { get; set; } = 120f;
    public float AvoidStrength { get; set; } = 3f;
    public float LeaderFollowDistance { get; set; } = 50f;
    public float LeaderFollowStrength { get; set; } = 0.5f;
    public float SeparationDistance { get; set; } = 30f;
    public float SeparationStrength { get; set; } = 0.3f;

    private Random random = new Random();

    public FishMotion()
        : base(active: true, visible: false)
    {
        Waters = new List<Rectangle>();
        InterferePoints = new List<Vector2>();
        Speed = Vector2.Zero;
    }

    public FishMotion(
        Vector2 initialPosition,
        FishMotionManager manager,
        List<Rectangle> waters = null,
        List<Vector2> interferePoints = null,
        int groupTag = -1,
        bool ignoreWater = false,
        Rectangle roomBounds = default)
        : this()
    {
        this.initialPosition = initialPosition;
        Position = initialPosition;
        this.manager = manager;
        Waters = waters ?? new List<Rectangle>();
        InterferePoints = interferePoints ?? new List<Vector2>();
        GroupTag = groupTag;
        IgnoreWater = ignoreWater;
        RoomBounds = roomBounds;

        // 注册到管理器
        manager?.Register(this);

        // 验证初始位置
        if (!IgnoreWater && !IsInWaters(Position))
        {
            Valid = false;
        }
        else
        {
            PickNewTarget();
        }
    }

    /// <summary>
    /// 设置管理器（如果在构造时无法提供）
    /// </summary>
    public void SetManager(FishMotionManager newManager)
    {
        if (manager != null)
        {
            manager.Unregister(this);
        }
        manager = newManager;
        manager?.Register(this);
    }

    /// <summary>
    /// 更新分组标签
    /// </summary>
    public void SetGroupTag(int newGroupTag)
    {
        if (GroupTag != newGroupTag)
        {
            int oldTag = GroupTag;
            GroupTag = newGroupTag;
            manager?.UpdateGroupTag(this, newGroupTag);
        }
    }

    /// <summary>
    /// 从管理器中移除并失效
    /// </summary>
    public void Remove()
    {
        manager?.Unregister(this);
        Valid = false;
    }

    public override void Update()
    {
        if (!Valid)
            return;

        // 检查是否在有效水域内
        if (!IsInWaters(Position))
        {
            Valid = false;
            Speed = Vector2.Zero;
            return;
        }

        Vector2 desiredVelocity = Vector2.Zero;

        // 1. 向目标点移动
        Vector2 toTarget = targetPosition - Position;
        if (toTarget.Length() > TargetReachedThreshold)
        {
            if (toTarget != Vector2.Zero)
                desiredVelocity += Vector2.Normalize(toTarget) * MathHelper.Lerp(MinSpeed, MaxSpeed, 0.5f);
        }
        else
        {
            PickNewTarget();
        }

        // 2. 避开干扰点
        desiredVelocity += CalculateAvoidanceFromInterferePoints();

        // 3. 鱼群行为
        if (GroupTag >= 0 && manager != null)
        {
            desiredVelocity += CalculateFlockingBehavior();
        }

        // 4. 确保目标点在水域内
        if (!IsInWaters(targetPosition))
        {
            PickNewTarget();
        }

        // 平滑速度变化
        float currentSpeed = Speed.Length();
        float targetSpeed = desiredVelocity.Length();
        float newSpeed = MathHelper.Lerp(currentSpeed, MathHelper.Clamp(targetSpeed, MinSpeed, MaxSpeed), 0.1f);

        if (desiredVelocity != Vector2.Zero)
        {
            Speed = Vector2.Normalize(desiredVelocity) * newSpeed;
        }
        else
        {
            Speed *= 0.95f;
        }

        // 更新位置
        Vector2 newPosition = Position + Speed * Engine.DeltaTime;

        // 边界检查
        if (!IsInWaters(newPosition))
        {
            newPosition = ClampToNearestWater(newPosition);
            if (!IsInWaters(newPosition))
            {
                Valid = false;
                Speed = Vector2.Zero;
                return;
            }
            PickNewTarget();
        }

        Position = newPosition;

        // 更新目标切换计时器
        changeTargetCooldown -= Engine.DeltaTime;
        if (changeTargetCooldown <= 0)
        {
            if (Vector2.Distance(Position, targetPosition) < TargetReachedThreshold)
            {
                PickNewTarget();
            }
        }
    }

    private bool IsInWaters(Vector2 point)
    {
        if (IgnoreWater)
            return RoomBounds.Contains((int)point.X, (int)point.Y);

        if (Waters == null || Waters.Count == 0)
            return false;

        foreach (var water in Waters)
        {
            if (water.Contains((int)point.X, (int)point.Y))
                return true;
        }
        return false;
    }

    private Vector2 GetRandomPointInWaters()
    {
        if (IgnoreWater)
        {
            return new Vector2(
                RoomBounds.X + (float)(random.NextDouble() * RoomBounds.Width),
                RoomBounds.Y + (float)(random.NextDouble() * RoomBounds.Height)
            );
        }

        if (Waters == null || Waters.Count == 0)
            return Position;

        var chosenWater = Waters[random.Next(Waters.Count)];
        return new Vector2(
            chosenWater.X + (float)(random.NextDouble() * chosenWater.Width),
            chosenWater.Y + (float)(random.NextDouble() * chosenWater.Height)
        );
    }

    private void PickNewTarget()
    {
        targetPosition = GetRandomPointInWaters();
        changeTargetCooldown = ChangeTargetInterval;
    }

    private Vector2 CalculateAvoidanceFromInterferePoints()
    {
        Vector2 avoidance = Vector2.Zero;

        if (InterferePoints == null || InterferePoints.Count == 0)
            return avoidance;

        foreach (var point in InterferePoints)
        {
            float distance = Vector2.Distance(Position, point);
            if (distance < InterfereRadius)
            {
                Vector2 awayFromPoint = Position - point;
                if (awayFromPoint != Vector2.Zero)
                {
                    float strength = (1f - distance / InterfereRadius) * AvoidStrength;
                    avoidance += Vector2.Normalize(awayFromPoint) * strength * MaxSpeed;
                }
                else
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2);
                    avoidance += new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MaxSpeed;
                }
            }
        }

        return avoidance;
    }

    private Vector2 CalculateFlockingBehavior()
    {
        Vector2 flockingForce = Vector2.Zero;

        if (manager == null)
            return flockingForce;

        // 通过管理器获取同组成员
        var sameGroupFish = manager.GetGroupMembers(this);

        // 只保留有效的鱼
        sameGroupFish.RemoveAll(f => !f.Valid);

        if (sameGroupFish.Count == 0)
            return flockingForce;

        // 随机选择一个Leader
        var leader = sameGroupFish[random.Next(sameGroupFish.Count)];

        // 跟随Leader
        Vector2 toLeader = leader.Position - Position;
        float distToLeader = toLeader.Length();

        if (distToLeader > LeaderFollowDistance)
        {
            if (toLeader != Vector2.Zero)
            {
                flockingForce += Vector2.Normalize(toLeader) * LeaderFollowStrength * MaxSpeed;
            }
        }

        // 与其他鱼保持分离
        foreach (var other in sameGroupFish)
        {
            float dist = Vector2.Distance(Position, other.Position);
            if (dist < SeparationDistance && dist > 0)
            {
                Vector2 awayFromOther = Position - other.Position;
                flockingForce += Vector2.Normalize(awayFromOther) * SeparationStrength * MaxSpeed * (1f - dist / SeparationDistance);
            }
        }

        return flockingForce;
    }

    private Vector2 ClampToNearestWater(Vector2 point)
    {
        if (IgnoreWater)
        {
            return new Vector2(
                MathHelper.Clamp(point.X, RoomBounds.Left, RoomBounds.Right),
                MathHelper.Clamp(point.Y, RoomBounds.Top, RoomBounds.Bottom)
            );
        }

        if (Waters == null || Waters.Count == 0)
            return Position;

        Vector2 bestPoint = Position;
        float bestDistance = float.MaxValue;

        foreach (var water in Waters)
        {
            Vector2 clamped = new Vector2(
                MathHelper.Clamp(point.X, water.Left, water.Right),
                MathHelper.Clamp(point.Y, water.Top, water.Bottom)
            );

            float dist = Vector2.Distance(point, clamped);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestPoint = clamped;
            }
        }

        return bestPoint;
    }
}