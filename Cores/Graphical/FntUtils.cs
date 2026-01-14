using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores.Graphical;


public static class FntUtils
{
    /// <summary>
    /// 从指定虚拟路径的 BMFont .fnt 文件读取字符信息、偏移量，并关联对应的 MTexture。
    /// 支持多页纹理图集。
    /// </summary>
    /// <param name="virtualFntPathWithoutExtension">
    /// 相对于 Mod 内容根目录的 .fnt 文件虚拟路径 (不带 .fnt 扩展名)。
    /// 例如: "Graphics/Atlases/Gameplay/ChroniaHelper/MinecraftFont/chinese"
    /// </param>
    /// <param name="textures">
    /// 输出参数：一个字典，键是字符 ID (int)，值是该字符对应的 MTexture。
    /// </param>
    /// <param name="offsets">
    /// 输出参数：一个字典，键是字符 ID (int)，值是该字符的渲染偏移量 (Vector2)。
    /// Vector2.X 对应 xoffset, Vector2.Y 对应 yoffset。
    /// </param>
    public static void CreateFntFontTextures(
        this string virtualFntPathWithoutExtension,
        out Dictionary<int, MTexture> textures,
        out Dictionary<int, Vector2> offsets)
    {
        // 初始化输出参数
        textures = new Dictionary<int, MTexture>();
        offsets = new Dictionary<int, Vector2>();

        try
        {
            // 1. 构造 .fnt 文件的虚拟路径 (加上 .fnt 扩展名用于查找)
            string virtualFntPathWithExt = virtualFntPathWithoutExtension + ".fnt";

            // 2. 使用 Everest.Content 获取 .fnt 文件对应的 ModAsset
            ModAsset fntAsset = null;
            bool assetFound = Everest.Content.TryGet("Graphics/Atlases/Gameplay/" + virtualFntPathWithExt, out fntAsset, false);

            if (!assetFound || fntAsset == null)
            {
                Log.Warn($"[ZIP-SAFE] Could not find FNT asset for virtual path: '{virtualFntPathWithExt}' using Everest.Content.TryGet.");
                return; // 直接返回，输出字典为空
            }

            Log.Info($"[ZIP-SAFE] Found FNT asset for virtual path: {virtualFntPathWithExt}");

            XDocument doc = null;

            // 3. 直接从 ModAsset.Data 获取字节，然后解析为 XDocument
            try
            {
                byte[] fntData = fntAsset.Data;
                if (fntData == null || fntData.Length == 0)
                {
                    Log.Warn($"[ZIP-SAFE] FNT asset data is null or empty for: '{virtualFntPathWithExt}'");
                    return;
                }

                string fntXmlString = Encoding.UTF8.GetString(fntData);
                doc = XDocument.Parse(fntXmlString);
                Log.Info($"[ZIP-SAFE] Successfully parsed FNT XML data from asset: {virtualFntPathWithExt}");
            }
            catch (Exception parseEx)
            {
                Log.Warn($"[ZIP-SAFE] Failed to parse FNT data from asset '{virtualFntPathWithExt}': {parseEx.Message}");
                return;
            }

            // 4. 验证根元素
            XElement root = doc?.Root;
            if (root?.Name != "font")
            {
                Log.Warn($"[ZIP-SAFE] Invalid FNT file format or root element is not 'font' for: '{virtualFntPathWithExt}'");
                return;
            }

            // 5. 获取 .fnt 文件所在的虚拟目录 (用于定位纹理)
            // 例如: virtualFntPathWithoutExtension = "Graphics/Atlases/Gameplay/ChroniaHelper/MinecraftFont/chinese"
            //      fntDirectoryVirtualPath = "Graphics/Atlases/Gameplay/ChroniaHelper/MinecraftFont"
            string fntDirectoryVirtualPath = Path.GetDirectoryName(virtualFntPathWithoutExtension)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(fntDirectoryVirtualPath))
            {
                // 如果 .fnt 在根目录?
                fntDirectoryVirtualPath = "";
            }

            // 6. 加载所有页面纹理 (支持 Multi-Page Atlases)
            var pagesElement = root.Element("pages");
            if (pagesElement == null)
            {
                Log.Warn($"[ZIP-SAFE] No <pages> section found in FNT file: '{virtualFntPathWithExt}'");
                return;
            }

            // 字典缓存: key = page id, value = 对应的 MTexture 图集
            var pageTextures = new Dictionary<int, MTexture>();

            foreach (var pageElement in pagesElement.Elements("page"))
            {
                if (int.TryParse(pageElement.Attribute("id")?.Value, out int pageId))
                {
                    string textureRelativeFileName = pageElement.Attribute("file")?.Value;

                    if (!string.IsNullOrEmpty(textureRelativeFileName))
                    {
                        // 构造纹理在 GFX.Game 中的虚拟路径
                        // 例如: fntDirectoryVirtualPath = "Graphics/Atlases/Gameplay/ChroniaHelper/MinecraftFont"
                        //      textureRelativeFileName = "chinese_233.png"
                        //      textureVirtualPath = "Graphics/Atlases/Gameplay/ChroniaHelper/MinecraftFont/chinese_233"
                        string textureFilenameNoExt = Path.GetFileNameWithoutExtension(textureRelativeFileName);
                        string textureVirtualPath;
                        if (!string.IsNullOrEmpty(fntDirectoryVirtualPath))
                        {
                            textureVirtualPath = $"{fntDirectoryVirtualPath}/{textureFilenameNoExt}";
                        }
                        else
                        {
                            textureVirtualPath = textureFilenameNoExt;
                        }

                        //Log.Info($"[ZIP-SAFE] Attempting to load texture atlas for page {pageId} from GFX.Game using virtual path: {textureVirtualPath}");

                        try
                        {
                            MTexture pageTextureAtlas = GFX.Game[textureVirtualPath];

                            if (pageTextureAtlas != null && pageTextureAtlas.Texture != null)
                            {
                                pageTextures[pageId] = pageTextureAtlas;
                                //Log.Info($"[ZIP-SAFE] Successfully loaded texture atlas for page {pageId}.");
                            }
                            else
                            {
                                Log.Warn($"[ZIP-SAFE] Base texture atlas '{textureVirtualPath}' for page {pageId} is null or invalid in GFX.Game.");
                                // 可以选择在这里返回错误，或者记录警告并跳过该页的字符
                                // 这里选择记录警告，让后续找不到纹理的字符处理失败
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn($"[ZIP-SAFE] Failed to load base texture atlas '{textureVirtualPath}' for page {pageId} from GFX.Game: {ex.Message}");
                        }
                    }
                    else
                    {
                        Log.Warn($"[ZIP-SAFE] Page {pageId} entry has no 'file' attribute in FNT file: '{virtualFntPathWithExt}'");
                    }
                }
                else
                {
                    string pageIdStr = pageElement.Attribute("id")?.Value ?? "unknown";
                    Log.Warn($"[ZIP-SAFE] Skipping page entry with invalid 'id': {pageIdStr} in FNT file: '{virtualFntPathWithExt}'");
                }
            }

            if (pageTextures.Count == 0)
            {
                Log.Warn($"[ZIP-SAFE] No valid page textures were loaded from FNT file: '{virtualFntPathWithExt}'. Aborting character processing.");
                return; // 没有可用纹理，无法创建字符纹理
            }

            // 7. 遍历字符信息
            var charsElement = root.Element("chars");
            if (charsElement != null)
            {
                foreach (var charElement in charsElement.Elements("char"))
                {
                    // 尝试解析所有必需属性
                    if (int.TryParse(charElement.Attribute("id")?.Value, out int id) &&
                        int.TryParse(charElement.Attribute("x")?.Value, out int x) &&
                        int.TryParse(charElement.Attribute("y")?.Value, out int y) &&
                        int.TryParse(charElement.Attribute("width")?.Value, out int width) &&
                        int.TryParse(charElement.Attribute("height")?.Value, out int height) &&
                        int.TryParse(charElement.Attribute("page")?.Value, out int pageId)) // 新增: 获取 page id
                    {
                        // 尝试解析可选的 offset 属性
                        int.TryParse(charElement.Attribute("xoffset")?.Value ?? "0", out int xoffset);
                        int.TryParse(charElement.Attribute("yoffset")?.Value ?? "0", out int yoffset);

                        // 检查是否有所需页面的纹理
                        if (!pageTextures.TryGetValue(pageId, out MTexture baseTextureAtlas))
                        {
                            Log.Warn($"[ZIP-SAFE] Character ID {id} references unknown or unloaded page {pageId}. Skipping.");
                            continue; // 跳过此字符
                        }

                        try
                        {
                            // 添加严格的边界检查
                            if (x >= 0 && y >= 0 && width > 0 && height > 0 &&
                                x + width <= baseTextureAtlas.Width && y + height <= baseTextureAtlas.Height)
                            {
                                // 创建字符纹理
                                MTexture charTexture = new MTexture(baseTextureAtlas, new Rectangle(x, y, width, height));

                                // 存储纹理和偏移量
                                textures[id] = charTexture;
                                offsets[id] = new Vector2(xoffset, yoffset); // 存储 offset

                                // Log.Verbose($"[ZIP-SAFE] Created MTexture for char ID {id} from page {pageId}");
                            }
                            else
                            {
                                Log.Warn($"[ZIP-SAFE] Character ID {id} (page {pageId}) has invalid/out-of-bounds dimensions/position (x:{x}, y:{y}, w:{width}, h:{height}) for atlas size ({baseTextureAtlas.Width}, {baseTextureAtlas.Height}). Skipping.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn($"[ZIP-SAFE] Failed to create MTexture for character ID {id} (page {pageId}): {ex.Message}");
                        }
                    }
                    else
                    {
                        string charIdStr = charElement.Attribute("id")?.Value ?? "unknown";
                        string pageIdStr = charElement.Attribute("page")?.Value ?? "unknown";
                        Log.Warn($"[ZIP-SAFE] Skipping character entry with invalid data, ID: {charIdStr}, Page: {pageIdStr}");
                    }
                }
            }
            else
            {
                Log.Warn($"[ZIP-SAFE] No <chars> section found in FNT file: '{virtualFntPathWithExt}'");
            }

            Log.Info($"[ZIP-SAFE] Finished processing FNT file '{virtualFntPathWithExt}'. Loaded {textures.Count} characters.");
        }
        catch (Exception ex)
        {
            Log.Error($"[ZIP-SAFE] Unhandled error in CreateFntFontTextures for virtual path '{virtualFntPathWithoutExtension}': {ex}");
            // 确保即使发生未处理异常，输出参数也是初始化状态（空字典）
            textures = new Dictionary<int, MTexture>();
            offsets = new Dictionary<int, Vector2>();
        }
    }
}
