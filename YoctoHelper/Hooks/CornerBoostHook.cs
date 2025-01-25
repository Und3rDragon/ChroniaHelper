using MonoMod.Cil;
using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using YoctoHelper.Entities;

namespace YoctoHelper.Hooks;

[HookRegister(id: HookId.CornerBoost, useData: false)]
public class CornerBoostHook
{

    [Load]
    private void Load()
    {
        IL.Celeste.Player.WallJumpCheck += this.CornerBoost;
    }

    [Unload]
    private void Unload()
    {
        IL.Celeste.Player.WallJumpCheck -= this.CornerBoost;
    }

    private void CornerBoost(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, (instr) => (((instr.Operand as MethodReference)?.FullName) == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)")))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<bool, Player, int, bool>>(this.CheckCornerBoostBlock);
        }
    }

    private bool CheckCornerBoostBlock(bool previousResult, Player player, int dir)
    {
        if (previousResult)
        {
            return true;
        }
        int increment = Math.Max(3, (int)(Math.Abs(player.Speed.X) * Engine.RawDeltaTime) + 1);
        if ((player.ClimbBoundsCheck(dir)) && (!ClimbBlocker.EdgeCheck(player.Scene, player, dir * increment)))
        {
            return player.CollideCheck<CornerBoostBlock>(player.Position + Vector2.UnitX * dir * increment);
        }
        return false;
    }

}
