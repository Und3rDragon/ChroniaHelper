using System;
using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

/// <summary>
/// GravityHelper 的可选兼容接口：用于查询玩家的重力是否被翻转。
/// 未安装该模组时查询结果恒为 false。
/// </summary>
[ModImportName("GravityHelper")]
public static class APIGravityHelper
{
    /// <summary>
    /// 玩家的重力是否已被翻转。
    /// </summary>
    public static bool playerInverted => IsPlayerInverted?.Invoke() ?? false;

    public static Func<bool> IsPlayerInverted;
}
