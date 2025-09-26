using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/RoomTagController")]
public class RoomTagController : Entity
{
    public RoomTagController(EntityData data, Vector2 offset): base(data.Position + offset)
    {
        
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}
