using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Cores.Graphical;

/// <summary>
/// Alternate SerialImage that renders by Draw.SpriteBatch.Draw(), compatible for HD Renders
/// </summary>
public class SerialImageRaw
{
    public List<MTexture> textures = new();
    public Vc2 position = Vc2.Zero;
    public Vc2 segmentOrigin = Vc2.One * 0.5f;
    public Vc2 origin = Vc2.One * 0.5f;
    public enum RenderMode { Compact = 0, EqualDistance = 1}
    /// <summary>
    /// Compact = 0, EqualDistance = 1
    /// </summary>
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

    public SerialImageRaw(string path)
    {
        GFX.Game.GetAtlasSubtextures(path).ApplyTo(out textures);
    }
    public SerialImageRaw(List<MTexture> source)
    {
        source.ApplyTo(out textures);
    }

    public Vc2 p1, p2;
    public List<Vc2> segmentPosition = new();
    public Vc2 overallSize = Vc2.Zero;
    public Vc2 segmentStart = Vc2.Zero;

    // ---- 测量结果缓存：内容与布局参数未变化时复用上一次的测量数据 ----
    private string cachedStringSource;
    private object cachedSelector;
    private float cachedScale;
    private int cachedRenderMode;
    private float cachedDistance;
    private Vc2 cachedSegmentOrigin;
    private int cachedCount = -1;

    private MTexture[] measuredTextures;
    private Vc2[] measuredDrawOffsets;
    private Vc2[] measuredOrigins;
    private int measuredLength;

    private static bool SameSelector(object a, object b)
    {
        if (ReferenceEquals(a, b)) { return true; }
        if (a is null || b is null) { return false; }

        // 委托按「目标实例 + 方法」比较，调用方每次传入等价的新委托时缓存依然有效
        return a.Equals(b);
    }

    private bool MeasureCacheValid(string source, object selector)
    {
        return cachedStringSource == source
            && SameSelector(cachedSelector, selector)
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

    private void PrepareBuffers(int length)
    {
        if (measuredTextures == null || measuredTextures.Length < length)
        {
            measuredTextures = new MTexture[length];
            measuredDrawOffsets = new Vc2[length];
            measuredOrigins = new Vc2[length];
        }

        if (segmentPosition == null)
        {
            segmentPosition = new List<Vc2>(length);
        }
        else if (segmentPosition.Capacity < length)
        {
            segmentPosition.Capacity = length;
        }

        segmentPosition.Clear();
    }

    private void FinishMeasure(int length)
    {
        measuredLength = length;
        overallSize = p2 - p1;
        segmentStart = -p1;

        // 每段的绘制偏移与旋转轴心在此算好，渲染循环中不再重复计算
        for (int i = 0; i < length; i++)
        {
            MTexture asset = measuredTextures[i];
            measuredDrawOffsets[i] = segmentStart + segmentPosition[i];
            measuredOrigins[i] = segmentOrigin * new Vc2(asset.Width, asset.Height);
        }
    }

    public void Measure(string source, Func<char, int> selector)
    {
        if (MeasureCacheValid(source, selector))
        {
            return;
        }

        StoreMeasureCache(source, selector);

        p1 = Vc2.Zero; p2 = Vc2.Zero;
        PrepareBuffers(source.Length);
        overallSize = Vc2.Zero;

        Vc2 cal = Vc2.Zero;

        for (int i = 0; i < source.Length; i++)
        {
            int idx = selector(source[i]);
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

        FinishMeasure(source.Length);
    }

    public void Measure<T>(IList<T> source, Func<T, int> selector)
    {
        p1 = Vc2.Zero; p2 = Vc2.Zero;
        PrepareBuffers(source.Count);
        overallSize = Vc2.Zero;

        Vc2 cal = Vc2.Zero;

        for (int i = 0; i < source.Count; i++)
        {
            int idx = selector(source[i]);
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

        FinishMeasure(source.Count);
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

        DrawSegments(renderPosition, source.Count);
    }

    public void Render(string source, Func<char, int> selector, Vc2 worldPosition)
    {
        Measure(source, selector);

        DrawSegments(worldPosition, source.Length);
    }

    /// <summary>
    /// Draws the current measured data. It is used by the container classes which
    /// have just measured this instance, so no measuring is repeated here.
    /// </summary>
    internal void DrawMeasured(Vc2 renderPosition)
    {
        DrawSegments(renderPosition, measuredLength);
    }

    private void DrawSegments(Vc2 renderPosition, int count)
    {
        Vc2 anchor = renderPosition - overallSize * origin + overallOffset;

        Color parsedColor = color.Parsed();
        float rad = rotation.ToRad();
        SpriteEffects fx = GetSpriteEffect();

        // 未设置分段偏移时完全跳过字典查找
        bool hasSegmentOffsets = segmentOffset.Count > 0;

        for (int i = 0; i < count; i++)
        {
            MTexture texture = measuredTextures[i];

            Vc2 segOffset = Vc2.Zero;
            if (hasSegmentOffsets)
            {
                segmentOffset.TryGetValue(i, out segOffset);
            }

            Draw.SpriteBatch.Draw(texture.Texture.Texture, anchor + measuredDrawOffsets[i] + segOffset,
                null, parsedColor, rad, measuredOrigins[i],
                scale, fx, depth);
        }
    }
}
