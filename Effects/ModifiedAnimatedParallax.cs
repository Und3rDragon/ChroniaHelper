using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Celeste.Mod.Backdrops;
using Celeste.Mod.MaxHelpingHand.Effects;
using Celeste.Mod.Meta;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using VivHelper.Entities;

namespace ChroniaHelper.Effects;

[CustomBackdrop("ChroniaHelper/ModifiedAnimatedParallax")]
public class ModifiedAnimatedParallax : Parallax
{
    public ModifiedAnimatedParallax(BinaryPacker.Element child, MTexture texture) : base(texture)
    {
        // remove the frame number, much like decals do.
        string texturePath = Regex.Replace(child.Attr("texture"), "\\d+$", string.Empty);

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
            fps = fpsCount.Groups[1].Value.ParseFloat(12f);
        }
        else
        {
            // use 12 FPS by default, like decals.
            fps = child.AttrFloat("fps", 12f);
        }

        if (!child.Attr("frames").IsNullOrEmpty())
        {
            frameOrder = Calc.ReadCSVIntWithTricks(child.Attr("frames"));
        }
        
        texture = frames[frameOrder[0]];
        currentFrame = 0;
        currentFrameTimer = 1f / fps;

        triggerFlag = child.Attr("triggerFlag", "");
        playOnce = child.AttrBool("playOnce", false);
        resetFlag = child.Attr("resetFlag", "");
        resetFrame = child.AttrInt("resetFrame", 0);
    }
    private List<MTexture> frames;
    private int[] frameOrder;
    private float fps;
    private int currentFrame;
    private float currentFrameTimer;

    private string triggerFlag, resetFlag;
    private int resetFrame = 0;
    private bool playOnce;

    public override void Update(Scene scene)
    {
        base.Update(scene);

        if (IsVisible(scene as Level))
        {
            if (!resetFlag.IsNullOrEmpty())
            {
                if (resetFlag.GetFlag())
                {
                    currentFrame = 0;
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
            currentFrameTimer -= Engine.DeltaTime;
            if (currentFrameTimer < 0f)
            {
                currentFrameTimer += (1f / fps);
                if(playOnce && currentFrame != frameOrder.MaxIndex())
                {
                    currentFrame++;
                }
                currentFrame %= frameOrder.Length;
                Texture = frames[frameOrder[currentFrame]];
            }
        }
    }
}
