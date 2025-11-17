using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagClearTrigger")]
public class FlagClearTrigger : FlagManageTrigger
{

    private HashSet<string> flags, globalFlags;
    private bool clearGlobal;
    public FlagClearTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        clearGlobal = data.Bool("clearGlobal", false);
    }

    protected override void OnEnterExecute(Player player)
    {
        this.flags = base.GetFlags();
        base.Clear();

        if (clearGlobal)
        {
            Md.SaveData.flags.Clear();
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.SetFlags(this.flags);
    }

}
