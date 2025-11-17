using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/ClearPackedFlagsTrigger")]
public class ClearPackedFlagsTrigger : BaseTrigger
{
    public ClearPackedFlagsTrigger(EntityData e, Vector2 p) : base(e, p)
    {
        labels = e.Attr("labels").Split(',', StringSplitOptions.TrimEntries);
        mode = e.Int("mode", 0);
    }
    private string[] labels;
    private int mode;
    private enum Operation
    {
        Remove, RemoveTag
    }

    protected override void OnEnterExecute(Player player)
    {
        if((Operation)mode == Operation.RemoveTag)
        {
            Md.SaveData.PackedFlags.Clear();
            Md.SaveData.CurrentPackedFlags.Clear();
        }
        else
        {
            foreach(var f in Md.SaveData.PackedFlags)
            {
                foreach(var i in f.Value)
                {
                    i.SetFlag(false);
                }
            }
            Md.SaveData.PackedFlags.Clear();
            Md.SaveData.CurrentPackedFlags.Clear();
        }
    }
}
