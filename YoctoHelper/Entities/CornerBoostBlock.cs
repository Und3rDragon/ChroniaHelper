using Celeste.Mod.Entities;
using Celeste;
using System;
using Monocle;

namespace YoctoHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/CornerBoostBlock")]
public class CornerBoostBlock : CrumbleBlock
{

    public CornerBoostBlock(Vector2 position, EntityData data, EntityID id) : base(position, data, id)
    {
    }

    public CornerBoostBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data, id)
    {
    }

}
