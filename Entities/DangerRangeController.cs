using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/DangerRangeController")]
public class DangerRangeController : BaseEntity
{
    public DangerRangeController(EntityData data, Vc2 offset) : base(data, offset)
    {
        flag = data.Attr("flag");
        isX = data.Bool("calculateX", false);
        isY = data.Bool("calculateY", false);
        value = data.Float("value", 0f);
        greater = data.Bool("greaterThan", true);
    }
    public string flag;
    public bool isX, isY;
    public float value;
    public bool greater;

    protected override void UpdateExecute()
    {
        if (flag.IsNotNullOrEmpty() && !flag.GetFlag()) { return; }
        
        if(PUt.TryGetPlayer(out Player player))
        {
            bool argX = greater ? player.Center.X > value : player.Center.X < value;
            bool argY = greater ? player.Center.Y > value : player.Center.Y < value;
            bool arg = (isX && argX) || (argY && isY);
            Log.Info(argX, argY, isX && argX, argY && isY, arg);
            
            if (arg) { player.Die(player.Speed.SafeNormalize()); }
        }
    }
}
