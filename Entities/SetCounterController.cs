using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetCounterController")]
public class SetCounterController : GeneralSetupController
{
    public SetCounterController(EntityData data, Vc2 offset) : base(data, offset)
    {
        counters = data.Attr("counters").Split(',',StringSplitOptions.TrimEntries);
        value = data.Attr("value", "0");
        value2 = data.Attr("value2");
        randomize = data.Bool("randomizeValue", false);
        canRandomize = value2.HasValidContent();

        valueType = (ValueType)data.Int("valueType", 0);
    }
    private string[] counters;
    private string value, value2;
    private bool randomize, canRandomize;
    private enum ValueType
    {
        Set, Add, Minus, Multiply, Divide
    }
    private ValueType valueType;

    public override void Execute()
    {
        int target = 0, alt = 0;
        foreach (var i in counters)
        {
            target = (int)value.ParseMathExpression();
            if (canRandomize && randomize)
            {
                alt = (int)value2.ParseMathExpression();
                target = RandomUtils.RandomInt(target, alt);
            }
            
            if(valueType == ValueType.Add)
            {
                i.SetCounter(i.GetCounter() + target);
            }
            else if(valueType == ValueType.Minus)
            {
                i.SetCounter(i.GetCounter() - target);
            }
            else if(valueType == ValueType.Multiply)
            {
                i.SetCounter(i.GetCounter() * target);
            }
            else if(valueType == ValueType.Divide)
            {
                if(target == 0)
                {
                    i.SetCounter(10000000); // represents a rather large value, not specifically
                }
                else
                {
                    i.SetCounter(i.GetCounter() / target);
                }
            }
            else
            {
                i.SetCounter(target);
            }
        }
    }
}
