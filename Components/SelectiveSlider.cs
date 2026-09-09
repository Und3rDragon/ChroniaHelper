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

        _valid = !(string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression));
        if (_valid)
        {
            if (float.TryParse(Expression, out float n))
            {
                _cached = n;
            }
        }
    }
    public float Fallback;
    public Clamper.Float Limiter = new();

    private bool _valid;
    private float? _cached = null;

    public float Value => Limiter.Operate(GetValue());
    private float GetValue()
    {
        if (!_valid) { return Fallback; }

        if(_cached != null) { return (float)_cached; }

        return Expression.GetSlider(Fallback);
    }
}

public static class SelectiveSliderExtension
{
    public static SelectiveSlider Slider(this EntityData data, string field, float fallback = 0f, Clamper.Float limiter = null)
    {
        return new SelectiveSlider(data.Attr(field), fallback, limiter);
    }
}
