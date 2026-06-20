using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System.Xml.Schema;
using static ChroniaHelper.Entities.SeamlessSpinner;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetFlagController")]
public class SetFlagController : GeneralSetupController
{
    public SetFlagController(EntityData data, Vc2 offset) : base(data, offset)
    {
        flags = data.Attr("flags").Split(',',StringSplitOptions.TrimEntries);

        valueType = (ValueType)data.Int("valueType", 0);
    }
    private string[] flags;
    private enum ValueType { Set, Toggle }
    private ValueType valueType;

    public override void Execute()
    {
        if(valueType == ValueType.Toggle)
        {
            flags.ToggleGeneralFlags("*", "?");
        }
        else
        {
            flags.SetGeneralFlags("!", "*", "?");
        }
    }
}
