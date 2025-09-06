using System.Collections;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores;

[Tracked(true)]
public abstract class PublicBaseTrigger : Trigger
{

    public RelationalOperator levelDeathMode;

    public int levelDeathCount;

    public RelationalOperator totalDeathMode;

    public int totalDeathCount;

    public TriggerEnterMode enterMode;

    public float enterDelay;

    public string[] enterIfFlag;

    public string enterSound;

    public TriggerLeaveMode leaveMode;

    public float leaveDelay;

    public string[] leaveIfFlag;

    public string leaveSound;

    public string[] updateIfFlag;

    public float updateDelay;

    public float freeze;

    public bool onlyOnce;

    public bool leaveReset;

    public Level level;

    public Session session;

    private bool isCorrectEnter;

    public bool inzone;

    public PublicBaseTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
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

    public virtual void AddedExecute(Scene scene)
    {
    }

    public virtual IEnumerator AddedRoutine(Scene scene)
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

    public virtual void AwakeExecute(Scene scene)
    {
    }

    public virtual IEnumerator AwakeRoutine(Scene scene)
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

    public virtual void OnEnterExecute(Player player)
    {
    }

    public virtual IEnumerator OnEnterRoutine(Player player)
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

    public virtual void OnStayExecute(Player player)
    {
    }

    public virtual IEnumerator OnStayRoutine(Player player)
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

    public virtual void OnLeaveExecute(Player player)
    {
    }

    public virtual IEnumerator OnLeaveRoutine(Player player)
    {
        yield break;
    }

    public virtual void LeaveReset(Player player)
    {
    }

    public virtual bool OnlyOnceExpression(Player player) => true;

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

    public virtual bool UpdateInvokeExpression(Player player) => true;

    private IEnumerator UpdateInvoke(Player player)
    {
        if (updateDelay > 0F)
        {
            yield return updateDelay;
        }
        UpdateExecute(player);
        yield return UpdateRoutine(player);
    }

    public virtual void UpdateExecute(Player player)
    {
    }

    public virtual IEnumerator UpdateRoutine(Player player)
    {
        yield break;
    }

}
