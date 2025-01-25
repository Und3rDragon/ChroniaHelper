using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagReplaceTrigger")]
public class FlagReplaceTrigger : FlagManageTrigger
{

    private string[] oldFlag;

    private string[] newFlag;

    private bool strict;

    private bool isEnterExecute;

    private string[] intersect;

    public FlagReplaceTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.oldFlag = FlagUtils.Parse(data.Attr("oldFlag", null));
        this.newFlag = FlagUtils.Parse(data.Attr("newFlag", null));
        this.strict = data.Bool("strict", false);
    }

    protected override void OnEnterExecute(Player player)
    {
        this.intersect = base.Intersect(this.oldFlag);
        if (this.isEnterExecute = (!this.strict) || (base.Contains(this.oldFlag)))
        {
            base.Remove(oldFlag);
            base.Add(newFlag);
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
        base.Remove(this.newFlag);
        base.Add(this.intersect);
    }

}
