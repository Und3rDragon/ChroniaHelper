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
using static Celeste.TrackSpinner;

namespace ChroniaHelper.Entities
{
    [TrackedAs(typeof(SwapBlock))]
    [Tracked(false)]
    [CustomEntity("ChroniaHelper/teraSwapBlock")]
    public class TeraSwapBlock : SwapBlock, ITeraBlock
    {
        public TeraType tera { get; set; }
        private Image image;
        private float origForwardSpeed;
        private float origBackwardSpeed;

        public TeraSwapBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
        {
            tera = data.Enum("tera", TeraType.Normal);
            Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
            image.CenterOrigin();
            image.Position = new Vector2(data.Width / 2, data.Height / 2);
            origForwardSpeed = 360f / Vector2.Distance(Position, data.Nodes[0] + offset);
            origBackwardSpeed = origForwardSpeed * 0.4f;
        }

        public override void Render()
        {
            base.Render();
            image.Render();
        }

        public static void OnLoad()
        {
            On.Celeste.SwapBlock.OnDash += OnTeraDash;
        }
        public static void OnUnload()
        {
            On.Celeste.SwapBlock.OnDash -= OnTeraDash;
        }
        private static void OnTeraDash(On.Celeste.SwapBlock.orig_OnDash orig, SwapBlock self, Vector2 direction)
        {
            if (self is TeraSwapBlock teraSwap)
            {
                var player = self.Scene.Tracker.GetEntity<Player>();
                var effect = teraSwap.EffectAsDefender(player.GetTera());
                if (effect == TeraEffect.None)
                    return;
                DynamicData swapData = DynamicData.For(teraSwap);
                var rate = GetSpeedMultipler(effect);
                swapData.Set("maxForwardSpeed", rate * teraSwap.origForwardSpeed);
                swapData.Set("maxBackwardSpeed", rate * teraSwap.origBackwardSpeed);
            }
            orig(self, direction);
        }
        private static float GetSpeedMultipler(TeraEffect effect)
        {
            return effect switch
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
