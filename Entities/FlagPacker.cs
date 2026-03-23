using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
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
        
        Md.SaveData.PackedFlags.Enter(label, flags.ToList());
    }
}
