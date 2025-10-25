using System;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace ChroniaHelper.Entities
{
    [TrackedAs(typeof(DreamBlock))]
    [Tracked(false)]
    [CustomEntity("ChroniaHelper/teraDreamBlock")]
    public class TeraDreamBlock : DreamBlock, ITeraBlock
    {
        public TeraType tera { get; set; }
        private Image image;

        public TeraDreamBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), data.Bool("fastMoving"), data.Bool("oneUse"), data.Bool("below"))
        {
            tera = data.Enum("tera", TeraType.Normal);
            Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
            image.CenterOrigin();
        }
        public override void Render()
        {
            base.Render();
            var shake = DynamicData.For(this).Get<Vector2>("shake");
            image.Position = new Vector2(Width / 2, Height / 2) + shake;
            image.Render();
        }
        [LoadHook]
        public static void OnLoad()
        {
            IL.Celeste.Player.DreamDashCheck += TeraDreamCheck;
            IL.Celeste.Player.DreamDashUpdate += TeraDreamUpdate;
            On.Celeste.Player.DreamDashUpdate += TeraDreamUpdateSpeed;
        }
        [UnloadHook]
        public static void OnUnload()
        {
            IL.Celeste.Player.DreamDashCheck -= TeraDreamCheck;
            IL.Celeste.Player.DreamDashUpdate -= TeraDreamUpdate;
            On.Celeste.Player.DreamDashUpdate -= TeraDreamUpdateSpeed;
        }
        private static void TeraDreamCheck(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(0) && instr.Next.MatchLdloc(0)))
            {
                Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on dream block check at {cursor.Index} in IL for {cursor.Method.Name}");
                ILLabel label = null;
                cursor.GotoNext(MoveType.After, instr => instr.MatchBrfalse(out label));
                cursor.Emit(OpCodes.Ldloc_0);
                cursor.EmitDelegate(PlayerActivate);
                cursor.Emit(OpCodes.Brfalse, label);
            }
        }
        private static void TeraDreamUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("DreamDashedIntoSolid")))
            {
                cursor.Index++;
                var labelIntoSolid = cursor.MarkLabel();
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdarg(0) && instr.Next.MatchLdloc(1)))
                {
                    Logger.Log(nameof(ChroniaHelperModule), $"Injecting code to apply tera effect on dream dash into solid at {cursor.Index} in IL for {cursor.Method.Name}");
                    cursor.MoveAfterLabels();
                    cursor.Emit(OpCodes.Ldloc_1);
                    cursor.EmitDelegate(PlayerActivate);
                    cursor.Emit(OpCodes.Brfalse, labelIntoSolid);
                    cursor.MoveBeforeLabels();
                }
            }
        }
        private static int TeraDreamUpdateSpeed(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
        {
            var playerData = DynamicData.For(self);
            if (playerData.TryGet("dreamBlock", out DreamBlock dream))
            {
                if (dream is TeraDreamBlock teraDream)
                {
                    if (teraDream != null)
                    {
                        var dir = new Vector2(Math.Sign(self.Speed.X), Math.Sign(self.Speed.Y));
                        if (dir.Length() > 1)
                        {
                            dir = dir.SafeNormalize();
                        }
                        self.Speed = 240f * dir;
                        var effect = teraDream.EffectAsDefender(self.GetTera());
                        self.Speed *= GetSpeedMultipler(effect);
                    }
                }
            }
            return orig(self);
        }
        private static bool PlayerActivate(DreamBlock block)
        {
            if (block is not TeraDreamBlock teraBlock)
                return true;
            var player = teraBlock.SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null) return false;
            return teraBlock.EffectAsDefender(player.GetTera()) != TeraEffect.None;
        }
        private static float GetSpeedMultipler(TeraEffect effect)
        {
            return effect switch
            {
                TeraEffect.Super => 2f,
                TeraEffect.Normal => 1f,
                TeraEffect.Bad => 0.5f,
                TeraEffect.None => 1f,
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
