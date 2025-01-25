using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class BoxSprite : GraphicsComponent
{
    public class Animation
    {
        public float Delay;
        public MTexture[][,] SubtextureFrames;
        public Chooser<string> Goto;
    }

    public Action<string, string> OnAnimationChange;
    public Action<string> OnFrameChange;
    public Action<string> OnLastFrame;
    public Action<string> OnLoop;
    public Action<string> OnFinish;

    public float Rate = 1f;
    public string SpritePath;

    public bool IsAnimating { get; private set; }
    public string LastAnimationID { get; private set; }
    public string CurrentAnimationID { get; private set; }
    public int CurrentAnimationFrame { get; private set; }

    private Atlas atlas;
    private Dictionary<string, Animation> animations;
    private Animation currentAnimation;
    private MTexture[,] currentSubtexture;
    private float animationTimer;

    public BoxSprite(Atlas atlas, string spritePath) : base(active: true)
    {
        this.atlas = atlas;
        SpritePath = spritePath;
        animations = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
        CurrentAnimationID = "";
    }

    public override void Update()
    {
        if (!IsAnimating)
            return;

        animationTimer += Engine.DeltaTime * Rate;
        if (!(Math.Abs(animationTimer) >= currentAnimation.Delay))
            return;

        int direction = Math.Sign(animationTimer);
        int frameCount = currentAnimation.SubtextureFrames.Length;
        CurrentAnimationFrame += direction;
        animationTimer -= direction * currentAnimation.Delay;

        if (CurrentAnimationFrame < 0 ||  CurrentAnimationFrame >= frameCount)
        {
            OnLastFrame?.Invoke(CurrentAnimationID);
            
            if (currentAnimation.Goto != null)
            {
                LastAnimationID = CurrentAnimationID;
                CurrentAnimationID = currentAnimation.Goto.Choose();
                currentAnimation = animations[CurrentAnimationID];
                CurrentAnimationFrame = CurrentAnimationFrame < 0 ? currentAnimation.SubtextureFrames.Length - 1 : 0;

                if (LastAnimationID != CurrentAnimationID)
                    OnAnimationChange?.Invoke(LastAnimationID, CurrentAnimationID);

                if (currentAnimation.Goto != null && currentAnimation.Goto.Choices[0].Value == CurrentAnimationID)
                    OnLoop?.Invoke(CurrentAnimationID);
            }
            else
            {
                CurrentAnimationFrame = CurrentAnimationFrame < 0 ? 0 : frameCount - 1;

                OnFinish?.Invoke(CurrentAnimationID);

                IsAnimating = false;
                CurrentAnimationID = "";
                currentAnimation = null;
                animationTimer = 0f;
                return;
            }
        }
        SetFrame(currentAnimation.SubtextureFrames[CurrentAnimationFrame]);
    }

    public override void Render()
    {
        if (currentSubtexture == null) 
            return;

        float entityWidth = Entity.Width;
        float entityHeight = Entity.Height;

        int tilesX = (int)(entityWidth / 8f);
        int tilesY = (int)(entityHeight / 8f);

        float widthLimit = tilesX - 1f;
        float heightLimit = tilesY - 1f;

        for (int i = 0; i < tilesX; i++)
        {
            for (int j = 0; j < tilesY; j++)
            {
                int num1 = (i == 0) ? 0 : (i == widthLimit ? 2 : 1);
                int num2 = (j == 0) ? 0 : (j == heightLimit ? 2 : 1);

                if (num1 != 1 || num2 != 1)
                    currentSubtexture[num1, num2].Draw(RenderPosition + new Vector2(X + i * 8f, Y + j * 8f)); 
            }
        }
    }

    public override void Removed(Entity entity)
    {
        animations.Clear();
        animations = null;
    }

    /*
    internal void PrintAnimationsDebugInfo()
    {
        foreach (var item in animations)
        {
            Console.WriteLine($"AnimationID: {item.Key}");
            Console.WriteLine($"Frames: {item.Value.SubtextureFrames.Length}");
            Console.WriteLine($"Delay: {item.Value.Delay}");
            for (int i = 0; i < item.Value.SubtextureFrames.Length; i++)
            {
                Console.WriteLine($"Frame: {i}  SubtexturePath: {item.Value.SubtextureFrames[i][0, 0].Parent.AtlasPath}");
            }
        }
    }
    */

    public void Play(string id, bool restart = false, bool randomizeFrame = false)
    {
        if (CurrentAnimationID == id && !restart)
            return;

        OnAnimationChange?.Invoke(LastAnimationID, id);
        LastAnimationID = CurrentAnimationID;
        CurrentAnimationID = id;
        currentAnimation = animations[id];
        IsAnimating = currentAnimation.Delay > 0;

        if (randomizeFrame)
        {
            animationTimer = Calc.Random.NextFloat(currentAnimation.Delay);
            CurrentAnimationFrame = Calc.Random.Next(currentAnimation.SubtextureFrames.Length);
        }
        else
        {
            animationTimer = 0f;
            CurrentAnimationFrame = 0;
        }

        SetFrame(currentAnimation.SubtextureFrames[CurrentAnimationFrame]);
    }

    public void Reverse(string id, bool restart = false)
    {
        if (Rate > 0f)
            Rate *= -1f;

        Play(id, restart);
    }

    public void Stop()
    {
        IsAnimating = false;
        currentAnimation = null;
        CurrentAnimationID = "";
    }

    public bool Has(string id)
    {
        if (id != null)
            return animations.ContainsKey(id);
        return false;
    }

    public void SetAnimationFrame(int frame)
    {
        animationTimer = 0f;
        CurrentAnimationFrame = frame % currentAnimation.SubtextureFrames.Length;
        SetFrame(currentAnimation.SubtextureFrames[CurrentAnimationFrame]);
    }

    public void AddAnimation(string id, string path, float delay)
    {
        animations[id] = new Animation
        {
            Delay = delay,
            SubtextureFrames = GetFrames(path),
            Goto = null
        };
    }

    public void AddAnimation(string id, string path, float delay, string into)
    {
        animations[id] = new Animation
        {
            Delay = delay,
            SubtextureFrames = GetFrames(path),
            Goto = Chooser<string>.FromString<string>(into)
        };
    }

    public void AddAnimation(string id, string path, float delay, params int[] frames)
    {
        animations[id] = new Animation
        {
            Delay = delay,
            SubtextureFrames = GetFrames(path, frames),
            Goto = null
        };
    }

    public void AddAnimation(string id, string path, float delay, string into, params int[] frames)
    {
        animations[id] = new Animation
        {
            Delay = delay,
            SubtextureFrames = GetFrames(path, frames),
            Goto = Chooser<string>.FromString<string>(into)
        };
    }

    public void AddAnimationLoop(string id, string path, float delay)
    {
        animations[id] = new Animation
        {
            Delay = delay,
            SubtextureFrames = GetFrames(path),
            Goto = Chooser<string>.FromString<string>(id)
        };
    }

    public void AddAnimationLoop(string id, string path, float delay, params int[] frames)
    {
        animations[id] = new Animation
        {
            Delay = delay,
            SubtextureFrames = GetFrames(path, frames),
            Goto = Chooser<string>.FromString<string>(id)
        };
    }

    public void RemoveAnimation(string id)
    {
        animations.Remove(id);
    }

    private MTexture[][,] GetFrames(string path, int[] frames = null)
    {
        string fullPath = SpritePath + path;

        MTexture[] textures = (frames == null || frames.Length == 0)
        ? GFX.Game.GetAtlasSubtextures(fullPath).ToArray()
        : frames.Select(frame => atlas.GetAtlasSubtexturesAt(fullPath, frame) ??
            throw new Exception($"Can't find sprite {fullPath} with index {frame}")).ToArray();

        return textures.Select(texture =>
        {
            if (texture.Width != 24 || texture.Height != 24)
                throw new Exception($"Sprite {fullPath} should be 24*24 px!");

            var subtexture = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    subtexture[i, j] = texture.GetSubtexture(i * 8, j * 8, 8, 8);
                }
            }
            return subtexture;
        }).ToArray();
    }

    private void SetFrame(MTexture[,] subtexture)
    {
        if (currentSubtexture != subtexture)
        {
            currentSubtexture = subtexture;

            OnFrameChange?.Invoke(CurrentAnimationID);
        }
    }
}
