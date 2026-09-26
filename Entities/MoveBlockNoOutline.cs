using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/MoveBlockNoOutline")]
public class MoveBlockNoOutline : MoveBlock
{
    public MoveBlockNoOutline(EntityData data, Vc2 offset) : base(data, offset)
    {
        
    }

    public override void Update()
    {
        base.Update();

        border.Visible = false;
    }
}
