using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;

namespace ChroniaHelper.Entities;

[Credits("Original source code published by EllaTAS")]
[CustomEntity("ChroniaHelper/Balloon")]
public class Balloon : BaseEntity
{
    private static int BalloonCount;
    /// <summary>
    /// The sprite must contains "idle", "pop", and "spawn"
    /// </summary>
    private readonly Sprite sprite;
    private readonly float floatyOffset;
    private readonly bool oneUse, superBounce;
    private bool isLead, inBubble;

    private float floatScale = 1.5f;
    private string popSound;

    public Balloon(EntityData data, Vector2 offset) : base(data, offset)
    {
        oneUse = data.Bool("oneUse");
        superBounce = data.Bool("superBounce");
        Collider = data.Attr("collider", "r,16,16,-8,-8").ParseColliderList();
        Depth = data.Int("depth", -1);
        floatyOffset = (int)(data.Float("floatPhase", 0f) * Calc.Random.NextFloat());
        Add(sprite = GFX.SpriteBank.Create(data.Attr("spriteXMLTag", "ChroniaHelper_balloon")));
        Add(new PlayerCollider(onPlayer));
        sprite.Play("idle", true, true);

        // newly added customizations
        floatScale = data.Float("floatScale", 0f);
        popSound = data.Attr("popSound", "event:/game/general/diamond_touch");
    }

    public override void Update()
    {
        base.Update();

        // check if inside a Collidable VortexHelper bubble
        bool collideBubble = false;
        foreach (Type t in SceneAs<Level>().Tracker.Entities.Keys)
        {
            // check if DashBubbles are tracked
            if (t.ToString() == "Celeste.Mod.VortexHelper.Entities.DashBubble")
            {
                collideBubble = SceneAs<Level>().Tracker.Entities[t].Any(e => e.Collidable && CollideCheck(e));
                break;
            }
        }
        if (collideBubble && !inBubble)
        {
            Collidable = false;
            inBubble = true;
        }
        if (!collideBubble && inBubble)
        {
            Collidable = true;
            inBubble = false;
        }

        sprite.RenderPosition = Position + (floatScale * Vector2.UnitY * (float)Math.Sin(2 * (Engine.Scene.TimeActive + floatyOffset)));

        if (isLead)
        {
            if (SceneAs<Level>().Tracker.GetEntity<Player>()?.OnGround() == true)
            {
                BalloonCount = 0;
            }
        }
    }

    private void onPlayer(Player player)
    {
        if (superBounce)
        {
            float speedX = player.Speed.X;
            player.SuperBounce(Y);
            player.Speed.X = speedX;
        }
        else
        {
            player.Bounce(Y);
        }
        player.AutoJumpTimer = 0f;
        player.Speed.X *= 1.2f;
        sprite.Play("pop");
        Audio.Play(popSound);
        if (BalloonCount < 7)
        {
            BalloonCount++;
        }
        Collidable = false;
        Add(new Coroutine(RoutineRespawn()));
    }

    private IEnumerator RoutineRespawn()
    {
        yield return 2.5f;
        if (oneUse)
        {
            RemoveSelf();
            yield break;
        }
        Collidable = true;
        sprite.Play("spawn");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        BalloonCount = -1;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (BalloonCount == -1)
        {
            isLead = true;
            BalloonCount = 0;
        }
    }
}