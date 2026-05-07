using Celeste.Mod.Entities;
using ChroniaHelper.References;
using ChroniaHelper.Utils;
using MonoMod.Utils;
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
        
        store.Clear();
        RefLakeside.CancelFishing(out store);
        foreach (var item in lakesideHUD)
        {
            item.Visible = false;
        }
    }

    public List<Entity> lakesideBarrels = new();
    public List<Entity> lakesideHUD = new();
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        lakesideBarrels.Clear();
        lakesideHUD.Clear();
        foreach (var entity in MaP.level.Entities)
        {
            if (entity.GetType().ToString().Contains("Celeste.Mod.LakeSideCode.Entities.FishingRodBarrel"))
            {
                lakesideBarrels.Add(entity);
            }

            if (entity.GetType().ToString() == "Celeste.Mod.LakeSideCode.Entities.FishingHUD")
            {
                lakesideHUD.Add(entity);
            }
        }
    }

    public override void Update()
    {
        base.Update();

        if (PUt.TryGetPlayer(out Player player))
        {
            foreach (var entity in lakesideBarrels)
            {
                if (entity.CollideCheck(player))
                {
                    foreach (var item in store)
                    {
                        item.Active = true;
                    }
                    
                    foreach (var item in lakesideHUD)
                    {
                        item.Visible = true;
                    }

                    return;
                }
            }
        }
    }
}