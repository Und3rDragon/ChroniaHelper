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
    public static int ToInt(bool b)
    {
        return b ? 1 : 0;
    }

    public static bool TryNegative(ref this bool basic, bool enter)
    {
        return basic = basic ? enter : false;
    }

    public static bool TryPositive(ref this bool basic, bool enter)
    {
        return basic = basic ? true : enter;
    }
}
