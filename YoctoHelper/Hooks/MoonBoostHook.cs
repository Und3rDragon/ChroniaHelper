using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using YoctoHelper.Entities;

namespace YoctoHelper.Hooks;

[HookRegister(id: HookId.MoonBoost, useData: false)]
public class MoonBoostHook
{

    [Load]
    private void Load()
    {
        IL.Celeste.Player.OnCollideH += this.MoonBoost;
        IL.Celeste.Player.OnCollideV += this.MoonBoost;
    }

    [Unload]
    private void Unload()
    {
        IL.Celeste.Player.OnCollideH -= this.MoonBoost;
        IL.Celeste.Player.OnCollideV -= this.MoonBoost;
    }

    private void MoonBoost(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, (instr) => instr.MatchLdcI4(4) || instr.MatchLdcI4(-4)))
        {
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<int, CollisionData, int>>(this.GetMoonBoost);
        }
    }

    private int GetMoonBoost(int previousResult, CollisionData collisionData)
    {
        return ((collisionData.Hit is MoonBoostBlock) && (collisionData.Direction.Y == 0)) ? (Math.Sign(previousResult) * 8) : previousResult;
    }

}
