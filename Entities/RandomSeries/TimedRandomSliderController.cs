using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities.RandomSeries;

[Tracked]
[CustomEntity("ChroniaHelper/TimedRandomSliderController")]
public class TimedRandomSliderController : BaseEntity
{
    public TimedRandomSliderController(EntityData d, Vc2 o) : base(d, o)
    {
        slider = d.Attr("slider", "slider");
        value1 = d.Float("value1", 0f);
        value2 = d.Float("value2", 1f);
        interval = d.Float("interval", 1f).GetAbs().ClampMin(Engine.DeltaTime / 2f);
        mode = (Modes)d.Int("mode", 0);

        startDelay = d.Float("startDelay", -1f);
        if(mode == Modes.OnAdded)
        {
            AddedAwait = startDelay;
            active = false;
        }

        if (global = d.Bool("globalEntity", false))
        {
            Tag = Tags.Global;
        }

        string _seed = d.Attr("seed");
        if (_seed.HasValidContent())
        {
            if (int.TryParse(_seed, out int n))
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
            seed = ID;
        }
    }
    public string slider;
    public float value1, value2;
    public float interval;
    public enum Modes { OnAdded = 0 }
    public Modes mode;
    public float startDelay;
    public bool global;
    public int seed;

    public float timer;
    public bool active = false;
    protected override void AddedExecute(Scene scene)
    {
        if (global)
        {
            if (Md.Session.GlobalEntitiesRegistry.Contains(SourceId))
            {
                RemoveSelf();
                return;
            }
            Md.Session.GlobalEntitiesRegistry.Add(SourceId);
        }
        timer = 0f;
        active = true;
    }

    protected override void UpdateExecute()
    {
        if (!active)
        {
            return;
        }

        if(timer <= 0f)
        {
            slider.SetSlider(GenerateRandom());
            timer = interval;
        }

        timer -= Engine.DeltaTime;
    }

    public float GenerateRandom()
    {
        return RandomUtils.RandomFloat(value1, value2, seed);
    }
}
