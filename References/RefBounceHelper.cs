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
/// 对 BounceHelper 的引用，用于让 CH 自己的实体也能被它的「反弹」机制处理。
/// BounceHelper 是可选的第三方模组，未安装时这里的所有查询都安全地返回默认值。
/// </summary>
public static class RefBounceHelper
{
    /// <summary>
    /// BounceHelper 的模块类型全名，用于在不直接引用它的情况下查找它。
    /// </summary>
    private const string ModuleTypeName = "Celeste.Mod.BounceHelper.BounceHelperModule";

    /// <summary>
    /// BounceHelper 是否已加载。
    /// </summary>
    public static bool IsLoaded => GetModule() != null;

    private static EverestModule GetModule()
    {
        return Everest.Modules.FirstOrDefault(m => m.GetType().FullName == ModuleTypeName);
    }

    /// <summary>
    /// 当前是否处于 Bounce 模式（BounceHelper 触发器设置的会话标记，或它的全局开关）。
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
    /// BounceHelper 处理反弹的方法（对私有实例方法 bounce 的反射引用）。
    /// </summary>
    public static MethodInfo GetBounceMethod()
    {
        return GetModule()?.GetType().GetMethod("bounce",
            BindingFlags.Instance | BindingFlags.NonPublic);
    }
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
        if (!RefBounceHelper.IsLoaded)
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
    /// BounceHelper 反弹时遍历到的方块；如果它是 CH 的方块，就做出对应反应。
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
