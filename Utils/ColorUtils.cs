using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ChroniaHelper.Triggers.TriggerExtension;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public static class ColorUtils
{

    public static string RgbaToHex(this Color color)
    {
        return RgbaToHex(color.R, color.G, color.B, color.A, false);
    }

    public static string RgbaToHex(this Color color, bool sign)
    {
        return RgbaToHex(color.R, color.G, color.B, color.A, sign);
    }

    public static string RgbaToHex(int red, int green, int blue, int alpha)
    {
        return RgbaToHex(red, green, blue, alpha, false);
    }

    public static string RgbaToHex(int red, int green, int blue, int alpha, bool sign)
    {
        return $"{(sign ? "#" : "")}{red:X2}{green:X2}{blue:X2}{alpha:X2}";
    }

    public static Color ParseColor(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Color.Transparent;
        }
        input = input.Replace("#", "");
        int convert = NumberUtils.ParseInt(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        if (input.Length == 6)
        {
            return new Color(convert >> 16, convert >> 8, convert);
        }
        if (input.Length == 8)
        {
            return new Color(convert >> 24, convert >> 16, convert >> 8, convert);
        }
        return Color.Transparent;
    }

    /// <summary>
    /// Parse separated colors by ","
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Color[] ParseColors(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        string[] split = StringUtils.Split(input.Trim(), ",");
        List<Color> colors = new List<Color>();
        foreach (string str in split)
        {
            colors.Add(ParseColor(str));
        }
        return colors.ToArray();
    }
    
    public static Color UintToColor(uint hex)
    {
        Color result = default(Color);
        result.PackedValue = hex;
        return result;
    }
    
    public static Color ColorArrayLerp(float lerp, params Color[] colors)
    {
        float m = NumberUtils.Mod(lerp, colors.Length);
        int fromIndex = (int)Math.Floor(m);
        int toIndex = NumberUtils.Mod(fromIndex + 1, colors.Length);
        float clampedLerp = m - fromIndex;

        return Color.Lerp(colors[fromIndex], colors[toIndex], clampedLerp);
    }
    
    public static Color[] ParseColorArray(string input)
    {
        if (StringUtils.IsNullOrWhiteSpace(input))
        {
            return new Color[0];
        }
        string[] split = StringUtils.Split(input);
        Color[] colorArray = new Color[split.Length];
        for (int i = 0; i < split.Length; i++)
        {
            colorArray[i] = ParseColor(split[i]);
        }
        return colorArray;
    }

    public struct HSLColor
    {
        public float H;
        public float S;
        public float L;
        public float A;

        public HSLColor(float H = 0, float S = 0, float L = 0, float A = 1f)
        {
            this.H = Math.Clamp(H, 0f, 360f);
            this.S = Math.Clamp(S, 0f, 1f);
            this.L = Math.Clamp(L, 0f, 1f);
            this.A = Math.Clamp(A, 0f, 1f);
        }

        public HSLColor(Color color)
        {
            RGBToHSL(color);
        }

        public HSLColor(ChroniaColor color)
        {
            RGBToHSL(color.color);
            A = color.alpha;
        }

        public HSLColor(string hex)
        {
            RGBToHSL(Calc.HexToColorWithAlpha(hex));
        }

        private void RGBToHSL(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float l = (max + min) / 2f;

            float s = 0f;
            if (delta != 0)
            {
                if (l < 0.5f)
                    s = delta / (max + min);
                else
                    s = delta / (2f - max - min);
            }

            float h = 0f;
            if (delta != 0)
            {
                if (max == r)
                    h = ((g - b) / delta + (g < b ? 6 : 0)) * 60;
                else if (max == g)
                    h = ((b - r) / delta + 2) * 60;
                else if (max == b)
                    h = ((r - g) / delta + 4) * 60;
            }

            H = h;
            S = s;
            L = l;
            A = Math.Clamp(color.A / 255f, 0f, 1f);
        }

        public Color ToColor()
        {
            float h = H;              // 0 ~ 360
            float s = S;       // 0 ~ 1
            float l = L;       // 0 ~ 1

            float c = (1 - Math.Abs(2 * l - 1)) * s;  // Chroma
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = l - c / 2;

            float r1 = 0, g1 = 0, b1 = 0;
            int sector = (int)(h / 60) % 6;

            switch (sector)
            {
                case 0: r1 = c; g1 = x; b1 = 0; break;
                case 1: r1 = x; g1 = c; b1 = 0; break;
                case 2: r1 = 0; g1 = c; b1 = x; break;
                case 3: r1 = 0; g1 = x; b1 = c; break;
                case 4: r1 = x; g1 = 0; b1 = c; break;
                case 5: r1 = c; g1 = 0; b1 = x; break;
            }

            int R = Math.Clamp((int)((r1 + m) * 255), 0, 255);
            int G = Math.Clamp((int)((g1 + m) * 255), 0, 255);
            int B = Math.Clamp((int)((b1 + m) * 255), 0, 255);

            return new Color(R, G, B, A);
        }

    }

    public struct HSVColor
    {
        public float H;
        public float S;
        public float V;
        public float A;

        public HSVColor(float h = 0, float s = 0, float v = 0, float a = 1f)
        {
            this.H = Math.Clamp(h, 0f, 360f);
            this.S = Math.Clamp(s, 0f, 1f);
            this.V = Math.Clamp(v, 0f, 1f);
            this.A = Math.Clamp(a, 0f, 1f);
        }

        public HSVColor(Color color)
        {
            RGBToHSV(color);
        }

        public HSVColor(ChroniaColor color)
        {
            RGBToHSV(color.color);
            A = color.alpha;
        }

        public HSVColor(string hex)
        {
            RGBToHSV(Calc.HexToColorWithAlpha(hex));
        }

        private void RGBToHSV(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = MathHelper.Max(r, MathHelper.Max(g, b));
            float min = MathHelper.Min(r, MathHelper.Min(g, b));
            float delta = max - min;

            // Value (V)
            float v = max;

            // Saturation (S)
            float s = max == 0 ? 0 : delta / max;

            // Hue (H)
            float h = 0;
            if (delta != 0)
            {
                if (max == r)
                    h = ((g - b) / delta + (g < b ? 6 : 0)) * 60;
                else if (max == g)
                    h = ((b - r) / delta + 2) * 60;
                else if (max == b)
                    h = ((r - g) / delta + 4) * 60;
            }
            // h ∈ [0, 360)

            // 映射到 byte (0~255)
            H = h;
            S = s;
            V = v;
            A = Math.Clamp(color.A / 255f, 0f, 1f);
        }

        public Color ToColor()
        {
            // 先将 byte 映射回浮点范围
            float h = H;  // 0~360
            float s = S;         // 0~1
            float v = V;         // 0~1

            // HSV → RGB 算法
            float c = v * s;           // Chroma
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = v - c;

            float r1 = 0, g1 = 0, b1 = 0;
            int sector = (int)(h / 60) % 6;

            switch (sector)
            {
                case 0: r1 = c; g1 = x; b1 = 0; break;
                case 1: r1 = x; g1 = c; b1 = 0; break;
                case 2: r1 = 0; g1 = c; b1 = x; break;
                case 3: r1 = 0; g1 = x; b1 = c; break;
                case 4: r1 = x; g1 = 0; b1 = c; break;
                case 5: r1 = c; g1 = 0; b1 = x; break;
            }

            int R = Math.Clamp((int)((r1 + m) * 255), 0, 255);
            int G = Math.Clamp((int)((g1 + m) * 255), 0, 255);
            int B = Math.Clamp((int)((b1 + m) * 255), 0, 255);

            return new Color(R, G, B, A);
        }
    }

    public struct ChroniaColor
    {
        public static CColor White = new(Color.White);
        public static CColor Black = new(Color.Black);
        public static CColor Red = new(Color.Red);
        public static CColor Blue = new(Color.Blue);
        public static CColor Green = new(Color.Green);

        public Color color;
        public float alpha;

        public ChroniaColor(Color color, float alpha = 1f)
        {
            this.color = color;
            this.alpha = Math.Clamp(alpha, 0f, 1f);
        }

        public ChroniaColor(byte R, byte G, byte B, byte A)
        {
            color = new Color(R, G, B);
            alpha = A / 255f;
        }

        public ChroniaColor(float R = 0f, float G = 0f, float B = 0f, float A = 0f)
        {
            color = new Color((int)R, (int)G, (int)B);
            alpha = Math.Clamp(A, 0f, 1f);
        }

        public ChroniaColor(string hex)
        {
            Color c = Calc.HexToColorWithAlpha(hex);
            color = new Color(c.R, c.G, c.B);
            alpha = c.A / 255f;
        }

        public ChroniaColor(HSLColor hsl)
        {
            alpha = hsl.A;
            Color c = hsl.ToColor();
            color = new Color(c.R, c.G, c.B);
        }

        public ChroniaColor(HSVColor hsv)
        {
            alpha = hsv.A;
            Color c = hsv.ToColor();
            color = new Color(c.R, c.G, c.B);
        }

        public Color Parsed()
        {
            return color * alpha;
        }

        public Color Parsed(params float[] additionalAlpha)
        {
            float value = 1f;
            for (int i = 0; i < additionalAlpha.Length; i++)
            {
                value *= additionalAlpha[i];
            }

            if (value.IsBetween(0.99999f, 1.00001f))
            {
                return color * alpha;
            }
            else
            {
                return color * alpha * value;
            }
        }

        public Color OverrideParse(params float[] overrideAlpha)
        {
            float value = 1f;
            for (int i = 0; i < overrideAlpha.Length; i++)
            {
                value *= overrideAlpha[i];
            }

            if (value.IsBetween(0.99999f, 1.00001f))
            {
                return color;
            }
            else
            {
                return color * value;
            }
        }

        public static ChroniaColor operator *(ChroniaColor c, float f)
        {
            c.alpha *= f;
            return c;
        }

        public static ChroniaColor operator *(float f, ChroniaColor c)
        {
            c.alpha *= f;
            return c;
        }

        public static ChroniaColor operator /(ChroniaColor c, float f)
        {
            c.alpha /= f;
            return c;
        }

        public static ChroniaColor operator /(float f, ChroniaColor c)
        {
            c.alpha /= f;
            return c;
        }
    }

    public static ChroniaColor GetChroniaColor(this Color color, float alpha = 1f)
    {
        return new ChroniaColor(color, alpha);
    }

    public static ChroniaColor GetChroniaColor(this string hex)
    {
        return new ChroniaColor(hex);
    }
    
    public static ChroniaColor GetChroniaColor(this EntityData data, string colorAttributeName, string defaultColor = "ffffff")
    {
        return new ChroniaColor(data.Attr(colorAttributeName, defaultColor));
    }

    public static ChroniaColor GetChroniaColor(this EntityData data, string colorAttributeName, Color defaultColor)
    {
        return data.GetChroniaColor(colorAttributeName, defaultColor.RgbaToHex());
    }
}
