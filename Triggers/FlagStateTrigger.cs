using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagStateTrigger")]
public class FlagStateTrigger : FlagManageTrigger
{

    private string[] flag;

    private StateMode stateMode;

    private float stateDelay;

    private bool persistent;

    public enum StateMode
    {
        None,
        Add,
        Remove,
        Clear
    }

    public FlagStateTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.flag = FlagUtils.Parse(data.Attr("flag", null));
        this.stateMode = data.Enum<StateMode>("stateMode", StateMode.None);
        this.stateDelay = data.Float("stateDelay", 0F);
        this.persistent = data.Bool("persistent", true);
    }

    protected override IEnumerator AddedRoutine(Scene scene)
    {
        if (!TriggerUtils.IsDeathCount(this.level, this.levelDeathMode, this.levelDeathCount, this.totalDeathMode, this.totalDeathCount) || !this.Contains())
        {
            yield break;
        }
        if (this.stateDelay > 0F)
        {
            yield return this.stateDelay;
        }
        this.State();
        if (base.onlyOnce)
        {
            base.RemoveSelf();
        }
        yield break;
    }

    protected override bool UpdateInvokeExpression(Player player) => !(this.persistent && this.Contains());

    protected override IEnumerator UpdateRoutine(Player player)
    {
        if (this.stateDelay > 0F)
        {
            yield return this.stateDelay;
        }
        this.State();
        if (base.onlyOnce)
        {
            base.RemoveSelf();
        }
        yield break;
    }

    private bool Contains()
    {
        bool contains = base.Contains(this.flag);
        return this.stateMode == StateMode.Add ? !contains : (this.stateMode != StateMode.Remove || contains);
    }

    private void State()
    {
        switch (this.stateMode)
        {
            case FlagStateTrigger.StateMode.Add:
                base.Add(this.flag);
                return;
            case FlagStateTrigger.StateMode.Remove:
                base.Remove(this.flag);
                return;
            case FlagStateTrigger.StateMode.Clear:
                base.Clear();
                return;
        }
    }

}
