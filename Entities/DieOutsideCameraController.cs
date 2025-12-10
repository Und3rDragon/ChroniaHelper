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

[Tracked(false)]
[CustomEntity("ChroniaHelper/DieOutsideCameraController")]
public class DieOutsideCameraController : BaseEntity
{
    public DieOutsideCameraController(EntityData d, Vc2 o) : base(d, o)
    {
        flag = d.Attr("flag", "ChroniaHelper_DieOutsideCamera");
    }
    private string flag;
    
    protected override void UpdateExecute()
    {
        if (!flag.GetFlag()) { return; }
        
        var cam = MaP.level.Camera.GetBounds();

        if (!PUt.TryGetPlayer(out Player player)) { return; }

        bool inBound = player.CollideRect(cam);

        if (!inBound) { player.Die(player.Speed); }
    }
}
