using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.StopwatchSystem;


namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/StopclockActionController")]
public class StopclockActionController : Entity
{
    public StopclockActionController(EntityData d, Vc2 o) : base(d.Position + o)
    {
        clockTag = d.Attr("stopclockTag", "stopclock");
        flag = d.Attr("setFlag");
        flagAvailable = !flag.IsNullOrEmpty();
        killPlayer = d.Bool("killPlayer", false);

        Tag = Tags.Persistent;
    }
    private string clockTag;
    private string flag;
    private bool flagAvailable = false;
    private bool killPlayer;

    public override void Update()
    {
        base.Update();

        if (!clockTag.GetStopclock(out Stopclock clock)) { return; }

        if (clock.FetchSignal())
        {
            if (killPlayer)
            {
                PUt.player?.Die(Vc2.Zero);
            }

            if (flagAvailable)
            {
                flag.SetFlag(true);
            }
        }
    }
}
