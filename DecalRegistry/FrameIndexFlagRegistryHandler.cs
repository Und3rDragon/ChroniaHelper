using Celeste.Mod.Registry.DecalRegistryHandlers;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ChroniaHelper.DecalRegistry;

public class FrameIndexFlagRegistryHandler : DecalRegistryHandler
{
    [LoadHook]
    public static void Load()
    {
        Celeste.Mod.DecalRegistry.AddPropertyHandler<FrameIndexFlagRegistryHandler>();
    }

    public override string Name => "chronia.frameIndexFlag";

    public override void Parse(XmlAttributeCollection xml)
    {
        Handler = new(
            GetCSVIntWithTricks(xml, "indexes", ""),
            GetString(xml, "flags", ""));
    }

    public override void ApplyTo(Decal decal)
    {
        Handler.AddTo(decal);
    }

    public FrameIndexFlagRegistry Handler;
}

public class FrameIndexFlagRegistry : BaseComponent
{
    public FrameIndexFlagRegistry(int[] targets, string flags)
    {
        Targets = targets.ToList();
        FlagExpression = flags;
    }
    public List<int> Targets;
    public string FlagExpression;

    public override void Update()
    {
        if (base.Entity?.Scene == null) { return; }

        if (base.Entity is not Decal) { return; }

        Decal decal = base.Entity as Decal;

        FlagExpression.SetGeneralFlags(flip: Targets.Contains((int)decal.frame));
    }
}
