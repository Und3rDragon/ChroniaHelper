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
    public float interval = 0.1f;
    public Vc2 origin = Vc2.Zero;
    public CColor color = new(Color.White);
    public float scale = 1f;
    public float rotation = 0f;
    public Dictionary<string,bool> loop = new();
    public bool reversed = false;
    public Dictionary<string, List<int>> frameSet = new();
    public bool playing = true;
    public AnimatedImage() { }
    public AnimatedImage(string id, List<MTexture> textures)
    {
        this.textures.Enter(id, textures);
    }
    
    public void Render(Vc2 worldPosition)
    {
        if (!textures.ContainsKey(currentAnimation)) { return; }
        if (textures[currentAnimation].Count == 0 || textures[currentAnimation].IsNull()) { return; }

        MTexture asset = textures[currentAnimation][currentFrame.ClampWhole(0, textures[currentAnimation].Count - 1)];
        Vc2 dPos = - new Vc2(asset.Width, asset.Height) * origin;
        asset.Draw(worldPosition + dPos, Vc2.Zero, color.Parsed(), scale, rotation.ToRad());
    }

    private int frameSetIndex = 0;
    public void Update()
    {
        if (!playing) { return; }

        if (!textures.ContainsKey(currentAnimation)) { return; }

        if (MaP.scene?.OnInterval(interval.ClampMin(Engine.DeltaTime))?? false)
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
        currentAnimation = animationID;
        
        playing = true;
    }

    public void Stop()
    {
        playing = false;
    }
}
