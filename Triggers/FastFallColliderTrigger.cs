using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using YoctoHelper.Hooks;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FastFallColliderTrigger")]
public class FastFallColliderTrigger : BaseTrigger
{

    private bool enable;

    private bool before;

    public FastFallColliderTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.enable = data.Bool("enable", false);
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset)
        {
            this.before = this.GetValue();
        }
        this.SetValue(this.enable);
    }

    protected override void LeaveReset(Player player)
    {
        this.SetValue(this.before);
    }

    private bool GetValue()
    {
        return ChroniaHelperModule.Instance.HookManager.GetHookDataValue<bool>(HookId.FastFallCollider);
    }

    private void SetValue(bool value)
    {
        ChroniaHelperModule.Instance.HookManager.SetHookDataValue<bool>(HookId.FastFallCollider, value, false);
    }

}
