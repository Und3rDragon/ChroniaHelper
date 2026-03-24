using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Triggers.RandomSeries;

[Tracked]
[CustomEntity("ChroniaHelper/RandomCounterTrigger")]
public class RandomCounterTrigger : BaseTrigger
{
    public RandomCounterTrigger(EntityData d, Vc2 o) : base(d, o)
    {
        counter = d.Attr("counter", "counter");
        value1 = d.Int("value1", 0);
        value2 = d.Int("value2", 1);
        enterDelay = d.Float("enterDelay", -1f);
        interval = d.Float("interval", 1f).GetAbs().ClampMin(Engine.DeltaTime / 2f);
        continuously = d.Bool("continuously", false);
        onlyOnce = d.Bool("onlyOnce", true);
    }
    public string counter;
    public int value1, value2;
    public float interval;
    public bool continuously;

    public float timer = 0f;
    public bool active = false;
    protected override void AddedExecute(Scene scene)
    {
        active = false;
    }

    protected override void OnEnterExecute(Player player)
    {
        active = continuously;
        timer = 0f;
        counter.SetCounter(GenerateRandom());
    }

    protected override void UpdateExecute(Player player)
    {
        if (!active) { return; }

        if(timer <= 0f)
        {
            timer = interval;
            counter.SetCounter(GenerateRandom());
        }

        timer -= Engine.DeltaTime;
    }

    public int GenerateRandom()
    {
        return RandomUtils.RandomInt(value1, value2);
    }
}
