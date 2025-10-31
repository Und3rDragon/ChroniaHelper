using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

public class SerialCameraOffsetController : BaseEntity
{
    public SerialCameraOffsetController(EntityData data, Vc2 offset) : base(data, offset)
    {
        nodes = data.NodesWithPosition(offset);
    }
}
