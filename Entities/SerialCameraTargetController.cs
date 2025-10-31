using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

public class SerialCameraTargetController : BaseEntity
{
    public SerialCameraTargetController(EntityData data, Vc2 offset) : base(data, offset)
    {
        nodes = data.NodesWithPosition(offset);
    }
}
