using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities.TrackIndicator;

[Tracked(true)]
[WorkingInProgress]
public class TrackProgressBar : Entity
{
    public TrackProgressBar(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        ID = data.ID;
    }
    private int ID;

}
