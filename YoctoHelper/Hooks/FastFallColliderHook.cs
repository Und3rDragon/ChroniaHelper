using System;
using ChroniaHelper;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using MonoMod.Cil;

namespace YoctoHelper.Hooks;

[HookRegister(id: HookId.FastFallCollider, useData: true)]
public class FastFallColliderHook
{

    private readonly Collider fastFallCollider = new Hitbox(4F, 16F, -2F, -16F);

    private readonly Hitbox fastFallHitbox = new Hitbox(4F, 12F, -2F, -16F);

    [Load]
    private void Load()
    {
        IL.Celeste.Player.UpdateSprite += this.FastFallCollider;
    }

    [Unload]
    private void Unload()
    {
        IL.Celeste.Player.UpdateSprite -= this.FastFallCollider;
    }

    [DefaultValue]
    private bool DefaultValue()
    {
        return false;
    }

    private void FastFallCollider(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Action<Player>>(this.UpdateFastFallCollider);
    }

    private void UpdateFastFallCollider(Player player)
    {
        if (!ChroniaHelperModule.Instance.HookManager.GetHookDataValue<bool>(HookId.FastFallCollider))
        {
            return;
        }
        Vector2 scale = player.Sprite.Scale;
        if (((scale.X == 0.5F) && (scale.Y == 1.5F)) && (!player.OnGround()) && (Input.MoveY.Value == 1))
        {
            DynData<Player> dynData = new DynData<Player>(player);
            dynData.Set<Collider>("Collider", this.fastFallCollider);
            dynData.Set<Hitbox>("hurtbox", this.fastFallHitbox);
        }
    }

}
