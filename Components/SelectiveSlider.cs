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

public class SelectiveSlider : SelectiveSessionValue
{
    public SelectiveSlider(string name, float fallback = 0f, 
        Clamper.Float restraints = null) : base(name)
    {
        this.Fallback = fallback;
        this.Limiter = restraints ?? new();
    }
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

    public override void EntityAdded(Scene scene)
    {
        if (string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression))
        {
            return;
        }

        if (float.TryParse(Expression, out float f)) { return; }

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

        base.EntityAdded(scene);
    }
}

public static class SelectiveSliderExtension
{
    public static SelectiveSlider Slider(this EntityData data, string field, float fallback = 0f, Clamper.Float limiter = null)
    {
        return new SelectiveSlider(data.Attr(field), fallback, limiter);
    }
}
