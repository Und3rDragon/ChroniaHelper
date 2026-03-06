using ChroniaHelper.Cores;
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
    public SelectiveSlider(string name, float fallback = 0f) : base()
    {
        Expression = name;
        this.Fallback = fallback;
    }
    public string Expression;
    public float Fallback;

    public float Value => GetValue();
    private float GetValue()
    {
        float n = Fallback;

        if (string.IsNullOrEmpty(Expression))
        {
            return n;
        }

        if (!float.TryParse(Expression, out n))
        {
            return Expression.GetSlider();
        }

        return n;
    }
}

public static class SelectiveSliderExtension
{
    public static SelectiveSlider Slider(this EntityData data, string field, float fallback = 0f)
    {
        return new SelectiveSlider(data.Attr(field), fallback);
    }
}
