namespace ChroniaHelper.Utils;

public static class PlayerUtils
{

    public static bool IsHeld(Player player)
    {
        return (player != null) && (player.Holding != null) && (player.Holding.IsHeld);
    }

}
