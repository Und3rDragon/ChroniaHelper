using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Cores;

public class SerialImage
{
    public List<MTexture> textures = new();
    public Vc2 position = Vc2.Zero;
    public Vc2 segmentOrigin = Vc2.One * 0.5f;
    public Vc2 origin = Vc2.One * 0.5f;
    public enum RenderMode { Compact = 0, EqualDistance = 1}
    public int renderMode = 0;
    public float distance = 4f;
    public CColor color = new CColor(Color.White, 1f);
    public float scale = 1f;
    public float rotation = 0f;
    public Vc2 overallOffset = Vc2.Zero;
    public Dictionary<int, Vc2> segmentOffset = new();
    public bool flipX = false;
    public bool flipY = false;
    public SpriteEffects GetSpriteEffect()
    {
        SpriteEffects result = SpriteEffects.None;
        if (flipX) result |= SpriteEffects.FlipHorizontally;
        if (flipY) result |= SpriteEffects.FlipVertically;
        return result;
    }

    public SerialImage(string path)
    {
        GFX.Game.GetAtlasSubtextures(path).ApplyTo(out textures);
    }
    public SerialImage(List<MTexture> source)
    {
        source.ApplyTo(out textures);
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
        
        for(int i = 0; i < source.Count; i++)
        {

            MTexture asset = textures[selector(source[i])];
            
            if (i == 0)
            {
                p1 = new Vector2(-asset.Width, -asset.Height) * segmentOrigin * scale;
                p2 = new Vector2(asset.Width, asset.Height) * (Vc2.One - segmentOrigin) * scale;
                segmentPosition.Add(cal);
                
                continue;
            }
            
            MTexture lastAsset = textures[selector(source[i - 1])];

            if(renderMode == (int)RenderMode.EqualDistance)
            {
                cal.X = cal.X + distance;
            }
            else
            {
                cal.X = cal.X + lastAsset.Width * (1 - segmentOrigin.X) * scale + asset.Width * segmentOrigin.X * scale + distance;
            }
            
            Vc2 _p1 = cal + new Vector2(-asset.Width, -asset.Height) * segmentOrigin * scale;
            Vc2 _p2 = cal + new Vector2(asset.Width, asset.Height) * (Vc2.One - segmentOrigin) * scale;

            segmentPosition.Add(cal);

            p1.X = _p1.X < p1.X ? _p1.X : p1.X;
            p1.Y = _p1.Y < p1.Y ? _p1.Y : p1.Y;
            p2.X = _p2.X > p2.X ? _p2.X : p2.X;
            p2.Y = _p2.Y > p2.Y ? _p2.Y : p2.Y;
        }
        
        overallSize = p2 - p1;
        segmentStart = - p1;
    }

    public void Measure(string source, Func<char, int> selector)
    {
        Measure(source.ToArray(), selector);
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
        
        for(int i = 0; i < source.Count; i++)
        {
            MTexture texture = textures[selector(source[i])];
            Vc2 dPos = shift + segmentStart + segmentPosition[i];

            bool hasSegOffset = segmentOffset.TryGetValue(i, out Vc2 segOffset);

            //Log.Info($"Source: {source[i]} => {selector(source[i])}");
            //Log.Info($"Position: {position}");
            //Log.Info($"origin: {origin}");
            //Log.Info($"segmentOrigin: {segmentOrigin}");
            //Log.Info($"renderMode: {renderMode}");
            //Log.Info($"distance: {distance}");
            //Log.Info($"texturePath: {texture.AtlasPath}");
            //Log.Error("_____________________________");

            texture.Draw(renderPosition + dPos + overallOffset + (hasSegOffset? segOffset : Vc2.Zero), 
                segmentOrigin * new Vc2(texture.Width, texture.Height), color.Parsed(), scale, rotation.ToRad(),
                GetSpriteEffect());
            //Draw.HollowRect(renderPosition + dPos + overallOffset + (hasSegOffset ? segOffset : Vc2.Zero) - segmentOrigin * new Vc2(texture.Width, texture.Height),
            //    texture.Width, texture.Height, Color.Red);
        }
    }
    
    public void Render(string source, Func<char, int> selector, Vc2 worldPosition)
    {
        Render(source.ToArray(), selector, worldPosition);
    }
}
