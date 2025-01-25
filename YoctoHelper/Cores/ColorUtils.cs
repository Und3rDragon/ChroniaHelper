using System.Globalization;

namespace YoctoHelper.Cores;

public static class ColorUtils
{

    public static Color ParseColor(string input)
    {
        if (StringUtils.IsNullOrWhiteSpace(input))
        {
            return Color.Transparent;
        }
        int convert = NumberUtils.ParseInt(input.Replace("#", string.Empty).Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return (input.Length) switch
        {
            6 => new Color(convert >> 16, convert >> 8, convert),
            8 => new Color(convert >> 24, convert >> 16, convert >> 8, convert),
            _ => Color.Transparent
        };
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

    public static Color GetRainbowHue(Scene scene, Vector2 position)
    {
        if ((ObjectUtils.IsNull(scene)) || (ObjectUtils.IsNull(position)))
        {
            return Color.Transparent;
        }
        float gradientSize = 280F;
        float progress = (position.Length() + scene.TimeActive * 50F) % gradientSize / gradientSize;
        return Calc.HsvToColor(0.4F + Calc.YoYo(progress) * 0.4F, 0.4F, 0.9F);
    }

    /*public static Color GetRainbowHue(Color[] colors, Scene scene, Vector2 position)
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
    }*/

}
