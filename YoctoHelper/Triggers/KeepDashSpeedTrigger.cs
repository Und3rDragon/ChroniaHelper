using Celeste.Mod.Entities;
using ChroniaHelper;
using ChroniaHelper.Utils;
using YoctoHelper.Cores;
using YoctoHelper.Hooks;

namespace YoctoHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/KeepDashSpeedTrigger")]
public class KeepDashSpeedTrigger : BaseTrigger
{

    public KeepDashSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        Set = data.Bool("set", true);
    }
    private bool Set;

    protected override void OnEnterHandle(Player player)
    {
        this.SetValue(Set);
    }

    protected override void RevertOnLeaveHandle(Player player)
    {
        this.SetValue(!Set);
    }

    private void SetValue(bool value)
    {
        if (MaP.level.Session.Area.SID.HasValidContent())
        {
            Md.Session.KeepDashSpeed.Enter(MaP.level.Session.Area.SID, value);
        }
    }

}
