using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetFlagController")]
public class SetFlagController : GeneralSetupController
{
    public SetFlagController(EntityData data, Vc2 offset) : base(data, offset)
    {
        flags = data.Attr("flags").Split(',',StringSplitOptions.TrimEntries);
    }
    private string[] flags;

    public override void ApplyValue()
    {
        flags.SetGeneralFlags(",", "!", "*", "?");
    }
}
