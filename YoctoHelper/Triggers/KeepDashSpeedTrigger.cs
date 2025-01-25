using Celeste.Mod.Entities;
using ChroniaHelper;
using YoctoHelper.Cores;
using YoctoHelper.Hooks;

namespace YoctoHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/KeepDashSpeedTrigger")]
public class KeepDashSpeedTrigger : BaseTrigger
{

    public KeepDashSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
    }

    protected override void OnEnterHandle(Player player)
    {
        this.SetValue(true);
    }

    protected override void RevertOnLeaveHandle(Player player)
    {
        this.SetValue(false);
    }

    private void SetValue(bool value)
    {
        ChroniaHelperModule.Instance.HookManager.SetHookDataValue<bool>(HookId.KeepDashSpeed, value, base.revertOnDeath);
    }

}
