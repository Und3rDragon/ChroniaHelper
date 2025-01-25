using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using YoctoHelper.Cores;
using YoctoHelper.Entities;

namespace YoctoHelper.Hooks;

[HookRegister(id: HookId.NoCoyoteTime, useData: false)]
public class NoCoyoteTimeHook
{

    private ILHook PlayerOrigUpdateHook { get; set; }

    [Load]
    private void Load()
    {
        this.PlayerOrigUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), this.NoCoyoteTime);
    }

    [Unload]
    private void Unload()
    {
        if (ObjectUtils.IsNotNull(this.PlayerOrigUpdateHook))
        {
            this.PlayerOrigUpdateHook.Dispose();
            this.PlayerOrigUpdateHook = null;
        }
    }

    private void NoCoyoteTime(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, (instr) => instr.MatchLdcR4(0.1F)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Player, float>>(this.GetCoyoteTime);
        }
    }

    private float GetCoyoteTime(float previousResult, Player player)
    {
        return (player.CollideCheck<NoCoyoteTimeBlock>(player.Position + Vector2.UnitY)) ? previousResult * Engine.RawDeltaTime : previousResult;
    }

}
