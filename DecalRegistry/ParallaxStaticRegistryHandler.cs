using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using MonoMod.Cil;

namespace ChroniaHelper.DecalRegistry;

public class ParallaxStaticRegistryHandler : DecalRegistryHandler
{
    public override string Name => "chronia.parallaxStatic";

    public override void Parse(XmlAttributeCollection xml)
    {
        StaticValue.X = this.Get<float>(xml, "x", 160f);
        StaticValue.Y = this.Get<float>(xml, "y", 90f);
        Handler = new(StaticValue);
    }

    public Vc2 StaticValue = new Vc2(160f, 90f);
    private ParallaxStatic Handler;

    public override void ApplyTo(Decal decal)
    {
        Handler.AddTo(decal);
    }
}

public class ParallaxStatic : BaseComponent
{
    public override bool Equals(object obj)
    {
        if (obj is ParallaxStatic ps)
        {
            return ps.StaticValue == StaticValue;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return StaticValue.GetHashCode();
    }
    
    public static bool operator ==(ParallaxStatic a, ParallaxStatic b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(ParallaxStatic a, ParallaxStatic b)
    {
        return !(a == b);
    }

    public ParallaxStatic(Vc2 staticValue)
    {
        StaticValue = staticValue;
    }

    public Vc2 StaticValue = Vc2.One;

    [LoadHook]
    public static void Load()
    {
        On.Celeste.Decal.Added += HookParallax;
        // IL.Celeste.Decal.Render += RenderModification;
        On.Celeste.Decal.Render += AfterRender;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Decal.Added -= HookParallax;
        // IL.Celeste.Decal.Render -= RenderModification;
        On.Celeste.Decal.Render += AfterRender;
    }

    public static void HookParallax(On.Celeste.Decal.orig_Added orig, Decal self, Scene scene)
    {
        orig(self, scene);

        List<ParallaxStatic> comps = new();
        foreach (var i in self.Components)
        {
            if (i is ParallaxStatic ps)
            {
                comps.Add(ps);
            }
        }

        if (comps.Count > 0)
        {
            self.parallax = true;
        }
    }

    // public static void RenderModification(ILContext il)
    // {
    //     ILCursor c = new ILCursor(il);
    //     
    //     MethodInfo m = typeof(Vc2).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
    //
    //     if (c.TryGotoNext(MoveType.After,
    //             ins => ins.MatchCall(m),
    //             ins => ins.MatchStfld<Decal>("Position")))
    //     {
    //         c.EmitDelegate(DelegateUtils.SetDecalParallaxStatic);
    //     }
    // }

    public static void AfterRender(On.Celeste.Decal.orig_Render orig, Decal self)
    {
        orig(self);

        ParallaxStatic parent = null;
        foreach (var i in self.Components)
        {
            if (i is ParallaxStatic ps)
            {
                parent = ps;
                break;
            }
        }

        if (parent != null)
        {
            self.Position -= new Vc2(160f, 90f);
            self.Position += parent.StaticValue;
        }
    }
}