using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/CrumbleBlockOnJump"), Tracked]
public class CrumbleBlockOnJump : GroupedBaseSolid
{
    private float delay;
    private string breakSound;
    private bool triggerOnLean;
    private bool permanent;
    private bool destroyStaticMovers;
    private bool createDebris;
    private EntityID id;

    public CrumbleBlockOnJump(EntityData data, Vector2 offset, EntityID id)
        : base(data, offset, false)
    {
        delay = data.Float("delay");
        tileType = data.Char("tiletype", '3');
        breakSound = data.Attr("breakSound", SFX.game_10_quake_rockbreak);
        triggerOnLean = data.Bool("triggerOnLean");
        permanent = data.Bool("permanent");
        blendIn = data.Bool("blendIn");
        destroyStaticMovers = data.Bool("destroyStaticMovers");
        createDebris = data.Bool("createDebris", true);

        Depth = data.Int("depth", blendIn ? -10501 : -12999);

        if (delay >= 0f)
            Add(new Coroutine(Sequence()));

        Add(new LightOcclude());

        if(data.Bool("triggerFromBottom", false))
        {
            standaloneCollider = new(CollideBottom,
            new Hitbox(Width, 2f, 0f, Height));
            Add(standaloneCollider);
        }

        autoGroupping = data.Bool("autoGroupping", true);
    }
    private bool autoGroupping;
    private PlayerCollider standaloneCollider;

    private void CollideBottom(Player player)
    {
        Trigger();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        //TileGrid tileGrid;

        //if (blendIn)
        //{
        //    var level = SceneAs<Level>();
        //    var bounds = level.Session.MapData.TileBounds;

        //    tileGrid = GFX.FGAutotiler.GenerateOverlay(
        //        tileType,
        //        (int)(X / 8f) - bounds.Left,
        //        (int)(Y / 8f) - bounds.Top,
        //        (int)Width / 8,
        //        (int)Height / 8, level.SolidsData).TileGrid;
        //}
        //else
        //    tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;

        //Add(tileGrid);
        //Add(new TileInterceptor(tileGrid, true));
    }

    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        if (!autoGroupping)
        {
            return false;
        }

        if(other is CrumbleBlockOnJump c)
        {
            return c.delay == delay &&
                c.Depth == Depth &&
                c.tileType == tileType &&
                c.triggerOnLean == triggerOnLean &&
                c.permanent == permanent &&
                c.blendIn == blendIn &&
                c.destroyStaticMovers == destroyStaticMovers &&
                c.createDebris == createDebris;
        }

        return false;
    }

    public override void PostGroupping()
    {
        base.PostGroupping();

        foreach(CrumbleBlockOnJump c in Group)
        {
            c.delay = (master as CrumbleBlockOnJump).delay;
            c.Depth = (master as CrumbleBlockOnJump).Depth;
            c.tileType = (master as CrumbleBlockOnJump).tileType;
            c.triggerOnLean = (master as CrumbleBlockOnJump).triggerOnLean;
            c.permanent = (master as CrumbleBlockOnJump).permanent;
            c.blendIn = (master as CrumbleBlockOnJump).blendIn;
            c.destroyStaticMovers = (master as CrumbleBlockOnJump).destroyStaticMovers;
            c.createDebris = (master as CrumbleBlockOnJump).createDebris;
        }
    }

    //public override void OnStaticMoverTrigger(StaticMover sm) => Triggered = true;

    public void Break()
    {
        if (!Collidable)
            return;

        Audio.Play(breakSound, Position);
        Collidable = false;

        if (createDebris)
        {
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    if (!Scene.CollideCheck<Solid>(new Rectangle((int)X + i, (int)Y + j, 8, 8)))
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(i + 4, j + 4), tileType, true).BlastFrom(TopCenter));
                }
            }
        }

        if (permanent)
            SceneAs<Level>().Session.DoNotLoad.Add(id);

        if (destroyStaticMovers)
            DestroyStaticMovers();

        RemoveSelf();
    }

    private IEnumerator Sequence()
    {
        while (!Triggered && !HasPlayerRider() && (!triggerOnLean || Input.MoveX == 0 || !CollideCheck<Player>(Position - Input.MoveX * Vector2.UnitX)))
            yield return null;

        for (float time = 0f; time < delay; time += Engine.DeltaTime)
            yield return null;

        Break();
    }
}