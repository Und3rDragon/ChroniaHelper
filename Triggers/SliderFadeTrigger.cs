using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/SliderFadeTrigger")]
public class SliderFadeTrigger : BaseTrigger
{
    public SliderFadeTrigger(EntityData data, Vc2 offset) : base(data, offset)
    {
        name = data.Attr("sliderName");
        _fadeFrom = data.Attr("fadeFrom");
        fadeTo = data.Float("fadeTo", 0);
        positionMode = data.Attr("positionMode", "NoEffect").MatchEnum(PositionModes.NoEffect);
        easeMode = data.Attr("easing", "Linear").MatchEnum(EaseMode.Linear);
        timed = data.Float("timed", -1f);
    }
    public string _fadeFrom, name;
    public float fadeFrom, fadeTo, timed, timer;
    public PositionModes positionMode;
    public EaseMode easeMode;

    protected override void OnEnterExecute(Player player)
    {
        if (_fadeFrom.IsNullOrEmpty())
        {
            fadeFrom = name.GetSlider();
        }
        else
        {
            fadeFrom = _fadeFrom.ParseFloat(0f);
        }

        timer = timed;
    }

    protected override void OnStayExecute(Player player)
    {
        if (timed >= 0f) { return; }
        
        float p = GetPositionLerp(player, positionMode);

        name.SetSlider(p.LerpValue(0f, 1f, fadeFrom, fadeTo, easeMode));
    }

    public override void Update()
    {
        base.Update();

        if (timed < 0) { return; }

        if(timer > 0f)
        {
            timer = Calc.Approach(timer, 0f, Engine.DeltaTime);
            name.SetSlider(timer.LerpValue(timed, 0f, fadeFrom, fadeTo, easeMode));
        }
    }
}
