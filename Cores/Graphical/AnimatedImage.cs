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
/// An image class that renders by MTexture.Draw()
/// </summary>
public class AnimatedImage
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
    
    public AnimatedImage() { }
    public AnimatedImage(string id, List<MTexture> textures)
    {
        this.textures.Enter(id, textures);
    }

    // ---- 当前动画数据缓存：动画名与字典内容未变化时不再重复查表 ----
    private string resolvedAnimation;
    private List<MTexture> resolvedFrames;
    private List<int> resolvedFrameSet;
    private float resolvedInterval = 0.1f;
    private int resolvedTextureCount = -1;
    private int resolvedIntervalCount = -1;
    private int resolvedFrameSetCount = -1;

    /// <summary>
    /// Re-reads the frames, interval and frame set of <see cref="currentAnimation"/> from the dictionaries.
    /// Call it after editing those dictionaries while the animation name stays the same.
    /// </summary>
    public void Refresh()
    {
        resolvedAnimation = currentAnimation;
        resolvedTextureCount = textures.Count;
        resolvedIntervalCount = interval.Count;
        resolvedFrameSetCount = frameSet.Count;

        textures.TryGetValue(currentAnimation, out resolvedFrames);
        resolvedInterval = interval.TryGetValue(currentAnimation, out float iv) ? iv : 0.1f;
        frameSet.TryGetValue(currentAnimation, out resolvedFrameSet);
    }

    private void EnsureResolved()
    {
        if (resolvedAnimation != currentAnimation
            || resolvedTextureCount != textures.Count
            || resolvedIntervalCount != interval.Count
            || resolvedFrameSetCount != frameSet.Count)
        {
            Refresh();
        }
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
        EnsureResolved();

        List<MTexture> frames = resolvedFrames;
        if (frames == null || frames.Count == 0) { return; }

        MTexture asset = frames[currentFrame.Clamp(0, frames.Count - 1)];

        Color parsedColor = color.Parsed();
        float rad = rotation.ToRad();
        SpriteEffects fx = GetSpriteEffect();

        asset.Draw(renderPosition + offset - origin * new Vc2(asset.Width, asset.Height),
            Vc2.Zero, parsedColor, scale, rad, fx);
    }

    private int frameSetIndex = 0;
    public void Update()
    {
        if (!playing) { return; }

        EnsureResolved();

        List<MTexture> frames = resolvedFrames;
        if (frames == null) { return; }

        float dt = resolvedInterval.ClampMin(Engine.DeltaTime);

        if (MaP.scene?.OnInterval(dt) ?? false)
        {
            if (resolvedFrameSet == null)
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
                if (frameSetIndex < 0) { frameSetIndex = loopAnim ? resolvedFrameSet.Count - 1 : 0; }
                if (frameSetIndex > resolvedFrameSet.Count - 1) { frameSetIndex = loopAnim ? 0 : resolvedFrameSet.Count - 1; }

                currentFrame = resolvedFrameSet[frameSetIndex];
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
        EnsureResolved();

        if (resolvedFrames == null) { return 0; }

        return resolvedFrames.Count;
    }
}
