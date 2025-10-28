using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using IL.MonoMod;
using Microsoft.Build.Framework;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/MessageDisplayer")]
public class MessageDisplayer : Entity
{
    public MessageDisplayer(EntityData d, Vc2 o) : base(d.Position + o)
    {
        base.Depth = d.Int("depth", -100000);

        SerialImage template = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"));

        template.renderMode = d.Int("renderMode", 0);
        template.origin = new Vc2(d.Float("lineOriginX", 0.5f), d.Float("lineOriginY", 0.5f));
        template.segmentOrigin = Vc2.Zero;
        template.distance = d.Float("letterDistance", 1f);
        template.color = d.GetChroniaColor("fontColor", Color.White);
        primaryAlpha = template.color.alpha;

        renderer = new SerialImageGroup(template, d.Attr("textures","ChroniaHelper/DisplayFonts/font").Split(',',StringSplitOptions.TrimEntries));
        renderer.groupOrigin = new Vc2(d.Float("originX", 0.5f), d.Float("originY", 0.5f));
        renderer.memberDistance = d.Float("lineDistance", 2f);

        content = d.Attr("dialogID");

        parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));
        staticScreen = new Vc2(d.Float("screenX", 160f), d.Float("screenY", 90f));

        renderDistance = d.Float("renderDistance", -1f);
        typingDisplay = d.Bool("typewriterEffect", false);
        fadeInSpeed = d.Float("fadeInSpeed", 4f);
        fadeOutSpeed = d.Float("fadeOutSpeed", 2f);
        letterInterval = d.Float("letterDisplayInterval", 0.1f).ClampMin(Engine.DeltaTime);

        overrideFlag = d.Attr("triggerFlag");
        hasOverrideFlag = !overrideFlag.IsNullOrEmpty();

        reference = d.Attr("characterReference", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =,");
    }
    public SerialImageGroup renderer;
    public string content;
    public Vc2 parallax = Vc2.Zero;
    public Vc2 staticScreen = Vc2.Zero;
    private bool typingDisplay = true;
    public float renderDistance = 256f, fadeInSpeed = 4f, fadeOutSpeed = 2f;
    public float primaryAlpha = 1f;
    public float letterInterval = 0.1f;
    public string overrideFlag;
    private bool hasOverrideFlag = false;

    public string reference = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =,";

    public List<string> ParseRenderTarget()
    {
        string text = Dialog.Clean(content, Dialog.Languages["english"]);
        
        var lines = text.Split(new char[] { '\n', '\r'}, StringSplitOptions.TrimEntries);
        var result = new List<string>();
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                result.Add(trimmed);
            }
        }
        
        return result;
    }

    public int Reflection(char c)
    {
        return reference.Contains(c) ? reference.IndexOf(c) : reference.IndexOf(" ");
    }

    List<string> progressedText = new();
    List<int> progress = new();
    public override void Render()
    {
        base.Render();

        List<string> orig = ParseRenderTarget();

        // set up typer text
        if (!typingDisplay) { progressedText = orig; }
        else
        {
            if (!renderArg && fadeEnded)
            {
                progressedText = new();
                progress = new();
                for (int i = 0; i < orig.Count; i++)
                {
                    progressedText.Add("");
                    progress.Add(-1);
                }
            }
            else if (renderArg)
            {
                if (Scene.OnInterval(letterInterval))
                {
                    for (int i = 0; i < progress.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (progress[0] < orig[0].Length - 1) { progress[0]++; }

                            progressedText[0] = orig[0].Substring(0, progress[0] + 1);
                            continue;
                        }

                        if (progress[i] < orig[i].Length - 1 && progress[i - 1] == orig[i - 1].Length - 1)
                        {
                            progress[i]++;
                        }

                        progressedText[i] = orig[i].Substring(0, progress[i] + 1);
                    }
                }
            }
        }
        
        renderer.Render(progressedText,
            (c) => Reflection(c),
            Position.InParallax(parallax, staticScreen));
    }
    public bool renderArg => (renderDistance > 0 && inRange) || (hasOverrideFlag && overrideFlag.GetFlag());

    public bool inRange = false;
    public bool fadeEnded = false;
    public override void Update()
    {
        base.Update();

        if(PUt.TryGetPlayer(out Player player))
        {
            inRange = renderDistance <= 0f ? true : (player.Center - Position).Length() <= renderDistance; 
        }
        
        renderer.template.color.alpha = Calc.Approach(renderer.template.color.alpha,
            updateArg ? primaryAlpha : 0f,
            (updateArg ? fadeInSpeed : fadeOutSpeed) * Engine.DeltaTime
            );
        fadeEnded = renderer.template.color.alpha == (updateArg ? primaryAlpha : 0f);
    }
    
    public bool updateArg => (renderDistance <= 0f && !hasOverrideFlag) ||
            (renderDistance > 0 && inRange) || (hasOverrideFlag && overrideFlag.GetFlag());

}
