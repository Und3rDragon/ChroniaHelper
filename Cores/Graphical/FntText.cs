using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace ChroniaHelper.Cores.Graphical;

public class FntText
{
    public Dictionary<int, MTexture> textures = new();
    public Vc2 position = Vc2.Zero;
    public Vc2 segmentOrigin = Vc2.One * 0.5f;
    public Vc2 origin = Vc2.One * 0.5f;
    public enum RenderMode { Compact = 0, EqualDistance = 1 }
    public int renderMode = 0;
    public float distance = 4f;
    public CColor color = new CColor(Color.White, 1f);
    public float scale = 1f;
    public float rotation = 0f;
    public Vc2 overallOffset = Vc2.Zero;
    public Dictionary<int, Vc2> segmentOffset = new();
    public bool flipX = false;
    public bool flipY = false;
    public float depth = 0f;
    public SpriteEffects GetSpriteEffect()
    {
        SpriteEffects result = SpriteEffects.None;
        if (flipX) result |= SpriteEffects.FlipHorizontally;
        if (flipY) result |= SpriteEffects.FlipVertically;
        return result;
    }

    public FntText(string fntPath)
    {
        textures = fntPath.CreateFntFontTextures();
    }

    public Vc2 p1, p2;
    public List<Vc2> segmentPosition;
    public Vc2 overallSize = Vc2.Zero;
    public Vc2 segmentStart = Vc2.Zero;
    public void Measure<T>(IList<T> source, Func<T, int> selector)
    {
        p1 = Vc2.Zero; p2 = Vc2.Zero;
        segmentPosition = new();
        overallSize = Vc2.Zero;

        Vc2 cal = Vc2.Zero;

        for (int i = 0; i < source.Count; i++)
        {

            MTexture asset = textures[selector(source[i])];

            if (i == 0)
            {
                p1 = new Vc2(-asset.Width, -asset.Height) * segmentOrigin * scale;
                p2 = new Vc2(asset.Width, asset.Height) * (Vc2.One - segmentOrigin) * scale;
                segmentPosition.Add(cal);

                continue;
            }

            MTexture lastAsset = textures[selector(source[i - 1])];

            if (renderMode == (int)RenderMode.EqualDistance)
            {
                cal.X = cal.X + distance;
            }
            else
            {
                cal.X = cal.X + lastAsset.Width * (1 - segmentOrigin.X) * scale + asset.Width * segmentOrigin.X * scale + distance;
            }

            Vc2 _p1 = cal + new Vc2(-asset.Width, -asset.Height) * segmentOrigin * scale;
            Vc2 _p2 = cal + new Vc2(asset.Width, asset.Height) * (Vc2.One - segmentOrigin) * scale;

            segmentPosition.Add(cal);

            p1.X = _p1.X < p1.X ? _p1.X : p1.X;
            p1.Y = _p1.Y < p1.Y ? _p1.Y : p1.Y;
            p2.X = _p2.X > p2.X ? _p2.X : p2.X;
            p2.Y = _p2.Y > p2.Y ? _p2.Y : p2.Y;
        }

        overallSize = p2 - p1;
        segmentStart = -p1;
    }

    public void Measure(string source)
    {
        Measure(source.ToArray(), (c) => (int)c);
    }

    public void Render<T>(IList<T> source, Func<T, int> selector)
    {
        Render(source, selector, position);
    }
    /// <param name="renderPosition">
    /// If the class using it is standalone, the position should be the world position
    /// If it's an entity using it, it should be the entity Position
    /// </param>
    public void Render<T>(IList<T> source, Func<T, int> selector, Vc2 renderPosition)
    {
        Measure(source, selector);

        Vc2 shift = -overallSize * origin;

        //Draw.HollowRect(renderPosition + shift, overallSize.X, overallSize.Y, Color.Orange);

        for (int i = 0; i < source.Count; i++)
        {
            MTexture texture = textures[selector(source[i])];
            Vc2 dPos = shift + segmentStart + segmentPosition[i];

            bool hasSegOffset = segmentOffset.TryGetValue(i, out Vc2 segOffset);

            texture.Draw(renderPosition + dPos + overallOffset - segmentOrigin * new Vc2(texture.Width, texture.Height) + (hasSegOffset ? segOffset : Vc2.Zero),
                Vc2.Zero, color.Parsed(), scale, rotation.ToRad(), GetSpriteEffect());
            //Draw.SpriteBatch.Draw(texture.Texture.Texture, renderPosition + dPos + overallOffset + (hasSegOffset ? segOffset : Vc2.Zero),
            //    null, color.Parsed(), rotation.ToRad(), segmentOrigin * new Vc2(texture.Width, texture.Height),
            //    scale, GetSpriteEffect(), depth);
        }
    }

    public void Render(string source, Func<char, int> selector, Vc2 worldPosition)
    {
        Render(source.ToArray(), selector, worldPosition);
    }
}

public static class FntUtils
{
    /// <summary>
    /// 从指定路径的 BMFont .fnt 文件读取字符信息，并关联对应的 MTexture。
    /// </summary>
    /// <param name="fntPath">相对于 GFX.Game 数据目录的 .fnt 文件路径。</param>
    /// <returns>一个字典，键是字符 ID (int)，值是对应的 MTexture。</returns>
    public static Dictionary<int, MTexture> CreateFntFontTextures(this string fntPath)
    {
        var result = new Dictionary<int, MTexture>();

        try
        {
            // 1. 构建完整的 .fnt 文件路径
            string fullPath = Path.Combine(Path.GetFullPath(GFX.Game.DataPath), fntPath);

            if (!File.Exists(fullPath))
            {
                Log.Warn($"FNT file not found: {fullPath}");
                return result; // 或者抛出异常
            }

            // 2. 加载并解析 XML
            XDocument doc = XDocument.Load(fullPath);
            XElement root = doc.Root;

            if (root?.Name != "font")
            {
                Log.Warn($"Invalid FNT file format or root element is not 'font': {fullPath}");
                return result;
            }

            // 3. 获取第一页的纹理文件名 (假设只有一个页面)
            // 注意：BMFont 可以有多页，这里简化处理，只取第一页。
            string textureAtlasPath = null;
            var firstPageElement = root?.Element("pages")?.Elements("page")?.FirstOrDefault();
            if (firstPageElement != null)
            {
                textureAtlasPath = firstPageElement.Attribute("file")?.Value;
            }

            if (string.IsNullOrEmpty(textureAtlasPath))
            {
                Log.Warn($"Could not find texture atlas path in FNT file: {fullPath}");
                return result;
            }

            // 4. 从 GFX.Game 获取基础纹理 Atlas (MTexture)
            // 假设 textureAtlasPath 是相对于 GFX.Game 数据目录的相对路径
            // 并且该图集已经被加载到 GFX.Game 中。
            // 通常 BMFont 工具生成的 PNG 会直接放在 Content/Graphics/ 下，
            // 所以 textureAtlasPath 可能就是 "Graphics/your_font_texture.png"
            // 你需要确保它能通过 GFX.Game[path] 访问到。
            MTexture baseTextureAtlas = null;
            try
            {
                baseTextureAtlas = GFX.Game[textureAtlasPath]; // 尝试获取基础图集
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to load base texture atlas '{textureAtlasPath}' from GFX.Game: {ex.Message}");
                // 可能需要更健壮的错误处理，比如检查文件是否存在等
                return result;
            }

            if (baseTextureAtlas == null || baseTextureAtlas.Texture == null)
            {
                Log.Warn($"Base texture atlas '{textureAtlasPath}' is null or invalid.");
                return result;
            }


            // 5. 遍历字符信息
            var charsElement = root.Element("chars");
            if (charsElement != null)
            {
                foreach (var charElement in charsElement.Elements("char"))
                {
                    // 尝试获取必要的属性
                    if (int.TryParse(charElement.Attribute("id")?.Value, out int id) &&
                        int.TryParse(charElement.Attribute("x")?.Value, out int x) &&
                        int.TryParse(charElement.Attribute("y")?.Value, out int y) &&
                        int.TryParse(charElement.Attribute("width")?.Value, out int width) &&
                        int.TryParse(charElement.Attribute("height")?.Value, out int height))
                    {
                        // 可选：获取 xOffset, yOffset, xAdvance 等用于更精确的渲染
                        // int xOffset = int.TryParse(charElement.Attribute("xoffset")?.Value, out int tmpXOff) ? tmpXOff : 0;
                        // int yOffset = int.TryParse(charElement.Attribute("yoffset")?.Value, out int tmpYOff) ? tmpYOff : 0;
                        // int xAdvance = int.TryParse(charElement.Attribute("xadvance")?.Value, out int tmpXAdv) ? tmpXAdv : width;

                        // 6. 创建子纹理 (MTexture)
                        // MTexture 的构造函数通常接受父 MTexture (atlas) 和裁剪矩形
                        // Rectangle rect = new Rectangle(x, y, width, height);
                        // MTexture charTexture = new MTexture(baseTextureAtlas, rect);

                        // 或者使用 MTexture.GetSubtexture 如果可用并且适用
                        // MTexture charTexture = baseTextureAtlas.GetSubtexture(x, y, width, height);

                        // 最稳妥的方式是使用 MTexture 的裁剪构造函数
                        MTexture charTexture;
                        try
                        {
                            // 确保坐标和尺寸在基础纹理范围内
                            if (x >= 0 && y >= 0 && x + width <= baseTextureAtlas.Width && y + height <= baseTextureAtlas.Height)
                            {
                                charTexture = new MTexture(baseTextureAtlas, new Rectangle(x, y, width, height));
                                result[id] = charTexture;
                            }
                            else
                            {
                                Log.Warn($"Character ID {id} has invalid bounds (x:{x}, y:{y}, w:{width}, h:{height}) for atlas size ({baseTextureAtlas.Width}, {baseTextureAtlas.Height}). Skipping.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn($"Failed to create MTexture for character ID {id}: {ex.Message}");
                        }

                    }
                    else
                    {
                        // 记录警告：无法解析某个字符条目
                        string charIdStr = charElement.Attribute("id")?.Value ?? "unknown";
                        Log.Warn($"Skipping character entry with invalid data, ID: {charIdStr}");
                    }
                }
            }
            else
            {
                Log.Warn($"No <chars> section found in FNT file: {fullPath}");
            }


        }
        catch (Exception ex)
        {
            // 记录任何未处理的异常
            Log.Error($"Error loading FNT font textures from '{fntPath}': {ex}");
            // 可以选择返回空字典或重新抛出异常
            // return result; // 返回已加载的部分
            // 或者
            // throw; // 重新抛出异常
        }

        return result;
    }
}