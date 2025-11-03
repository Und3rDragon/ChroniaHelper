using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers.Debugging;

// Chronia Flag System Debugging
[CustomEntity("Chroniahelper/ChroniaSliderTrigger")]
public class ChroniaSliderTrigger : BaseTrigger
{
    public ChroniaSlider parent;
    public string Name;
    public ChroniaSliderTrigger(EntityData d, Vc2 o) : base(d, o)
    {
        Name = d.Attr("name");
        parent = new ChroniaSlider()
        {
            Value = d.Float("Value", 0f),
            Global = d.Bool("Global", false),
            ResetOnDeath = d.Bool("ResetOnDeath", false),
            ResetOnTransition = d.Bool("ResetOnTransition", false),
            RemoveWhenReset = d.Bool("RemoveWhenReset", true),
            DefaultValue = d.Float("DefaultValue", 0f),
        };
        parent.SetTimer(d.Float("Timer", -1f));
    }

    protected override void OnEnterExecute(Player player)
    {
        parent.PushSlider(Name);
    }
}
