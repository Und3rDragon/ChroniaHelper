namespace ChroniaHelper.Utils;

public static class PlayerUtils
{

    public static bool IsHeld(Player player)
    {
        return (player != null) && (player.Holding != null) && (player.Holding.IsHeld);
    }

    public static Player GetPlayer()
    {
        return Engine.Scene?.Tracker?.GetEntity<Player>();
    }
    //Thanks to coloursofnoise for this code.
    public static bool TryGetPlayer(out Player player)
    {
        player = Engine.Scene?.Tracker?.GetEntity<Player>();
        return player != null;
    }
    public static bool TryGetAlivePlayer(out Player player)
    {
        player = Engine.Scene?.Tracker?.GetEntity<Player>();
        return !(player?.Dead ?? true);
    }

}
