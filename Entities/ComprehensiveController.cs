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
        touchingTriggers = data.Attr("PlayerTouchingTriggers", "ChroniaHelper_PlayerTouchingTriggers");
        collidingEntitiesAbove = data.Attr("PlayerCollidingEntitiesAbove", "ChroniaHelper_PlayerCollidingEntitiesAbove");
        collidingEntitiesBelow = data.Attr("PlayerCollidingEntitiesBelow", "ChroniaHelper_PlayerCollidingEntitiesBelow");
        collidingEntitiesSame = data.Attr("PlayerCollidingEntitiesWithSameDepth", "ChroniaHelper_PlayerCollidingEntitiesWithSameDepth");
    }
    private string collidingBGTiles, touchingTriggers;
    private string collidingEntitiesSame, collidingEntitiesAbove, collidingEntitiesBelow;

    public override void Update()
    {
        base.Update();

        if (!PUt.getPlayer) { return; }

        // Colliding BG Tiles
        collidingBGTiles.SetFlag(PUt.player.CollideCheck(MaP.bgSolidTiles(MaP.level)));
        // Colliding Triggers
        touchingTriggers.SetFlag(PUt.player.CollideCheck<Trigger>());

        // Colliding Different Entities
        bool colliding_overrideS = false, colliding_overrideA = false, colliding_overrideB = false;
        foreach(var item in MaP.level.Entities)
        {
            if (item.Depth == PUt.player.Depth && item.CollideCheck<Player>()) { 
                collidingEntitiesSame.SetFlag(true);
                colliding_overrideS = true;
            }
            if (item.Depth > PUt.player.Depth && item.CollideCheck<Player>())
            {
                collidingEntitiesAbove.SetFlag(true);
                colliding_overrideA = true;
            }
            if (item.Depth < PUt.player.Depth && item.CollideCheck<Player>())
            {
                collidingEntitiesBelow.SetFlag(true);
                colliding_overrideB = true;
            }
        }
        if (!colliding_overrideA)
        {
            collidingEntitiesAbove.SetFlag(false);
        }
        if (!colliding_overrideB)
        {
            collidingEntitiesBelow.SetFlag(false);
        }
        if (!colliding_overrideS)
        {
            collidingEntitiesSame.SetFlag(false);
        }
    }
}
