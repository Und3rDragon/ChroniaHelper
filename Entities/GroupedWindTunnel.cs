using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/GroupedWindTunnel")]
public class GroupedWindTunnel : BaseEntity
{
    public GroupedWindTunnel(EntityData data, Vc2 offset) : base(data, offset)
    {
        groupID = data.Attr("groupID");
        sizeData = (Position.X, Position.Y, data.Width, data.Height);
    }
    public string groupID;
    public (float, float, float, float) sizeData;
}