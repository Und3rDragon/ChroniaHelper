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

[CustomEntity("ChroniaHelper/CounterFadeTrigger")]
public class CounterFadeTrigger : BaseTrigger
{
    public CounterFadeTrigger(EntityData data, Vc2 offset) : base(data, offset)
    {
        name = data.Attr("counterName");
        _fadeFrom = data.Attr("fadeFrom");
        fadeTo = data.Int("fadeTo", 0);
        positionMode = data.Attr("positionMode", "NoEffect").MatchEnum(PositionModes.NoEffect);
        easeMode = data.Attr("easing", "Linear").MatchEnum(EaseMode.Linear);
    }
    public string _fadeFrom, name;
    public int fadeFrom, fadeTo;
    public PositionModes positionMode;
    public EaseMode easeMode;

    protected override void OnEnterExecute(Player player)
    {
        if (_fadeFrom.IsNullOrEmpty())
        {
            fadeFrom = name.GetCounter();
        }
        else
        {
            fadeFrom = _fadeFrom.ParseInt(0);
        }
    }

    protected override void OnStayExecute(Player player)
    {
        float p = GetPositionLerp(player, positionMode);

        name.SetCounter(p.LerpValue(0f, 1f, fadeFrom, fadeTo, easeMode));
    }
}
