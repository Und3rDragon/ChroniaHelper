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
[CustomEntity("ChroniaHelper/FntDisplayerHD")]
public class FntDisplayerHD : HDRenderEntity
{
    public FntDisplayerHD(EntityData d, Vc2 o) : base(d, o)
    {
        base.Depth = d.Int("depth", -100000);

        GraphicalParams.SerialImageTemplate template = new();

        template.renderMode = d.Int("renderMode", 0);
        template.origin = new Vc2(d.Float("lineOriginX", 0.5f), d.Float("lineOriginY", 0.5f));
        template.segmentOrigin = new Vc2(d.Float("letterOriginX", 0f), d.Float("letterOriginY", 0f));
        template.distance = d.Float("letterDistance", 1f);
        template.color = d.GetChroniaColor("fontColor", Color.White);
        primaryAlpha = template.color.alpha;
        
        renderer = new FntTextGroupHD(template, d.StringArray("textures"));
        renderer.groupOrigin = new Vc2(d.Float("overallOriginX", 0.5f), d.Float("overallOriginY", 0.5f));
        renderer.memberDistance = d.Float("lineDistance", 2f);
        string[] _scales = d.Attr("scale", "1").Split(',', StringSplitOptions.TrimEntries);
        foreach(var scale in _scales)
        {
            renderer.scales.Add(scale.ParseFloat(1f));
        }

        // offset index setup: pathIndex, charIndex, offsetX, offsetY
        string[] offsetIndex = d.Attr("offsetPerIndex").Split(';', StringSplitOptions.TrimEntries);
        foreach (var offset in offsetIndex)
        {
            string[] segs = offset.Split(',', StringSplitOptions.TrimEntries);
            if (segs.Length < 3) { continue; }

            if (segs[0].ParseInt(0) >= renderer.path.Count || segs[0].ParseInt(0) < 0) { continue; }

            Vc2 of = Vc2.Zero;
            of.X = segs[2].ParseFloat(0);
            if (segs.Length >= 4) { of.Y = segs[3].ParseFloat(0); }

            renderer.memberIndexOffsets.Create(segs[0].ParseInt(0), new());
            renderer.memberIndexOffsets[segs[0].ParseInt(0)].Create(segs[1].ParseInt(0), Vc2.Zero);
            renderer.memberIndexOffsets[segs[0].ParseInt(0)][segs[1].ParseInt(0)] = of;
        }

        // offset charcode setup: pathIndex, charcode, offsetX, offsetY
        string[] offsetCharcode = d.Attr("offsetPerCharcode").Split(';', StringSplitOptions.TrimEntries);
        foreach (var offset in offsetCharcode)
        {
            string[] segs = offset.Split(',', StringSplitOptions.TrimEntries);
            if (segs.Length < 3) { continue; }

            if (segs[0].ParseInt(0) >= renderer.path.Count || segs[0].ParseInt(0) < 0) { continue; }

            Vc2 of = Vc2.Zero;
            of.X = segs[2].ParseFloat(0);
            if (segs.Length >= 4) { of.Y = segs[3].ParseFloat(0); }

            renderer.memberCharcodeOffsets.Create(segs[0].ParseInt(0), new());
            renderer.memberCharcodeOffsets[segs[0].ParseInt(0)].Create(segs[1].ParseInt(0), Vc2.Zero);
            renderer.memberCharcodeOffsets[segs[0].ParseInt(0)][segs[1].ParseInt(0)] = of;
        }

        renderer.ApplyAllOffsetSetups();

        content = d.Attr("dialogID");

        Parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));
        StaticScreen = new Vc2(d.Float("screenX", 160f), d.Float("screenY", 90f));

        renderDistance = d.Float("renderDistance", -1f);
        typingDisplay = d.Bool("typewriterEffect", false);
        fadeInSpeed = d.Float("fadeInSpeed", 4f);
        fadeOutSpeed = d.Float("fadeOutSpeed", 2f);
        letterInterval = d.Float("letterDisplayInterval", 0.1f).ClampMin(Engine.DeltaTime);

        overrideFlag = d.Attr("triggerFlag");
        hasOverrideFlag = !overrideFlag.IsNullOrEmpty();
    }
    public FntTextGroupHD renderer;
    public string content;
    private bool typingDisplay = true;
    public float renderDistance = 256f, fadeInSpeed = 4f, fadeOutSpeed = 2f;
    public float primaryAlpha = 1f;
    public float letterInterval = 0.1f;
    public string overrideFlag;
    private bool hasOverrideFlag = false;
    
    public List<string> ParseRenderTarget()
    {
        string text = content.StartsWith('#') ?
            Md.Session.keystrings.GetValueOrDefault(content.TrimStart('#'), "") :
            content.ParseDialogToString();

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
    
    List<string> progressedText = new();
    List<int> progress = new();
    protected override void HDRender()
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
            ParseGlobalPositionToHDPosition(Position, Parallax, StaticScreen));
    }
    
    public bool renderArg => (renderDistance <= 0f && !hasOverrideFlag) || (renderDistance > 0 && inRange) || (hasOverrideFlag && overrideFlag.GetFlag());

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
