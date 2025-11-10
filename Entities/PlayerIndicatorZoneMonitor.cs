using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.ChroniaHelperIndicatorZone;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using static Celeste.Mod.ChroniaHelperIndicatorZone.PlayerIndicatorZone;

namespace ChroniaHelper.Entities;

[Tracked(true)]
public class PlayerIndicatorZoneMonitor : Entity
{
    public PlayerIndicatorZoneMonitor() : base() { }

    private Dictionary<int, bool> flagReset = new();
    private Dictionary<int, bool> innerFlagReset = new();

    public override void Update()
    {
        base.Update();
        
        foreach(var item in MaP.level.Tracker.GetEntities<PlayerIndicatorZone>())
        {
            var zone = item as PlayerIndicatorZone;
            if (!zone.independentFlag && zone.zoneMode is ZoneMode.None) return;

            bool flagC = zone.controlFlag.IsNotNullOrEmpty() && !zone.controlFlag.GetFlag();
            if (flagC)
            {
                zone.Visible = false;
                
                if(zone.flagMode is FlagMode.Zone && !flagReset.SafeGet(zone.SourceData.ID, false))
                {
                    zone.flag.SetFlag(false);
                    flagReset.Enter(zone.SourceData.ID, true);
                    innerFlagReset.Enter(zone.SourceData.ID, false);
                }
                return;
            }
            else
            {
                zone.Visible = true;
            }

            flagReset.Enter(zone.SourceData.ID, false);

            bool collided = zone.CollideCheck<Player>();
            
            if(collided && !zone.playerIn) // when enter
            {
                var renderer = Scene.Tracker.GetEntity<IconRenderer>();
                if (zone.zoneMode is ZoneMode.Toggle)
                    renderer.SwitchToHandle(zone);
                else
                    renderer.SwitchToHandle(null);
                zone.lastPlayer = PUt.player;

                switch (zone.flagMode)
                {
                    case FlagMode.Zone:
                    case FlagMode.Enable:
                        zone.flag.SetFlag(true);
                        break;
                    case FlagMode.Disable:
                        zone.flag.SetFlag(false);
                        break;
                }
            }
            
            if (!collided && zone.playerIn && zone.flagMode is FlagMode.Zone) // when leave
            {
                zone.flag.SetFlag(false);

                innerFlagReset.Enter(zone.SourceData.ID, false);
            }
            else if(collided && zone.playerIn && zone.flagMode is FlagMode.Zone && !innerFlagReset.SafeGet(zone.SourceData.ID, false)) // always inside
            {
                zone.flag.SetFlag(true);

                innerFlagReset.Enter(zone.SourceData.ID, true);
            }
            
            zone.playerIn = collided;
        }
    }
}
