using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities.SwimmingFish;

public class SwimmingFishZone : BaseEntity
{
    public SwimmingFishZone(EntityData data, Vc2 offset) : base(data, offset)
    {
        Vc2 p1 = data.Position;
        Vc2 p2 = data.Position + new Vc2(data.Width, data.Height);
        Vc2[] p = Utils.RandomUtils.GetRandomPoints(p1, p2, 1);
        motion = new(p[0], p1, p2);
        Add(motion);
    }
    public FishMotion motion;
}
