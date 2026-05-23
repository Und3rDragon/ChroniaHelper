using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/AnotherTileEntity")]
public class AnotherTileEntity : GroupedBaseSolid
{
    public AnotherTileEntity(EntityData data, Vc2 offset) : base(data, offset)
    {
        Safe = data.Bool("safe", true);

        tileType = data.Char("tiletype", '3');
        Depth = data.Int("depth", Depths.Solids);
        bgTexture = data.Bool("bgTexture", false);
        Add(new LightOcclude());
        SurfaceSoundIndex = data.Int("surfaceSoundIndex", 8);
    }
    private bool bgTexture;

    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        if(other is AnotherTileEntity a)
        {
            return a.tileType == tileType && Depth == a.Depth
                && a.bgTexture == bgTexture && a.SurfaceSoundIndex == SurfaceSoundIndex;
        }

        return false;
    }
}
