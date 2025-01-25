using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using YoctoHelper.Components;

namespace YoctoHelper.Hooks;

[HookRegister(id: HookId.JumpListener, useData: false)]
public class JumpListenerHook
{

    [Load]
    private void Load()
    {
        IL.Celeste.Player.Jump += this.JumpListener;
    }

    [Unload]
    private void Unload()
    {
        IL.Celeste.Player.Jump -= this.JumpListener;
    }

    private void JumpListener(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.Before, (instr) => instr.MatchRet()))
        {
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Action<Player>>(this.JumpListenerAction);
    }

    private void JumpListenerAction(Player player)
    {
        foreach (JumpListener component in player.Scene.Tracker.GetComponents<JumpListener>())
        {
            if (component.OnJump != null)
            {
                component.OnJump(true);
            }
        }
    }

}
