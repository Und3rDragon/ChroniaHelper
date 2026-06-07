using MonoMod.Cil;
using System;
using System.Reflection;
using ChroniaHelper;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using YoctoHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Hooks;

//[HookRegister(id: HookId.KeepDashSpeed, useData: true)]
public class KeepDashSpeedHook
{
    private ILHook dashCoroutineHook { get; set; }

    private ILHook birdDashTutorialCoroutineHook { get; set; }

    [LoadHook]
    private void Load()
    {
        MethodInfo dashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        this.dashCoroutineHook = new ILHook(dashCoroutine, this.KeepDashSpeed);
        MethodInfo birdDashTutorialCoroutine = typeof(Player).GetMethod("BirdDashTutorialCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        this.birdDashTutorialCoroutineHook = new ILHook(birdDashTutorialCoroutine, this.KeepDashSpeed);
    }

    [UnloadHook]
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

    //[DefaultValue]
    //private bool DefaultValue()
    //{
    //    return false;
    //}

    private void KeepDashSpeed(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        // The function below is usable, but will be affected by bits & bolts hooks,
        // also Faerie Helper hooks
        // while (cursor.TryGotoNext(MoveType.Before, [(instr) => (instr.MatchLdfld<Player>("DashDir")), (instr) => (instr.MatchLdcR4(160)), (instr) => (instr.MatchCall<Vector2>("op_Multiply")), (instr) => (instr.MatchStfld<Player>("Speed"))]))
        // {
        //     cursor.RemoveRange(3);
        //     cursor.EmitDelegate<Func<Player, Vector2>>(this.GetDashSpeed);
        // }
        
        // Only delegating speed variables
        // 找到开头的两个连续 ldloc.1
        while (cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdloc(1),
                instr => instr.MatchLdloc(1)))
        {
            int startIndex = cursor.Index;
        
            // 向后查找 stfld（不限制中间有多少指令）
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld<Player>("Speed")))
            {
                // 确保找到的 stfld 在起始位置之后
                if (cursor.Index > startIndex)
                {
                    // 光标现在在 stfld 之前
                    cursor.EmitDelegate<Func<Vc2, Vector2>>(GetDashSpeed);
                }
            }
        }
    }

    // private Vector2 GetDashSpeed(Player player)
    // {
    //     return (Md.Session.KeepDashSpeed.GetValueOrDefault(MaP.level.Session.Area.SID, false) ? player.Speed : (player.DashDir * 160F));
    // }
    
    private Vector2 GetDashSpeed(Vc2 oldSpeed)
    {
        return (Md.Session.KeepDashSpeed.GetValueOrDefault(MaP.level.Session.Area.SID, false) ? (PUt.TryGetPlayer(out Player player) ? player.Speed : oldSpeed) : oldSpeed);
    }

}
