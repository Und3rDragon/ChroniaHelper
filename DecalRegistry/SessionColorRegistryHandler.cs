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

public class SessionColorRegistryHandler : DecalRegistryHandler
{
    [LoadHook]
    public static void Load()
    {
        Celeste.Mod.DecalRegistry.AddPropertyHandler<SessionColorRegistryHandler>();
    }

    public override string Name => "chronia.sessioncolor";

    public override void Parse(XmlAttributeCollection xml)
    {
        Handler = new(
            GetString(xml, "r", ""),
            GetString(xml, "g", ""),
            GetString(xml, "b", ""),
            GetString(xml, "a", "")
            );
    }

    public override void ApplyTo(Decal decal)
    {
        Handler.AddTo(decal);
    }

    public ChroniaSessionEditRegistry Handler;
}

public class ChroniaSessionEditRegistry : BaseComponent
{
    public ChroniaSessionEditRegistry(string r, string g, string b, string a)
    {
        this.r = new(r, 255, new(0, 255));
        this.g = new(g, 255, new(0, 255));
        this.b = new(b, 255, new(0, 255));
        this.a = new(a, 1f, new(0, 1f));
    }
    public SelectiveCounter r, g, b;
    public SelectiveSlider a;

    private Vc2 initPos;
    public override void Added(Entity entity)
    {
        base.Added(entity);

        entity.GetBaseComponents(r, g, b, a);
    }

    public override void Update()
    {
        if (base.Entity?.Scene == null) { return; }

        if (base.Entity is not Decal) { return; }

        Decal decal = base.Entity as Decal;

        int _r = 255, _g = 255, _b = 255;
        float _a = 1f;

        if (r.Expression.HasValidContent())
        {
            _r = r.Value;
        }

        if (g.Expression.HasValidContent())
        {
            _g = g.Value;
        }

        if (b.Expression.HasValidContent())
        {
            _b = b.Value;
        }

        if (a.Expression.HasValidContent())
        {
            _a = a.Value;
        }

        CColor color = new(_r, _g, _b, _a);

        decal.Color = color.Parsed();
    }
}
