using ChroniaHelper.Cores;
using System.Collections.Generic;

namespace ChroniaHelper.Components.SwimmingFish;

/// <summary>
/// 边界接口 - 所有边界类型必须实现
/// </summary>
public interface IFishBoundary
{
    /// <summary>
    /// 检查点是否在边界内（考虑保护距离）
    /// </summary>
    bool Contains(Vector2 point);
    
    /// <summary>
    /// 获取边界内最近的合法位置（用于夹紧）
    /// </summary>
    Vector2 ClampToBoundary(Vector2 point);
    
    /// <summary>
    /// 获取边界推力（用于引导鱼儿远离边界）
    /// </summary>
    Vector2 GetBoundaryForce(Vector2 position, float safeZone);
}

/// <summary>
/// 边界类型枚举
/// </summary>
public enum BoundaryType
{
    Rectangle,      // 矩形边界（原有）
    Polygon,        // 多边形边界
    MultiRectangle  // 复合矩形边界
}