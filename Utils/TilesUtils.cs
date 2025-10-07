using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChroniaHelper.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Utils;

public static class TilesUtils
{
    public static void GenerateTiles(this Entity entity, char tile, Coroutine coroutine, out TileGrid tiles, bool finalBoss = false)
    {
        EntityData data = entity.SourceData;
        int newSeed = Calc.Random.Next();
        Calc.PushRandom(newSeed);
        entity.Add(tiles = GFX.FGAutotiler.GenerateBox(tile, data.Width / 8, data.Height / 8).TileGrid);
        Calc.PopRandom();
        TileGrid highlight = GFX.FGAutotiler.GenerateBox('G', data.Width / 8, data.Height / 8).TileGrid;
        if (finalBoss)
        {
            Calc.PushRandom(newSeed);
            entity.Add(highlight);
            Calc.PopRandom();
            highlight.Alpha = 0f;
        }
        entity.Add(coroutine);
        entity.Add(new LightOcclude());
        entity.Add(new TileInterceptor(tiles, highPriority: false));
    }
    
    public static void GenerateWall(this Entity entity, char tileType)
    {
        //移除blendin判断，始终为blendin
        Rectangle tileBounds = MaP.level.Session.MapData.TileBounds;
        VirtualMap<char> solidsData = MaP.level.SolidsData;
        int x = (int)(entity.X / 8f) - tileBounds.Left;
        int y = (int)(entity.Y / 8f) - tileBounds.Top;
        int tilesX = (int)entity.Width / 8;
        int tilesY = (int)entity.Height / 8;
        TileGrid tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
        entity.Add(new EffectCutout());

        entity.Add(tileGrid);
        entity.Add(new TileInterceptor(tileGrid, highPriority: true));
    }
}
