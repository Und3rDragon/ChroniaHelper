using Celeste.Mod.Entities;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace ChroniaHelper.Entities
{
    [Tracked(false)]
    [CustomEntity("ChroniaHelper/teraZipMover")]
    public class TeraZipMover : ZipMover, ITeraBlock
    {
        private static ILHook sequenceHook;

        public TeraType tera { get; set; }
        private Image image;
        private TeraEffect lastEffect = TeraEffect.None;

        public TeraZipMover(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
        {
            tera = data.Enum("tera", TeraType.Normal);
            Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
            image.CenterOrigin();
            image.Position = new Vector2(data.Width / 2, data.Height / 2);
            var bloom = Get<BloomPoint>();
            bloom.Alpha = 0.3f;
        }
        public static void OnLoad()
        {
            sequenceHook = new ILHook(typeof(ZipMover).GetMethod("Sequence", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), TeraSequence);
        }
        public static void OnUnload()
        {
            sequenceHook?.Dispose();
        }

        private static void TeraSequence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Solid>("HasPlayerRider")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on zip mover activate at {cursor.Index} in IL for {cursor.Method.Name}");
                ILLabel label = null;
                cursor.GotoNext(MoveType.After, instr => instr.MatchBrfalse(out label));
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate(PlayerActivate);
                cursor.Emit(OpCodes.Brfalse, label);
            }
            cursor.Index = 0;
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Engine>("get_DeltaTime")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on zip mover speed at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Index++;
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate(GetSpeedMultipler);
                cursor.Emit(OpCodes.Mul);
            }
        }
        private static bool PlayerActivate(ZipMover block)
        {
            if (block is not TeraZipMover teraBlock)
                return true;
            var player = teraBlock.SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null) return false;
            teraBlock.lastEffect = teraBlock.EffectAsDefender(player.GetTera());
            return teraBlock.lastEffect != TeraEffect.None;
        }
        private static float GetSpeedMultipler(ZipMover block)
        {
            if (block is not TeraZipMover teraBlock)
                return 1f;
            return teraBlock.lastEffect switch
            {
                TeraEffect.Super => 2f,
                TeraEffect.Normal => 1f,
                TeraEffect.Bad => 0.5f,
                TeraEffect.None => 0.5f,
                _ => throw new NotImplementedException()
            };
        }
        public TeraEffect EffectAsAttacker(TeraType t)
        {
            return TeraUtil.GetEffect(tera, t);
        }

        public TeraEffect EffectAsDefender(TeraType t)
        {
            return TeraUtil.GetEffect(t, tera);
        }
        public void ChangeTera(TeraType newTera)
        {
            tera = newTera;
            image.Texture = GFX.Game[TeraUtil.GetImagePath(tera)];
        }
    }
}
