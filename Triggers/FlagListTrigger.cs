using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagListTrigger")]
public class FlagListTrigger : FlagManageTrigger
{

    protected string[][] flagList;

    private int index;

    private bool reverse;

    private string[] addFlag;

    private int count;

    public FlagListTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.flagList = FlagUtils.ParseList(data.Attr("flagList", null));
        this.index = data.Int("index", 0);
        this.reverse = data.Bool("reverse", false);
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.onlyOnce)
        {
            this.count++;
        }
        if (this.flagList.Length == 1)
        {
            base.Add(this.addFlag = (this.flagList[0]));
            return;
        }
        base.Add(this.addFlag = (this.flagList[index]));
        base.Remove(this.flagList[NumberUtils.Mutation(this.index, 0, this.flagList.Length - 1, !this.reverse)]);
        this.index = NumberUtils.Mutation(this.index, 0, this.flagList.Length - 1, this.reverse);
    }

    protected override bool OnlyOnceExpression(Player player) => this.count >= this.flagList.Length;

    protected override void LeaveReset(Player player)
    {
        base.Remove(this.addFlag);
    }

}
