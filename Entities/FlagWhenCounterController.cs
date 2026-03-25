using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Entities.RandomSeries;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using IL.Celeste.Mod.Registry.DecalRegistryHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/FlagWhenCounterController")]
public class FlagWhenCounterController : BaseEntity
{
    public FlagWhenCounterController(EntityData d, Vc2 o) : base(d, o)
    {
        flag = d.Attr("flag", "flag");
        counter = d.Attr("counter", "counter");
        paras = d.Attr("values","1,2-3,4-6");
        inverted = d.Bool("inverted", false);
        listener = new(counter, inverted, paras)
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
    public string flag, counter;
    public string paras;
    public bool inverted;
    public bool global;
    public CounterListener listener;

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
