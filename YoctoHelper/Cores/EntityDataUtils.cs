using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

public static class EntityDataUtils
{

    public static string String(this EntityData data, string key, string defaultValue = null)
    {
        return StringUtils.EmptyStringFiller(data.Attr(key, null), defaultValue);
    }

    public static float Float(this EntityData data, string key, float defaultValue = 0F, float min = 0F, float max = float.MaxValue)
    {
        return NumberUtils.FloatFix(data.Float(key, defaultValue), min, max);
    }

    public static int Int(this EntityData data, string key, int defaultValue = 0, int min = 0, int max = int.MaxValue)
    {
        return NumberUtils.IntFix(data.Int(key, defaultValue), min, max);
    }

}
