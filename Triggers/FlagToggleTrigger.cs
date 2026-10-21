using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/FlagToggleTrigger")]
public class FlagToggleTrigger : BaseTrigger
{
    public FlagToggleTrigger(EntityData data, Vc2 offset) : base(data, offset)
    {
        flag = data.Attr("flag", "flag");
        enterDelay = data.Float("enterDelay", -1);
    }
    private string flag;

    protected override void OnEnterExecute(Player player)
    {
        base.OnEnterExecute(player);

        flag.SetFlag(!flag.GetFlag());
    }
}
