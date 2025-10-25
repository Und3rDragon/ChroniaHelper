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
    [TrackedAs(typeof(FallingBlock))]
    [Tracked(false)]
    [CustomEntity("ChroniaHelper/teraFallingBlock")]
    public class TeraFallingBlock : FallingBlock, ITeraBlock
    {
        private static ILHook sequenceHook;
        private static ILHook crushSequenceHook;
        public TeraType tera { get; set; }
        private Image image;
        private TeraEffect lastEffect = TeraEffect.None;

        public TeraFallingBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data)
        {
            tera = data.Enum("tera", TeraType.Normal);
            Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
            image.CenterOrigin();
            image.Position = new Vector2(data.Width / 2, data.Height / 2);
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            image.Position += amount;
        }
        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            if (Triggered)
                return;
            if (HasStartedFalling)
                return;
            Triggered = true;
            if (sm.Entity is Spring spring)
            {
                var springData = DynamicData.For(spring);
                var user = springData.Get<Entity>("User");
                if (user is Player player)
                {
                    lastEffect = player != null ? EffectAsAttacker(player.GetTera()) : TeraEffect.Normal;
                    return;
                }
                else if (user is TeraCrystal crystal)
                {
                    lastEffect = crystal != null ? EffectAsAttacker(crystal.tera) : TeraEffect.Normal;
                    return;
                }
            }
            lastEffect = TeraEffect.Normal;
            return;
        }
        [LoadHook]
        public static void OnLoad()
        {
            IL.Celeste.FallingBlock.PlayerFallCheck += TeraFallCheck;
            
            //AI fixation
            MethodInfo methodToHook = typeof(FallingBlock).GetMethod("Sequence", BindingFlags.NonPublic | BindingFlags.Instance);

            sequenceHook = new ILHook(methodToHook, TeraSequence);
            //AI fixation end

            crushSequenceHook = new ILHook(typeof(CrushBlock).GetMethod("AttackSequence", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), TeraCrushSequence);
        }
        [UnloadHook]
        public static void OnUnload()
        {
            IL.Celeste.FallingBlock.PlayerFallCheck -= TeraFallCheck;
            sequenceHook?.Dispose();
            crushSequenceHook?.Dispose();
        }
        private static void TeraFallCheck(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            //if HasPlayerRider && PlayerActivate
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Solid>("HasPlayerRider") || instr.MatchCall<Solid>("HasPlayerOnTop")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on falling block check at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(PlayerActivate);
                cursor.Emit(OpCodes.And);
            }
        }
        private static void TeraSequence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f) || instr.MatchLdcR4(160f)))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on falling block speed at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate(GetSpeedMultipler);
                cursor.Emit(OpCodes.Mul);
            }
            cursor.Index = cursor.Instrs.Count - 1;
            if (cursor.TryGotoPrev(MoveType.After, instr => instr.OpCode == OpCodes.Brtrue_S))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on falling block restart at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Index--;
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate(UnableToCrush);
                cursor.Emit(OpCodes.And);
            }
        }
        private static bool UnableToCrush(FallingBlock block)
        {
            var platform = block.CollideFirst<Platform>(block.Position + Vector2.UnitY);
            if (platform == null)
                return true;
            if (block is TeraFallingBlock teraBlock && platform is TeraDashBlock teraDash)
            {
                return teraBlock.EffectAsAttacker(teraDash.tera) != TeraEffect.Super;
            }
            return true;
        }
        private static void TeraCrushSequence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<FallingBlock>("Triggered")))
            {
                var current = cursor.Index - 3;
                ILLabel label = null;
                cursor.GotoNext(MoveType.After, instr => instr.MatchBr(out label));
                if (cursor.TryGotoPrev(MoveType.After, instr => instr.OpCode == OpCodes.Stloc_S && (((VariableDefinition)instr.Operand).Index == 5)))
                {
                    VariableDefinition variable = (VariableDefinition)cursor.Prev.Operand;
                    Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on kevin trigger falling block at {cursor.Index} in IL for {cursor.Method.Name}");
                    cursor.Goto(current);
                    cursor.Emit(OpCodes.Ldloc_S, variable);
                    cursor.Emit(OpCodes.Ldloc_1);
                    cursor.EmitDelegate(CrushTrigger);
                    cursor.Emit(OpCodes.Brfalse, label);
                }
            }
        }
        private static bool CrushTrigger(FallingBlock falling, CrushBlock crush)
        {
            if (falling is TeraFallingBlock teraFalling)
            {
                var data = DynamicData.For(falling);
                bool playerActive = (bool)data.Invoke("PlayerFallCheck");
                if (teraFalling.Triggered || teraFalling.HasStartedFalling || playerActive)
                    return true;
                if (crush is TeraCrushBlock teraCrush)
                {
                    var effect = teraFalling.EffectAsDefender(teraCrush.tera);
                    if (effect == TeraEffect.None)
                        return false;
                    teraFalling.lastEffect = effect;
                    return true;
                }
                else
                {
                    teraFalling.lastEffect = TeraEffect.Normal;
                    return true;
                }
            }
            return true;
        }
        private static bool PlayerActivate(FallingBlock block)
        {
            if (block is not TeraFallingBlock teraBlock)
                return true;
            if (block.Triggered)
                return true;
            if (block.HasStartedFalling)
                return true;
            var player = teraBlock.SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null) return false;
            teraBlock.lastEffect = teraBlock.EffectAsDefender(player.GetTera());
            return teraBlock.lastEffect != TeraEffect.None;
        }
        private static float GetSpeedMultipler(FallingBlock block)
        {
            if (block is not TeraFallingBlock teraBlock)
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
