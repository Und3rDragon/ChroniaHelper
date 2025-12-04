using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.ChroniaSystem;

public static class ChroniaSliderUtils
{
    public static float GetSlider(this string name)
    {
        return MaP.level.Session.GetSlider(name);
    }

    public static void SetSlider(this string name, float value)
    {
        MaP.level.Session.SetSlider(name, value);
    }

    public static void SetSlider(this ICollection<string> source, float state)
    {
        foreach (var item in source)
        {
            item.SetSlider(state);
        }
    }

    public static void SetSlider<Type>(this ICollection<Type> source, Func<Type, string> translator, float state)
    {
        foreach (var item in source)
        {
            translator(item).SetSlider(state);
        }
    }

    public static void SetSlider<Type>(this ICollection<Type> source, Func<Type, string> createItem, Func<Type, float> getState)
    {
        foreach (var entry in source)
        {
            createItem(entry).SetSlider(getState(entry));
        }
    }
}
