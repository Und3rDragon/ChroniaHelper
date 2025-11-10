using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;
[Tracked]
[CustomEntity("ChroniaHelper/SeperatedTile")]
public class SeperatedBgTile : Platform
{
    
    private readonly char tileType;
    private TileGrid tiles;
    private SeperatedBgTile master;
    private bool awake;

    private float width, height;
    private Vector2 pos;

    private bool fg;

    // Inherits from Platform so that we can add the bg tile to a floaty fg tile
    public SeperatedBgTile(Vector2 position, 
        float width, float height, char tileType, 
        bool disableSpawnOffset, float lerp, int depth,
        bool fgTx)
        : base(position, true)
    {
        this.tileType = tileType;
        
        Depth = depth;
        
        Collider = new Hitbox(width, height);
        Collidable = false;
        
        this.width = width;
        this.height = height;

        pos = position;

        int width8 = (int)width / 8;
        int height8 = (int)height / 8;

        fg = fgTx;

        if (fg)
        {
            Add(tiles = GFX.FGAutotiler.GenerateBox(tileType, width8, height8).TileGrid);
        }
        else
        {
            Add(tiles = GFX.BGAutotiler.GenerateBox(tileType, width8, height8).TileGrid);
        }

    }

    public SeperatedBgTile(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, 
              data.Height, data.Char("tiletype", '3'), 
              data.Bool("disableSpawnOffset"), 
              data.Float("floatAmplitude"), 
              data.Int("Depth"),
              data.Bool("fgTexture",true))
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        awake = true;
        
        int num = (int)(pos.X / 8f);
        int num2 = (int)(pos.Y / 8f);
        int num3 = (int)(width / 8f);
        int num4 = (int)(height / 8f);
        VirtualMap<char> virtualMap = new(num3, num4, '0');
        for (int i = num; i < num + num3; i++)
        {
            for (int j = num2; j < num2 + num4; j++)
            {
                virtualMap[i, j] = tileType;
            }
        }

        /*
         tiles = GFX.BGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour
        {
            EdgesExtend = false,
            EdgesIgnoreOutOfLevel = false,
            PaddingIgnoreOutOfLevel = false
        }).TileGrid;
        tiles.Position = pos;
        tiles.ClipCamera = SceneAs<Level>().Camera;
        tiles.VisualExtend = 1;
        Add(tiles);
         */
        
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
    public override void MoveHExact(int move)
    {
        Position.X += move;
    }

    public override void MoveVExact(int move)
    {
        Position.Y += move;
    }
}