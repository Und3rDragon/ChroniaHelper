using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagIfTrigger")]
public class FlagIfTrigger : FlagManageTrigger
{

    private string[] ifFlag;

    private string[] trueFlag;

    private string[] falseFlag;

    private DeleteIfFlagMode deleteIfFlag;

    private bool judge;

    public enum DeleteIfFlagMode
    {
        None,
        True,
        False,
        Any
    }

    public FlagIfTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.ifFlag = FlagUtils.Parse(data.Attr("ifFlag", null));
        this.trueFlag = FlagUtils.Parse(data.Attr("trueFlag", null));
        this.falseFlag = FlagUtils.Parse(data.Attr("falseFlag", null));
        this.deleteIfFlag = data.Enum<DeleteIfFlagMode>("deleteIfFlag", DeleteIfFlagMode.None);
    }

    protected override void OnEnterExecute(Player player)
    {
        this.judge = base.Contains(this.ifFlag);
        base.Add(this.judge ? this.trueFlag : this.falseFlag);
        if (this.IsDeleteIfFlag())
        {
            base.Remove(this.ifFlag);
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.Remove(this.judge ? this.trueFlag : this.falseFlag);
        if (this.IsDeleteIfFlag())
        {
            base.Add(this.ifFlag);
        }
    }

    private bool IsDeleteIfFlag() => this.deleteIfFlag switch
    {
        FlagIfTrigger.DeleteIfFlagMode.Any or FlagIfTrigger.DeleteIfFlagMode.True => this.judge,
        FlagIfTrigger.DeleteIfFlagMode.False => false,
        _ => false
    };

}
