namespace ChroniaHelper.Utils;

public static class PlayerUtils
{

    public static bool IsHeld(Player player)
    {
        return (player != null) && (player.Holding != null) && (player.Holding.IsHeld);
    }

    public static Player player => MaP.level.Tracker.GetEntity<Player>();
    public static bool getPlayer => player != null;
    public static bool playerAlive => !(player?.Dead ?? true);
    public static Player scenePlayer => Engine.Scene?.Tracker?.GetEntity<Player>();
    public static bool sceneGetPlayer => scenePlayer != null;
    public static bool scenePlayerAlive => !(scenePlayer?.Dead ?? true);

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
