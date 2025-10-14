using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Celeste.Mod.Backdrops;
using Celeste.Mod.MaxHelpingHand.Effects;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using MonoMod.Cil;
using VivHelper.Entities;

namespace ChroniaHelper.Effects;

// The source code is modified from Maddie of Maddie's Helping Hand
public class ModifiedAnimatedParallax : Parallax
{
    public static void Load()
    {
        IL.Celeste.MapData.ParseBackdrop += onParseBackdrop;
    }

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
        public string SpeedFlags { get; set; } = null;
    }
    private string triggerFlag, resetFlag, speedFlags;
    private bool playOnce = false;
    private int resetFrame = 0;

    private List<MTexture> frames;
    private int[] frameOrder;
    private float fps, orig_fps;

    private int currentFrame;
    private float currentFrameTimer;

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
            
            if(meta.SpeedFlags != null)
            {
                speedFlags = meta.SpeedFlags;
            }
        }

        Texture = frames[frameOrder[0]];
        currentFrame = 0;
        currentFrameTimer = 1f / fps;

        // For speed up flags, we set up a flag system for it
        // Normally the speed-up flags should look like this in game:
        // "parallaxSpeed_flagName"
        // And look like this when setting up:
        // "flagName,1;flagName,2;flagName,3"
        if (!speedFlags.IsNullOrEmpty())
        {
            var _multipliers = speedFlags.ParseSquaredString();
            for(int i = 0; i < _multipliers.GetLength(0); i++)
            {
                if (_multipliers[i].Length < 2) { continue; }

                multipliers.Enter(_multipliers[i][0], _multipliers[i][1].ParseFloat(1f));
            }
        }
    }
    private Dictionary<string, float> multipliers = new();

    public override void Update(Scene scene)
    {
        base.Update(scene);

        if (IsVisible(scene as Level))
        {
            if (!resetFlag.IsNullOrEmpty())
            {
                if (resetFlag.GetFlag())
                {
                    currentFrame = resetFrame - 1; // For calculation priority
                    resetFlag.SetFlag(false);
                }
            }
            if (!triggerFlag.IsNullOrEmpty())
            {
                if (!triggerFlag.GetFlag())
                {
                    return;
                }
            }
            if(multipliers.Count > 0)
            {
                foreach(var entry in multipliers)
                {
                    if ($"parallaxSpeed_{entry.Key}".GetFlag())
                    {
                        fps = orig_fps * multipliers[entry.Key];
                        currentFrameTimer *= multipliers[entry.Key];
                        currentFrameTimer.ClampWhole(Engine.DeltaTime, 2592000f); // Preventing overflow
                        $"parallaxSpeed_{entry.Key}".SetFlag(false);
                    }
                }
            }
            currentFrameTimer -= Engine.DeltaTime;
            if (currentFrameTimer < 0f)
            {
                currentFrameTimer += (1f / fps);
                if (!playOnce || currentFrame != frameOrder.Length - 1)
                {
                    currentFrame++;
                }
                currentFrame = currentFrame < 0 ? 0 : currentFrame; // For frame index protection
                currentFrame %= frameOrder.Length;
                Texture = frames[frameOrder[currentFrame]];
            }
        }
    }
}
