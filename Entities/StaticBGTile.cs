using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace ChroniaHelper.Entities;
[Tracked]
[CustomEntity("ChroniaHelper/StaticBgTile")]
public class StaticBgTile : Platform
{
    public List<StaticBgTile> Group;
    public Dictionary<Entity, Vector2> Moves;
    public Point GroupBoundsMin;
    public Point GroupBoundsMax;

    private readonly char tileType;
    private TileGrid tiles;
    private StaticBgTile master;
    private bool awake;
    private bool HookedToFg;

    // Inherits from Platform so that we can add the bg tile to a floaty fg tile
    public StaticBgTile(Vector2 position, float width, float height, char tileType, int depth)
        : base(position, true)
    {
        this.tileType = tileType;

        Depth = depth;
        
        Collider = new Hitbox(width, height);
        Collidable = false;
        HookedToFg = false;
        
    }

    public StaticBgTile(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Int("Depth"))
    {
    }

    public bool HasGroup { get; private set; }
    public bool MasterOfGroup { get; private set; }
    
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        awake = true;
        if (!HasGroup)
        {
            MasterOfGroup = true;
            Moves = new Dictionary<Entity, Vector2>();
            Group = new List<StaticBgTile>();
            GroupBoundsMin = new Point((int)X, (int)Y);
            GroupBoundsMax = new Point((int)Right, (int)Bottom);
            AddToGroupAndFindChildren(this);
            Rectangle rectangle = new(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, ((GroupBoundsMax.X - GroupBoundsMin.X) / 8) + 1, ((GroupBoundsMax.Y - GroupBoundsMin.Y) / 8) + 1);
            VirtualMap<char> virtualMap = new(rectangle.Width, rectangle.Height, '0');
            foreach (StaticBgTile item in Group)
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
        
    }

    public override void Update()
    {
        base.Update();
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
    
    private void AddToGroupAndFindChildren(StaticBgTile from)
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

        foreach (StaticBgTile entity in Scene.Tracker.GetEntities<StaticBgTile>())
        {
            bool cond1 = entity.Collider.Bounds.Crossover(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height));
            bool cond2 = entity.Collider.Bounds.Crossover(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2));
            
            if (!entity.HasGroup && (cond1 || cond2))
            {
                if (from.HookedToFg)
                {
                    entity.HookedToFg = true;
                }

                AddToGroupAndFindChildren(entity);
            }
        }
    }
    
}