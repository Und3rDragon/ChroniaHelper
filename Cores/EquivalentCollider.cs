using System;
using System.Collections.Generic;
using System.Linq;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.Cores;

public class EquivalentCollider : Collider
{
    // 内部代理 Hitbox，用于提供基本几何和 setter 支持
    private readonly Hitbox _proxy = new Hitbox(1f, 1f);

    // 正负碰撞体集合
    public HashSet<Collider> PositiveColliders = new HashSet<Collider>();
    public HashSet<Collider> NegativeColliders = new HashSet<Collider>();

    // 缓存有效碰撞的边界（仅在有效碰撞后更新）
    private bool _hasValidCollision = false;
    private float _cachedTop;
    private float _cachedBottom;
    private float _cachedLeft;
    private float _cachedRight;

    // --- 抽象属性实现 ---
    public override float Width
    {
        get => _proxy.Width;
        set => _proxy.Width = value;
    }

    public override float Height
    {
        get => _proxy.Height;
        set => _proxy.Height = value;
    }

    public override float Top
    {
        get => _hasValidCollision ? _cachedTop : _proxy.Top;
        set => _proxy.Top = value;
    }

    public override float Bottom
    {
        get => _hasValidCollision ? _cachedBottom : _proxy.Bottom;
        set => _proxy.Bottom = value;
    }

    public override float Left
    {
        get => _hasValidCollision ? _cachedLeft : _proxy.Left;
        set => _proxy.Left = value;
    }

    public override float Right
    {
        get => _hasValidCollision ? _cachedRight : _proxy.Right;
        set => _proxy.Right = value;
    }
    
    public EquivalentCollider(params Collider[] positiveColliders)
    {
        for(int i = 0; i < positiveColliders.Length; i++)
        {
            PositiveColliders.Add(positiveColliders[i]);
        }
    }
    
    public void Add(bool toNegative = false, params Collider[] colliders)
    {
        for(int i = 0; i < colliders.Length; i++)
        {
            if (toNegative)
            {
                NegativeColliders.Add(colliders[i]);
            }
            else
            {
                PositiveColliders.Add(colliders[i]);
            }
        }
    }

    public void Remove(bool negative = false, params Collider[] colliders)
    {
        foreach(Collider collider in colliders)
        {
            if (negative)
            {
                NegativeColliders.SafeRemove(collider);
            }
            else
            {
                PositiveColliders.SafeRemove(collider);
            }
        }
    }

    // --- 碰撞检测：核心逻辑 ---
    private void UpdateCollisionState()
    {
        // 重置缓存
        _hasValidCollision = false;

        // 收集所有发生碰撞的 collider（需在 Entity 场景中才有 Absolute 值）
        var collidedColliders = new List<Collider>();

        int positiveCount = 0;
        int negativeCount = 0;

        // 检查 PositiveColliders
        foreach (var c in PositiveColliders)
        {
            if (c != null && OverlapsAny(c))
            {
                positiveCount++;
                collidedColliders.Add(c);
            }
        }

        // 检查 NegativeColliders
        foreach (var c in NegativeColliders)
        {
            if (c != null && OverlapsAny(c))
            {
                negativeCount++;
                collidedColliders.Add(c);
            }
        }

        // 判断是否有效碰撞
        if (positiveCount > negativeCount && collidedColliders.Count > 0)
        {
            _hasValidCollision = true;

            // 计算所有碰撞 collider 的绝对边界极值
            _cachedTop = collidedColliders.Max(c => c.AbsoluteTop);
            _cachedBottom = collidedColliders.Min(c => c.AbsoluteBottom);
            _cachedLeft = collidedColliders.Min(c => c.AbsoluteLeft);
            _cachedRight = collidedColliders.Max(c => c.AbsoluteRight);
        }
    }

    // 辅助方法：判断当前 collider 是否与目标 collider 重叠
    private bool OverlapsAny(Collider other)
    {
        if (other == null || Entity == null) return false;

        // 使用 Monocle 的 Collide 方法（会调用具体重载）
        return CollideWith(other);
    }

    // 调用基类的 Collide(Collider) 逻辑
    private bool CollideWith(Collider other)
    {
        try
        {
            return base.Collide(other);
        }
        catch (Exception)
        {
            // 如果类型不支持，尝试反向调用（某些 collider 可能只实现单向）
            return other.Collide(this);
        }
    }

    // --- 重写所有 Collide 方法 ---
    public override bool Collide(Vector2 point)
    {
        // 先让 proxy 更新到当前位置（同步 Position）
        SyncProxy();
        UpdateCollisionState();
        return _hasValidCollision;
    }

    public override bool Collide(Rectangle rect)
    {
        SyncProxy();
        UpdateCollisionState();
        return _hasValidCollision;
    }

    public override bool Collide(Vector2 from, Vector2 to)
    {
        SyncProxy();
        UpdateCollisionState();
        return _hasValidCollision;
    }

    public override bool Collide(Hitbox hitbox)
    {
        SyncProxy();
        UpdateCollisionState();
        return _hasValidCollision;
    }

    public override bool Collide(Grid grid)
    {
        SyncProxy();
        UpdateCollisionState();
        return _hasValidCollision;
    }

    public override bool Collide(Circle circle)
    {
        SyncProxy();
        UpdateCollisionState();
        return _hasValidCollision;
    }

    public override bool Collide(ColliderList list)
    {
        SyncProxy();
        UpdateCollisionState();
        return _hasValidCollision;
    }

    // 同步代理 collider 的位置到当前 Entity + Position
    private void SyncProxy()
    {
        if (Entity != null)
        {
            _proxy.Entity = Entity;
            _proxy.Position = Position;
        }
        else
        {
            _proxy.Entity = null;
            _proxy.Position = Position;
        }
    }

    // --- 其他必需方法 ---
    public override Collider Clone()
    {
        var clone = new EquivalentCollider
        {
            Width = Width,
            Height = Height,
            Position = Position,
            PositiveColliders = new HashSet<Collider>(PositiveColliders),
            NegativeColliders = new HashSet<Collider>(NegativeColliders)
        };
        return clone;
    }

    public override void Render(Camera camera, Color color)
    {
        // 可选：渲染代理 collider 或正负 collider（调试用）
        _proxy.Render(camera, color);
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        _proxy.Entity = entity;
    }

    public override void Removed()
    {
        base.Removed();
        _proxy.Entity = null;
    }
}