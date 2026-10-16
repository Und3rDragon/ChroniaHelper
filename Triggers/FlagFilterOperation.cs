using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/FlagFilterOperation")]
public class FlagFilterOperation : BaseTrigger
{
    public FlagFilterOperation(EntityData data, Vc2 offset) : base(data, offset)
    {
        contains = data.Attr("contains");
        starts = data.Attr("startsWith");
        ends = data.Attr("endsWith");
        mode = data.Int("mode", 0);
        parameter = data.Attr("parameter");
    }
    private string contains, starts, ends, parameter;
    private enum FlagTweak
    {
        Off = 0,
        Flag = 1,
        ChroniaExpression = 2,
        FrostExpression = 3,
    }
    private EnumWrapper<FlagTweak> mode;

    private bool GetCondition(string flag)
    {
        bool b = false;

        if (contains.HasValidContent())
        {
            b = flag.Contains(contains);
        }

        if(starts.HasValidContent())
        {
            b = flag.StartsWith(starts);
        }

        if (ends.HasValidContent())
        {
            b = flag.EndsWith(ends);
        }

        return b;
    }

    protected override void OnEnterExecute(Player player)
    {
        base.OnEnterExecute(player);

        if(!starts.HasValidContent() && 
            !contains.HasValidContent() &&
            !ends.HasValidContent())
        {
            return;
        }

        HashSet<string> targets = new();
        HashSet<string> flags = SceneAs<Level>().Session.Flags;

        foreach(var f in flags)
        {
            if (GetCondition(f))
            {
                targets.Add(f);
            }
        }

        switch ((FlagTweak)mode)
        {
            case FlagTweak.Flag:
                bool value = parameter.HasValidContent() ? parameter.GetFlag() : false;
                foreach (var t in targets)
                {
                    t.SetFlag(value);
                }
                break;
            case FlagTweak.ChroniaExpression:
                value = parameter.ParseMathExpression() != 0;
                foreach(var t in targets)
                {
                    t.SetFlag(value);
                }
                break;
            case FlagTweak.FrostExpression:
                value = Md.FrostHelperLoaded ?
                    parameter.tryCreateSessionExpression(out object exp) ?
                        exp.getFloatSessionExpressionValue() != 0 : false
                    : false;
                foreach (var t in targets)
                {
                    t.SetFlag(value);
                }
                break;
            case FlagTweak.Off:
            default:
                foreach (var t in targets)
                {
                    t.SetFlag(false);
                }
                break;
        }
    }
}
