using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace ChroniaHelper.DecalRegistry;

[RegistryHandler]
public class ParallaxStaticRegistryHandler : DecalRegistryHandler
{
    public override string Name => "chronia.parallaxStatic";

    public override void Parse(XmlAttributeCollection xml)
    {
        x = this.GetString(xml, "x", "160");
        y = this.GetString(xml, "y", "90");
        Handler = new(x, y);
    }
    private string x, y;
    private ParallaxStatic Handler;

    public override void ApplyTo(Decal decal)
    {
        if (!decal.Contains(Handler))
        {
            Handler.AddTo(decal);
        }
    }
}

public class ParallaxStatic : BaseComponent
{
    public ParallaxStatic(string x, string y)
    {
        StaticValueX = new(x, 160f);
        StaticValueY = new(y, 90f);
    }
    public SelectiveSlider StaticValueX, StaticValueY;
    public Vc2 StaticValue => new Vc2(StaticValueX.Value, StaticValueY.Value);

    public override void Update()
    {
        StaticValueX?.Update();
        StaticValueY?.Update();
    }

    [LoadHook]
    public static void Load()
    {
        On.Celeste.Decal.Added += HookParallax;
        IL.Celeste.Decal.Render += RenderModification;
        // On.Celeste.Decal.Render += HookRender;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Decal.Added -= HookParallax;
        IL.Celeste.Decal.Render -= RenderModification;
        // On.Celeste.Decal.Render += HookRender;
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

    public static Decal parent { get; private set; }

    public static void RenderModification(ILContext il)
    {
        ILCursor c = new ILCursor(il);
    
        // 定位到 new Vector2(160f, 90f) 创建之后
        if (c.TryGotoNext(
                MoveType.After,
                ins => ins.MatchLdcR4(160f),
                ins => ins.MatchLdcR4(90f),
                ins => ins.OpCode == OpCodes.Newobj &&
                       ins.Operand is MethodReference mr &&
                       mr.DeclaringType.FullName == "Microsoft.Xna.Framework.Vector2" &&
                       mr.Name == ".ctor"))
        {
            // 此时栈顶是 Vector2(160f, 90f)
            // 加载 this
            c.Emit(OpCodes.Ldarg_0);
        
            // 调用委托：传入 (fallback, decal) -> 返回 Vector2
            c.EmitDelegate<Func<Vector2, Decal, Vector2>>((Vector2 fallback, Decal decal) =>
            {
                var list = decal.Components.GetAll<ParallaxStatic>();
                if (list.Count() > 0)
                    return list.First().StaticValue;
                return fallback;
            });
        
            // 此时栈顶是委托的返回值（要么是原来的(160f,90f)，要么是StaticValue）
            // 后续的 op_Addition 会正常使用这个值
        }
    }

    // public static void HookRender(On.Celeste.Decal.orig_Render orig, Decal self)
    // {
    //     Vc2 initialPosition = self.Position;
    //     
    //     orig(self);
    //
    //     Vc2 processedPosition = self.Position;
    //
    //     Vc2 vanillaProcess = processedPosition - initialPosition;
    //
    //     var L = self.Components.GetAll<ParallaxStatic>();
    //     
    //     if (L.Count() > 0 && self.parallaxAmount == 0)
    //     {
    //         ParallaxStatic ps = L.First();
    //         
    //         // recalculate position
    //         self.Position = (self.Scene as Level).Camera.Position + ps.StaticValue + vanillaProcess;
    //     }
    // }
}