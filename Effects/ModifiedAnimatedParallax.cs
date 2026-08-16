using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using MonoMod.Cil;

namespace ChroniaHelper.Effects;

// The source code is modified from Maddie of Maddie's Helping Hand
public class ModifiedAnimatedParallax : Parallax
{
    [LoadHook]
    public static void Load()
    {
        IL.Celeste.MapData.ParseBackdrop += onParseBackdrop;
    }
    [UnloadHook]
    public static void Unload()
    {
        IL.Celeste.MapData.ParseBackdrop -= onParseBackdrop;
    }

    private static void onParseBackdrop(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj(typeof(Parallax))))
        {
            Logger.Log("ChroniaHelper Modification: MaxHelpingHand/AnimatedParallax", $"Handling animated parallaxes at {cursor.Index} in IL for MapData.ParseBackdrop");

            cursor.EmitDelegate<Func<Parallax, Parallax>>(orig => {
                // This part is for avoiding conflict with Maddie's Anim Parallax
                // But the paths are now differentiated
                if (orig.Texture?.AtlasPath?.StartsWith("bgs/ChroniaHelper/modifiedParallax/") ?? false)
                {
                    // nah, this is an ANIMATED parallax, mind you!
                    return new ModifiedAnimatedParallax(orig.Texture);
                }
                return orig;
            });
        }
    }

    private class ParallaxMeta
    {
        public float? FPS { get; set; } = null;
        public string Frames { get; set; } = null;
        public string TriggerFlag { get; set; } = null;
        public bool? PlayOnce { get; set; } = null;
        public string ResetFlag { get; set; } = null;
        public int? ResetFrame { get; set; } = null;
        public string SpeedSlider { get; set; } = null;
        public string AlphaExpression { get; set; } = null;
        public List<string> FrameIndexFlag { get; set; } = new();
        public List<string> TextureIndexFlag { get; set; } = new();
        public string OverrideFrameCounter { get; set; } = null;
        public string OverrideTextureCounter { get; set; } = null;
        public string OverridePositionX { get; set; } = null;
        public string OverridePositionY { get; set; } = null;
        public string OverrideSpeedX { get; set; } = null;
        public string OverrideSpeedY { get; set; } = null;
        public string DeltaPositionX { get; set; } = null;
        public string DeltaPositionY { get; set; } = null;
        public string OverrideScrollX { get; set; } = null;
        public string OverrideScrollY { get; set; } = null;
        public string OverrideScale { get; set; } = null;
    }
    private string alphaExpression = null;
    private string triggerFlag, resetFlag;
    private string speedSlider = null;
    private bool playOnce = false;
    private int resetFrame = 0;

    private List<MTexture> frames;
    private int[] frameOrder;
    private float fps, orig_fps, last_fps = 12f;

    private int currentFrame;
    private float currentFrameTimer, orig_currentFrameTimer;

    private List<string> frameIndexFlag = new(), textureIndexFlag = new();

    private string overrideFrameCounter = null;
    private string overrideTextureCounter = null;
    private string overridePositionX = null;
    private string overridePositionY = null;
    private string overrideSpeedX = null;
    private string overrideSpeedY = null;
    private string deltaPosX = null, deltaPosY = null;
    private string overrideScrollX = null, overrideScrollY = null;
    private string overrideScale = null;

    //public ModifiedAnimatedParallax(BinaryPacker.Element c, MTexture texture) : this(texture)
    //{

    //}
    public ModifiedAnimatedParallax(MTexture texture) : base(texture)
    {
        // remove the frame number, much like decals do.
        string texturePath = Regex.Replace(texture.AtlasPath, "\\d+$", string.Empty);

        // then load all frames from that prefix.
        frames = GFX.Game.GetAtlasSubtextures(texturePath);

        // by default, the frames are just in order and last the same duration.
        frameOrder = new int[frames.Count];
        for (int i = 0; i < frameOrder.Length; i++)
        {
            frameOrder[i] = i;
        }

        Match fpsCount = Regex.Match(texturePath, "[^0-9]((?:[0-9]+\\.)?[0-9]+)fps$");
        if (fpsCount.Success)
        {
            // we found an FPS count! use it.
            orig_fps = fps = float.Parse(fpsCount.Groups[1].Value);
        }
        else
        {
            // use 12 FPS by default, like decals.
            orig_fps = fps = 12f;
        }

        if (Everest.Content.Map.TryGetValue("Graphics/Atlases/Gameplay/" + texturePath + ".meta", out ModAsset metaYaml) && metaYaml.Type == typeof(AssetTypeYaml))
        {
            // the styleground has a metadata file! we should read it.
            ParallaxMeta meta;
            using (TextReader r = new StreamReader(metaYaml.Stream))
            {
                meta = YamlHelper.Deserializer.Deserialize<ParallaxMeta>(r);
            }

            if (meta.FPS != null)
            {
                orig_fps = fps = meta.FPS.Value;
            }

            if (meta.Frames != null)
            {
                frameOrder = Calc.ReadCSVIntWithTricks(meta.Frames);
            }
            
            if(meta.TriggerFlag != null)
            {
                triggerFlag = meta.TriggerFlag;
            }
            
            if(meta.PlayOnce != null)
            {
                playOnce = meta.PlayOnce.Value;
            }
            
            if(meta.ResetFlag != null)
            {
                resetFlag = meta.ResetFlag;
            }
            
            if(meta.ResetFrame != null)
            {
                resetFrame = meta.ResetFrame.Value;
            }
            
            if(meta.SpeedSlider != null)
            {
                speedSlider = meta.SpeedSlider;
            }
            
            if(meta.AlphaExpression != null)
            {
                alphaExpression = meta.AlphaExpression;
            }

            frameIndexFlag = meta.FrameIndexFlag;
            textureIndexFlag = meta.TextureIndexFlag;

            if(meta.OverrideFrameCounter != null)
            {
                overrideFrameCounter = meta.OverrideFrameCounter;
            }

            if(meta.OverrideTextureCounter != null)
            {
                overrideTextureCounter = meta.OverrideTextureCounter;
            }

            if(meta.OverridePositionX != null)
            {
                overridePositionX = meta.OverridePositionX;
            }

            if(meta.OverridePositionY != null)
            {
                overridePositionY = meta.OverridePositionY;
            }

            if(meta.OverrideSpeedX != null)
            {
                overrideSpeedX = meta.OverrideSpeedX;
            }

            if(meta.OverrideSpeedY != null)
            {
                overrideSpeedY = meta.OverrideSpeedY;
            }

            if(meta.DeltaPositionX != null)
            {
                deltaPosX = meta.DeltaPositionX;
            }

            if(meta.DeltaPositionY != null)
            {
                deltaPosY = meta.DeltaPositionY;
            }
            
            if(meta.OverrideScrollX != null)
            {
                overrideScrollX = meta.OverrideScrollX;
            }

            if (meta.OverrideScrollY != null)
            {
                overrideScrollY = meta.OverrideScrollY;
            }

            if(meta.OverrideScale != null)
            {
                overrideScale = meta.OverrideScale;
            }
        }
        
        AnalyzeIndexFlags();
        
        Texture = frames[frameOrder[0]];
        currentFrame = 0;
        orig_currentFrameTimer = currentFrameTimer = 1f / fps;
    }

    private Dictionary<int, List<string>> frameIndexFlags = new(),
        textureIndexFlags = new();
    private void AnalyzeIndexFlags()
    {
        foreach (var item in frameIndexFlag)
        {
            string[] pair = item.Split(';', StringSplitOptions.TrimEntries);
            if (pair.Length < 2)
            {
                continue;
            }

            string[] indexes = pair[0].Split(',', StringSplitOptions.TrimEntries);
            string[] flags = pair[1].Split(',', StringSplitOptions.TrimEntries);
            foreach (var index in indexes)
            {
                if (int.TryParse(index, out int n))
                {
                    frameIndexFlags.Create(n, new());
                    foreach (var flag in flags)
                    {
                        frameIndexFlags[n].Create(flag);
                    }
                }
            }
        }
        
        foreach (var item in textureIndexFlag)
        {
            string[] pair = item.Split(';', StringSplitOptions.TrimEntries);
            if (pair.Length < 2)
            {
                continue;
            }

            string[] indexes = pair[0].Split(',', StringSplitOptions.TrimEntries);
            string[] flags = pair[1].Split(',', StringSplitOptions.TrimEntries);
            foreach (var index in indexes)
            {
                if (int.TryParse(index, out int n))
                {
                    textureIndexFlags.Create(n, new());
                    foreach (var flag in flags)
                    {
                        textureIndexFlags[n].Create(flag);
                    }
                }
            }
        }
    }

    public Vc2? originalPosition = null;
    public override void Update(Scene scene)
    {
        if(originalPosition != null)
        {
            Position = (Vc2)originalPosition;
        }
        
        base.Update(scene);

        // override speed and position first
        if (overridePositionX.HasValidContent())
        {
            Position.X = overridePositionX.GetSlider();
        }
        if (overridePositionY.HasValidContent())
        {
            Position.Y = overridePositionY.GetSlider();
        }
        if (overrideSpeedX.HasValidContent())
        {
            Speed.X = overrideSpeedX.GetSlider();
        }
        if (overrideSpeedY.HasValidContent())
        {
            Speed.Y = overrideSpeedY.GetSlider();
        }

        if (overrideScrollX.HasValidContent())
        {
            Scroll.X = overrideScrollX.GetSlider();
        }
        if (overrideScrollY.HasValidContent())
        {
            Scroll.Y = overrideScrollY.GetSlider();
        }

        // bugfix: setting new position
        originalPosition = Position;

        if (deltaPosX.HasValidContent())
        {
            Position.X += deltaPosX.GetSlider();
        }
        if (deltaPosY.HasValidContent())
        {
            Position.Y += deltaPosY.GetSlider();
        }

        if (!resetFlag.IsNullOrEmpty())
        {
            if (resetFlag.GetFlag())
            {
                currentFrame = resetFrame >= 0 ? resetFrame : frameOrder.Length + resetFrame; // For calculation priority
                resetFlag.SetFlag(false);
            }
        }
        
        if (alphaExpression != null)
        {
            Alpha = alphaExpression.ParseMathExpression();
        }
        
        if (!IsVisible(scene as Level))
        {
            return;
        }

        float? scale = null;

        if(overrideScale.HasValidContent())
        {
            if(float.TryParse(overrideScale, out float f))
            {
                scale = f;
            }
            else
            {
                scale = overrideScale.GetSlider();
            }
        }

        // If frame or index is overrided by counter
        if(overrideFrameCounter.HasValidContent())
        {
            int n = overrideFrameCounter.GetCounter();
            n %= frameOrder.Length;
            Texture = frames[frameOrder[n]];
            if(scale != null)
            {
                Texture.ScaleFix = (float)scale;
            }

            return;
        }
        if (overrideTextureCounter.HasValidContent())
        {
            int n = overrideTextureCounter.GetCounter();
            n %= frames.Count;
            Texture = frames[n];
            if (scale != null)
            {
                Texture.ScaleFix = (float)scale;
            }

            return;
        }

        if (speedSlider != null)
        {
            float multiplier = (speedSlider.GetSlider() + 1f).ClampMin(0f);
            fps = orig_fps * multiplier;
            if (fps != last_fps)
            {
                currentFrameTimer *= fps / last_fps;
            }
        }
        last_fps = fps;
        
        currentFrameTimer -= Engine.DeltaTime;

        if (currentFrameTimer < 0f)
        {
            while (currentFrameTimer < 0f)
            {
                currentFrameTimer += (1f / fps).Clamp(Engine.DeltaTime, 2592000f);
            }
            
            currentFrame = currentFrame.ClampMin(0); // For frame index protection
            currentFrame %= frameOrder.Length;
            Texture = frames[frameOrder[currentFrame]];
            if (scale != null)
            {
                Texture.ScaleFix = (float)scale;
            }

            if (frameIndexFlags.ContainsKey(currentFrame))
            {
                foreach (var indexFlag in frameIndexFlags[currentFrame])
                {
                    indexFlag.SetFlag(true);
                }
            }
            
            if (textureIndexFlags.ContainsKey(frameOrder[currentFrame]))
            {
                foreach (var indexFlag in textureIndexFlags[frameOrder[currentFrame]])
                {
                    indexFlag.SetFlag(true);
                }
            }

            if (!triggerFlag.IsNullOrEmpty())
            {
                if (!triggerFlag.GetFlag())
                {
                    return;
                }
            }
            
            if (!playOnce || currentFrame != frameOrder.Length - 1)
            {
                currentFrame++;
            }
        }
    }
}
