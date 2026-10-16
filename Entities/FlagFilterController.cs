using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/FlagFilterController")]
public class FlagFilterController : GeneralSetupController
{
    public FlagFilterController(EntityData data, Vc2 offset) : base(data, offset)
    {
        contains = data.Attr("contains");
        starts = data.Attr("startsWith");
        ends = data.Attr("endsWith");
        operationMode = data.Int("operationMode", 0);
        operationParameter = data.Attr("operationParameter");
    }
    private string contains, starts, ends, operationParameter;
    private enum FlagTweak
    {
        Off = 0,
        Flag = 1,
        ChroniaExpression = 2,
        FrostExpression = 3,
    }
    private EnumWrapper<FlagTweak> operationMode;

    private bool GetCondition(string flag)
    {
        bool b = false;

        if (contains.HasValidContent())
        {
            b = flag.Contains(contains);
        }

        if (starts.HasValidContent())
        {
            b = flag.StartsWith(starts);
        }

        if (ends.HasValidContent())
        {
            b = flag.EndsWith(ends);
        }

        return b;
    }

    public override void ExecuteByUpdateState(bool current, bool previous)
    {
        base.ExecuteByUpdateState(current, previous);

        if (current != previous && current)
        {
            Operate();
        }
    }

    private void Operate()
    {
        Player player = SceneAs<Level>().Tracker.GetEntity<Player>();

        if (!starts.HasValidContent() &&
            !contains.HasValidContent() &&
            !ends.HasValidContent())
        {
            return;
        }

        HashSet<string> targets = new();
        HashSet<string> flags = SceneAs<Level>().Session.Flags;

        foreach (var f in flags)
        {
            if (GetCondition(f))
            {
                targets.Add(f);
            }
        }

        switch ((FlagTweak)operationMode)
        {
            case FlagTweak.Flag:
                bool value = operationParameter.HasValidContent() ? operationParameter.GetFlag() : false;
                foreach (var t in targets)
                {
                    t.SetFlag(value);
                }
                break;
            case FlagTweak.ChroniaExpression:
                value = operationParameter.ParseMathExpression() != 0;
                foreach (var t in targets)
                {
                    t.SetFlag(value);
                }
                break;
            case FlagTweak.FrostExpression:
                value = Md.FrostHelperLoaded ?
                    operationParameter.tryCreateSessionExpression(out object exp) ?
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
