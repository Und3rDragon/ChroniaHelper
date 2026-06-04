using Celeste.Mod.Registry.DecalRegistryHandlers;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ChroniaHelper.DecalRegistry;

[RegistryHandler]
public class ChroniaColorRegistryHandler : DecalRegistryHandler
{
    public override string Name => "chronia.color";

    public override void Parse(XmlAttributeCollection xml)
    {
        Handler = new(base.GetString(xml, "tag", ""));
    }

    public override void ApplyTo(Decal decal)
    {
        Handler.AddTo(decal);
    }

    public ChroniaColorRegistry Handler;
}

public class ChroniaColorRegistry : BaseComponent
{
    public ChroniaColorRegistry(string colorName)
    {
        ColorName = colorName;
    }
    public string ColorName;

    public override void Update()
    {
        if (!ColorName.HasValidContent()) { return; }
        
        if (base.Entity?.Scene == null) { return; }
        
        if (base.Entity is not Decal) { return; }
        
        Decal decal = base.Entity as Decal;

        decal.Color = Md.Session.chroniaColors.GetValueOrDefault(ColorName, CColor.White).Parsed();
    }
}
