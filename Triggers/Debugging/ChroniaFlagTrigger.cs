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

[Tracked(true)]
[CustomEntity("ChroniaHelper/ChroniaFlagTrigger")]
public class ChroniaFlagTrigger : BaseTrigger
{
    private int ID;

    private ChroniaFlag parent = new();
    private string Name;

    public ChroniaFlagTrigger(EntityData e, Vc2 offset) : base(e, offset)
    {
        ID = e.ID;

        Name = e.Attr("Name");
        parent = new()
        {
            Active = e.Bool("Active", false),
            Global = e.Bool("Global", false),
            ResetOnDeath = e.Bool("ResetOnDeath", false),
            ResetOnReload = e.Bool("ResetOnReload",false),
            ResetOnTransition = e.Bool("ResetOnTransition", false),
            RemoveWhenReset = e.Bool("RemoveWhenReset", true),
            Force = e.Bool("Force", false),
            DefaultResetState = (ExpectedResetState)e.Int("DefaultResetState", 0),
            Tags = e.Attr("Tags").Split(',',StringSplitOptions.TrimEntries).ToList(),
            CustomData = e.Attr("CustomData").ParseSquaredString().SquareStringToDictionary(),
            PresetTags = e.Attr("PresetTags").Split(',', StringSplitOptions.TrimEntries)
                .EachDo<string[], List<Labels>, string, Labels>(
                (entry) => entry.MatchEnum<Labels>(0)),
        };

        parent.ResetTimer(e.Float("Timed", -1f));
    }

    protected override void OnEnterExecute(Player player)
    {
        parent.PushFlag(Name);
    }
}
