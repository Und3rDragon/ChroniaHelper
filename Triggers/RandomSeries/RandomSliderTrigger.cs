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
[CustomEntity("ChroniaHelper/RandomSliderTrigger")]
public class RandomSliderTrigger : BaseTrigger
{
    public RandomSliderTrigger(EntityData d, Vc2 o) : base(d, o)
    {
        slider = d.Attr("slider", "slider");
        value1 = d.Float("value1", 0f);
        value2 = d.Float("value2", 1f);
        enterDelay = d.Float("enterDelay", -1f);
        interval = d.Float("interval", 1f).GetAbs().ClampMin(Engine.DeltaTime / 2f);
        continuously = d.Bool("continuously", false);
        onlyOnce = d.Bool("onlyOnce", true);

        string _seed = d.Attr("seed");
        if (_seed.HasValidContent())
        {
            if(int.TryParse(_seed, out int n))
            {
                seed = n;
            }
            else
            {
                seed = _seed.GetHashCode();
            }
        }
        else
        {
            seed = SourceData.ID;
        }
    }
    public string slider;
    public float value1, value2;
    public float interval;
    public bool continuously;
    public int seed;

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
        slider.SetSlider(GenerateRandom());
    }

    protected override void UpdateExecute(Player player)
    {
        if (!active) { return; }

        if (timer <= 0f)
        {
            timer = interval;
            slider.SetSlider(GenerateRandom());
        }

        timer -= Engine.DeltaTime;
    }

    public float GenerateRandom()
    {
        return RandomUtils.RandomFloat(value1, value2, seed);
    }
}
