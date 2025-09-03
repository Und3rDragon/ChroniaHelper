using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public static class ColorUtils
{

    public static string RgbaToHex(this Color color)
    {
        return ColorUtils.RgbaToHex(color.R, color.G, color.B, color.A, false);
    }

    public static string RgbaToHex(this Color color, bool sign)
    {
        return ColorUtils.RgbaToHex(color.R, color.G, color.B, color.A, sign);
    }

    public static string RgbaToHex(int red, int green, int blue, int alpha)
    {
        return ColorUtils.RgbaToHex(red, green, blue, alpha, false);
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
            colors.Add(ColorUtils.ParseColor(str));
        }
        return colors.ToArray();
    }

    public static Color GetRainbowHue(Scene scene, Vector2 position)
    {
        float num = 280f;
        float num2 = (position.Length() + scene.TimeActive * 50f) % num / num;
        return Calc.HsvToColor(0.4f + Calc.YoYo(num2) * 0.4f, 0.4f, 0.9f);
    }

    public static Color GetRainbowHue(Color[] colors, Scene scene, Vector2 position)
    {
        if (colors.Length == 1)
        {
            return colors[0];
        }
        float progress = position.Length() + scene.TimeActive * 50F;
        while (progress < 0)
        {
            progress += 280F;
        }
        progress = progress % 280F / 280F;
        progress = Calc.YoYo(progress);
        if (progress == 1)
        {
            return colors[colors.Length - 1];
        }
        float globalProgress = (colors.Length - 1) * progress;
        int colorIndex = (int) globalProgress;
        float progressInIndex = globalProgress - colorIndex;
        return Color.Lerp(colors[colorIndex], colors[colorIndex + 1], progressInIndex);
    }

    public static Dictionary<string, Color> colorHelper;

    public static Color ColorCopy(Color color, int alpha)
    {
        return new Color(color.R, color.G, color.B, Calc.Clamp(alpha, 0, 255));
    }

    public static Color ColorCopy(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, (byte)Calc.Clamp(255 * alpha, 0, 255));
    }
    public static Color ColorFix(string s)
    {
        /*
        if (colorHelper.ContainsKey(s.ToLower()))
            return colorHelper[s.ToLower()];
        */
        return AdvHexToColor(s);
    }


    public static Color ColorFix(string s, float alpha)
    {
        /*
        if (colorHelper.ContainsKey(s.ToLower()))
            return colorHelper[s.ToLower()];
        */
        return ColorCopy(AdvHexToColor(s), alpha);
    }


    public static Color AdvHexToColor(string hex, bool nullIfInvalid = false)
    {
        string hexplus = hex.Trim('#');
        if (hexplus.StartsWith("0x"))
            hexplus = hexplus.Substring(2);
        uint result;
        if (hexplus.Length == 6 && uint.TryParse(hexplus, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result))
        {
            return Calc.HexToColor((int)result);
        }
        else if (hexplus.Length == 8 && hexplus.Substring(0, 2) == "00" && Regex.IsMatch(hexplus.Substring(2), "[^0-9a-f]")) //Optimized check to determine Regex matching for a hex number, marginally faster for a check where you dont need the end value.
        {
            return Color.Transparent;
        }
        else if (uint.TryParse(hexplus, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result))
        {
            return UintToColor(result);
        }
        return Color.Transparent;
    }
    public static Color UintToColor(uint hex)
    {
        Color result = default(Color);
        result.PackedValue = hex;
        return result;
    }

    private static readonly PropertyInfo[] namedColors = typeof(Color).GetProperties();

    public static Color CopyColor(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, (byte)alpha * 255);
    }

    public static Color CopyColor(Color color, int alpha)
    {
        return new Color(color.R, color.G, color.B, alpha);
    }

    public static Color ColorArrayLerp(float lerp, params Color[] colors)
    {
        float m = NumberUtils.Mod(lerp, colors.Length);
        int fromIndex = (int)Math.Floor(m);
        int toIndex = NumberUtils.Mod(fromIndex + 1, colors.Length);
        float clampedLerp = m - fromIndex;

        return Color.Lerp(colors[fromIndex], colors[toIndex], clampedLerp);
    }

    public static Color TryParseColor(string str, float alpha = 1f)
    {
        foreach (PropertyInfo prop in namedColors)
        {
            if (str.Equals(prop.Name))
            {
                return CopyColor((Color)prop.GetValue(null), alpha);
            }
        }
        return CopyColor(Calc.HexToColor(str.Trim('#')), alpha);
    }

    public static Color HexToColorWithAlphaNonPremultiplied(string hex)
    {
        int num = 0;
        if (hex.Length >= 1 && hex[0] == '#')
        {
            num = 1;
        }

        switch (hex.Length - num)
        {
            case 6:
                {
                    int r2 = Calc.HexToByte(hex[num++]) * 16 + Calc.HexToByte(hex[num++]);
                    int g = Calc.HexToByte(hex[num++]) * 16 + Calc.HexToByte(hex[num++]);
                    int b = Calc.HexToByte(hex[num++]) * 16 + Calc.HexToByte(hex[num++]);
                    return new Color(r2, g, b);
                }
            case 8:
                {
                    int r = Calc.HexToByte(hex[num++]) * 16 + Calc.HexToByte(hex[num++]);
                    int g = Calc.HexToByte(hex[num++]) * 16 + Calc.HexToByte(hex[num++]);
                    int b = Calc.HexToByte(hex[num++]) * 16 + Calc.HexToByte(hex[num++]);
                    int alpha = Calc.HexToByte(hex[num++]) * 16 + Calc.HexToByte(hex[num++]);
                    return new Color(r, g, b) * (alpha / 255f); //don't set alpha, multiply the color, i really still don't understand this... :3c
                }
            default:
                return Color.White;
        }
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
            colorArray[i] = ColorUtils.ParseColor(split[i]);
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

}
