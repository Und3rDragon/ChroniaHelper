using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

[Tracked(true)]
public abstract class BaseTrigger : Trigger
{

    protected TriggerDirections enterFrom { get; private set; }

    protected string[] flagsForEnter { get; private set; }

    protected TriggerDirections leaveFrom { get; private set; }

    protected string[] flagsForLeave { get; private set; }

    protected bool onlyOnce { get; private set; }

    protected bool revertOnLeave { get; private set; }

    protected bool revertOnDeath { get; private set; }

    protected Level level { get; private set; }

    protected Session session { get; private set; }

    private bool isCorrectEnter { get; set; }

    private bool isCorrectLeave { get; set; }

    protected bool enableStay { get; set; }

    protected BaseTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.enterFrom = data.Enum<TriggerDirections>("enterFrom", TriggerDirections.Any);
        this.flagsForEnter = FlagUtils.Parse(data.Attr("flagsForEnter", null));
        this.leaveFrom = data.Enum<TriggerDirections>("leaveFrom", TriggerDirections.Any);
        this.flagsForLeave = FlagUtils.Parse(data.Attr("flagsForLeave", null));
        this.onlyOnce = data.Bool("onlyOnce", false);
        this.revertOnLeave = data.Bool("revertOnLeave", false);
        this.revertOnDeath = data.Bool("revertOnDeath", true);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
        this.session = this.level.Session;
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        this.isCorrectLeave = false;
        if ((!TriggerUtils.CheckCorrectEnterDirection(this, player, enterFrom)) || (!FlagUtils.CheckCorrectFlags(this.level, this.flagsForEnter)))
        {
            return;
        }
        this.isCorrectEnter = true;
        this.OnEnterHandle(player);
    }

    protected virtual void OnEnterHandle(Player player)
    {
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        if ((!this.enableStay) || (!this.isCorrectEnter) || (this.isCorrectLeave))
        {
            return;
        }
        this.OnStayHandle(player);
    }

    protected virtual void OnStayHandle(Player player)
    {
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        this.isCorrectEnter = false;
        if ((!TriggerUtils.CheckCorrectLeaveDirection(this, player, leaveFrom)) || (!FlagUtils.CheckCorrectFlags(this.level, this.flagsForLeave)))
        {
            return;
        }
        this.isCorrectLeave = true;
        this.OnLeaveHandle(player);
        if (this.revertOnLeave)
        {
            this.RevertOnLeaveHandle(player);
        }
        if ((this.onlyOnce) && (!this.isCorrectEnter) && (this.isCorrectLeave) && (this.OnlyOnceCondition(player)))
        {
            base.RemoveSelf();
        }
    }

    protected virtual void OnLeaveHandle(Player player)
    {
    }

    protected virtual void RevertOnLeaveHandle(Player player)
    {
    }

    protected virtual bool OnlyOnceCondition(Player player)
    {
        return true;
    }

}
