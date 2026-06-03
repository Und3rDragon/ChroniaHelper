using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities.RandomSeries;

[Tracked]
[CustomEntity("ChroniaHelper/SetChroniaHelperRandomSeedController")]
public class SetChroniaHelperRandomSeedController : GeneralSetupController
{
    public SetChroniaHelperRandomSeedController(EntityData data, Vc2 offset) : base(data, offset)
    {
        seed = data.Int("seed", 0);
    }
    private int seed;

    public override void ApplyValue()
    {
        RandomUtils.RandomSeed = seed;
    }
}

