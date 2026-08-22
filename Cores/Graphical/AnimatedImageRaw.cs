using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace ChroniaHelper.Cores.Graphical;

/// <summary>
/// Alternate AnimatedImage that renders by Draw.SpriteBatch.Draw(), compatible for HD Renders
/// </summary>
public class AnimatedImageRaw
{
    public Dictionary<string, List<MTexture>> textures = new();
    public string currentAnimation = "";
    public int currentFrame = 0;
    public Dictionary<string, float> interval = new();
    public Vc2 position = Vc2.Zero;
    public Vc2 origin = Vc2.Zero;
    public Vc2 offset = Vc2.Zero;
    public CColor color = new(Color.White);
    public float scale = 1f;
    public float rotation = 0f;
    public Dictionary<string,bool> loop = new();
    public bool reversed = false;
    public Dictionary<string, List<int>> frameSet = new();
    public bool playing = false;
    public bool flipX = false;
    public bool flipY = false;
    public float depth = 0;
    public SpriteEffects GetSpriteEffect()
    {
        SpriteEffects result = SpriteEffects.None;
        if (flipX) result |= SpriteEffects.FlipHorizontally;
        if (flipY) result |= SpriteEffects.FlipVertically;
        return result;
    }
    
    public AnimatedImageRaw() { }
    public AnimatedImageRaw(string id, List<MTexture> textures)
    {
        this.textures.Enter(id, textures);
    }
    
    public void Render()
    {
        Render(position);
    }
    
    /// <param name="renderPosition">
    /// If the class using it is standalone, the position should be the world position
    /// If it's an entity using it, it should be the entity Position
    /// </param>
    public void Render(Vc2 renderPosition)
    {
        if (!textures.TryGetValue(currentAnimation, out List<MTexture> frames)) { return; }
        if (frames == null || frames.Count == 0) { return; }

        MTexture asset = frames[currentFrame.Clamp(0, frames.Count - 1)];

        Color parsedColor = color.Parsed();
        float rad = rotation.ToRad();
        SpriteEffects fx = GetSpriteEffect();

        //asset.Draw(renderPosition + offset, origin, color.Parsed(), scale, rotation.ToRad(), GetSpriteEffect());
        Draw.SpriteBatch.Draw(asset.Texture.Texture, renderPosition + offset, null, parsedColor, rad,
            origin * new Vc2(asset.Width, asset.Height),
             scale, fx, depth);
    }

    private int frameSetIndex = 0;
    public void Update()
    {
        if (!playing) { return; }

        if (!textures.TryGetValue(currentAnimation, out List<MTexture> frames)) { return; }

        float dt = interval.TryGetValue(currentAnimation, out float iv) ? iv.ClampMin(Engine.DeltaTime) : 0.1f;

        if (MaP.scene?.OnInterval(dt) ?? false)
        {
            if (!frameSet.TryGetValue(currentAnimation, out List<int> frameSetList))
            {
                currentFrame += reversed ? -1 : 1;

                bool loopAnim = loop.GetValueOrDefault(currentAnimation, true);
                if (currentFrame < 0) { currentFrame = loopAnim ? frames.Count - 1 : 0; }
                if (currentFrame > frames.Count - 1) { currentFrame = loopAnim ? 0 : frames.Count - 1; }
            }
            else
            {
                frameSetIndex += reversed ? -1 : 1;

                bool loopAnim = loop.GetValueOrDefault(currentAnimation, true);
                if (frameSetIndex < 0) { frameSetIndex = loopAnim ? frameSetList.Count - 1 : 0; }
                if (frameSetIndex > frameSetList.Count - 1) { frameSetIndex = loopAnim ? 0 : frameSetList.Count - 1; }

                currentFrame = frameSetList[frameSetIndex];
            }
        }
    }
    
    public void ResetAnimation()
    {
        currentFrame = 0;
        frameSetIndex = 0;
    }
    
    public void Play()
    {
        playing = true;
    }

    public void Play(string animationID)
    {
        ResetAnimation();
        
        currentAnimation = animationID;
        
        playing = true;
    }
    
    public void Switch(string animationID)
    {
        ResetAnimation();

        currentAnimation = animationID;

        playing = false;
    }

    public void Stop()
    {
        playing = false;
    }

    public int CurrentAnimationLength()
    {
        if (!textures.TryGetValue(currentAnimation, out List<MTexture> frames)) { return 0; }
        if (frames == null) { return 0; }

        return frames.Count;
    }
}
