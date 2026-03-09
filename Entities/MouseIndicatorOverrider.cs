using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/MouseIndicatorOverrider")]
public class MouseIndicatorOverrider : BaseEntity
{
    public MouseIndicatorOverrider(EntityData data, Vc2 offset) : base(data, offset)
    {
        indicator = data.Attr("path", Displayers.defaultMouseIndicator);
        Add(size = new("size", 32, new(0, 1000)));
    }
    private string indicator;
    private SelectiveCounter size;

    private int currentSize;
    public override void Added(Scene scene)
    {
        base.Added(scene);

        currentSize = Md.Settings.mouseSize;
        Displayers.Instance?.mouseIndicator = GFX.Game[indicator];
        Md.Settings.mouseSize = size.Value;
    }

    public override void Removed(Scene scene)
    {
        Displayers.Instance?.mouseIndicator = GFX.Game[Displayers.defaultMouseIndicator];
        Md.Settings.mouseSize = currentSize;

        base.Removed(scene);
    }
}
