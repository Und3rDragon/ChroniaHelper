using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ChroniaHelper.Cores.Graphical;

public static class NewPixelFontUtils
{
    public static int GetStringWidth(this NewPixelFont font, string text)
    {
        int width = 0;
        foreach (char c in text)
        {
            if (font.FontCharacterWidths.TryGetValue(c, out int value))
            {
                width += value + 1;
            }
        }
        return width;
    }

    public static string Cleanup(this NewPixelFont font, string text, int maxWidth)
    {
        string[] array = text.Split('\n', StringSplitOptions.None);
        List<string> processedLines = new List<string>(array.Length);
        int ellipsisWidth = font.GetStringWidth("...");
        foreach (string line in array)
        {
            if (font.GetStringWidth(line) <= maxWidth)
            {
                processedLines.Add(line);
            }
            else
            {
                string processedLine = line + "...";
                for (int currentWidth = font.GetStringWidth(processedLine); currentWidth > maxWidth - ellipsisWidth; currentWidth = font.GetStringWidth(processedLine))
                {
                    processedLine = processedLine.Substring(0, processedLine.Length - 4) + "...";
                }
                processedLines.Add(processedLine);
            }
        }
        return string.Join("\n", processedLines);
    }

}

public class NewPixelFont
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fontMap">A series of characters</param>
    /// <param name="fontTexture">GFX.Game[path (relative to Gameplay)].Texture</param>
    /// <param name="fontCharacterWidth"></param>
    /// <param name="fontCharacterHeight"></param>
    /// <param name="fontCharacterWidths"></param>
    public NewPixelFont(string fontMap, Texture2D fontTexture, int fontCharacterWidth, int fontCharacterHeight, Dictionary<char, int> fontCharacterWidths = null)
    {
        this.FontMap = fontMap;
        this.FontTexture = fontTexture;
        this.FontCharacterWidth = fontCharacterWidth;
        this.FontCharacterHeight = fontCharacterHeight;
        if (fontCharacterWidths != null)
        {
            this.FontCharacterWidths = new Dictionary<char, int>();
            foreach (KeyValuePair<char, int> c in fontCharacterWidths)
            {
                if (c.Value <= 0)
                {
                    this.FontCharacterWidths[c.Key] = fontCharacterWidth;
                }
                else
                {
                    this.FontCharacterWidths[c.Key] = c.Value;
                }
            }
        }
        this.characterPositionRects = new Dictionary<char, Rectangle>();
        int currentY = 0;
        int fontMapPos = 0;
        while (currentY < this.FontTexture.Height / this.FontCharacterHeight)
        {
            int currentX = 0;
            while (currentX < this.FontTexture.Width / this.FontCharacterWidth)
            {
                if (this.FontMap.Length > fontMapPos)
                {
                    int charWidth = this.FontCharacterWidth;
                    Dictionary<char, int> fontCharacterWidths2 = this.FontCharacterWidths;
                    if (fontCharacterWidths2 != null && fontCharacterWidths2.ContainsKey(this.FontMap[fontMapPos]))
                    {
                        charWidth = this.FontCharacterWidths[this.FontMap[fontMapPos]];
                    }
                    this.characterPositionRects[this.FontMap[fontMapPos]] = new Rectangle(currentX * this.FontCharacterWidth, currentY * this.FontCharacterHeight, charWidth, this.FontCharacterHeight);
                }
                currentX++;
                fontMapPos++;
            }
            currentY++;
        }
    }

    // Token: 0x0600044D RID: 1101 RVA: 0x000325FC File Offset: 0x000307FC
    public int MeasureWidth(string str)
    {
        int length = 0;
        foreach (char ch in str)
        {
            if (FontCharacterWidths != null && FontCharacterWidths.TryGetValue(ch, out int w))
            {
                length += w + 1;
            }
            else
            {
                length += FontCharacterWidth;
            }
        }
        return length;
    }

    // Token: 0x0600044E RID: 1102 RVA: 0x00032658 File Offset: 0x00030858
    public int MeasureWidth(char ch)
    {
        if (FontCharacterWidths != null && FontCharacterWidths.TryGetValue(ch, out int w))
        {
            return w + 1;
        }
        return FontCharacterWidth;
    }

    // Token: 0x0600044F RID: 1103 RVA: 0x00032684 File Offset: 0x00030884
    public void Draw(string text, Point position, Color color, Rectangle? crop = null, Vector2? justify = null, bool wrap = false, float scale = 1f)
    {
        // 预先拆分一次所有行，避免循环内反复 Split（原实现每个换行符都重新 Split 整串，O(n²)）
        string[] lines = text.Split('\n', StringSplitOptions.None);
        int lineCount = lines.Length;

        Vector2 i = justify ?? Vector2.Zero;
        int line = 0;
        int textX = position.X - (int)(MeasureWidth(lines[line]) * i.X);
        int textY = position.Y - (int)(lineCount * FontCharacterHeight * i.Y);
        foreach (char ch in text)
        {
            if (ch == '\n')
            {
                line++;
                if (line >= lineCount) { break; }
                textX = position.X - (int)(MeasureWidth(lines[line]) * i.X);
                textY += FontCharacterHeight;
            }
            else if (crop == null || (textY <= crop.Value.Bottom + FontCharacterHeight && textY >= crop.Value.Top - FontCharacterHeight))
            {
                if (wrap && crop != null && textX > crop.Value.Right - FontCharacterWidth - 2)
                {
                    line++;
                    if (line >= lineCount) { break; }
                    textX = position.X;
                    textY += FontCharacterHeight;
                }
                Rectangle r;
                if (characterPositionRects.TryGetValue(ch, out r))
                {
                    if (crop != null)
                    {
                        int topClipVal = Calc.Clamp(crop.Value.Top - textY, 0, (int)(r.Height * scale));
                        int bottomClipVal = Calc.Clamp(textY - crop.Value.Bottom + FontCharacterHeight, 0, (int)(r.Height * scale));
                        int rightClipVal = Calc.Clamp(textX - crop.Value.Right + (int)(scale * MeasureWidth(ch)), 0, (int)(r.Width * scale));
                        int leftClipVal = Calc.Clamp(crop.Value.Left - textX, 0, (int)(r.Width * scale));
                        Monocle.Draw.SpriteBatch.Draw(FontTexture, new Vector2(textX + leftClipVal, textY + topClipVal), new Rectangle?(new Rectangle(r.X + leftClipVal, r.Y + topClipVal, r.Width - rightClipVal - leftClipVal, r.Height - bottomClipVal - topClipVal)), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                    }
                    else
                    {
                        Monocle.Draw.SpriteBatch.Draw(FontTexture, new Vector2(textX, textY), new Rectangle?(r), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                    }
                }
                textX += (int)(MeasureWidth(ch) * scale);
            }
        }
    }

    // Token: 0x04000498 RID: 1176
    public readonly string FontMap;

    // Token: 0x04000499 RID: 1177
    public readonly int FontCharacterWidth;

    // Token: 0x0400049A RID: 1178
    public readonly int FontCharacterHeight;

    // Token: 0x0400049B RID: 1179
    public readonly Texture2D FontTexture;

    // Token: 0x0400049C RID: 1180
    public Dictionary<char, int> FontCharacterWidths;

    // Token: 0x0400049D RID: 1181
    private Dictionary<char, Rectangle> characterPositionRects;
}
