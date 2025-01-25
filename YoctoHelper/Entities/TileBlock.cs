using Celeste.Mod.Entities;

namespace YoctoHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/TileBlock")]
public class TileBlock : CrumbleBlock
{

    public TileBlock(Vector2 position, EntityData data, EntityID id) : base(position, data, id)
    {
    }

    public TileBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data, id)
    {
    }

}
