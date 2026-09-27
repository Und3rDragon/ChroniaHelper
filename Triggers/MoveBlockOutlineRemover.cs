using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Components;
using Celeste.Mod.Entities;

namespace ChroniaHelper.Triggers;

[Tracked]
[CustomEntity("ChroniaHelper/MoveBlockOutlineRemover")]
public class MoveBlockOutlineRemover : BaseTrigger
{
    public MoveBlockOutlineRemover(EntityData data, Vc2 offset):base(data, offset)
    {
        
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        foreach(var b in SceneAs<Level>().Entities.FindAll<MoveBlock>())
        {
            if (CollideCheck(b) && b.Components.GetAll<NoMoveBlockOutline>().Count() == 0)
            {
                b.Add(new NoMoveBlockOutline());
            }
        }
    }
}

public class NoMoveBlockOutline : BaseComponent
{
    public NoMoveBlockOutline()
    {
        Ldm.LoadHook(typeof(MoveBlockOutlineRemoveUtils));
    }
}

public static class MoveBlockOutlineRemoveUtils
{
    [ExA.SelectiveLoadHook]
    public static void Load()
    {
        On.Celeste.MoveBlock.Update += OnMoveBlockLoad;
    }
    [ExA.SelectiveUnloadHook]
    public static void Unload()
    {
        On.Celeste.MoveBlock.Update -= OnMoveBlockLoad;
    }

    public static void OnMoveBlockLoad(On.Celeste.MoveBlock.orig_Update orig, MoveBlock self)
    {
        orig(self);

        var packs = self.Components.GetAll<NoMoveBlockOutline>();
        if(packs.ToList().Count > 0)
        {
            self.border.Visible = false;
        }
    }
}
