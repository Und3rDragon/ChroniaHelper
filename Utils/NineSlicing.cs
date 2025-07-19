using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ChroniaHelper.Utils;

public class NineSlicing
{
    public string Path { get; set; }
    public int SliceWidth { get; set; } = 8;
    public int SliceHeight { get; set; } = 8;
    public int SizeX { get; set; } = 3;
    public int SizeY { get; set; } = 3;

    public MTexture Parent = null;

    public NineSlicing(string path)
    {
        Path = path;

        Parent = GFX.Game[Path] ?? throw new ArgumentException($"Texture not found: {Path}");

        CreateSlicing();
    }

    public NineSlicing(string path, int sliceWidth, int sliceHeight, int unitX, int unitY)
    {
        Path = path;
        Parent = GFX.Game[Path] ?? throw new ArgumentException($"Texture not found: {Path}");

        SliceWidth = sliceWidth.GetAbs() == 0 ? 8 : sliceWidth.GetAbs();
        SliceHeight = sliceHeight.GetAbs() == 0 ? 8 : sliceHeight.GetAbs();
        SizeX = unitX.GetAbs() == 0 ? 3 : unitX.GetAbs();
        SizeY = unitY.GetAbs() == 0 ? 3 : unitY.GetAbs();

        CreateSlicing();
    }

    private int[,] Pad = new int[3,3]
    {
        {1,2,3 },
        {4,5,6 },
        {7,8,9 }
    };

    public Dictionary<int, List<MTexture>> Slices = new()
    {
        {1, new()}, {2, new()}, {3, new()},
        {4, new()}, {5, new()}, {6, new()}, {7, new()},
        {8, new()}, {9, new()},
    };

    public void CreateSlicing()
    {
        foreach (var list in Slices.Values)
        {
            list.Clear();
        }

        MTexture t0 = Parent.GetSubtexture(0, 0, SliceWidth, SliceHeight);
        if (SizeX == 1)
        {
            if(SizeY == 1)
            {
                for (int i = 1; i <= 9; i++)
                {
                    Slices[i].Enter(t0);
                }
            }
            else if(SizeY == 2)
            {
                // n1
                // n2
                List<int> n1 = new List<int> { 1, 2, 3 },
                    n2 = new List<int> { 4, 5, 6, 7, 8, 9 };
                MTexture t1 = Parent.GetSubtexture(0, SliceHeight, SliceWidth, SliceHeight);
                foreach (int i in n1)
                {
                    Slices[i].Enter(t0);
                }
                foreach(int i in n2)
                {
                    Slices[i].Enter(t1);
                }
            }
            else if(SizeY > 2)
            {
                // n1
                // n2
                // n3
                List<int> n1 = new List<int> { 1, 2, 3 },
                    n2 = new List<int> { 4, 5, 6 }, n3 = new List<int> { 7, 8, 9 };
                foreach(int item in n1)
                {
                    Slices[item].Enter(t0);
                }
                for(int i = 1; i < SizeY - 1; i++)
                {
                    MTexture t1 = Parent.GetSubtexture(0, i * SliceHeight, SliceWidth, SliceHeight);
                    foreach (int item in n2)
                    {
                        Slices[item].Enter(t1);
                    }
                }
                MTexture t2 = Parent.GetSubtexture(0, (SizeY - 1) * SliceHeight, SliceWidth, SliceHeight);
                foreach (int item in n3)
                {
                    Slices[item].Enter(t2);
                }
            }
        }

        else if(SizeX == 2)
        {
            if (SizeY == 1)
            {
                // n0 n1
                MTexture t1 = Parent.GetSubtexture(SliceWidth, 0, SliceWidth, SliceHeight);
                List<int> n0 = new() { 1, 4, 7 }, n1 = new() { 2, 5, 8, 3, 6, 9 };
                foreach(var item in n0)
                {
                    Slices[item].Enter(t0);
                }
                foreach(var item in n1)
                {
                    Slices[item].Enter(t1);
                }
            }
            else if (SizeY == 2)
            {
                // 1 n1
                // n2 n3
                Slices[1].Enter(t0);
                List<int> n1 = new() { 2, 3 }, n2 = new() { 4, 7 }, n3 = new() { 5, 6, 8, 9 };
                MTexture t1 = Parent.GetSubtexture(SliceWidth, 0, SliceWidth, SliceHeight),
                    t2 = Parent.GetSubtexture(0, SliceHeight, SliceWidth, SliceHeight),
                    t3 = Parent.GetSubtexture(SliceWidth, SliceHeight, SliceWidth, SliceHeight);
                foreach (var item in n1)
                {
                    Slices[item].Enter(t1);
                }
                foreach (var item in n2)
                {
                    Slices[item].Enter(t2);
                }
                foreach (var item in n3)
                {
                    Slices[item].Enter(t3);
                }
            }
            else if (SizeY > 2)
            {
                // 1 n1
                // 4 n2
                // 7 n3
                List<int> n1 = new() { 2, 3 }, n2 = new() { 5, 6 }, n3 = new() { 8, 9 };

                Slices[1].Enter(t0);
                MTexture t1 = Parent.GetSubtexture(SliceWidth, 0, SliceWidth, SliceHeight);
                foreach(var item in n1)
                {
                    Slices[item].Enter(t1);
                }

                for (int i = 1; i < SizeY - 1; i++)
                {
                    MTexture t4 = Parent.GetSubtexture(0, i * SliceHeight, SliceWidth, SliceHeight),
                        t2 = Parent.GetSubtexture(SliceWidth, i * SliceHeight, SliceWidth, SliceHeight);

                    foreach(var item in n2)
                    {
                        Slices[item].Enter(t2);
                    }
                    Slices[4].Enter(t4);
                }

                MTexture t7 = Parent.GetSubtexture(0, (SizeY - 1) * SliceHeight, SliceWidth, SliceHeight),
                    t3 = Parent.GetSubtexture(SliceWidth, (SizeY - 1) * SliceHeight, SliceWidth, SliceHeight);
                Slices[7].Enter(t7);
                foreach(var item in n3)
                {
                    Slices[item].Enter(t3);
                }
            }
        }

        else if(SizeX > 2)
        {
            if(SizeY == 1)
            {
                // 1 t1 t2

                Slices[1].Enter(t0);
                Slices[4].Enter(t0);
                Slices[7].Enter(t0);
                
                for (int tileX = 1; tileX < SizeX - 1; tileX++)
                {
                    MTexture t1 =Parent.GetSubtexture(tileX * SliceWidth, 0, SliceWidth, SliceHeight);
                    Slices[2].Enter(t1);
                    Slices[5].Enter(t1);
                    Slices[8].Enter(t1);
                }

                MTexture t2 = Parent.GetSubtexture((SizeX - 1) * SliceWidth, 0, SliceWidth, SliceHeight);
                Slices[3].Enter(t2);
                Slices[6].Enter(t2);
                Slices[9].Enter(t2);
            }
            else if(SizeY == 2)
            {
                // 1 t1 t2
                // t3 t4 t5

                Slices[1].Enter(t0);

                for (int tileX = 1; tileX < SizeX - 1; tileX++)
                {
                    MTexture t1 = Parent.GetSubtexture(tileX * SliceWidth, 0, SliceWidth, SliceHeight);
                    Slices[2].Enter(t1);
                }

                MTexture t2 = Parent.GetSubtexture((SizeX - 1) * SliceWidth, 0, SliceWidth, SliceHeight);
                Slices[3].Enter(t2);

                MTexture t3 = Parent.GetSubtexture(0, SliceHeight, SliceWidth, SliceHeight);
                Slices[4].Enter(t3);
                Slices[7].Enter(t3);

                for (int tileX = 1; tileX < SizeX - 1; tileX++)
                {
                    MTexture t4 = Parent.GetSubtexture(tileX * SliceWidth, SliceHeight, SliceWidth, SliceHeight);
                    Slices[5].Enter(t4);
                    Slices[8].Enter(t4);
                }

                MTexture t5 = Parent.GetSubtexture((SizeX - 1) * SliceWidth, SliceHeight, SliceWidth, SliceHeight);
                Slices[6].Enter(t5);
                Slices[9].Enter(t5);
            }
            else if (SizeY > 2)
            {
                // t1 t2 t3
                // t4 t5 t6
                // t7 t8 t9

                MTexture t1 = t0;
                Slices[1].Enter(t1);

                for (int tileX = 1; tileX < SizeX - 1; tileX++)
                {
                    MTexture t2 = Parent.GetSubtexture(tileX * SliceWidth, 0, SliceWidth, SliceHeight);
                    Slices[2].Enter(t2);
                }

                MTexture t3 = Parent.GetSubtexture((SizeX - 1) * SliceWidth, 0, SliceWidth, SliceHeight);
                Slices[3].Enter(t3);

                for (int tileY = 1; tileY < SizeY - 1; tileY++)
                {
                    MTexture t4 = Parent.GetSubtexture(0, tileY * SliceHeight, SliceWidth, SliceHeight);
                    Slices[4].Enter(t4);
                }

                for (int tileX = 1; tileX < SizeX - 1; tileX++)
                {
                    for (int tileY = 1; tileY < SizeY - 1; tileY++)
                    {
                        MTexture t5 = Parent.GetSubtexture(tileX * SliceWidth, tileY * SliceHeight, SliceWidth, SliceHeight);
                        Slices[5].Enter(t5);
                    }
                }

                for (int tileY = 1; tileY < SizeY - 1; tileY++)
                {
                    MTexture t6 = Parent.GetSubtexture((SizeX - 1) * SliceWidth, tileY * SliceHeight, SliceWidth, SliceHeight);
                    Slices[6].Enter(t6);
                }

                MTexture t7 = Parent.GetSubtexture(0, (SizeY - 1) * SliceHeight, SliceWidth, SliceHeight);
                Slices[7].Enter(t7);

                for (int tileX = 1; tileX < SizeX - 1; tileX++)
                {
                    MTexture t8 = Parent.GetSubtexture(tileX * SliceWidth, (SizeY - 1) * SliceHeight, SliceWidth, SliceHeight);
                    Slices[8].Enter(t8);
                }

                MTexture t9 = Parent.GetSubtexture((SizeX - 1) * SliceWidth, (SizeY - 1) * SliceHeight, SliceWidth, SliceHeight);
                Slices[9].Enter(t9);
            }
        }
    }

    public void RenderSlicing(float x, float y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        int sw = SliceWidth;
        int sh = SliceHeight;

        // 根据实际宽度和高度计算每个部分的实际尺寸
        int leftWidth = Math.Min(sw, width / 3);
        int rightWidth = Math.Min(sw, width - leftWidth);
        int topHeight = Math.Min(sh, height / 3);
        int bottomHeight = Math.Min(sh, height - topHeight);

        Vector2[] positions = new Vector2[9];
        positions[0] = new Vector2(x, y); // 左上
        positions[1] = new Vector2(x + leftWidth, y); // 上中
        positions[2] = new Vector2(x + width - rightWidth, y); // 右上
        positions[3] = new Vector2(x, y + topHeight); // 中左
        positions[4] = new Vector2(x + leftWidth, y + topHeight); // 中心
        positions[5] = new Vector2(x + width - rightWidth, y + topHeight); // 中右
        positions[6] = new Vector2(x, y + height - bottomHeight); // 左下
        positions[7] = new Vector2(x + leftWidth, y + height - bottomHeight); // 下中
        positions[8] = new Vector2(x + width - rightWidth, y + height - bottomHeight); // 右下

        DrawSlice(1, new Rectangle((int)positions[0].X, (int)positions[0].Y, leftWidth, topHeight));
        DrawSlice(3, new Rectangle((int)positions[2].X, (int)positions[2].Y, rightWidth, topHeight));
        DrawSlice(7, new Rectangle((int)positions[6].X, (int)positions[6].Y, leftWidth, bottomHeight));
        DrawSlice(9, new Rectangle((int)positions[8].X, (int)positions[8].Y, rightWidth, bottomHeight));

        DrawPseudoRepeatedCenterSlice(positions[1], width - leftWidth - rightWidth, topHeight, 2); // 上中 -> Slices[2]
        DrawPseudoRepeatedCenterSlice(positions[3], leftWidth, height - topHeight - bottomHeight, 4); // 中左 -> Slices[4]
        DrawPseudoRepeatedCenterSlice(positions[4], width - leftWidth - rightWidth, height - topHeight - bottomHeight, 5); // 中心 -> Slices[5]
        DrawPseudoRepeatedCenterSlice(positions[5], rightWidth, height - topHeight - bottomHeight, 6); // 中右 -> Slices[6]
        DrawPseudoRepeatedCenterSlice(positions[7], width - leftWidth - rightWidth, bottomHeight, 8); // 下中 -> Slices[8]
    }

    // half check

    private Random random = new Random();
    private void DrawSlice(int sliceIndex, Rectangle rect)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;

        // 从指定切片区域中随机选择一个贴图
        List<MTexture> textures = Slices[sliceIndex - 1];
        if (textures == null || textures.Count == 0) return;

        MTexture sourceTex = textures[random.Next(textures.Count)];

        MTexture subTex = sourceTex.GetSubtexture(0, 0, rect.Width, rect.Height);
        subTex.Draw(new Vector2(rect.X, rect.Y));
    }

    private void DrawPseudoRepeatedCenterSlice(Vector2 position, int width, int height, int sliceIndex)
    {
        if (width <= 0 || height <= 0) return;

        List<MTexture> textures = Slices[sliceIndex - 1];
        if (textures == null || textures.Count == 0) return;

        for (float i = 0; i < width; i += SliceWidth)
        {
            for (float j = 0; j < height; j += SliceHeight)
            {
                int w = (int)Math.Min(SliceWidth, width - i);
                int h = (int)Math.Min(SliceHeight, height - j);

                MTexture selectedTex = textures[random.Next(textures.Count)];
                MTexture subTex = selectedTex.GetSubtexture(0, 0, w, h);
                subTex.Draw(new Vector2(position.X + i, position.Y + j));
            }
        }
    }
}

public static class SlicingUtils
{
    /// <summary>
    /// Generate a list of images by slicing the original image column by column
    /// unitX and unitY are the size of each slice
    /// sizeX and sizeY are the count of slices for the original asset
    /// </summary>
    /// <param name="source"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="unitX"></param>
    /// <param name="unitY"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <returns></returns>
    public static List<Image> BuildSprite(this MTexture source, float width, float height, int unitX, int unitY, int sizeX, int sizeY)
    {
        List<Image> list = new List<Image>();
        int num = source.Width / unitX;
        int num2 = source.Height / unitY;
        for (int i = 0; (float)i < width; i += sizeX)
        {
            for (int j = 0; (float)j < height; j += sizeY)
            {
                int num3 = ((i != 0) ? ((!((float)i >= width - unitX)) ? Calc.Random.Next(1, num - 1) : (num - 1)) : 0);
                int num4 = ((j != 0) ? ((!((float)j >= height - unitY)) ? Calc.Random.Next(1, num2 - 1) : (num2 - 1)) : 0);
                Image image = new Image(source.GetSubtexture(num3 * unitX, num4 * unitY, unitX, unitY));
                image.Position = new Vector2(i, j);
                list.Add(image);
            }
        }

        return list;
    }
}