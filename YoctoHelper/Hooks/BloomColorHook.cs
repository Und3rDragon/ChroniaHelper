using MonoMod.Cil;
using System;
using ChroniaHelper;

namespace YoctoHelper.Hooks;

// 很奇怪的钩子，没有实体在使用但是在运行，需要后续检查?
// 暂定取消HookRegister

//[HookRegister(id: HookId.BloomColor, useData: true)]
public class BloomColorHook
{

    private Color previous { get; set; } = Color.White;

    [Load]
    private void Load()
    {
        IL.Celeste.BloomRenderer.Apply += this.BloomColor;
    }

    [Unload]
    private void Unload()
    {
        IL.Celeste.BloomRenderer.Apply -= this.BloomColor;
    }

    [DefaultValue]
    private Color DefaultValue()
    {
        return Color.White;
    }

    private void BloomColor(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, (instr) => instr.MatchCall<Color>("get_White")))
        {
            cursor.EmitDelegate<Func<Color, Color>>(this.GetBloomColor);
        }
    }

    private Color GetBloomColor(Color original)
    {
        if (this.previous != original)
        {
            ChroniaHelperModule.Instance.HookManager.SetHookDataValue<Color>(HookId.BloomColor, this.previous = original, false);
        }
        return ChroniaHelperModule.Instance.HookManager.GetHookDataValue<Color>(HookId.BloomColor);
    }

}
