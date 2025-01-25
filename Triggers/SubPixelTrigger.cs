using Celeste.Mod.Entities;
using MonoMod.Utils;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SubPixelTrigger")]
public class SubPixelTrigger : BaseTrigger
{

    private float subPixelX;

    private float subPixelY;

    private bool stay;

    private Vector2 movementCounter;

    public SubPixelTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.subPixelX = data.Float("subPixelX", 0F);
        this.subPixelY = data.Float("subPixelY", 0F);
        this.stay = data.Bool("stay", true);
        this.movementCounter = new Vector2(this.subPixelX, this.subPixelY);
    }

    protected override void OnEnterExecute(Player player)
    {
        DynData<Actor> dynData = new DynData<Actor>(player);
        dynData.Set<Vector2>("movementCounter", this.movementCounter);
    }

    protected override void OnStayExecute(Player player)
    {
        if (this.stay)
        {
            DynData<Actor> dynData = new DynData<Actor>(player);
            dynData.Set<Vector2>("movementCounter", this.movementCounter);
        }
    }

}
