using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/BGTileCollideController")]
[Tracked(true)]
public class BGTilesCollideController : Entity
{
    public BGTilesCollideController(EntityData data, Vector2 offset): base(data.Position + offset)
    {
        flag = data.Attr("flag", "ChroniaHelper_PlayerCollidingBGTiles");
        condition = data.Attr("conditionFlags");
        setFlagOnCollide = data.Bool("indicatesColliding", true);
        killPlayer = data.Bool("killPlayer", false);
        killWhenNotColliding = data.Bool("killWhenNotColliding", true);
    }
    private string flag, condition;
    private string[] conditions;
    private bool setFlagOnCollide, killPlayer, killWhenNotColliding;
    
    public override void Update()
    {
        base.Update();
        
        if (!condition.IsNullOrEmpty())
        {
            if (!condition.ConfirmFlags()) { return; }
        }

        if (!PUt.getPlayer) { return; }

        if (!flag.IsNullOrEmpty())
        {
            flag.SetFlag(PUt.player.CollideCheck(MaP.bgSolidTiles) == setFlagOnCollide);
        }
        
        if (killPlayer)
        {
            if (killWhenNotColliding == !PUt.player.CollideCheck(MaP.bgSolidTiles))
            {
                if (PUt.player.IsNotNull()) { PUt.player.Die(Vector2.Zero); }
            }
        }
    }
}
