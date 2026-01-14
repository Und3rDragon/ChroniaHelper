using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.StopwatchSystem;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/FlagListenerStopclockController")]
public class FlagListenerStopclockController :  BaseEntity
{
    public FlagListenerStopclockController(EntityData d, Vc2 o) : base(d, o)
    {
        clock = new Stopclock(d.Bool("countdown", true), 
            d.Attr("time", "5:0:0"), followPause: d.Bool("followLevelPause", true));
        stopclockName = d.Attr("stopclockName", "stopclock");
        flag = d.Attr("flag");

        clock.Register(stopclockName, false);
    }
    private string flag, stopclockName;
    private Stopclock clock;
    private bool flagState = false;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
    }
    
    public override void Update()
    {
        base.Update();
        
        if(!flagState && flag.GetFlag())
        {
            clock.Reset();
            clock.Start();
        }
        
        flagState = flag.GetFlag();
    }
}
