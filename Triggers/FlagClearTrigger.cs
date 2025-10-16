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
            HashSet<string> remove = new();
            foreach(var item in Md.SaveData.ChroniaFlags)
            {
                if (item.Value.Global)
                {
                    remove.Add(item.Key);
                }
            }
            foreach(var i in remove)
            {
                Md.SaveData.ChroniaFlags.SafeRemove(i);
                i.SetFlag(false);
            }
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.SetFlags(this.flags);
    }

}
