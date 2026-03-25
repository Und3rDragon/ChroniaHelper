using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using IL.Celeste.Mod.Registry.DecalRegistryHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/FlagWhenSliderController")]
public class FlagWhenSliderController : BaseEntity
{
    public FlagWhenSliderController(EntityData d, Vc2 o) : base(d, o)
    {
        flag = d.Attr("flag", "flag");
        slider = d.Attr("slider", "slider");
        paras = d.Attr("values", "0,0.5-1.2");
        inverted = d.Bool("inverted", false);
        listener = new(slider, inverted, paras)
        {
            onEnable = () =>
            {
                flag.SetFlag(true);
            },
            onDisable = () =>
            {
                flag.SetFlag(false);
            }
        };
        Add(listener);

        global = d.Bool("globalEntity");
        if (global)
        {
            Tag = Tags.Global;
        }
    }
    public string flag, slider;
    public string paras;
    public bool inverted;
    public bool global;
    public SliderListener listener;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (global)
        {
            if (Md.Session.GlobalEntitiesRegistry.Contains(SourceId))
            {
                RemoveSelf();
                return;
            }
            Md.Session.GlobalEntitiesRegistry.Add(SourceId);
        }
    }
}
