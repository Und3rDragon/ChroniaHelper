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
        }
        
        Texture = frames[frameOrder[0]];
        currentFrame = 0;
        orig_currentFrameTimer = currentFrameTimer = 1f / fps;
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);

        if (!resetFlag.IsNullOrEmpty())
        {
            if (resetFlag.GetFlag())
            {
                currentFrame = resetFrame >= 0 ? resetFrame : frameOrder.Length + resetFrame; // For calculation priority
                resetFlag.SetFlag(false);
            }
        }
        
        if (IsVisible(scene as Level))
        {
            if (alphaExpression != null)
            {
                Alpha = alphaExpression.ParseMathExpression();
            }
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
