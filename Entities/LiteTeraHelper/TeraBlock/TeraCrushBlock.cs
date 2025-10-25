using System;
using System.Reflection;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace ChroniaHelper.Entities
{
    [Tracked(false)]
    [CustomEntity("ChroniaHelper/teraKevinBlock")]
    public class TeraCrushBlock : CrushBlock, ITeraBlock
    {
        private static ILHook sequenceHook;

        public TeraType tera { get; set; }
        private Image image;
        private TeraEffect lastEffect = TeraEffect.None;

        public TeraCrushBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Enum("axes", Axes.Both), false)
        {
            tera = data.Enum("tera", TeraType.Normal);
            Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
            image.CenterOrigin();
            image.Position = new Vector2(data.Width / 2, data.Height / 2 - 12);
        }
        [LoadHook]
        public static void OnLoad()
        {
            On.Celeste.CrushBlock.CanActivate += TeraCanActivate;
            sequenceHook = new ILHook(typeof(CrushBlock).GetMethod("AttackSequence", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), TeraSequence);
        }
        [UnloadHook]
        public static void OnUnload()
        {
            On.Celeste.CrushBlock.CanActivate -= TeraCanActivate;
            sequenceHook?.Dispose();
        }
        private static bool TeraCanActivate(On.Celeste.CrushBlock.orig_CanActivate orig, CrushBlock self, Vector2 direction)
        {
            var res = orig(self, direction);
            if (res && self is TeraCrushBlock teraCrush)
            {
                var player = teraCrush.Scene.Tracker.GetEntity<Player>();
                teraCrush.lastEffect = teraCrush.EffectAsDefender(player.GetTera());
                if (teraCrush.lastEffect == TeraEffect.None)
                    return false;
            }
            return res;
        }
        private static void TeraSequence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(240f) || instr.MatchLdcR4(60f)))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on kevin block speed at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate(GetSpeedMultipler);
                cursor.Emit(OpCodes.Mul);
            }
        }
        private static float GetSpeedMultipler(CrushBlock block)
        {
            if (block is not TeraCrushBlock teraBlock)
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
