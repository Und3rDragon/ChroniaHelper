using Celeste.Mod.Registry.DecalRegistryHandlers;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ChroniaHelper.DecalRegistry;

public class SessionScaleRegistryHandler : DecalRegistryHandler
{
    [LoadHook]
    public static void Load()
    {
        Celeste.Mod.DecalRegistry.AddPropertyHandler<SessionScaleRegistryHandler>();
    }

    public override string Name => "chronia.sessionScale";

    public override void Parse(XmlAttributeCollection xml)
    {
        Handler = new(
            GetString(xml, "x", ""),
            GetString(xml, "y", ""));
    }

    public override void ApplyTo(Decal decal)
    {
        Handler.AddTo(decal);
    }

    public SessionScaleRegistry Handler;
}

public class SessionScaleRegistry : BaseComponent
{
    public SessionScaleRegistry(string x, string y)
    {
        this.x = new(x, 1f);
        this.y = new(y, 1f);
    }
    public SelectiveSlider x, y;

    public override void Added(Entity entity)
    {
        base.Added(entity);

        entity.GetBaseComponents(x, y);
    }

    public override void Update()
    {
        if (base.Entity?.Scene == null) { return; }

        if (base.Entity is not Decal) { return; }

        Decal decal = base.Entity as Decal;

        Vc2 scale = Vc2.One;

        if (x.Expression.HasValidContent())
        {
            scale.X = x.Value;
        }

        if (y.Expression.HasValidContent())
        {
            scale.Y = y.Value;
        }

        decal.Scale = scale;
    }
}
