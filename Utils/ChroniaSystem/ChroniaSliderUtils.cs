using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.ChroniaSystem;

public static class ChroniaSliderUtils
{
    public static void SliderRefresh()
    {
        HashSet<string> removing = new();

        Md.SaveData.ChroniaSliders.EachDo((s) =>
        {
            if (!s.Value.Operating())
            {
                removing.Add(s.Key);
            }
        });

        removing.EachDo((i) =>
        {
            Md.SaveData.ChroniaSliders.SafeRemove(i);
        });
    }
    public static bool CheckSlider(this string name)
    {
        if (MaP.session == null) { return false; }

        foreach (var item in MaP.session.Sliders)
        {
            if (item.Key == name) { return true; }
        }

        return false;
    }

    public static bool CheckSliderRecord(this string name)
    {
        return Md.SaveData.ChroniaSliders.ContainsKey(name);
    }

    public static ChroniaSlider PullSlider(this string name)
    {
        ChroniaSlider s = new()
        {
            Value = name.GetSlider()
        };

        Md.SaveData.ChroniaSliders.Create(name, s);

        return Md.SaveData.ChroniaSliders[name];
    }

    public static void PushSlider(this ChroniaSlider Slider, string name)
    {
        Md.SaveData.ChroniaSliders.Enter(name, Slider);
        Slider.SetSlider(name);
    }

    public static void ResetSlider(this string name)
    {
        if (name.CheckSliderRecord())
        {
            name.PullSlider().Reset();
            name.PullSlider().SetSlider(name);
        }
    }

    public static float GetSlider(this string name)
    {
        return MaP.session.GetSlider(name);
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
