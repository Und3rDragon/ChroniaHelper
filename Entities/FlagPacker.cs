using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagPacker")]
public class FlagPacker : Entity
{
    public FlagPacker(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        flags = data.Attr("flags").Split(',',StringSplitOptions.TrimEntries);
        label = data.Attr("label");
    }
    private string[] flags;
    private string label;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        ChroniaFlag f = new();

        foreach (var flag in flags)
        {
            if (flag.StartsWith('!'))
            {
                // temporary flag
                string name = flag.RemoveFirst("!");
                f = name.PullFlag();

                f.Global = false;
                f.ResetOnDeath = true;
                f.PresetTags.Create(Labels.Packed);
                f.CustomData.Enter("packed_label", label);
                f.CustomData.Create("packed_triggered", "false");
                f.PushFlag(name);
            }
            else if (flag.StartsWith('*'))
            {
                // global flag
                string name = flag.RemoveFirst("*");
                f = name.PullFlag();

                f.Global = true;
                f.ResetOnDeath = false;
                f.PresetTags.Create(Labels.Packed);
                f.CustomData.Enter("packed_label", label);
                f.CustomData.Create("packed_triggered", "false");
                f.PushFlag(name);
            }
            else
            {
                f = flag.PullFlag();
                
                f.Global = false;
                f.ResetOnDeath = false;
                f.PresetTags.Create(Labels.Packed);
                f.CustomData.Enter("packed_label", label);
                f.CustomData.Create("packed_triggered", "false");
                f.PushFlag(flag);
            }
        }
    }
}
