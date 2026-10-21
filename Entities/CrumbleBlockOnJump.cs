using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Components.StateListeners;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.References;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/CrumbleBlockOnJump"), Tracked]
[ExA.Credits("Programmatic for Prog Helper Crumble Block On Jump source codes")]
public class CrumbleBlockOnJump : GroupedBaseSolid
{
    /// <summary>
    /// 跳跃类钩子：玩家起跳离开方块的那一瞬间，立刻打断倒计时并让整组破碎。
    /// </summary>
    [SelectiveLoadHook]
    public static void Load()
    {
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.WallJump += Player_WallJump;
        On.Celeste.Player.ClimbJump += Player_ClimbJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
    }

    [SelectiveUnloadHook]
    public static void Unload()
    {
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.WallJump -= Player_WallJump;
        On.Celeste.Player.ClimbJump -= Player_ClimbJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
    }

    /// <summary>
    /// 玩家当前重力方向（重力翻转时为向上）。
    /// </summary>
    private static Vector2 GravityDirection => APIGravityHelper.playerInverted ? -Vector2.UnitY : Vector2.UnitY;

    /// <summary>
    /// 找出紧贴玩家、位于指定方向上的方块，并让整组立刻破碎。
    /// </summary>
    private static void BreakNearby(Player player, Vector2 dir)
    {
        if (player.CollideFirst<CrumbleBlockOnJump>(player.Position + dir) is { } block)
        {
            (block.master as CrumbleBlockOnJump)?.BreakAll();
        }
    }

    private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playsfx)
    {
        orig(self, particles, playsfx);
        BreakNearby(self, GravityDirection);
    }

    private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        orig(self);
        BreakNearby(self, GravityDirection);
    }

    private static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        orig(self, dir);
        BreakNearby(self, -3 * dir * Vector2.UnitX);
    }

    private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        orig(self);

        int dir = (int)Input.MoveX;
        if (dir == 0)
        {
            dir = (int)self.Facing;
        }

        BreakNearby(self, 3 * dir * Vector2.UnitX);
    }

    private static void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        orig(self, dir);
        BreakNearby(self, -5 * dir * Vector2.UnitX);
    }

    private float delay;
    private string breakSound;
    private bool triggerOnLean;
    private bool permanent;
    private bool destroyStaticMovers;
    private bool createDebris;
    public CrumbleBlockOnJump(EntityData data, Vector2 offset, EntityID id)
        : base(data, offset, false)
    {
        Ldm.LoadHook<CrumbleBlockOnJump>();
        Ldm.LoadHook(typeof(BounceHelperUtils));

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

        triggerFromBottom = data.Bool("triggerFromBottom", false);
        if (triggerFromBottom)
        {
            listener = new(CollideBottom,
            new Hitbox(Width, 4f, 0f, Height));
            Add(listener);

            bottomBreak = new(BottomBreak());
            bottomBreak.Active = false;
            Add(bottomBreak);
        }

        autoGroupping = data.Bool("autoGroupping", true);

        bounceModeAdapter = new("bounceModeEnabled")
        {
            onEnable = () => Ldm.LoadHook(typeof(BounceHelperUtils)),
            onDisable = () => Ldm.UnloadHook(typeof(BounceHelperUtils)),
        };
        
        Add(bounceModeAdapter);
    }
    private bool triggerFromBottom;
    private bool autoGroupping;
    private PlayerCollider listener;
    private Coroutine bottomBreak;
    private FlagListener bounceModeAdapter;

    private void CollideBottom(Player p)
    {
        if (!RefBounceHelper.BounceModeEnabled)
        {
            bottomBreak?.Active = true;
        }
    }

    private IEnumerator BottomBreak()
    {
        yield return 0.1f;

        (master as CrumbleBlockOnJump).BreakAll();
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

    public override void OnStaticMoverTrigger(StaticMover sm) => TriggerAll();

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
            SceneAs<Level>().Session.DoNotLoad.Add(SourceId);

        if (destroyStaticMovers)
            DestroyStaticMovers();

        RemoveSelf();
    }

    private IEnumerator Sequence()
    {
        while (!Triggered && !HasPlayerRider() && (!triggerOnLean || Input.MoveX == 0 || !CollideCheck<Player>(Position - Input.MoveX * Vector2.UnitX)))
            yield return null;

        for (float time = 0f; time < delay; time += Engine.DeltaTime)
        {
            yield return null;
        }

        (master as CrumbleBlockOnJump).BreakAll();
    }

    private bool breaking = false;
    public void BreakAll()
    {
        if (breaking) { return; }

        breaking = true;
        foreach(CrumbleBlockOnJump j in Group)
        {
            j.Break();
        }
    }
}