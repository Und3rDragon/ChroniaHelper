using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagSwitchTrigger")]
public class FlagSwitchTrigger : FlagManageTrigger
{

    private string[] flagA;

    private string[] flagB;

    private SwitchDefaultFlagMode defaultFlagMode;

    private bool isFlagA;

    private bool isFlagB;

    private string[] addFlag;

    private string[] removeFlag;

    public enum SwitchDefaultFlagMode
    {
        None,
        FlagA,
        FlagB
    }

    public FlagSwitchTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.flagA = FlagUtils.Parse(data.Attr("flagA", null));
        this.flagB = FlagUtils.Parse(data.Attr("flagB", null));
        this.defaultFlagMode = data.Enum<SwitchDefaultFlagMode>("defaultFlagMode", SwitchDefaultFlagMode.None);
    }

    protected override void OnEnterExecute(Player player)
    {
        this.isFlagA = base.Contains(this.flagA);
        this.isFlagB = base.Contains(this.flagB);
        if (!this.isFlagA && !this.isFlagB)
        {
            switch (this.defaultFlagMode)
            {
                case FlagSwitchTrigger.SwitchDefaultFlagMode.FlagA:
                    base.Add(this.addFlag = this.flagA);
                    return;
                case FlagSwitchTrigger.SwitchDefaultFlagMode.FlagB:
                    base.Add(this.addFlag = this.flagB);
                    return;
                default:
                    return;
            }
        }
        if (this.isFlagA && this.isFlagB)
        {
            return;
        }
        base.Remove(this.removeFlag = (this.isFlagA ? this.flagA : this.flagB));
        base.Add(this.addFlag = (this.isFlagA ? this.flagB : this.flagA));
    }

    public override void OnLeave(Player player)
    {
        if (this.isFlagA || this.isFlagB)
        {
            base.OnLeave(player);
        }
    }

    protected override void LeaveReset(Player player)
    {
        if (this.isFlagA && this.isFlagB)
        {
            return;
        }
        base.Remove(this.addFlag);
        base.Add(this.removeFlag);
    }

}
