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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VivHelper.Entities;

namespace ChroniaHelper.Effects;

[CustomBackdrop("ChroniaHelper/ModifiedAnimatedParallax")]
public class ModifiedAnimatedParallax : Backdrop
{
    public ModifiedAnimatedParallax(BinaryPacker.Element child) : base()
    {
        // default setups
        Position = new Vector2(child.AttrFloat("x", 0), child.AttrFloat("y", 0));
        OnlyIn = child.Attr("only", "*").Split(',', StringSplitOptions.TrimEntries).ToHashSet();
        ExcludeFrom = child.Attr("exclude").Split(',', StringSplitOptions.TrimEntries).ToHashSet();
        OnlyIfFlag = child.Attr("flag");
        OnlyIfNotFlag = child.Attr("notflag");
        Tags = child.Attr("tag").Split(',', StringSplitOptions.TrimEntries).ToHashSet();
        Scroll = new Vector2(child.AttrFloat("scrollx", 1f), child.AttrFloat("scrolly", 1f));
        LoopX = child.AttrBool("loopX", false);
        LoopY = child.AttrBool("loopY", false);

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

        Texture = frames[frameOrder[0]];
        Name = Texture.AtlasPath;
        currentFrame = 0;
        currentFrameTimer = 1f / fps;

        triggerFlag = child.Attr("triggerFlag", "");
        playOnce = child.AttrBool("playOnce", false);
        resetFlag = child.Attr("resetFlag", "");
        resetFrame = child.AttrInt("resetFrame", 0).GetAbs();
    }
    private List<MTexture> frames;
    private int[] frameOrder;
    private float fps;
    private int currentFrame;
    private float currentFrameTimer;
    private MTexture Texture;
    private string triggerFlag, resetFlag;
    private int resetFrame = 0;
    private bool playOnce;

    public Vector2 CameraOffset = Vector2.Zero;
    public BlendState BlendState = BlendState.AlphaBlend;
    public bool DoFadeIn;
    public float Alpha = 1f;
    public float fadeIn = 1f;

    public override void Update(Scene scene)
    {
        base.Update(scene);
        CameraOffset = MaP.level?.CameraOffset ?? Vector2.Zero;
        Position += Speed * Engine.DeltaTime;
        Position += WindMultiplier * (scene as Level).Wind * Engine.DeltaTime;
        
        if (IsVisible(scene as Level))
        {
            if (!resetFlag.IsNullOrEmpty())
            {
                if (resetFlag.GetFlag())
                {
                    currentFrame = resetFrame;
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

    // original parallax code
    public override void Render(Scene scene)
    {
        Vector2 vector = ((scene as Level).Camera.Position + CameraOffset).Floor();
        Vector2 position = (Position - vector * Scroll).Floor();
        float num = fadeIn * Alpha * FadeAlphaMultiplier;
        if (FadeX != null)
        {
            num *= FadeX.Value(vector.X + 160f);
        }

        if (FadeY != null)
        {
            num *= FadeY.Value(vector.Y + 90f);
        }

        Color color = Color;
        if (num < 1f)
        {
            color *= num;
        }
        
        if (color.A > 1)
        {
            if (LoopX)
            {
                position.X = (position.X % (float)Texture.Width - (float)Texture.Width) % (float)Texture.Width;
            }

            if (LoopY)
            {
                position.Y = (position.Y % (float)Texture.Height - (float)Texture.Height) % (float)Texture.Height;
            }

            Log.Info(position);

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (FlipX)
            {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }

            if (FlipY)
            {
                spriteEffects |= SpriteEffects.FlipVertically;
            }

            int num2 = (LoopX ? ((int)Math.Ceiling(320f - position.X)) : Texture.Width);
            int num3 = (LoopY ? ((int)Math.Ceiling(180f - position.Y)) : Texture.Height);
            Rectangle value = new Rectangle(FlipX ? (-num2) : 0, FlipY ? (-num3) : 0, num2, num3);
            float scaleFix = Texture.ScaleFix;
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, position, value, color, 0f, -Texture.DrawOffset / scaleFix, scaleFix, spriteEffects, 0f);
        }
    }
}
