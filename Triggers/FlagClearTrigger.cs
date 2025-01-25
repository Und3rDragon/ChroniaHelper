using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagClearTrigger")]
public class FlagClearTrigger : FlagManageTrigger
{

    private HashSet<string> flags;

    public FlagClearTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
    }

    protected override void OnEnterExecute(Player player)
    {
        this.flags = base.GetFlags();
        base.Clear();
    }

    protected override void LeaveReset(Player player)
    {
        base.SetFlags(this.flags);
    }

}
