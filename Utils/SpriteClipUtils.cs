using System;
using System.Collections.Generic;

namespace ChroniaHelper.Utils;

/// <summary>
/// 按矩形裁剪贴图绘制：只画落在裁剪框内的那部分像素，框外直接不画。
/// </summary>
public static class SpriteClipUtils
{
    /// <summary>
    /// 缓存已生成的局部贴图，避免逐帧重复创建。
    /// </summary>
    private static readonly Dictionary<(MTexture, int, int, int, int), MTexture> clippedTextures = new();

    /// <summary>
    /// 尖刺的选择框：沿尖刺长度、一路向外 8 像素的那条矩形。
    /// </summary>
    public static Rectangle GetSpikeClipBox(Vector2 position, Vector2 outwards, int size)
    {
        int x = (int)Math.Round(position.X);
        int y = (int)Math.Round(position.Y);

        if (outwards.Y < 0F)
        {
            return new Rectangle(x, y - 8, size, 8);
        }

        if (outwards.Y > 0F)
        {
            return new Rectangle(x, y, size, 8);
        }

        if (outwards.X < 0F)
        {
            return new Rectangle(x - 8, y, 8, size);
        }

        return new Rectangle(x, y, 8, size);
    }

    /// <summary>
    /// 绘制贴图，只保留落在裁剪框内的部分。
    /// 旋转或缩放为负值时无法用局部贴图表达裁剪，退回完整绘制。
    /// </summary>
    public static void DrawClipped(this MTexture texture, Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation, Rectangle clipBox)
    {
        if (rotation != 0F || scale.X <= 0F || scale.Y <= 0F)
        {
            texture.DrawJustified(position, justify, color, scale, rotation);
            return;
        }

        float width = texture.Width * scale.X;
        float height = texture.Height * scale.Y;

        float left = position.X - justify.X * width;
        float top = position.Y - justify.Y * height;

        float clipLeft = Math.Max(left, clipBox.Left);
        float clipTop = Math.Max(top, clipBox.Top);
        float clipRight = Math.Min(left + width, clipBox.Right);
        float clipBottom = Math.Min(top + height, clipBox.Bottom);

        if (clipRight <= clipLeft || clipBottom <= clipTop)
        {
            return;
        }

        int subX = (int)Math.Round((clipLeft - left) / scale.X);
        int subY = (int)Math.Round((clipTop - top) / scale.Y);
        int subWidth = (int)Math.Round((clipRight - clipLeft) / scale.X);
        int subHeight = (int)Math.Round((clipBottom - clipTop) / scale.Y);

        subX = Calc.Clamp(subX, 0, texture.Width - 1);
        subY = Calc.Clamp(subY, 0, texture.Height - 1);
        subWidth = Calc.Clamp(subWidth, 1, texture.Width - subX);
        subHeight = Calc.Clamp(subHeight, 1, texture.Height - subY);

        MTexture subTexture = GetClippedTexture(texture, subX, subY, subWidth, subHeight);

        Vector2 drawPosition = new Vector2(
            left + subX * scale.X,
            top + subY * scale.Y);

        subTexture.Draw(drawPosition, Vector2.Zero, color, scale, 0F);
    }

    private static MTexture GetClippedTexture(MTexture texture, int x, int y, int width, int height)
    {
        (MTexture, int, int, int, int) key = (texture, x, y, width, height);

        if (!clippedTextures.TryGetValue(key, out MTexture clipped))
        {
            clipped = texture.GetSubtexture(x, y, width, height);
            clippedTextures[key] = clipped;
        }

        return clipped;
    }
}
