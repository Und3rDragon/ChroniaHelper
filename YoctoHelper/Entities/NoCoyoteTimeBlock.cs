using Celeste.Mod.Entities;

namespace YoctoHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/NoCoyoteTimeBlock")]
public class NoCoyoteTimeBlock : CrumbleBlock
{

    public NoCoyoteTimeBlock(Vector2 position, EntityData data, EntityID id) : base(position, data, id)
    {
    }

    public NoCoyoteTimeBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data, id)
    {
    }

}
