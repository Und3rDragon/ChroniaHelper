using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace ChroniaHelper.Utils
{
    public static class CircleTextureCache
    {
        // Bucket size in pixels.
        // If you don't like the default, smaller number makes a better fit.
        private const float RadiusStep = 0.25f;

        private static readonly Dictionary<(int radiusUnits, int resolution, bool filled), VirtualRenderTarget> cache = new();
        private static SpriteBatch bakeBatch;

        public static VirtualRenderTarget GetOutline(float radius, int resolution, out float drawScale)
            => GetOrCreate(radius, resolution, false, out drawScale);

        public static VirtualRenderTarget GetFilled(float radius, int resolution, out float drawScale)
            => GetOrCreate(radius, resolution, true, out drawScale);

        private static VirtualRenderTarget GetOrCreate(float radius, int resolution, bool filled, out float drawScale)
        {
            int units = Math.Max(1, (int)MathF.Round(radius / RadiusStep));
            float bakedRadius = units * RadiusStep;
            drawScale = radius / bakedRadius;

            var key = (units, resolution, filled);
            if (cache.TryGetValue(key, out var vrt) && vrt.Target != null && !vrt.Target.IsDisposed)
                return vrt;

            vrt = Bake(key, bakedRadius, resolution, filled);
            cache[key] = vrt;
            return vrt;
        }

        private static VirtualRenderTarget Bake((int units, int resolution, bool filled) key, float bakedRadius, int resolution, bool filled)
        {
            var gd = Engine.Instance.GraphicsDevice;
            int size = (int)Math.Ceiling((bakedRadius + 6) * 2);

            var vrt = VirtualContent.CreateRenderTarget(
                $"chroniahelper_circle_{(filled ? "fill" : "outline")}_{key.units}_{resolution}",
                size, size);

            // Snapshot state we're about to disturb.
            var previousTargets = gd.GetRenderTargets();
            var previousBatch = Draw.SpriteBatch;

            bakeBatch ??= new SpriteBatch(gd);

            gd.SetRenderTarget(vrt.Target);
            gd.Clear(Color.Transparent);

            Draw.SpriteBatch = bakeBatch;
            bakeBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var center = new Vector2(size / 2f);
            if (filled)
                Draw.Circle(center, bakedRadius / 2f, Color.White, bakedRadius, resolution);
            else
                Draw.Circle(center, bakedRadius, Color.White, resolution);

            bakeBatch.End();

            // Restore what was previously active, so the interrupted render pass can continue.
            Draw.SpriteBatch = previousBatch;
            if (previousTargets.Length == 0)
                gd.SetRenderTarget(null);
            else
                gd.SetRenderTargets(previousTargets);

            return vrt;
        }

        public static void DrawAt(VirtualRenderTarget vrt, Vector2 position, Color tint, float scale = 1f)
        {
            var origin = new Vector2(vrt.Target.Width / 2f, vrt.Target.Height / 2f);
            Draw.SpriteBatch.Draw(vrt.Target, position, null, tint, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        public static void Clear()
        {
            foreach (var vrt in cache.Values)
                vrt.Dispose();
            cache.Clear();
            bakeBatch?.Dispose();
            bakeBatch = null;
        }
    }
}