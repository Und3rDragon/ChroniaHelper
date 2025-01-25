using Celeste.Mod.Entities;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/teraBarrier")]
public class TeraBarrier : Solid
{
    private List<Vector2> particles = new List<Vector2>();
    private float[] speeds = new float[3] { 12f, 20f, 40f };
    public TeraType tera { get; private set; }
    private Image image;
    private static ILHook playerUpdateHook;

    public TeraBarrier(Vector2 position, float width, float height, TeraType tera)
        : base(position, width, height, safe: false)
    {
        Collidable = false;
        for (int i = 0; (float)i < Width * Height / 16f; i++)
        {
            particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
        }
        this.tera = tera;
        Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
        image.CenterOrigin();
        image.Position = new Vector2(width/2, height/2);
    }

    public TeraBarrier(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Enum("tera", TeraType.Normal))
    {
    }
    public static void OnLoad()
    {
        playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), UpdateTeraBarrier);
    }
    public static void OnUnload()
    {
        playerUpdateHook?.Dispose();
    }
    private static void UpdateTeraBarrier(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchRet()))
        {
            Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on tera barrier at {cursor.Index} in IL for {cursor.Method.Name}");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CollideBarrier);
        }
    }

    private static void CollideBarrier(Player player)
    {
        var tera = player.GetTera();
        var lastTera = tera;
        foreach (TeraBarrier barrier in player.Scene.Tracker.GetEntities<TeraBarrier>())
        {
            barrier.Collidable = true;
            bool collide = player.CollideCheck(barrier);
            barrier.Collidable = false;
            if (collide && barrier.tera != lastTera)
                lastTera = barrier.tera;
        }
        if (lastTera != tera)
            player.ChangeTera(lastTera);
    }

    public override void Update()
    {
        int num = speeds.Length;
        float height = Height;
        int i = 0;
        for (int count = particles.Count; i < count; i++)
        {
            Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
            value.Y %= height - 1f;
            particles[i] = value;
        }
        base.Update();
    }

    public override void Render()
    {
        Color color = TeraUtil.GetColor(tera);
        foreach (Vector2 particle in particles)
        {
            Draw.Pixel.Draw(Position + particle, Vector2.Zero, color * 0.5f);
        }
        Draw.Rect(Collider, color * 0.1f);
        image.Render();
    }
}