using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class SelectiveSlider : BaseComponent
{
    public SelectiveSlider(string name, float fallback = 0f, Clamper.Float restraints = null) : base()
    {
        Expression = name;
        this.Fallback = fallback;
        this.Limiter = restraints ?? new();
    }
    public string Expression;
    public float Fallback;
    public Clamper.Float Limiter = new();

    public float Value => Limiter.Operate(GetValue());
    private float GetValue()
    {
        float n = Fallback;

        if (string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression))
        {
            return n;
        }

        if (float.TryParse(Expression, out n))
        {
            return n;
        }

        return Expression.GetSlider();
    }

    protected override void BeforeEntityAdded(Scene scene)
    {
        if (string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression))
        {
            return;
        }

        var counters = MaP.level?.Session?.Sliders;

        if (counters == null) { return; }

        foreach (var counter in counters)
        {
            if (counter.Key == Expression)
            {
                return;
            }
        }

        Expression.SetSlider(Fallback);
    }
}

public static class SelectiveSliderExtension
{
    public static SelectiveSlider Slider(this EntityData data, string field, float fallback = 0f)
    {
        return new SelectiveSlider(data.Attr(field), fallback);
    }
}
