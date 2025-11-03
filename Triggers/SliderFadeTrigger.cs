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
    }
    public string _fadeFrom, name;
    public float fadeFrom, fadeTo;
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
    }

    protected override void OnStayExecute(Player player)
    {
        float p = GetPositionLerp(player, positionMode);

        name.SetSlider(p.LerpValue(0f, 1f, fadeFrom, fadeTo, easeMode));
    }
}
