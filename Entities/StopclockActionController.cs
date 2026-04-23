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
        countUpTo = d.Attr("countUpTimer", "0:30:0");
        countUpTo.Split(":", StringSplitOptions.TrimEntries).ApplyTo(out List<string> nums);
        List<int> spanValue = new();
        for (int i = nums.Count - 1; i >= 0; i--)
        {
            int n = 0;
            int.TryParse(nums[i], out n);
            spanValue.Add(n);
        }
        countUpSpan = new(
            spanValue.SafeGet(6, 0) * 365 + spanValue.SafeGet(5, 0) * 30 + spanValue.SafeGet(4, 0),
            spanValue.SafeGet(3, 0),
            spanValue.SafeGet(2, 0),
            spanValue.SafeGet(1, 0),
            spanValue.SafeGet(0, 0)
            );
        resetCountUpTimer = d.Bool("resetCountUpTimerWhenTriggered", false);

        Tag = Tags.Persistent;
    }
    private string clockTag;
    private string flag;
    private bool flagAvailable = false;
    private bool killPlayer;
    private string sessionKey;
    private bool sessionKeyAvailable;
    private string countUpTo;
    private TimeSpan countUpSpan;
    private bool resetCountUpTimer = false;

    private bool countUpCheck = false, _countUpCheck = false;

    public override void Update()
    {
        base.Update();

        if (!clockTag.GetStopclock(out Stopclock clock)) 
        {
            return; 
        }

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

        clock.onComplete.Register($"StopclockActionController_{SourceData.ID}",
            () =>
            {
                if (killPlayer)
                {
                    PUt.player?.Die(Vc2.Zero);
                }

                if (flagAvailable)
                {
                    flag.SetFlag(true);
                }
            });
        
        // if not a countdown, use following logic:
        if (!clock.countdown)
        {
            countUpCheck = clock.ClockToTimeSpan() > countUpSpan;
            
            if (countUpCheck && !_countUpCheck)
            {
                if (killPlayer)
                {
                    PUt.player?.Die(Vc2.Zero);
                }

                if (flagAvailable)
                {
                    flag.SetFlag(true);
                }

                if (resetCountUpTimer)
                {
                    clock.Restart();
                }
            }
            
            _countUpCheck = countUpCheck;
        }
    }
}
