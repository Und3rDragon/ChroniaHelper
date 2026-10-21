using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/FlagRegexOperation")]
public class FlagRegexOperation : BaseTrigger
{
    public FlagRegexOperation(EntityData data, Vc2 offset) : base(data, offset)
    {
        regex = data.Attr("regex");
        mode = data.Int("mode", 0);
        parameter = data.Attr("parameter");
    }
    private string regex;
    private EnumWrapper<Operations> mode;
    private enum Operations
    {
        Off = 0,
        Replace = 1,
        Count = 2,
    }
    private string parameter;

    protected override void OnEnterExecute(Player player)
    {
        base.OnEnterExecute(player);

        if (!regex.HasValidContent()) { return; }

        Regex reg = new(regex);
        HashSet<string> flags = SceneAs<Level>().Session.Flags;

        switch ((Operations)mode)
        {
            case Operations.Count:
                int counts = 0;
                foreach(var f in flags)
                {
                    if (reg.IsMatch(f))
                    {
                        counts++;
                    }
                }
                if (parameter.HasValidContent())
                {
                    parameter.SetCounter(counts);
                }
                break;
            case Operations.Replace:
                if (!parameter.HasValidContent())
                {
                    break;
                }
                foreach(var f in flags)
                {
                    reg.Replace(f, parameter);
                }
                break;
            case Operations.Off:
            default:
                HashSet<string> remove = new();
                foreach(var f in flags)
                {
                    if (reg.IsMatch(f))
                    {
                        remove.Add(f);
                    }
                }
                foreach(var f in remove)
                {
                    f.SetFlag(false);
                }
                break;
        }
    }
}
