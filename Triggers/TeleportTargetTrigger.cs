using System.Linq;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TeleportTargetTrigger")]
public class TeleportTargetTrigger : TeleportPositionTrigger
{

    private string targetId;

    private PositionPoint positionPoint;

    private bool positionOffset;

    public TeleportTargetTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.targetId = data.Attr("targetId", null);
        this.positionPoint = data.Enum<PositionPoint>("positionPoint", PositionPoint.TopLeft);
        this.positionOffset = data.Bool("positionOffset", true);
    }

    protected override void OnEnterExecute(Player player)
    {
        EntityData targetIdTrigger = null;
        if (string.IsNullOrEmpty(this.targetRoom))
        {
            targetIdTrigger = MapDataUtils.GetMapDataTrigger(this.level, "ChroniaHelper/TargetIdTrigger", (entityData) => entityData.Attr("targetId") == this.targetId);
        }
        else
        {
            targetIdTrigger = base.session.MapData.Get(this.targetRoom).Triggers.FirstOrDefault((entityData) => (entityData.Name == "ChroniaHelper/TargetIdTrigger") && (entityData.Attr("targetId") == this.targetId));
        }
        if ((targetIdTrigger == null) || (!FlagUtils.IsCorrectFlag(base.level, targetIdTrigger.Attr("ifFlag", null))))
        {
            return;
        }
        base.targetRoom = targetIdTrigger.Level.Name;
        base.targetPositionX += (int) (targetIdTrigger.Position.X + (this.positionOffset ? 8 : 0));
        base.targetPositionY += (int) (targetIdTrigger.Position.Y + (this.positionOffset ? 16 : 0));
        switch (this.positionPoint)
        {
            case PositionPoint.TopCenter:
                base.targetPositionX += (int) (this.Width / 2);
                break;
            case PositionPoint.TopRight:
                base.targetPositionX += (int) (this.Width);
                break;
            case PositionPoint.CenterLeft:
                base.targetPositionY += (int) (this.Height / 2);
                break;
            case PositionPoint.Center:
                base.targetPositionX += (int) (this.Width / 2);
                base.targetPositionY += (int) (this.Height / 2);
                break;
            case PositionPoint.CenterRight:
                base.targetPositionX += (int) (this.Width);
                base.targetPositionY += (int) (this.Height / 2);
                break;
            case PositionPoint.BottomLeft:
                base.targetPositionY += (int) (this.Height);
                break;
            case PositionPoint.BottomCenter:
                base.targetPositionX += (int) (this.Width / 2);
                base.targetPositionY += (int) (this.Height);
                break;
            case PositionPoint.BottomRight:
                base.targetPositionX += (int) (this.Width);
                base.targetPositionY += (int) (this.Height);
                break;
        }
        base.OnEnterExecute(player);
    }

}
