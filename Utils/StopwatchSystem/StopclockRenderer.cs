using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

// Reference code from FrostHelper
public static class StopclockRenderer
{
    static bool SizesCalculated = false;
    static float SpacerWidth;
    static float NumberWidth;

    private static void CalculateBaseSizes()
    {
        if (SizesCalculated)
            return;
        SizesCalculated = true;

        // compute the max size of a digit and separators in the English font, for the timer part.
        PixelFont font = Dialog.Languages["english"].Font;
        float fontFaceSize = Dialog.Languages["english"].FontFaceSize;
        PixelFontSize pixelFontSize = font.Get(fontFaceSize);
        for (int i = 0; i < 10; i++)
        {
            float digitWidth = pixelFontSize.Measure(i.ToString()).X;
            if (digitWidth > NumberWidth)
            {
                NumberWidth = digitWidth;
            }
        }
        SpacerWidth = pixelFontSize.Measure('.').X;
    }

    public static Vc2 Measure(string text)
    {
        var lang = Dialog.Languages["english"];
        var pixelFontSize = lang.Font.Get(lang.FontFaceSize);

        return pixelFontSize.Measure(text);
    }

    public static void DrawTime(Vc2 position, string timeString, Color color, float scale = 1f, float alpha = 1f)
    {
        CalculateBaseSizes();

        position -= GetTimeWidth(timeString) * Vc2.UnitX / 2;
        var lang = Dialog.Languages["english"];

        PixelFont font = lang.Font;
        float fontFaceSize = lang.FontFaceSize;
        float currentScale = scale;
        float currentX = position.X;
        float currentY = position.Y;
        color *= alpha;
        Color colorDoubleAlpha = color * alpha;
        Color outlineColor = Color.Black * alpha;

        foreach (char c in timeString)
        {
            if (c == '.')
            {
                currentScale = scale * 0.7f;
                currentY -= 5f * scale;
            }

            Color currentColor, currentOutlineColor;
            if (c == ':' || c == '.' || currentScale < scale)
            {
                currentColor = colorDoubleAlpha;
                currentOutlineColor = outlineColor * alpha;
            }
            else
            {
                currentColor = color;
                currentOutlineColor = outlineColor;
            }

            float advance = ((c == ':' || c == '.' ? SpacerWidth : NumberWidth) + 4f) * currentScale;
            font.DrawOutline(fontFaceSize, c.ToString(), new Vc2(currentX + advance / 2, currentY), new Vc2(0.5f, 1f), Vc2.One * currentScale, currentColor, 2f, outlineColor);
            currentX += advance;
        }
    }

    public static float GetTimeWidth(string timeString, float scale = 1f)
    {
        float currentScale = scale;
        float currentWidth = 0f;
        foreach (char c in timeString)
        {
            if (c == '.')
            {
                currentScale = scale * 0.7f;
            }
            currentWidth += ((c == ':' || c == '.' ? SpacerWidth : NumberWidth) + 4f) * currentScale;
        }
        return currentWidth;
    }
}
