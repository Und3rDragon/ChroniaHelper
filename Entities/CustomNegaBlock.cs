using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using YoctoHelper.Cores;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomNegaBlock")]
public class CustomNegaBlock : NegaBlock
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Player.RefillDash += WhenRefillDash;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Player.RefillDash -= WhenRefillDash;
    }

    public static bool WhenRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self)
    {
        if (self.CollideCheck<CustomNegaBlock>(self.Position + Vc2.UnitY))
        {
            return false;
        }
        else
        {
            return orig(self);
        }
    }

    public CustomNegaBlock(EntityData data, Vc2 offset) : base(data.Position + offset, data.Width, data.Height)
    {
        Depth = data.Int("depth", -9000);
        tileType = data.Char("tileType", '3');
        lightOcclude = data.Float("lightOcclude", 1F, 0F, 1F);
        blendIn = data.Bool("blendIn", false);
    }
    private bool blendIn = false;
    private char tileType = '3';
    private float lightOcclude = 0f;
    private TileGrid tiles;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        int tilesX = (int)(base.Width / 8);
        int tilesY = (int)(base.Height / 8);
        if (this.blendIn)
        {
            Level level = base.SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)base.X / 8 - tileBounds.Left;
            int y = (int)base.Y / 8 - tileBounds.Top;
            this.tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            base.Depth = Depths.Solids;
        }
        else
        {
            this.tiles = GFX.FGAutotiler.GenerateBox(tileType, tilesX, tilesY).TileGrid;
        }
        base.Add(this.tiles);
        base.Add(new TileInterceptor(this.tiles, true));
        base.Add(new LightOcclude(lightOcclude));
        base.SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
    }

    public override void Render()
    {
        Components.Render();
    }
}
