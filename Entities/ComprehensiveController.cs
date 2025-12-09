using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/ComprehensiveController")]
[Tracked(true)]
public class ComprehensiveController : Entity
{
    public ComprehensiveController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        collidingBGTiles = data.Attr("PlayerCollidingBGTiles", "ChroniaHelper_PlayerCollidingBGTiles");
        collidingEntityBGTiles = data.Attr("PlayerCollidingEntityBGTiles", "ChroniaHelper_PlayerCollidingEntityBGTiles");
        touchingTriggers = data.Attr("PlayerTouchingTriggers", "ChroniaHelper_PlayerTouchingTriggers");
        collidingEntitiesAbove = data.Attr("PlayerCollidingEntitiesAbove", "ChroniaHelper_PlayerCollidingEntitiesAbove");
        collidingEntitiesBelow = data.Attr("PlayerCollidingEntitiesBelow", "ChroniaHelper_PlayerCollidingEntitiesBelow");
        collidingEntitiesSame = data.Attr("PlayerCollidingEntitiesWithSameDepth", "ChroniaHelper_PlayerCollidingEntitiesWithSameDepth");
    }
    private string collidingBGTiles, touchingTriggers;
    private string collidingEntitiesSame, collidingEntitiesAbove, collidingEntitiesBelow;
    private string collidingEntityBGTiles;
    
    public override void Update()
    {
        base.Update();

        if (!PUt.getPlayer) { return; }

        // Colliding BG Tiles
        collidingBGTiles.SetFlag(PUt.player.CollideCheck(MaP.bgSolidTiles(MaP.level)));
        // Colliding Triggers
        touchingTriggers.SetFlag(PUt.player.CollideCheck<Trigger>());

        // Colliding Different Entities
        bool sameDepth = false, aboveDepth = false, belowDepth = false;
        bool entityBGTiles = false;
        foreach(var item in MaP.level.Entities)
        {
            if (!item.CollideCheck<Player>()) { continue; }
            
            if (item.Depth == PUt.player.Depth) {
                sameDepth = true;
            }
            if (item.Depth > PUt.player.Depth)
            {
                aboveDepth = true;
            }
            if (item.Depth < PUt.player.Depth)
            {
                belowDepth = true;
            }
            
            if(item is FloatyBgTile || item is StaticBgTile || item is SeperatedBgTile)
            {
                entityBGTiles = true;
            }
        }
        collidingEntitiesAbove.SetFlag(aboveDepth);
        collidingEntitiesBelow.SetFlag(belowDepth);
        collidingEntitiesSame.SetFlag(sameDepth);
        collidingEntityBGTiles.SetFlag(entityBGTiles);
    }
}
