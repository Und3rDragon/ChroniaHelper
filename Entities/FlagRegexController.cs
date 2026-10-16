using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/FlagRegexController")]
public class FlagRegexController : GeneralSetupController
{
    public FlagRegexController(EntityData data, Vc2 offset) : base(data, offset)
    {
        regex = data.Attr("regex");
        regexMode = data.Int("regexMode", 0);
        regexParameter = data.Attr("regexParameter");
    }
    private string regex;
    private EnumWrapper<Operations> regexMode;
    private enum Operations
    {
        Off = 0,
        Replace = 1,
        Count = 2,
    }
    private string regexParameter;

    public override void ExecuteByUpdateState(bool current, bool previous)
    {
        base.ExecuteByUpdateState(current, previous);

        if(current != previous && current)
        {
            Operate();
        }
    }

    private void Operate()
    {
        if (!regex.HasValidContent()) { return; }

        Regex reg = new(regex);
        HashSet<string> flags = SceneAs<Level>().Session.Flags;

        switch ((Operations)regexMode)
        {
            case Operations.Count:
                int counts = 0;
                foreach (var f in flags)
                {
                    if (reg.IsMatch(f))
                    {
                        counts++;
                    }
                }
                if (regexParameter.HasValidContent())
                {
                    regexParameter.SetCounter(counts);
                }
                break;
            case Operations.Replace:
                if (!regexParameter.HasValidContent())
                {
                    break;
                }
                foreach (var f in flags)
                {
                    reg.Replace(f, regexParameter);
                }
                break;
            case Operations.Off:
            default:
                HashSet<string> remove = new();
                foreach (var f in flags)
                {
                    if (reg.IsMatch(f))
                    {
                        remove.Add(f);
                    }
                }
                foreach (var f in remove)
                {
                    f.SetFlag(false);
                }
                break;
        }
    }
}
