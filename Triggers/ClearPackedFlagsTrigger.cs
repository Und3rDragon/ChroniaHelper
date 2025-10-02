using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

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
        HashSet<string> toRemove = new();
        foreach(var item in Md.SaveData.ChroniaFlags)
        {
            if (item.Value.PresetTags.Contains(Utils.ChroniaSystem.Labels.Packed))
            {
                if (!labels.Contains(item.Value.CustomData["packed_label"])) { continue; }
                
                
            }
        }
    }
}
