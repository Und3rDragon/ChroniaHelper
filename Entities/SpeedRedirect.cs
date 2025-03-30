using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SpeedRedirect")]
public class SpeedRedirect : Entity
{

    public SpeedRedirect(Vector2 position) : base(position) { }

    public SpeedRedirect(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset)
    {
        ID = id;
        size = new Vector2(data.Width, data.Height);

        nodes = data.NodesWithPosition(offset);

        angle = nodes[1] - Position;

        Collider = new Hitbox(size.X, size.Y);
        Add(new PlayerCollider(OnPlayer, new Hitbox(data.Width, data.Height)));

        once = data.Bool("onlyOnce", false);
        multiplier = data.Float("speedMultiplier", 1f);
    }
    private Vector2 size;
    private EntityID ID;
    private Vector2[] nodes;
    private float moveTime = 0.1f;
    private Vector2 playerSpeed, angle;
    private bool once;
    private float multiplier;

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public void OnPlayer(Player player)
    {
        if (!sequenceActive)
        {
            Add(new Coroutine(OnCollide(player)));
        }
    }

    private bool sequenceActive = false;
    private IEnumerator OnCollide(Player player)
    {
        sequenceActive = true;

        // Move Player to the center
        playerSpeed = player.Speed;
        Vector2 playerPosition = player.Center; 
        Vector2 center = Position + size / 2f;

        // reset player state
        player.StateMachine.state = 0;

        float timer = 0f;
        while (timer < moveTime)
        {
            player.Center = Calc.LerpSnap(playerPosition, center, timer / moveTime);

            timer = Calc.Approach(timer, moveTime, Engine.DeltaTime);

            yield return null;
        }

        // Strange bug, but could be interesting?
        //player.MoveTowardsX(center.X, 3 * Engine.DeltaTime);
        //player.MoveTowardsY(center.Y, 3 * Engine.DeltaTime);

        // Redirect layer speed
        player.Speed = angle.SafeNormalize(new Vector2(1f, 0f)) * playerSpeed.Length() * multiplier;

        while (CollideCheck<Player>())
        {
            yield return null;
        }

        if (once)
        {
            RemoveSelf();
        }

        sequenceActive = false;

        yield return null;
    }

    public override void Render()
    {
        base.Render();
        Draw.HollowRect(Position.X, Position.Y, size.X, size.Y, Color.White);
    }

}
