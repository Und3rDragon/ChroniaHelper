using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/HoldableFlagController")]
public class HoldableFlagController : GeneralSetupController
{
    public HoldableFlagController(EntityData data, Vc2 offset) : base(data, offset)
    {

    }

    public override void Update()
    {
        base.Update();

        if(PUt.TryGetPlayer(out Player player))
        {
            MaP.level.Session.Flags.RemoveWhere(f => f.StartsWith("ChroniaHelper_PlayerHolding_"));
            
            Holdable holding = player.Holding;
            
            if(holding?.Entity is not null)
            {
                $"ChroniaHelper_PlayerHolding_{holding.Entity.GetType()}".SetFlag(true);
            }
        }
    }
}
