using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/CustomBillboard")]
[Tracked(false)]
public class CustomBillboard : Entity
{
    private class FG : Entity
    {
        public CustomBillboard Parent;

        public FG(CustomBillboard parent)
        {
            Parent = parent;
            base.Depth = Parent.Depth - 5;
        }

        public override void Render()
        {
            uint seed = Parent.Seed;
            Parent.DrawNoise(Parent.Collider.Bounds, ref seed, Color.White * 0.1f, Parent.noiseTx);
            for (int i = (int)Parent.Y; (float)i < Parent.Bottom; i += 2)
            {
                float num = 0.05f + (1f + (float)Math.Sin((float)i / 16f + base.Scene.TimeActive * 2f)) / 2f * 0.2f;
                Draw.Line(Parent.X, i, Parent.X + Parent.Width, i, Color.Teal * num);
            }
        }
    }

    public Color BackgroundColor;

    public uint Seed;

    private MTexture[,] tiles;

    private string borderTx;

    private string noiseTx;

    public CustomBillboard(EntityData e, Vector2 offset)
    {
        Position = e.Position + offset;
        base.Collider = new Hitbox(e.Width, e.Height);
        base.Depth = 9010;
        Add(new CustomBloom(RenderBloom));
        borderTx = e.Attr("borderTexture", "scenery/tvSlices");
        noiseTx = e.Attr("noiseTexture", "util/noise");
        BackgroundColor = e.HexColor("screenColor", Color.DarkSlateBlue);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(new FG(this));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        MTexture mTexture = GFX.Game[borderTx];
        tiles = new MTexture[mTexture.Width / 8, mTexture.Height / 8];
        for (int i = 0; i < mTexture.Width / 8; i++)
        {
            for (int j = 0; j < mTexture.Height / 8; j++)
            {
                tiles[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }

        int num = (int)(base.Width / 8f);
        int num2 = (int)(base.Height / 8f);
        for (int k = -1; k <= num; k++)
        {
            AutoTile(k, -1);
            AutoTile(k, num2);
        }

        for (int l = 0; l < num2; l++)
        {
            AutoTile(-1, l);
            AutoTile(num, l);
        }
    }


    private void AutoTile(int x, int y)
    {
        if (Empty(x, y))
        {
            bool flag = !Empty(x - 1, y);
            bool flag2 = !Empty(x + 1, y);
            bool flag3 = !Empty(x, y - 1);
            bool flag4 = !Empty(x, y + 1);
            bool flag5 = !Empty(x - 1, y - 1);
            bool flag6 = !Empty(x + 1, y - 1);
            bool flag7 = !Empty(x - 1, y + 1);
            bool flag8 = !Empty(x + 1, y + 1);
            if (!flag2 && !flag4 && flag8)
            {
                Tile(x, y, tiles[0, 0]);
            }
            else if (!flag && !flag4 && flag7)
            {
                Tile(x, y, tiles[2, 0]);
            }
            else if (!flag3 && !flag2 && flag6)
            {
                Tile(x, y, tiles[0, 2]);
            }
            else if (!flag3 && !flag && flag5)
            {
                Tile(x, y, tiles[2, 2]);
            }
            else if (flag2 && flag4)
            {
                Tile(x, y, tiles[3, 0]);
            }
            else if (flag && flag4)
            {
                Tile(x, y, tiles[4, 0]);
            }
            else if (flag2 && flag3)
            {
                Tile(x, y, tiles[3, 2]);
            }
            else if (flag && flag3)
            {
                Tile(x, y, tiles[4, 2]);
            }
            else if (flag4)
            {
                Tile(x, y, tiles[1, 0]);
            }
            else if (flag2)
            {
                Tile(x, y, tiles[0, 1]);
            }
            else if (flag)
            {
                Tile(x, y, tiles[2, 1]);
            }
            else if (flag3)
            {
                Tile(x, y, tiles[1, 2]);
            }
        }
    }


    private void Tile(int x, int y, MTexture tile)
    {
        Image image = new Image(tile);
        image.Position = new Vector2(x, y) * 8f;
        Add(image);
    }


    private bool Empty(int x, int y)
    {
        return !base.Scene.CollideCheck<CustomBillboard>(new Rectangle((int)base.X + x * 8, (int)base.Y + y * 8, 8, 8));
    }


    public override void Update()
    {
        base.Update();
        if (base.Scene.OnInterval(0.1f))
        {
            Seed++;
        }
    }


    private void RenderBloom()
    {
        Draw.Rect(base.Collider, Color.White * 0.4f);
    }


    public override void Render()
    {
        base.Render();
        uint seed = Seed;
        Draw.Rect(base.Collider, BackgroundColor);
        DrawNoise(base.Collider.Bounds, ref seed, Color.White * 0.1f, noiseTx);
    }


    public void DrawNoise(Rectangle bounds, ref uint seed, Color color, string noiseTexturePath)
    {
        MTexture mTexture = GFX.Game[noiseTexturePath];
        Vector2 vector = new Vector2(PseudoRandRange(ref seed, 0f, mTexture.Width / 2), PseudoRandRange(ref seed, 0f, mTexture.Height / 2));
        Vector2 vector2 = new Vector2(mTexture.Width, mTexture.Height) / 2f;
        for (float num = 0f; num < (float)bounds.Width; num += vector2.X)
        {
            float num2 = Math.Min((float)bounds.Width - num, vector2.X);
            for (float num3 = 0f; num3 < (float)bounds.Height; num3 += vector2.Y)
            {
                float num4 = Math.Min((float)bounds.Height - num3, vector2.Y);
                int x = (int)((float)mTexture.ClipRect.X + vector.X);
                int y = (int)((float)mTexture.ClipRect.Y + vector.Y);
                Rectangle value = new Rectangle(x, y, (int)num2, (int)num4);
                Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, new Vector2((float)bounds.X + num, (float)bounds.Y + num3), value, color);
            }
        }
    }


    private static uint PseudoRand(ref uint seed)
    {
        seed ^= seed << 13;
        seed ^= seed >> 17;
        return seed;
    }


    private static float PseudoRandRange(ref uint seed, float min, float max)
    {
        return min + (float)(PseudoRand(ref seed) % 1000) / 1000f * (max - min);
    }
}