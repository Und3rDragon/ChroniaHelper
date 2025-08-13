using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace ChroniaHelper.Entities.TrackIndicator;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TrackProgressBar")]
public class TrackProgressBar : Entity
{
    public TrackProgressBar(EntityData data, Vector2 offset) : base(data.Position + offset)
    {

    }

}
