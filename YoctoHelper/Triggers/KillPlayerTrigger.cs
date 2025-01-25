using Celeste.Mod.Entities;
using YoctoHelper.Cores;

namespace YoctoHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/KillPlayerTrigger")]
public class KillPlayerTrigger : BaseTrigger
{

    private EntityID id;

    public KillPlayerTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
    {
        this.id = id;
    }

    public override void OnStay(Player player)
    {
        if (ObjectUtils.IsNull(player))
        {
            return;
        }
        if (flagsForEnter.IsNullOrEmpty())
        {
            player.Die((player.Position - base.Position).SafeNormalize());
        }
        foreach (var item in base.flagsForEnter)
        {
            if (level.Session.GetFlag(item))
            {
                player.Die((player.Position - base.Position).SafeNormalize());
            }
        }
        
        if (base.onlyOnce)
        {
            this.level.Session.DoNotLoad.Add(this.id);
        }
    }

}
