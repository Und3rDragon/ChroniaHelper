using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace ChroniaHelper.Entities;
[Tracked]
[CustomEntity("ChroniaHelper/FloatyBgTile")]
public class FloatyBgTile : Platform
{
    public List<FloatyBgTile> Group;
    public List<FloatySpaceBlock> Floaties;
    public Dictionary<Entity, Vector2> Moves;
    public Point GroupBoundsMin;
    public Point GroupBoundsMax;

    private readonly char tileType;
    private TileGrid tiles;
    private float yLerp;
    private float sinkTimer;
    private float sineWave;
    private float dashEase;
    private FloatyBgTile master;
    private bool awake;
    private bool HookedToFg;
    private float floatAmp;

    // Inherits from Platform so that we can add the bg tile to a floaty fg tile
    public FloatyBgTile(Vector2 position, float width, float height, char tileType, bool disableSpawnOffset, float lerp, int depth)
        : base(position, true)
    {
        this.tileType = tileType;

        Depth = depth;
        sineWave = !disableSpawnOffset ? Calc.Random.NextFloat((float)Math.PI * 2f) : 0f;
        Collider = new Hitbox(width, height);
        //Collidable = false;
        HookedToFg = false;

        this.floatAmp = lerp >= 0 ? lerp : 4f;

        sinkTimer = disableSpawnOffset ? 0f : 0.3f;
    }

    public FloatyBgTile(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("disableSpawnOffset"), data.Float("floatAmplitude"), data.Int("Depth"))
    {
    }

    public bool HasGroup { get; private set; }
    public bool MasterOfGroup { get; private set; }

    private static List<FloatyBgTile> GetBgTileList(FloatySpaceBlock block) => DynamicData.For(block).Get<List<FloatyBgTile>>(Cons.BgTileListDynamicDataName);
    private static void SetBgTileList(FloatySpaceBlock block, List<FloatyBgTile> tiles) => DynamicData.For(block).Set(Cons.BgTileListDynamicDataName, tiles);
    private static void FloatySpaceBlock_AddToGroupAndFindChildren(FloatySpaceBlock parent, FloatySpaceBlock child) => DynamicData.For(parent).Invoke("AddToGroupAndFindChildren", new object[] { child });

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        awake = true;
        if (!HasGroup)
        {
            MasterOfGroup = true;
            Moves = new Dictionary<Entity, Vector2>();
            Group = new List<FloatyBgTile>();
            GroupBoundsMin = new Point((int)X, (int)Y);
            GroupBoundsMax = new Point((int)Right, (int)Bottom);
            AddToGroupAndFindChildren(this);
            Rectangle rectangle = new(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, ((GroupBoundsMax.X - GroupBoundsMin.X) / 8) + 1, ((GroupBoundsMax.Y - GroupBoundsMin.Y) / 8) + 1);
            VirtualMap<char> virtualMap = new(rectangle.Width, rectangle.Height, '0');
            foreach (FloatyBgTile item in Group)
            {
                int num = (int)(item.X / 8f) - rectangle.X;
                int num2 = (int)(item.Y / 8f) - rectangle.Y;
                int num3 = (int)(item.Width / 8f);
                int num4 = (int)(item.Height / 8f);
                for (int i = num; i < num + num3; i++)
                {
                    for (int j = num2; j < num2 + num4; j++)
                    {
                        virtualMap[i, j] = item.tileType;
                    }
                }
            }

            tiles = GFX.BGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour
            {
                EdgesExtend = false,
                EdgesIgnoreOutOfLevel = false,
                PaddingIgnoreOutOfLevel = false
            }).TileGrid;
            tiles.Position = new Vector2(GroupBoundsMin.X - X, GroupBoundsMin.Y - Y);
            tiles.ClipCamera = SceneAs<Level>().Camera;
            tiles.VisualExtend = 1;
            Add(tiles);
        }

        TryToInitPosition();
    }

    public override void Update()
    {
        base.Update();

        if (MasterOfGroup)
        {
            if (sinkTimer > 0f)
            {
                sinkTimer -= Engine.DeltaTime;
            }

            yLerp = sinkTimer > 0f ? Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime) : Calc.Approach(yLerp, 0f, 1f * Engine.DeltaTime);
            sineWave += Engine.DeltaTime;
            dashEase = Calc.Approach(dashEase, 0f, Engine.DeltaTime * 1.5f);
            MoveToTarget();
        }
    }

    public override void MoveHExact(int move)
    {
        Position.X += move;
    }

    public override void MoveVExact(int move)
    {
        Position.Y += move;
    }

    public override void Removed(Scene scene)
    {
        tiles = null;
        Moves = null;
        Group = null;
        base.Removed(scene);
    }

    internal static void Load()
    {
        On.Celeste.FloatySpaceBlock.AddToGroupAndFindChildren += AddToGroupAndFindChildrenAddendum;
        On.Celeste.FloatySpaceBlock.Awake += AwakeAddendum;

    }
    internal static void Unload()
    {
        On.Celeste.FloatySpaceBlock.AddToGroupAndFindChildren -= AddToGroupAndFindChildrenAddendum;
        On.Celeste.FloatySpaceBlock.Awake -= AwakeAddendum;
    }

    private static void AddToGroupAndFindChildrenAddendum(On.Celeste.FloatySpaceBlock.orig_AddToGroupAndFindChildren orig, FloatySpaceBlock self, FloatySpaceBlock from)
    {
        orig(self, from);

        if (GetBgTileList(self) is not List<FloatyBgTile> bgTileList)
        {
            return;
        }

        foreach (FloatyBgTile bgTile in self.Scene.CollideAll<FloatyBgTile>(new Rectangle((int)from.X, (int)from.Y, (int)from.Width, (int)from.Height)))
        {
            if (!bgTileList.Contains(bgTile))
            {
                if (!bgTile.awake)
                {
                    bgTile.Awake(self.Scene);
                }

                bgTileList.Add(bgTile);

                // make the FG tile handle moving our BG tile.
                self.Moves[bgTile] = bgTile.Position;

                FloatyBgTile masterBgTile = bgTile.MasterOfGroup ? bgTile : bgTile.master;
                masterBgTile.HookedToFg = true;

                foreach (FloatyBgTile otherBg in masterBgTile.Group)
                {
                    // also attach all remaining floaty tiles in our group to the fg tile.
                    // this is needed to make sure that the group forms correctly regardless of load order.
                    self.Moves[otherBg] = otherBg.Position;

                    // try to continue expanding our group via nearby floaty fg tiles.
                    foreach (FloatySpaceBlock entity in self.Scene.Tracker.GetEntities<FloatySpaceBlock>())
                    {
                        if (!entity.HasGroup && self.Scene.CollideCheck(new Rectangle((int)otherBg.X, (int)otherBg.Y, (int)otherBg.Width, (int)otherBg.Height), entity))
                        {
                            FloatySpaceBlock_AddToGroupAndFindChildren(self, entity);
                        }
                    }
                }
            }
        }
    }

    private static void AwakeAddendum(On.Celeste.FloatySpaceBlock.orig_Awake orig, FloatySpaceBlock self, Scene scene)
    {
        if (!self.HasGroup)
        {
            SetBgTileList(self, new List<FloatyBgTile>());
        }

        orig(self, scene);
    }

    private void TryToInitPosition()
    {
        if (MasterOfGroup)
        {
            foreach (FloatyBgTile item in Group)
            {
                if (!item.awake)
                {
                    return;
                }
            }

            MoveToTarget();
        }
        else
        {
            master.TryToInitPosition();
        }
    }

    private void AddToGroupAndFindChildren(FloatyBgTile from)
    {
        if (from.X < GroupBoundsMin.X)
        {
            GroupBoundsMin.X = (int)from.X;
        }

        if (from.Y < GroupBoundsMin.Y)
        {
            GroupBoundsMin.Y = (int)from.Y;
        }

        if (from.Right > GroupBoundsMax.X)
        {
            GroupBoundsMax.X = (int)from.Right;
        }

        if (from.Bottom > GroupBoundsMax.Y)
        {
            GroupBoundsMax.Y = (int)from.Bottom;
        }

        from.HasGroup = true;
        Group.Add(from);
        Moves.Add(from, from.Position);
        if (from != this)
        {
            from.master = this;
        }

        foreach (FloatyBgTile entity in Scene.Tracker.GetEntities<FloatyBgTile>())
        {
            if (!entity.HasGroup && (Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), entity) || Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), entity)))
            {
                if (from.HookedToFg)
                {
                    entity.HookedToFg = true;
                }

                AddToGroupAndFindChildren(entity);
            }
        }
    }

    private void MoveToTarget()
    {
        float num = (float)Math.Sin(sineWave) * this.floatAmp;
        Vector2 vector = Vector2.Zero;

        for (int i = 0; i < 2; i++)
        {
            foreach (KeyValuePair<Entity, Vector2> move in Moves)
            {
                Entity key = move.Key;
                Vector2 value = move.Value;
                if (!HookedToFg)
                {
                    float num2 = MathHelper.Lerp(value.Y, value.Y + 12f, Ease.SineInOut(yLerp)) + num;
                    key.Position.Y = num2;
                    key.Position.X = value.X;
                }
            }
        }
    }
}