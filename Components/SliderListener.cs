using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class SliderListener : StateListener
{
    public SliderListener(string name, bool inverted, params ValueTuple<float, float?>[] parameters)
    {
        this.Name = name;
        this.Inverted = inverted;
        this.References = parameters.ToList();
    }
    public string Name;
    public List<ValueTuple<float, float?>> References = new();
    public bool Inverted = false;
    public float Threshold = 0.0001f;

    public SliderListener(string name, bool inverted, params float[] parameters)
    {
        this.Name = name;
        this.Inverted = inverted;
        foreach(float n in parameters)
        {
            References.Add((n, null));
        }
    }

    private float f = 0f;
    protected override bool GetState()
    {
        f = Name.GetSlider();

        foreach(var set in References)
        {
            if(set.Item2 is null)
            {
                float n1 = set.Item1 - Threshold,
                    n2 = set.Item1 + Threshold;

                if(f.IsBetween(n1, n2))
                {
                    return !Inverted;
                }
            }
            else
            {
                float item2 = (float)set.Item2,
                    n1 = float.Min(set.Item1, item2),
                    n2 = float.Max(set.Item1, item2);

                if (f.IsBetween(n1, n2))
                {
                    return !Inverted;
                }
            }
        }

        return Inverted;
    }
}
