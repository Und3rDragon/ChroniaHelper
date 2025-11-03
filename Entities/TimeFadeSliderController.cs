using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/TimeFadeSliderController")]
public class TimeFadeSliderController : BaseEntity
{
    public TimeFadeSliderController(EntityData data, Vc2 offset) : base(data, offset)
    {
        flag = data.Attr("flag");
        sliderName = data.Attr("sliderName");
        target = data.Int("targetValue", 0);
        duration = data.Float("duration", 1f).ClampMin(0.001f);
        easeMode = data.Attr("easing", "Linear").MatchEnum(EaseMode.Linear);
    }
    public string flag, sliderName;
    public float target;
    public float duration;
    public EaseMode easeMode;

    protected override IEnumerator AddedRoutine(Scene scene)
    {
        while (!flag.GetFlag()) { yield return null; }

        float start = sliderName.GetSlider();
        float end = target;

        float p = 0f;
        while (p < 1f)
        {
            p = Calc.Approach(p, 1f, Engine.DeltaTime / duration);
            sliderName.SetSlider(p.LerpValue(0f, 1f, start, end, easeMode));
        }
    }
}
