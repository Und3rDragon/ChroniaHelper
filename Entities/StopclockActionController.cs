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
        sessionKey = d.Attr("exportToSessionKey", "");
        sessionKeyAvailable = !sessionKey.IsNullOrEmpty();

        Tag = Tags.Persistent;
    }
    private string clockTag;
    private string flag;
    private bool flagAvailable = false;
    private bool killPlayer;
    private string sessionKey;
    private bool sessionKeyAvailable;

    public override void Update()
    {
        base.Update();

        if (!clockTag.GetStopclock(out Stopclock clock)) { return; }

        if (sessionKeyAvailable)
        {
            clock.GetClampedTimeData(out int[] data);

            string sessionData = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 0)
                {
                    sessionData = $"{data[i]:000}";
                    continue;
                }
                else
                {
                    sessionData = $"{data[i]:00}:" + sessionData;
                }
            }

            Md.Session.keystrings[sessionKey] = sessionData;
        }
        
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
