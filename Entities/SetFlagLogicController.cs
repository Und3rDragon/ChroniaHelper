using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.LogicExpression;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/SetFlagLogicController")]
public class SetFlagLogicController : GeneralSetupController
{
    public SetFlagLogicController(EntityData data, Vector2 offset) : base(data, offset)
    {
        flags = data.Attr("flags").Split(',',StringSplitOptions.TrimEntries);
        value = data.Attr("value");
        emptyExpressionValue = data.Bool("emptyExpressionValue");

        allowRevert = data.Bool("allowRevert", false);
        revertValue = data.Attr("revertValue");
        emptyRevertExpressionValue = data.Bool("emptyRevertExpressionValue");
    }
    private string[] flags;
    private string value;
    private string revertValue;
    private bool emptyExpressionValue;
    private bool allowRevert;
    private bool emptyRevertExpressionValue;

    public override void Execute()
    {
        base.Execute();

        foreach (var i in flags)
        {
            i.SetFlag(value.ParseLogicExpression(fallback: emptyExpressionValue));
        }
    }

    public override void Revert()
    {
        base.Revert();

        if (!allowRevert)
        {
            return;
        }

        bool b = false;
        if (revertValue.HasValidContent())
        {
            b = revertValue.ParseLogicExpression(fallback: emptyRevertExpressionValue);
        }
        
        foreach (var i in flags)
        {
            i.SetFlag(b);
        }
    }
}