using Celeste.Mod.Registry.DecalRegistryHandlers;
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

public class TextureIndexFlagRegistryHandler :DecalRegistryHandler
{
    [LoadHook]
    public static void Load()
    {
        Celeste.Mod.DecalRegistry.AddPropertyHandler<TextureIndexFlagRegistryHandler>();
    }

    public override string Name => "chronia.textureIndexFlag";

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

public class TextureIndexFlagRegistry : BaseComponent
{
    public TextureIndexFlagRegistry(int[] targets, string flags)
    {
        Targets = targets.ToList();
        FlagExpression = flags;
    }
    public List<int> Targets;
    public string FlagExpression;

    private string Path = "";
    private List<MTexture> texture = new();
    public override void Added(Entity entity)
    {
        base.Added(entity);

        if (entity is not Decal) { return; }

        Path = (entity as Decal).Name;

        texture = GFX.Game.GetAtlasSubtextures(Path);
    }

    public override void Update()
    {
        if (base.Entity?.Scene == null) { return; }

        if (base.Entity is not Decal) { return; }

        Decal decal = base.Entity as Decal;

        MTexture t = decal.textures[(int)decal.frame];

        FlagExpression.SetGeneralFlags(flip: Targets.Contains(ExtractIndex(t.AtlasPath)));
    }

    public int ExtractIndex(string input)
    {
        if (!input.HasValidContent())
            return 0;

        int endIndex = input.Length - 1;
        string numberStr = "";

        while (endIndex >= 0 && char.IsDigit(input[endIndex]))
        {
            numberStr = input[endIndex] + numberStr;
            endIndex--;
        }

        if (numberStr.Length > 0)
        {
            if (int.TryParse(numberStr, out int result))
            {
                return result;
            }
        }

        return 0;
    }
}
