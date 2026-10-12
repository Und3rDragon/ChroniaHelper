using System;
using System.Reflection;
using Celeste.Mod.BounceHelper;
using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.References;

/// <summary>
/// 对 BounceHelper 的引用，用于让 CH 自己的实体也能被它的「反弹」机制处理。
/// BounceHelper 在 everest.yaml 中是可选依赖，加载早于 ChroniaHelper。
/// </summary>
public static class RefBounceHelper
{
    /// <summary>
    /// 当前是否处于 Bounce 模式（BounceHelper 触发器设置的会话标记，或它的全局开关）。
    /// </summary>
    public static bool BounceModeEnabled =>
        ((Engine.Scene is Level level && level.Session != null) && level.Session.GetFlag("bounceModeEnabled"))
        || BounceHelperModule.Settings.ForceBounceMode;
}

/// <summary>
/// 把 CH 的方块接入 BounceHelper 的反弹流程：
/// 在它的 bounce 方法遍历碰撞到的方块时，一并处理 CH 自己的方块。
/// </summary>
public static class BounceHelperUtils
{
    private static ILHook bounceHook;

    [SelectiveLoadHook]
    public static void Load()
    {
        MethodInfo bounce = typeof(BounceHelperModule).GetMethod("bounce",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (bounce == null)
        {
            Log.Warn("BounceHelperUtils: BounceHelperModule.bounce not found; CH blocks won't react to bounces.");
            return;
        }

        bounceHook = new ILHook(bounce, PatchBounce);
    }

    [SelectiveUnloadHook]
    public static void Unload()
    {
        bounceHook?.Dispose();
        bounceHook = null;
    }

    /// <summary>
    /// 在 bounce 遍历碰撞到的方块时，插入对 CH 方块的检查。
    /// </summary>
    private static void PatchBounce(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        // 锚点：foreach 循环体里 Solid 局部变量的第一次使用（isinst DreamBlock）
        if (!cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdloc(out _),
                instr => instr.MatchIsinst<DreamBlock>()))
        {
            Log.Warn("BounceHelperUtils: bounce foreach anchor not found.");
            return;
        }

        // 此时游标停在 ldloc（循环变量 Solid）之前；取该局部变量以便复用
        VariableDefinition solidVar = (cursor.Next.Operand as VariableDefinition);

        cursor.Emit(OpCodes.Ldloc, solidVar);
        cursor.EmitDelegate<Action<Solid>>(HandleBouncedSolid);
    }

    /// <summary>
    /// BounceHelper 反弹时遍历到的方块；如果它是 CH 的可碎方块，就让它破碎。
    /// </summary>
    private static void HandleBouncedSolid(Solid solid)
    {
        if (solid is Entities.CrumbleBlockOnJump block)
        {
            block.BreakAll();
        }
        else if(solid is CustomFallingBlock block2)
        {
            block2.TriggerAll();
        }
    }
}
