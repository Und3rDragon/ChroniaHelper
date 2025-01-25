using Celeste.Mod.Entities;
using YoctoHelper.Cores;

namespace YoctoHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/CrumbleBlock")]
public class CrumbleBlock : BaseSolid
{

    private TileGrid tiles;

    public CrumbleBlock(Vector2 position, EntityData data, EntityID id) : base(position, data, id)
    {
    }

    public CrumbleBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data, id)
    {
    }

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
            this.tiles = GFX.FGAutotiler.GenerateOverlay(base.tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            base.Depth = Depths.Solids;
        }
        else
        {
            this.tiles = GFX.FGAutotiler.GenerateBox(base.tileType, tilesX, tilesY).TileGrid;
        }
        base.Add(this.tiles);
        base.Add(new TileInterceptor(this.tiles, true));
        base.Add(new LightOcclude(base.lightOcclude));
        base.SurfaceSoundIndex = SurfaceIndex.TileToIndex[this.tileType];
        if (base.CollideCheck<Player>())
        {
            base.RemoveSelf();
        }
    }

}
