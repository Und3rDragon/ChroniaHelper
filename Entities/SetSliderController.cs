using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetSliderController")]
public class SetSliderController : GeneralSetupController
{
    public SetSliderController(EntityData data, Vc2 offset) : base(data, offset)
    {
        sliders = data.Attr("sliders").Split(',',StringSplitOptions.TrimEntries);
        value = data.Attr("value", "0");
        value2 = data.Attr("value2");
        randomize = data.Bool("randomizeValue", false);
        canRandomize = value2.HasValidContent();
    }
    private string[] sliders;
    private string value, value2;
    private bool randomize, canRandomize;

    public override void ApplyValue()
    {
        float target = 0f, alt = 0f;
        foreach (var i in sliders)
        {
            target = value.ParseMathExpression();
            if(canRandomize && randomize)
            {
                alt = value2.ParseMathExpression();
                target = RandomUtils.RandomFloat(target, alt);
            }
            i.SetSlider(target);
        }
    }
}
