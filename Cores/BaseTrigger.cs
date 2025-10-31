using System.Collections;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores;

[Tracked(true)]
public abstract class BaseTrigger : Trigger
{

    protected RelationalOperator levelDeathMode;

    protected int levelDeathCount;

    protected RelationalOperator totalDeathMode;

    protected int totalDeathCount;

    protected TriggerEnterMode enterMode;

    protected float enterDelay;

    protected string[] enterIfFlag;

    protected string enterSound;

    protected TriggerLeaveMode leaveMode;

    protected float leaveDelay;

    protected string[] leaveIfFlag;

    protected string leaveSound;

    protected string[] updateIfFlag;

    protected float updateDelay;

    protected float freeze;

    protected bool onlyOnce;

    protected bool leaveReset;

    protected Level level;

    protected Session session;

    private bool isCorrectEnter;

    protected bool inzone;

    public Vc2[] nodes;

    protected BaseTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        nodes = data.NodesWithPosition(offset);
        
        levelDeathMode = data.Enum("levelDeathMode", RelationalOperator.Equal);
        levelDeathCount = data.Int("levelDeathCount", -1);
        totalDeathMode = data.Enum("totalDeathMode", RelationalOperator.Equal);
        totalDeathCount = data.Int("totalDeathCount", -1);
        enterMode = data.Enum("enterMode", TriggerEnterMode.Any);
        enterDelay = data.Float("enterDelay", 0F);
        enterIfFlag = FlagUtils.Parse(data.Attr("enterIfFlag", null));
        enterSound = data.Attr("enterSound", null);
        leaveMode = data.Enum("leaveMode", TriggerLeaveMode.Any);
        leaveDelay = data.Float("leaveDelay", 0F);
        leaveIfFlag = FlagUtils.Parse(data.Attr("leaveIfFlag", null));
        leaveSound = data.Attr("leaveSound", null);
        updateIfFlag = FlagUtils.Parse(data.Attr("updateIfFlag", null));
        updateDelay = data.Float("updateDelay", 0F);
        freeze = data.Int("freeze", 0) / 60F;
        onlyOnce = data.Bool("onlyOnce", false);
        leaveReset = data.Bool("leaveReset", false);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        session = level.Session;
        AddedExecute(scene);
        Add(new Coroutine(AddedRoutine(scene), true));
    }

    protected virtual void AddedExecute(Scene scene)
    {
    }

    protected virtual IEnumerator AddedRoutine(Scene scene)
    {
        yield break;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        level = SceneAs<Level>();
        session = level.Session;
        AwakeExecute(scene);
        Add(new Coroutine(AwakeRoutine(scene), true));
    }

    protected virtual void AwakeExecute(Scene scene)
    {
    }

    protected virtual IEnumerator AwakeRoutine(Scene scene)
    {
        yield break;
    }

    public override void OnEnter(Player player)
    {
        inzone = true;
        base.OnEnter(player);
        if (!FlagUtils.IsCorrectFlag(level, enterIfFlag) || !TriggerUtils.IsDeathCount(level, levelDeathMode, levelDeathCount, totalDeathMode, totalDeathCount) || !TriggerUtils.IsCorrectEnterMode(this, enterMode, player))
        {
            return;
        }
        isCorrectEnter = true;
        if (enterDelay == 0F && freeze == 0F)
        {
            if (!string.IsNullOrEmpty(enterSound))
            {
                Audio.Play(enterSound, player.Position);
            }
            OnEnterExecute(player);
            Add(new Coroutine(OnEnterRoutine(player), true));
        }
        else
        {
            Add(new Coroutine(OnEnterInvoke(player), true));
        }
    }

    private IEnumerator OnEnterInvoke(Player player)
    {
        if (enterDelay > 0F)
        {
            yield return enterDelay;
        }
        if (!string.IsNullOrEmpty(enterSound))
        {
            Audio.Play(enterSound, player.Position);
        }
        if (freeze > 0F)
        {
            Celeste.Celeste.Freeze(freeze);
            yield return null;
        }
        OnEnterExecute(player);
        yield return OnEnterRoutine(player);
    }

    protected virtual void OnEnterExecute(Player player)
    {
    }

    protected virtual IEnumerator OnEnterRoutine(Player player)
    {
        yield break;
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        if (!isCorrectEnter)
        {
            return;
        }
        OnStayExecute(player);
        Add(new Coroutine(OnStayRoutine(player), true));
    }

    protected virtual void OnStayExecute(Player player)
    {
    }

    protected virtual IEnumerator OnStayRoutine(Player player)
    {
        yield break;
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        inzone = false;
        if (!FlagUtils.IsCorrectFlag(level, leaveIfFlag) || !TriggerUtils.IsDeathCount(level, levelDeathMode, levelDeathCount, totalDeathMode, totalDeathCount) || !TriggerUtils.IsCorrectLeaveMode(this, leaveMode, player))
        {
            return;
        }
        if (leaveDelay == 0F)
        {
            if (!string.IsNullOrEmpty(leaveSound))
            {
                Audio.Play(leaveSound, player.Position);
            }
            OnLeaveExecute(player);
            Add(new Coroutine(OnLeaveAfter(player), true));
        }
        else
        {
            Add(new Coroutine(OnLeaveInvoke(player), true));
        }
    }

    private IEnumerator OnLeaveInvoke(Player player)
    {
        if (leaveDelay > 0F)
        {
            yield return leaveDelay;
        }
        if (!string.IsNullOrEmpty(leaveSound))
        {
            Audio.Play(leaveSound, player.Position);
        }
        OnLeaveExecute(player);
        yield return OnLeaveAfter(player);
    }

    private IEnumerator OnLeaveAfter(Player player)
    {
        yield return OnLeaveRoutine(player);
        if (leaveReset)
        {
            LeaveReset(player);
        }
        if (onlyOnce && isCorrectEnter && OnlyOnceExpression(player))
        {
            RemoveSelf();
        }
        isCorrectEnter = false;
        yield break;
    }

    protected virtual void OnLeaveExecute(Player player)
    {
    }

    protected virtual IEnumerator OnLeaveRoutine(Player player)
    {
        yield break;
    }

    protected virtual void LeaveReset(Player player)
    {
    }

    protected virtual bool OnlyOnceExpression(Player player) => true;

    public override void Update()
    {
        base.Update();
        if (!FlagUtils.IsCorrectFlag(level, updateIfFlag) || !TriggerUtils.IsDeathCount(level, levelDeathMode, levelDeathCount, totalDeathMode, totalDeathCount))
        {
            return;
        }
        Player player = level.Tracker.GetEntity<Player>();
        if (!UpdateInvokeExpression(player))
        {
            return;
        }
        if (updateDelay == 0F)
        {
            UpdateExecute(player);
            Add(new Coroutine(UpdateRoutine(player), true));
        }
        else
        {
            Add(new Coroutine(UpdateInvoke(player), true));
        }
    }

    protected virtual bool UpdateInvokeExpression(Player player) => true;

    private IEnumerator UpdateInvoke(Player player)
    {
        if (updateDelay > 0F)
        {
            yield return updateDelay;
        }
        UpdateExecute(player);
        yield return UpdateRoutine(player);
    }

    protected virtual void UpdateExecute(Player player)
    {
    }

    protected virtual IEnumerator UpdateRoutine(Player player)
    {
        yield break;
    }

}
