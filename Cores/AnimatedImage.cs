using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace ChroniaHelper.Cores;

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
        if (!textures.ContainsKey(currentAnimation)) { return; }
        if (textures[currentAnimation].Count == 0 || textures[currentAnimation].IsNull()) { return; }

        MTexture asset = textures[currentAnimation][currentFrame.Clamp(0, textures[currentAnimation].Count - 1)];
        
        asset.Draw(renderPosition + offset, origin * new Vc2(asset.Width, asset.Height), 
            color.Parsed(), scale, rotation.ToRad(), GetSpriteEffect());
    }

    private int frameSetIndex = 0;
    public void Update()
    {
        if (!playing) { return; }

        if (!textures.ContainsKey(currentAnimation)) { return; }

        float dt = interval.ContainsKey(currentAnimation) ? interval[currentAnimation].ClampMin(Engine.DeltaTime) : 0.1f;

        if (MaP.scene?.OnInterval(dt)?? false)
        {
            if (!frameSet.ContainsKey(currentAnimation))
            {
                currentFrame += reversed ? -1 : 1;
                
                if (currentFrame < 0) { currentFrame = loop.SafeGet(currentAnimation, true)? 
                        textures[currentAnimation].Count - 1 : 0; }
                if (currentFrame > textures[currentAnimation].Count - 1) { currentFrame = loop.SafeGet(currentAnimation, true) ? 
                        0 : textures[currentAnimation].Count - 1; }
            }
            else
            {
                frameSetIndex += reversed ? -1 : 1;

                if (frameSetIndex < 0) { frameSetIndex = loop.SafeGet(currentAnimation, true) ? 
                        frameSet[currentAnimation].Count - 1 : 0; }
                if (frameSetIndex > frameSet[currentAnimation].Count - 1) { frameSetIndex = loop.SafeGet(currentAnimation, true) ? 
                        0 : frameSet[currentAnimation].Count - 1; }

                currentFrame = frameSet[currentAnimation][frameSetIndex];
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

    public void Stop()
    {
        playing = false;
    }

    public int CurrentAnimationLength()
    {
        if (!textures.ContainsKey(currentAnimation)) { return 0; }
        if (textures[currentAnimation].IsNull()) { return 0; }

        return textures[currentAnimation].Count;
    }
}
