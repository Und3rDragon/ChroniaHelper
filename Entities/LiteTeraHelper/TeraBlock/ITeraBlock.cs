using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using ChroniaHelper.Entities;
using ChroniaHelper.Utils;
using Celeste.Mod.Entities;
using Celeste;

namespace ChroniaHelper.Entities
{
    public interface ITeraBlock
    {
        public TeraType tera { get; set; }
        public TeraEffect EffectAsAttacker(TeraType t);
        public TeraEffect EffectAsDefender(TeraType t);
        public void ChangeTera(TeraType newTera);
    }
    public static class TeraBlock
    {
        private static ILHook playerUpdateHook;
        private static Hook liftSpeedHook;
        private static ILHook liftBoostHook;
        public const float LiftBoostMultipler = 4f;
        public static void OnLoad()
        {
            On.Celeste.CrystalStaticSpinner.OnPlayer += SpinnerCheck;
            On.Celeste.DustStaticSpinner.OnPlayer += DustCheck;
            On.Celeste.Spikes.OnCollide += SpikeCheck;
            IL.Celeste.TriggerSpikes.SpikeInfo.OnPlayer += TriggerSpikeCheck;
            On.Celeste.Spring.OnCollide += SpringCheck;
            IL.Celeste.Spring.OnCollide += SpringOnPlayer;
            IL.Celeste.Spring.OnHoldable += SpringOnHoldable;
            IL.Celeste.Spring.OnPuffer += SpringOnPuffer;
            IL.Celeste.Spring.OnSeeker += SpringOnSeeker;
            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), CheckDangerousSpikes);
            On.Celeste.Actor.ctor += OnActorCreate;
            On.Celeste.Actor.Update += OnActorUpdate;
            liftSpeedHook = new Hook(
                typeof(Actor).GetMethod("set_LiftSpeed", BindingFlags.Public | BindingFlags.Instance),
                typeof(TeraBlock).GetMethod("OnSetLiftSpeed", BindingFlags.NonPublic | BindingFlags.Static));
            IL.Celeste.Solid.MoveHExact += OnSolidMoveH;
            IL.Celeste.Solid.MoveVExact += OnSolidMoveV;
            liftBoostHook = new ILHook(typeof(Player).GetMethod("get_LiftBoost", BindingFlags.NonPublic | BindingFlags.Instance), ModifyLiftBoost);
        }
        public static void OnUnload()
        {
            On.Celeste.CrystalStaticSpinner.OnPlayer -= SpinnerCheck;
            On.Celeste.DustStaticSpinner.OnPlayer -= DustCheck;
            On.Celeste.Spikes.OnCollide -= SpikeCheck;
            IL.Celeste.TriggerSpikes.SpikeInfo.OnPlayer -= TriggerSpikeCheck;
            On.Celeste.Spring.OnCollide -= SpringCheck;
            IL.Celeste.Spring.OnCollide -= SpringOnPlayer;
            IL.Celeste.Spring.OnHoldable -= SpringOnHoldable;
            IL.Celeste.Spring.OnPuffer -= SpringOnPuffer;
            IL.Celeste.Spring.OnSeeker -= SpringOnSeeker;
            playerUpdateHook?.Dispose();
            On.Celeste.Actor.ctor -= OnActorCreate;
            On.Celeste.Actor.Update -= OnActorUpdate;
            liftSpeedHook?.Dispose();
            IL.Celeste.Solid.MoveHExact -= OnSolidMoveH;
            IL.Celeste.Solid.MoveVExact -= OnSolidMoveV;
            liftBoostHook?.Dispose();
        }
        private static void SpinnerCheck(On.Celeste.CrystalStaticSpinner.orig_OnPlayer orig, CrystalStaticSpinner self, Player player)
        {
            if (self.AttachToSolid)
            {
                var mover = self.Get<StaticMover>();
                if (mover != null && mover.Platform != null)
                {
                    if (mover.Platform is ITeraBlock teraBlock)
                    {
                        var effect = teraBlock.EffectAsAttacker(player.GetTera());
                        if (effect == TeraEffect.None || effect == TeraEffect.Bad)
                        {
                            return;
                        }
                    }
                }
            }
            orig(self, player);
        }
        private static void DustCheck(On.Celeste.DustStaticSpinner.orig_OnPlayer orig, DustStaticSpinner self, Player player)
        {
            var mover = self.Get<StaticMover>();
            if (mover != null && mover.Platform != null)
            {
                if (mover.Platform is ITeraBlock teraBlock)
                {
                    var effect = teraBlock.EffectAsAttacker(player.GetTera());
                    if (effect == TeraEffect.None || effect == TeraEffect.Bad)
                    {
                        return;
                    }
                }
            }
            orig(self, player);
        }
        private static void SpikeCheck(On.Celeste.Spikes.orig_OnCollide orig, Celeste.Spikes self, Player player)
        {
            var mover = self.Get<StaticMover>();
            if (mover != null && mover.Platform != null)
            {
                if (mover.Platform is ITeraBlock teraBlock)
                {
                    var effect = teraBlock.EffectAsAttacker(player.GetTera());
                    if (effect == TeraEffect.None || effect == TeraEffect.Bad)
                    {
                        return;
                    }
                }
            }
            orig(self, player);
        }
        private static void TriggerSpikeCheck(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on trigger spikes at {cursor.Index} in IL for {cursor.Method.Name}");
            var assembly = typeof(TriggerSpikes).Assembly;
            var t = assembly.GetType("Celeste.TriggerSpikes+SpikeInfo");
            var f = t.GetField("Parent", BindingFlags.Public | BindingFlags.Instance);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, f);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(TriggerSpikeTeraCheck);
            var current = cursor.Index;
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, instr => instr.MatchBr(out label));
            cursor.Goto(current);
            cursor.Emit(OpCodes.Brfalse, label);
        }
        private static bool TriggerSpikeTeraCheck(TriggerSpikes spikes, Player player)
        {
            var mover = spikes.Get<StaticMover>();
            if (mover != null && mover.Platform != null)
            {
                if (mover.Platform is ITeraBlock teraBlock)
                {
                    var effect = teraBlock.EffectAsAttacker(player.GetTera());
                    if (effect == TeraEffect.None || effect == TeraEffect.Bad)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static void SpringCheck(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player player)
        {
            var mover = self.Get<StaticMover>();
            if (mover != null && mover.Platform != null)
            {
                if (mover.Platform is ITeraBlock teraBlock)
                {
                    var effect = teraBlock.EffectAsAttacker(player.GetTera());
                    if (effect == TeraEffect.None || effect == TeraEffect.Bad)
                    {
                        return;
                    }
                }
            }
            orig(self, player);
        }
        private static void SpringOnPlayer(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt<Spring>("BounceAnimate")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on spring collide player at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate(ActorUseSpring);
                cursor.Index += 2;
            }
        }
        private static void SpringOnHoldable(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt<Spring>("BounceAnimate")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on spring collide holdable at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate(HoldableUseSpring);
            }
        }
        private static void SpringOnPuffer(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt<Spring>("BounceAnimate")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on spring collide puffer at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate(ActorUseSpring);
            }
        }
        private static void SpringOnSeeker(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt<Spring>("BounceAnimate")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on spring collide seeker at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate(ActorUseSpring);
            }
        }
        private static void ActorUseSpring(Spring spring, Actor actor)
        {
            var data = DynamicData.For(spring);
            data.Set("User", actor);
        }
        private static void HoldableUseSpring(Spring spring, Holdable holdable)
        {
            var data = DynamicData.For(spring);
            data.Set("User", holdable.Entity);
        }
        private static void CheckDangerousSpikes(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call &&
                                                            (instr.Operand as MethodReference)?.GetID() == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Spikes>(Microsoft.Xna.Framework.Vector2)"))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on spikes at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(IsDangerousSpike);
            }
        }
        private static bool IsDangerousSpike(bool origResult, Player player)
        {
            /*
            if (!ChroniaHelperModule.teraMode)
            {
                return true;
            }*/
            var spikes = player.CollideAll<Spikes>();
            foreach (var spike in spikes)
            {
                var mover = spike.Get<StaticMover>();
                if (mover != null && mover.Platform != null)
                {
                    if (mover.Platform is ITeraBlock teraBlock)
                    {
                        var effect = teraBlock.EffectAsAttacker(player.GetTera());
                        if (effect == TeraEffect.None || effect == TeraEffect.Bad)
                        {
                            continue;
                        }
                    }
                }
                return true;
            }
            return origResult;
        }
        private static void OnActorCreate(On.Celeste.Actor.orig_ctor orig, Actor self, Vector2 position)
        {
            orig(self, position);
            var data = DynamicData.For(self);
            data.Set("teraBoost", false);
        }
        private static void OnActorUpdate(On.Celeste.Actor.orig_Update orig, Actor self)
        {
            var data = DynamicData.For(self);
            var teraBoost = data.Get<bool>("teraBoost");
            orig(self);
            data.Set("teraBoost", teraBoost);
        }
        private delegate void orig_SetLiftSpeed(Actor self, Vector2 value);
        private static void OnSetLiftSpeed(orig_SetLiftSpeed orig, Actor self, Vector2 value)
        {
            orig(self, value);
            var data = DynamicData.For(self);
            data.Set("teraBoost", false);
        }
        private static void OnSolidMoveH(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Actor>("set_LiftSpeed")))
            {
                var current = cursor.Index;
                if (cursor.TryGotoPrev(MoveType.After, instr => instr.OpCode == OpCodes.Stloc_S && (((VariableDefinition)instr.Operand).Index == 4)))
                {
                    VariableDefinition variable = (VariableDefinition)cursor.Prev.Operand;
                    Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on solid move H at {cursor.Index} in IL for {cursor.Method.Name}");
                    cursor.Goto(current);
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldloc_S, variable);
                    cursor.EmitDelegate(OnTeraSolidMove);
                }
            }
        }
        private static void OnSolidMoveV(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Actor>("set_LiftSpeed")))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on solid move V at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc_3);
                cursor.EmitDelegate(OnTeraSolidMove);
            }
        }
        private static void OnTeraSolidMove(Solid solid, Actor actor)
        {
            var data = DynamicData.For(actor);
            data.Set("teraBoost", solid is ITeraBlock);
        }
        private static void ModifyLiftBoost(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(250f) || instr.MatchLdcR4(-130f)))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on player lift boost at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(GetLiftBoost);
                cursor.Emit(OpCodes.Mul);
            }
        }
        private static float GetLiftBoost(Actor actor)
        {
            var data = DynamicData.For(actor);
            return data.Get<bool>("teraBoost") ? LiftBoostMultipler : 1f;
        }
    }
}
