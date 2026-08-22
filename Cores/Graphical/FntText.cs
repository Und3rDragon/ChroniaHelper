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
    /// <summary>
    /// 固定 char→int 转换器引用，保证 string 路径 Measure 缓存可命中
    /// </summary>
    private static readonly Func<char, int> CharCodeSelector = (c) => (int)c;

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
    public Dictionary<int, Vc2> offsetPerIndex = new();
    public Dictionary<int, Vc2> offsetPerCharCode = new();
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
        if (!Md.Session.cachedFntData.TryGetValue(fntPath, out FntData cached))
        {
            cached = new FntData(fntPath);
            Md.Session.cachedFntData[fntPath] = cached;
        }
        textures = cached.textures;
        segmentOffset = cached.offsets;
    }

    public Vc2 p1, p2;
    public List<Vc2> segmentPosition;
    public Vc2 overallSize = Vc2.Zero;
    public Vc2 segmentStart = Vc2.Zero;

    // ---- Measure 结果缓存：字符串路径（string 不可变，== 即内容比较，绝对可靠） ----
    private string cachedStringSource;
    private object cachedSelector;
    private float cachedScale;
    private int cachedRenderMode;
    private float cachedDistance;
    private Vc2 cachedSegmentOrigin;
    private int cachedCount = -1;
    private MTexture[] measuredTextures;
    private int[] measuredSelectors;

    private bool MeasureCacheValid(string source, object selector)
    {
        return cachedStringSource == source
            && ReferenceEquals(cachedSelector, selector)
            && cachedCount == source.Length
            && cachedScale == scale
            && cachedRenderMode == renderMode
            && cachedDistance == distance
            && cachedSegmentOrigin == segmentOrigin;
    }

    private void StoreMeasureCache(string source, object selector)
    {
        cachedStringSource = source;
        cachedSelector = selector;
        cachedCount = source.Length;
        cachedScale = scale;
        cachedRenderMode = renderMode;
        cachedDistance = distance;
        cachedSegmentOrigin = segmentOrigin;
    }

    public void Measure(string source)
    {
        Measure(source, CharCodeSelector);
    }

    public void Measure<T>(IList<T> source, Func<T, int> selector)
    {
        p1 = Vc2.Zero; p2 = Vc2.Zero;
        segmentPosition = new List<Vc2>(source.Count);
        if (measuredTextures == null || measuredTextures.Length < source.Count)
        {
            measuredTextures = new MTexture[source.Count];
            measuredSelectors = new int[source.Count];
        }
        overallSize = Vc2.Zero;

        Vc2 cal = Vc2.Zero;

        for (int i = 0; i < source.Count; i++)
        {
            int idx = selector(source[i]);
            measuredSelectors[i] = idx;
            MTexture asset = textures[idx];
            measuredTextures[i] = asset;

            if (i == 0)
            {
                p1 = new Vc2(-asset.Width, -asset.Height) * segmentOrigin * scale;
                p2 = new Vc2(asset.Width, asset.Height) * (Vc2.One - segmentOrigin) * scale;
                segmentPosition.Add(cal);

                continue;
            }

            MTexture lastAsset = measuredTextures[i - 1];

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

    public void Measure(string source, Func<char, int> selector)
    {
        if (MeasureCacheValid(source, selector))
        {
            return;
        }

        StoreMeasureCache(source, selector);

        p1 = Vc2.Zero; p2 = Vc2.Zero;
        segmentPosition = new List<Vc2>(source.Length);
        if (measuredTextures == null || measuredTextures.Length < source.Length)
        {
            measuredTextures = new MTexture[source.Length];
            measuredSelectors = new int[source.Length];
        }
        overallSize = Vc2.Zero;

        Vc2 cal = Vc2.Zero;

        for (int i = 0; i < source.Length; i++)
        {
            int idx = selector(source[i]);
            measuredSelectors[i] = idx;
            MTexture asset = textures[idx];
            measuredTextures[i] = asset;

            if (i == 0)
            {
                p1 = new Vc2(-asset.Width, -asset.Height) * segmentOrigin * scale;
                p2 = new Vc2(asset.Width, asset.Height) * (Vc2.One - segmentOrigin) * scale;
                segmentPosition.Add(cal);

                continue;
            }

            MTexture lastAsset = measuredTextures[i - 1];

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

        Color parsedColor = color.Parsed();
        float rad = rotation.ToRad();
        SpriteEffects fx = GetSpriteEffect();

        for (int i = 0; i < source.Count; i++)
        {
            MTexture texture = measuredTextures[i];
            Vc2 dPos = shift + segmentStart + segmentPosition[i];

            bool hasSegOffset = segmentOffset.TryGetValue(i, out Vc2 segOffset);
            bool hasIndexOffset = offsetPerIndex.TryGetValue(i, out Vc2 indexOffset);
            bool hasCharcodeOffset = offsetPerCharCode.TryGetValue(measuredSelectors[i], out Vc2 charcodeOffset);

            texture.Draw(renderPosition + dPos + overallOffset
                - scale * segmentOrigin * new Vc2(texture.Width, texture.Height) + (hasSegOffset ? segOffset : Vc2.Zero)
                + (hasIndexOffset ? indexOffset : Vc2.Zero) + (hasCharcodeOffset ? charcodeOffset : Vc2.Zero),
                Vc2.Zero, parsedColor, scale, rad, fx);
            //Draw.SpriteBatch.Draw(texture.Texture.Texture, renderPosition + dPos + overallOffset + (hasSegOffset ? segOffset : Vc2.Zero),
            //    null, color.Parsed(), rotation.ToRad(), segmentOrigin * new Vc2(texture.Width, texture.Height),
            //    scale, GetSpriteEffect(), depth);
        }
    }

    public void Render(string source, Vc2 worldPosition)
    {
        Render(source, CharCodeSelector, worldPosition);
    }

    public void Render(string source, Func<char, int> selector, Vc2 worldPosition)
    {
        Measure(source, selector);

        Vc2 shift = -overallSize * origin;

        //Draw.HollowRect(renderPosition + shift, overallSize.X, overallSize.Y, Color.Orange);

        Color parsedColor = color.Parsed();
        float rad = rotation.ToRad();
        SpriteEffects fx = GetSpriteEffect();

        for (int i = 0; i < source.Length; i++)
        {
            MTexture texture = measuredTextures[i];
            Vc2 dPos = shift + segmentStart + segmentPosition[i];

            bool hasSegOffset = segmentOffset.TryGetValue(i, out Vc2 segOffset);
            bool hasIndexOffset = offsetPerIndex.TryGetValue(i, out Vc2 indexOffset);
            bool hasCharcodeOffset = offsetPerCharCode.TryGetValue(measuredSelectors[i], out Vc2 charcodeOffset);

            texture.Draw(worldPosition + dPos + overallOffset
                - scale * segmentOrigin * new Vc2(texture.Width, texture.Height) + (hasSegOffset ? segOffset : Vc2.Zero)
                + (hasIndexOffset ? indexOffset : Vc2.Zero) + (hasCharcodeOffset ? charcodeOffset : Vc2.Zero),
                Vc2.Zero, parsedColor, scale, rad, fx);
        }
    }
}
