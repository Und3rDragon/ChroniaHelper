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
    [CustomEntity("ChroniaHelper/teraMoveBlock")]
    [Tracked(false)]
    public class TeraMoveBlock : MoveBlock, ITeraBlock
    {
        private static ILHook sequenceHook;
        public TeraType tera { get; set; }
        private Image image;
        private TeraEffect lastEffect = TeraEffect.None;
        public TeraMoveBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Enum("direction", Directions.Left), data.Bool("canSteer", defaultValue: true), data.Bool("fast")) 
        { 
            tera = data.Enum("tera", TeraType.Normal);
            Add(image = new Image(GFX.Game["ChroniaHelper/objects/tera/Player/" + tera.ToString()]));
            image.CenterOrigin();
        }
        public override void Render()
        {
            base.Render();
            image.Position = new Vector2(Width / 2, Height / 2 - 8) + Shake;
            image.Render();
        }
        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            var blockData = DynamicData.For(this);
            var triggered = blockData.Get<bool>("triggered");
            if (triggered)
                return;
            blockData.Set("triggered", true);
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
        public static void OnLoad()
        {
            sequenceHook = new ILHook(typeof(MoveBlock).GetMethod("Controller", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), TeraSequence);
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
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(60f) || instr.MatchLdcR4(75f)))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on zip mover speed at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate(GetSpeedMultipler);
                cursor.Emit(OpCodes.Mul);
            }
        }
        private static bool PlayerActivate(MoveBlock block)
        {
            if (block is not TeraMoveBlock teraBlock)
                return true;
            var blockData = DynamicData.For(block);
            var triggered = blockData.Get<bool>("triggered");
            if (triggered)
                return true;
            var player = teraBlock.SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null) return false;
            teraBlock.lastEffect = teraBlock.EffectAsDefender(player.GetTera());
            return teraBlock.lastEffect != TeraEffect.None;
        }
        private static float GetSpeedMultipler(MoveBlock block)
        {
            if (block is not TeraMoveBlock teraBlock)
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
