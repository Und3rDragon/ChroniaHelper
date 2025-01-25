using Celeste.Mod.Entities;
using System;
using YoctoHelper.Cores;

namespace YoctoHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/AlternateTileBlock")]
public class AlternateTileBlock : BaseSolid
{

    private TileGrid tiles;

    public AlternateTileBlock(Vector2 position, EntityData data, EntityID id) : base(position, data, id)
    {
        dir = data.Attr("sprite");
        size = data.Int("unitSize", 8);

        if(!string.IsNullOrEmpty(dir))
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    edges[i, j] = GFX.Game[dir].GetSubtexture(i * size, j * size, size, size);
        }

        base.Depth = data.Int("depth", -9000);
    }
    private int size;
    private string dir;
    private readonly MTexture[,] edges = new MTexture[3, 3];

    public AlternateTileBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data, id)
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        int tilesX = (int)(base.Width / size);
        int tilesY = (int)(base.Height / size);
        Level level = base.SceneAs<Level>();

        base.Add(new LightOcclude(base.lightOcclude));

        if (base.CollideCheck<Player>())
        {
            base.RemoveSelf();
        }
    }

    public override void Render()
    {
        float num = base.Collider.Width / size - 1f;
        float num2 = base.Collider.Height / size - 1f;
        for (int i = 0; (float)i <= num; i++)
        {
            for (int j = 0; (float)j <= num2; j++)
            {
                int num3 = (((float)i < num) ? Math.Min(i, 1) : 2);
                int num4 = (((float)j < num2) ? Math.Min(j, 1) : 2);
                edges[num3, num4].Draw(Position + base.Shake + new Vector2(i * size, j * size));
            }
        }

        base.Render();
    }

}
