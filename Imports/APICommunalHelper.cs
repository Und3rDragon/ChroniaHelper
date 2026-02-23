using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

[ModImportName("CommunalHelper.DashStates")]
public static class APICommunalHelper
{
    #region DreamTunnel

    public static Func<int> GetDreamTunnelDashState;
    public static int dreamTunnelDashState => GetDreamTunnelDashState();

    public static Func<bool> HasDreamTunnelDash;
    public static bool hasDreamTunnelDash => HasDreamTunnelDash();

    public static Func<int> GetDreamTunnelDashCount;
    public static int getDreamTunnelDashCount => GetDreamTunnelDashCount();

    public delegate Component DreamTunnelInteraction(Action<Player> onPlayerEnter, Action<Player> onPlayerExit);
    public static DreamTunnelInteraction _dreamTunnelInteraction;
    public static Component dreamTunnelInteraction(Action<Player> onPlayerEnter, Action<Player> onPlayerExit)
    {
        return _dreamTunnelInteraction(onPlayerEnter, onPlayerExit);
    }

    #endregion

    #region Seeker

    public static Func<bool> HasSeekerDash;
    public static bool hasSeekerDash => HasSeekerDash();

    public static Func<bool> IsSeekerDashAttacking;
    public static bool isSeekerDashAttacking => IsSeekerDashAttacking();

    #endregion
}
