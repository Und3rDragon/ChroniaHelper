using Celeste.Mod.Entities;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TargetIdTrigger")]
public class TargetIdTrigger : Trigger
{

    private string ifFlag;

    private string targetId;

    public TargetIdTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.ifFlag = data.Attr("ifFlag", null);
        this.targetId = data.Attr("targetId", null);
    }

}
