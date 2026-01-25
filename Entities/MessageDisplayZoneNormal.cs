using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Cores.Graphical;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using IL.MonoMod;
using Microsoft.Build.Framework;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/MessageDisplayZoneNormal")]
public class MessageDisplayZoneNormal : SerialImageRenderer
{
    public MessageDisplayZoneNormal(EntityData d, Vc2 o) : base(d, o)
    {
        Prm.SerialImageTemplate template = new();

        template.renderMode = d.Int("renderMode", 0);
        template.origin = new Vc2(d.Float("lineOriginX", 0.5f), d.Float("lineOriginY", 0.5f));
        template.segmentOrigin = new Vc2(d.Float("letterOriginX", 0f), d.Float("letterOriginY", 0f));
        template.distance = d.Float("letterDistance", 1f);
        template.color = d.GetChroniaColor("fontColor", Color.White);
        primaryAlpha = template.color.alpha;
        template.color.alpha = 0f;

        renderer = new SerialImageGroup(template, d.Attr("textures","ChroniaHelper/DisplayFonts/font").Split(',',StringSplitOptions.TrimEntries));
        renderer.groupOrigin = new Vc2(d.Float("overallOriginX", 0.5f), d.Float("overallOriginY", 0.5f));
        renderer.memberDistance = d.Float("lineDistance", 2f);
        string[] _scales = d.Attr("scale", "1").Split(',', StringSplitOptions.TrimEntries);
        foreach (var scale in _scales)
        {
            renderer.scales.Add(scale.ParseFloat(1f));
        }

        content = d.Attr("dialogID");

        Parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));
        StaticScreen = new Vc2(d.Float("screenX", 160f), d.Float("screenY", 90f));
        
        typingDisplay = d.Bool("typewriterEffect", false);
        fadeInSpeed = d.Float("fadeInSpeed", 4f);
        fadeOutSpeed = d.Float("fadeOutSpeed", 2f);
        letterInterval = d.Float("letterDisplayInterval", 0.1f).ClampMin(Engine.DeltaTime);

        overrideFlag = d.Attr("triggerFlag");
        hasOverrideFlag = !overrideFlag.IsNullOrEmpty();

        reference = d.Attr("characterReference", Cons.DisplayFontsReference);

        leaveReset = d.Bool("leaveReset", false);

        Collider = new Hitbox(d.Width, d.Height);
    }
    public SerialImageGroup renderer;
    public string content;
    private bool typingDisplay = true;
    public float fadeInSpeed = 4f, fadeOutSpeed = 2f;
    public float primaryAlpha = 1f;
    public float letterInterval = 0.1f;
    public string overrideFlag;
    private bool hasOverrideFlag = false;
    public bool leaveReset = false;

    public string reference = Cons.DisplayFontsReference;

    public List<string> ParseRenderTarget()
    {
        string text = content.StartsWith("#") ?
            Md.Session.keystrings.GetValueOrDefault(content.TrimStart('#'), "") :
            Dialog.Clean(content, Dialog.Languages["english"]);

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
        List<string> orig = ParseRenderTarget();

        // set up typer text
        if (!typingDisplay) { progressedText = orig; }
        else
        {
            if (!renderArg && fadeEnded)
            {
                progressedText.Clear();
                progress.Clear();
                for (int i = 0; i < orig.Count; i++)
                {
                    progressedText.Add("");
                    progress.Add(-1);
                }
            }
            else if (renderArg)
            {
                if(progress.Count == 0 || progressedText.Count == 0)
                {
                    progressedText.Clear();
                    progress.Clear();
                    for (int i = 0; i < orig.Count; i++)
                    {
                        progressedText.Add("");
                        progress.Add(-1);
                    }
                }
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
            nodes[1].InParallax(Parallax, StaticScreen));
    }
    public bool renderArg => inRange || hasOverrideFlag && overrideFlag.GetFlag();

    public bool collided = false;
    public bool inRange = false;
    public bool fadeEnded = false;
    public override void Update()
    {
        base.Update();
        
        if(PUt.TryGetPlayer(out Player player))
        {
            collided = CollideCheck(player);
        }
        else
        {
            collided = false;
        }

        inRange = leaveReset ? collided : inRange.TryPositive(collided);
        
        renderer.template.color.alpha = Calc.Approach(renderer.template.color.alpha,
            updateArg ? primaryAlpha : 0f,
            (updateArg ? fadeInSpeed : fadeOutSpeed) * Engine.DeltaTime
            );
        fadeEnded = renderer.template.color.alpha == (updateArg ? primaryAlpha : 0f);
    }
    
    public bool updateArg => inRange || hasOverrideFlag && overrideFlag.GetFlag();
}
