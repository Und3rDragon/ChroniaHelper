using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers.Debugging;

// Chronia Flag System Debugging
[CustomEntity("ChroniaHelper/ChroniaCounterTrigger")]
public class ChroniaCounterTrigger : BaseTrigger
{
    public ChroniaCounter parent;
    public string Name;
    public ChroniaCounterTrigger(EntityData d, Vc2 o) : base(d, o)
    {
        Name = d.Attr("Name");
        parent = new()
        {
            Value = d.Int("Value", 0),
            Global = d.Bool("Global", false),
            ResetOnDeath = d.Bool("ResetOnDeath", false),
            ResetOnTransition = d.Bool("ResetOnTransition", false),
            RemoveWhenReset = d.Bool("RemoveWhenReset", true),
            DefaultValue = d.Int("DefaultValue", 0),
        };
        parent.SetTimer(d.Float("Timer", -1f));
    }

    protected override void OnEnterExecute(Player player)
    {
        parent.PushCounter(Name);
    }
}
