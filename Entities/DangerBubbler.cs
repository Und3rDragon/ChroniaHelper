using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using YoctoHelper.Hooks;
using static Celeste.LavaRect;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/DangerBubbler")]
public class DangerBubbler : BaseEntity
{
    private static bool HooksLoaded;

    [LoadHook]
    public static void LoadHooksIfNeeded()
    {
        if (HooksLoaded)
        {
            return;
        }
        HooksLoaded = true;

        On.Celeste.Player.NormalBegin += Player_NormalBegin;
    }

    [UnloadHook]
    public static void Unload()
    {
        if (!HooksLoaded)
            return;
        HooksLoaded = false;

        On.Celeste.Player.NormalBegin -= Player_NormalBegin;
    }

    private static void Player_NormalBegin(On.Celeste.Player.orig_NormalBegin orig, Player self)
    {
        orig(self);

        if (self.Scene?.Tracker.GetEntities<DangerBubbler>() is { } bubblers)
        {
            foreach (DangerBubbler bubbler in bubblers)
            {
                bubbler.InBubbler = false;
            }
        }
    }
    
    public DangerBubbler(EntityData d, Vc2 o) : base(d, o)
    {
        
    }

    public bool InBubbler;

    public void PlayerActivated(Player player)
    {
        Collidable = false;
        
        // There are no sprites so there is no need for processing sprites
        Add(new Coroutine(NodeRoutine(player), true));
    }

    public IEnumerator NodeRoutine(Player player)
    {
        if (!player.Dead)
        {
            Audio.Play("event:/game/general/cassette_bubblereturn", SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
            player.Dashes = Math.Max(player.Dashes, player.MaxDashes);
            player.StartCassetteFly(Position, Position);

            InBubbler = true;
        }
        
        yield break;
    }
}
