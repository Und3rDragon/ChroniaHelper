using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.CommunalHelper.Components;
using Celeste.Mod.CommunalHelper.DashStates;
using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

[ModImportName("CommunalHelper.DashStates")]
public static class CommunalHelperImports
{
    #region DreamTunnel

    public static Func<int> _GetDreamTunnelDashState;
    public static int GetDreamTunnelDashState => _GetDreamTunnelDashState();

    public static Func<bool> _HasDreamTunnelDash;
    public static bool HasDreamTunnelDash => _HasDreamTunnelDash();

    public static Func<int> _GetDreamTunnelDashCount;
    public static int GetDreamTunnelDashCount => _GetDreamTunnelDashCount();

    public delegate Component _DreamTunnelInteraction(Action<Player> onPlayerEnter, Action<Player> onPlayerExit);
    public static _DreamTunnelInteraction _dreamTunnelInteraction;
    public static Component DreamTunnelInteraction(Action<Player> onPlayerEnter, Action<Player> onPlayerExit)
    {
        return _dreamTunnelInteraction(onPlayerEnter, onPlayerExit);
    }

    #endregion

    #region Seeker

    public static Func<bool> _HasSeekerDash;
    public static bool HasSeekerDash => _HasSeekerDash();

    public static Func<bool> _IsSeekerDashAttacking;
    public static bool IsSeekerDashAttacking => _IsSeekerDashAttacking();

    #endregion
}
