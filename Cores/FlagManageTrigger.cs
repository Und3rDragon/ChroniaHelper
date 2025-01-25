using System.Collections.Generic;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores;

[Tracked(true)]
public class FlagManageTrigger : BaseTrigger
{

    public FlagManageTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
    }

    protected HashSet<string> GetFlags()
    {
        return level.Session.Flags.DeepCopyHashSet();
    }

    protected void SetFlags(HashSet<string> flags)
    {
        level.Session.Flags = flags.DeepCopyHashSet();
    }

    protected bool Contains(string[] flag)
    {
        return FlagUtils.Contains(level, flag);
    }

    protected string[] Intersect(string[] flag)
    {
        return FlagUtils.Intersect(level, flag);
    }

    protected void Add(string[] flag)
    {
        FlagUtils.Add(ref level, flag);
    }

    protected void Remove(string[] flag)
    {
        FlagUtils.Remove(ref level, flag);
    }

    protected void Clear()
    {
        FlagUtils.Clear(ref level);
    }

}
