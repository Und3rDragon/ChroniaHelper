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

public class ChroniaSessionEditRegistryHandler : DecalRegistryHandler
{
    [LoadHook]
    public static void Load()
    {
        Celeste.Mod.DecalRegistry.AddPropertyHandler<ChroniaSessionEditRegistryHandler>();
    }

    public override string Name => "chronia.sessionEdit";

    public override void Parse(XmlAttributeCollection xml)
    {
        Handler = new(
            GetString(xml, "r", ""),
            GetString(xml, "g", ""),
            GetString(xml, "b", ""),
            GetString(xml, "a", ""),
            GetString(xml, "scaleX", ""),
            GetString(xml, "scaleY", ""),
            GetString(xml, "x", ""),
            GetString(xml, "y", "")
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
    public ChroniaSessionEditRegistry(string r, string g, string b, string a,
        string scaleX, string scaleY, string x, string y)
    {
        this.r = new(r, 255, new(0, 255));
        this.g = new(g, 255, new(0, 255));
        this.b = new(b, 255, new(0, 255));
        this.a = new(a, 1f, new(0, 1f));
        this.sx = new(scaleX, 1f);
        this.sy = new(scaleY, 1f);
        this.x = new(x);
        this.y = new(y);
    }
    public SelectiveCounter r, g, b;
    public SelectiveSlider a, sx, sy, x, y;

    private Vc2 initPos;
    public override void Added(Entity entity)
    {
        base.Added(entity);

        initPos = entity.Position;

        entity.GetBaseComponents(r, g, b, a, sx, sy);
    }

    public override void Removed(Entity entity)
    {
        entity.Position = initPos;

        base.Removed(entity);
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

        float _sx = 1f, _sy = 1f;

        if (sx.Expression.HasValidContent())
        {
            _sx = sx.Value;
        }

        if (sy.Expression.HasValidContent())
        {
            _sy = sy.Value;
        }

        decal.Scale = new Vc2(_sx, _sy);

        Vc2 pos = initPos;

        if (x.Expression.HasValidContent())
        {
            pos.X = x.Value;
        }

        if (y.Expression.HasValidContent())
        {
            pos.Y = y.Value;
        }

        Entity.Position = pos;
    }
}
