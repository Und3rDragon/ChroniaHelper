using System;
using System.Linq;
using System.Reflection;
using Celeste.Mod;
using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.References;

/// <summary>
/// 对 BounceHelper 的引用，用于查询其反弹模式状态。
/// 未安装该模组时所有查询都安全地返回默认值。
/// </summary>
public static class RefBounceHelper
{
    private const string BounceModuleName = "Celeste.Mod.BounceHelper.BounceHelperModule";

    private static EverestModule GetModule()
    {
        return Everest.Modules.FirstOrDefault(m => m.GetType().FullName == BounceModuleName);
    }

    /// <summary>
    /// 当前是否处于 Bounce 模式。
    /// </summary>
    public static bool BounceModeEnabled
    {
        get
        {
            EverestModule module = GetModule();

            if (module == null)
            {
                return false;
            }

            if (Engine.Scene is Level level && level.Session != null && level.Session.GetFlag("bounceModeEnabled"))
            {
                return true;
            }

            return DynamicData.For(module).Get<object>("Settings") is { } settings
                && DynamicData.For(settings).Get<bool>("ForceBounceMode");
        }
    }

    /// <summary>
    /// 获取 BounceHelper 处理反弹的方法。
    /// </summary>
    public static MethodInfo GetBounceMethod()
    {
        return GetModule()?.GetType().GetMethod("bounce",
            BindingFlags.Instance | BindingFlags.NonPublic);
    }
}

/// <summary>
/// 把 CH 的方块接入 BounceHelper 的反弹流程。
/// </summary>
public static class BounceHelperUtils
{
    private static ILHook bounceHook;

    [SelectiveLoadHook]
    public static void Load()
    {
        if (!Md.BounceHelperLoaded)
        {
            return;
        }

        MethodInfo bounce = RefBounceHelper.GetBounceMethod();

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
    /// 在 BounceHelper 处理碰撞到的方块时，一并处理 CH 的方块。
    /// </summary>
    private static void PatchBounce(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdloc(out _),
                instr => instr.MatchIsinst<DreamBlock>()))
        {
            Log.Warn("BounceHelperUtils: bounce foreach anchor not found.");
            return;
        }

        VariableDefinition solidVar = (cursor.Next.Operand as VariableDefinition);

        cursor.Emit(OpCodes.Ldloc, solidVar);
        cursor.EmitDelegate<Action<Solid>>(HandleBouncedSolid);
    }

    /// <summary>
    /// 对 BounceHelper 反弹时接触到的 CH 方块作出反应。
    /// </summary>
    private static void HandleBouncedSolid(Solid solid)
    {
        if (solid is Entities.CrumbleBlockOnJump block)
        {
            block.BreakAll();
        }
        else if (solid is CustomFallingBlock fallingBlock)
        {
            fallingBlock.TriggerAll();
        }
    }
}
