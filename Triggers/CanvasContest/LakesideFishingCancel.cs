using Celeste.Mod.Entities;
using Celeste.Mod.LakeSideCode.Entities;
using ChroniaHelper.References;
using ChroniaHelper.Utils;
using YoctoHelper.Cores;

namespace ChroniaHelper.Triggers.CanvasContest;

[CustomEntity("ChroniaHelper/LakesideFishingCancel")]
public class LakesideFishingCancel : BaseTrigger
{
    public LakesideFishingCancel(EntityData data, Vc2 offset) : base(data, offset)
    {
        
    }
    private List<Component> store = new();

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        
        RefLakeside.CancelFishing(out store);
    }

    public override void Update()
    {
        base.Update();

        if (Scene.OnInterval(0.05f))
        {
            if (PUt.TryGetPlayer(out Player player))
            {
                foreach (var entity in MaP.level.Entities)
                {
                    if (entity.GetType().ToString().StartsWith("Celeste.Mod.LakeSideCode.Entities")
                     && player.CollideCheck(entity))
                    {
                        foreach (var item in store)
                        {
                            item.Added(player);
                        }
                    }
                }
            }
        }
    }
}