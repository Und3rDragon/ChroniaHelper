using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagRemoveTrigger")]
public class FlagRemoveTrigger : FlagManageTrigger
{

    private string[] flag;

    private bool isEnterExecute;

    public FlagRemoveTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.flag = FlagUtils.Parse(data.Attr("flag", null));
    }

    protected override void OnEnterExecute(Player player)
    {
        this.isEnterExecute = base.Contains(this.flag);
        if (this.isEnterExecute)
        {
            base.Remove(this.flag);
        }
    }

    public override void OnLeave(Player player)
    {
        if (this.isEnterExecute)
        {
            base.OnLeave(player);
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.Add(this.flag);
    }

}
