using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Cores.SampleJumpThrough;

/// <summary>
/// 带提升速度回传的粘连组件：在每次移动前把宿主平台当前的速度交给订阅者，
/// 使贴附的实体与宿主以一致的节奏移动。
/// </summary>
[TrackedAs(typeof(StaticMover))]
public class StaticMoverWithLiftSpeed : StaticMover
{
    /// <summary>
    /// 移动开始前回传宿主速度。
    /// </summary>
    public Action<Vector2> OnSetLiftSpeed;

    private static readonly LinkedList<Platform> currentPlatforms = new();

    [LoadHook]
    public static void Load()
    {
        On.Celeste.Platform.MoveStaticMovers += OnMoveStaticMovers;
        On.Celeste.StaticMover.Move += OnStaticMoverMove;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Platform.MoveStaticMovers -= OnMoveStaticMovers;
        On.Celeste.StaticMover.Move -= OnStaticMoverMove;
    }

    private static void OnMoveStaticMovers(
        On.Celeste.Platform.orig_MoveStaticMovers orig, Platform self, Vector2 amount)
    {
        currentPlatforms.AddLast(self);
        orig(self, amount);
        currentPlatforms.RemoveLast();
    }

    private static void OnStaticMoverMove(
        On.Celeste.StaticMover.orig_Move orig, StaticMover self, Vector2 amount)
    {
        if (self is StaticMoverWithLiftSpeed staticMover && currentPlatforms.Last is not null)
        {
            staticMover.OnSetLiftSpeed?.Invoke(currentPlatforms.Last.Value.LiftSpeed);
        }

        orig(self, amount);
    }
}
