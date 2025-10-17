using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.StopwatchSystem;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/StopclockTrigger")]
public class StopclockTrigger :BaseTrigger
{
    public StopclockTrigger(EntityData d, Vc2 o) : base(d, o)
    {
        name = d.Attr("stopclockName", "stopclock");
        time = d.Attr("time", "00:01:00:000");
        countdown = d.Bool("countdown", true);
        operation = d.Int("operation", 0);
        onlyOnce = d.Bool("onlyOnce", true);
    }
    private string name, time;
    private bool countdown;
    private int operation = 0;
    private enum Operations { Set = 0, Add = 1, Minus = 2, Stop = 3, Resume = 4, }

    protected override void OnEnterExecute(Player player)
    {
        if (operation == (int)Operations.Add)
        {
            if (!name.GetStopclock(out Stopclock clock)) { return; }
            int[] dt = time.TimeToDigitals();
            clock.GetDigitals(out int[] t);
            int Lmax = dt.Length > t.Length ? dt.Length : t.Length;
            int[] tt = new int[Lmax];
            for(int i = 0; i < Lmax; i++)
            {
                tt[i] = (i < t.Length ? t[i] : 0) + (i < dt.Length ? dt[i] : 0);
            }
            clock.SetTime(tt, false, false);
        }
        else if(operation == (int)Operations.Minus)
        {
            if (!name.GetStopclock(out Stopclock clock)) { return; }
            int[] dt = time.TimeToDigitals();
            clock.GetDigitals(out int[] t);
            int Lmax = dt.Length > t.Length ? dt.Length : t.Length;
            int[] tt = new int[Lmax];
            for (int i = 0; i < Lmax; i++)
            {
                tt[i] = (i < t.Length ? t[i] : 0) - (i < dt.Length ? dt[i] : 0);
            }
            clock.SetTime(tt, false, false);
        }
        else if(operation == (int)Operations.Stop)
        {
            if (!name.GetStopclock(out Stopclock clock)) { return; }
            clock.Stop();
        }
        else if(operation == (int)Operations.Resume)
        {
            if (!name.GetStopclock(out Stopclock clock)) { return; }
            clock.Start();
        }
        else
        {
            new Stopclock(name, countdown, time);
        }
    }
}
