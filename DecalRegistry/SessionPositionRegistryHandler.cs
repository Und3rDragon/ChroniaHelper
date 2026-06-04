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

[RegistryHandler]
public class SessionPositionRegistryHandler : DecalRegistryHandler
{
    public override string Name => "chronia.sessionPosition";

    public override void Parse(XmlAttributeCollection xml)
    {
        Handler = new(GetString(xml, "x", ""),
            GetString(xml, "y", ""));
    }

    public override void ApplyTo(Decal decal)
    {
        Handler.AddTo(decal);
    }

    public SessionPositionRegistry Handler;
}

public class SessionPositionRegistry : BaseComponent
{
    public SessionPositionRegistry(string x, string y)
    {
        this.x = new(x);
        this.y = new(y);
    }
    public SelectiveSlider x, y;

    private Vc2 initPos;
    public override void Added(Entity entity)
    {
        base.Added(entity);

        initPos = entity.Position;

        entity.GetBaseComponents(x, y);
    }

    //public override void Removed(Entity entity)
    //{
    //    entity.Position = initPos;

    //    base.Removed(entity);
    //}

    public override void Update()
    {
        if (base.Entity?.Scene == null) { return; }

        if (base.Entity is not Decal) { return; }

        Decal decal = base.Entity as Decal;

        Vc2 pos = initPos;

        if (x.Expression.HasValidContent())
        {
            pos.X = x.Value;
        }

        if (y.Expression.HasValidContent())
        {
            pos.Y = y.Value;
        }

        decal.Position = pos;
    }
}
