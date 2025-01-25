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
