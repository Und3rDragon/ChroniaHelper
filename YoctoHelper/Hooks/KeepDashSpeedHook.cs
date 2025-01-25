using MonoMod.Cil;
using System;
using System.Reflection;
using ChroniaHelper;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using YoctoHelper.Cores;

namespace YoctoHelper.Hooks;

[HookRegister(id: HookId.KeepDashSpeed, useData: true)]
public class KeepDashSpeedHook
{

    private ILHook dashCoroutineHook { get; set; }

    private ILHook birdDashTutorialCoroutineHook { get; set; }

    [Load]
    private void Load()
    {
        MethodInfo dashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        this.dashCoroutineHook = new ILHook(dashCoroutine, this.KeepDashSpeed);
        MethodInfo birdDashTutorialCoroutine = typeof(Player).GetMethod("BirdDashTutorialCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        this.birdDashTutorialCoroutineHook = new ILHook(birdDashTutorialCoroutine, this.KeepDashSpeed);
    }

    [Unload]
    private void Unload()
    {
        if (ObjectUtils.IsNotNull(this.dashCoroutineHook))
        {
            this.dashCoroutineHook.Dispose();
            this.dashCoroutineHook = null;
        }
        if (ObjectUtils.IsNotNull(this.birdDashTutorialCoroutineHook))
        {
            this.birdDashTutorialCoroutineHook.Dispose();
            this.birdDashTutorialCoroutineHook = null;
        }
    }

    [DefaultValue]
    private bool DefaultValue()
    {
        return false;
    }

    private void KeepDashSpeed(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.Before, [(instr) => (instr.MatchLdfld<Player>("DashDir")), (instr) => (instr.MatchLdcR4(160)), (instr) => (instr.MatchCall<Vector2>("op_Multiply")), (instr) => (instr.MatchStfld<Player>("Speed"))]))
        {
            cursor.RemoveRange(3);
            cursor.EmitDelegate<Func<Player, Vector2>>(this.GetDashSpeed);
        }
    }

    private Vector2 GetDashSpeed(Player player)
    {
        return ChroniaHelperModule.Instance.HookManager.GetHookDataValue<bool>(HookId.KeepDashSpeed) ? player.Speed : (player.DashDir * 160F);
    }

}
