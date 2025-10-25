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
    [CustomEntity("ChroniaHelper/teraCoreBlock")]
    public class TeraBounceBlock : BounceBlock, ITeraBlock
    {
        public TeraType tera { get; set; }
        private Image image;
        private TeraEffect lastEffect = TeraEffect.None;

        public TeraBounceBlock(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            tera = data.Enum("tera", TeraType.Normal);
            Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
            image.CenterOrigin();
            image.Position = new Vector2(data.Width / 2, data.Height / 2);
        }
        [LoadHook]
        public static void OnLoad()
        {
            IL.Celeste.BounceBlock.Update += TeraBounceUpdate;
        }
        [UnloadHook]
        public static void OnUnload()
        {
            IL.Celeste.BounceBlock.Update -= TeraBounceUpdate;
        }
        private static void TeraBounceUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(100f) || instr.MatchLdcR4(35f) || instr.MatchLdcR4(40f) || instr.MatchLdcR4(140f) || instr.MatchLdcR4(200f)))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on bounce block speed at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(GetSpeedMultipler);
                cursor.Emit(OpCodes.Mul);
            }
            cursor.Index = 0;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(2)))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on bounce block activate at {cursor.Index} in IL for {cursor.Method.Name}");
                ILLabel label = null;
                cursor.GotoNext(MoveType.After, instr => instr.MatchBrfalse(out label));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(PlayerActivate);
                cursor.Emit(OpCodes.Brfalse, label);
            }
        }
        private static bool PlayerActivate(BounceBlock block)
        {
            if (block is not TeraBounceBlock teraBlock)
                return true;
            var player = teraBlock.SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null) return false;
            teraBlock.lastEffect = teraBlock.EffectAsDefender(player.GetTera());
            return teraBlock.lastEffect != TeraEffect.None;
        }
        private static float GetSpeedMultipler(BounceBlock block)
        {
            if (block is not TeraBounceBlock teraBlock)
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
