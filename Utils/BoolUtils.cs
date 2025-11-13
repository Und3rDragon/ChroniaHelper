namespace ChroniaHelper.Utils;

public class BoolMode
{
    public enum BoolValue
    {
        None,
        True,
        False
    }

    public static void Assignment(ref bool value, BoolMode.BoolValue boolValue)
    {
        if (boolValue == BoolMode.BoolValue.None)
        {
            return;
        }
        value = (boolValue == BoolMode.BoolValue.True);
    }

}

public static class BoolUtils
{
    public static int ToInt(this bool b, int t = 1, int f = 0)
    {
        return b ? t : f;
    }
    
    public static bool ToBool(this int i, int f = 0)
    {
        return i != f ? true : false;
    }
    
    public static float ToFloat(this bool b, float t = 1f, float f = 0f)
    {
        return b ? t : f;
    }

    public static bool TryNegative(ref this bool basic, bool enter)
    {
        return basic = basic ? enter : false;
    }

    public static bool TryPositive(ref this bool basic, bool enter)
    {
        return basic = basic ? true : enter;
    }
    
    public static bool ParseBool(this string str, bool defaultValue = false)
    {
        return str.ToLower() == "true" ? true : (str.ToLower() == "false" ? false : defaultValue);
    }
}
