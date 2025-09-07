using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod;
using System.Collections;
using Microsoft.Xna.Framework;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/FallThrough")]
public class FallThrough : JumpthruPlatform
{
    private float playerDuckTimer = 0;
    private float FallTime = 0.35f;
    private float mechanicTime = 0.25f;
    private bool collidableOverride;

    public FallThrough(EntityData data, Vector2 offset) : base(data, offset)
    {
        FallTime = data.Float("FallTime", 0.35f);
        collidableOverride = !data.Bool("ignoreAllJumpthrus", false);
        mechanicTime = data.Float("mechanicTime", 0.25f).GetAbs();
    }

    public override void Update()
    {
        base.Update();
        if (PlayerUtils.TryGetPlayer(out Player player) && player.Ducking && HasPlayerRider())
        {
            playerDuckTimer += Engine.DeltaTime;
            if (playerDuckTimer >= FallTime)
            {
                player.Add(new Coroutine(FallThru(player), true));
            }
        }
        else
        {
            playerDuckTimer = 0f;
        }
    }

    public IEnumerator FallThru(Player player)
    {
        if (player != null && !player.Dead)
        {
            GoThrough(player, true);
        }
        player.Speed = new Vector2(0f, 60f);
        yield return mechanicTime;
        while (!PlayerUtils.TryGetAlivePlayer(out player)) //Fixes up some janky stuff
        {
            yield return null;
        }
        GoThrough(player, false);
    }

    public void GoThrough(Player p, bool on)
    {
        if (collidableOverride)
        {
            Collidable = !on;
        }
        else
        {
            p.IgnoreJumpThrus = on;
        }
    }
}
