using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AsmResolver.DotNet.Serialized;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Utils.StopwatchSystem;

[Tracked(true)]
[CustomEntity("ChroniaHelper/StopclockRenderer")]
public class StopclockRenderer : SerialImageRenderer
{
    public StopclockRenderer(EntityData d, Vc2 o) : base(d, o)
    {
        Tag |= Tags.TransitionUpdate;
        Depth = d.Int("depth", -10000000);
        
        source = d.Attr("sourcePath", "ChroniaHelper/StopclockFonts/font");
        image = new SerialImage(GFX.Game.GetAtlasSubtextures(source));

        image.renderMode = d.Int("renderMode", 0);
        image.origin = AlignUtils.AlignToJustify[(AlignUtils.Aligns)d.Int("positionAlign", 5)];
        image.segmentOrigin = AlignUtils.AlignToJustify[(AlignUtils.Aligns)d.Int("segmentAlign", 5)];
        image.distance = d.Float("segmentDistance", 4f);
        image.color = d.GetChroniaColor("rendererColor", Color.White);
        maxAlpha = image.color.alpha;
        overrideAlpha = 0f;
        image.color.alpha = overrideAlpha;
        d.Attr("segmentOffset").Split(';', StringSplitOptions.TrimEntries).ApplyTo(out string[] _segOffset);
        foreach(var s in _segOffset)
        {
            s.Split(',', StringSplitOptions.TrimEntries).ApplyTo(out string[] seg);
            if (seg.Length < 1) { continue; }
            int index = seg[0].ParseInt(0);
            Vc2 offset = Vc2.Zero;
            if (seg.Length < 2) { image.segmentOffset.Enter(index, offset); continue; }
            offset = new Vc2(seg[1].ParseInt(0), 0);
            if (seg.Length < 3) { image.segmentOffset.Enter(index, offset); continue; }
            offset = new Vc2(seg[1].ParseInt(0), seg[2].ParseInt(0));
            image.segmentOffset.Enter(index, offset);
        }
        image.scale = d.Float("scale", 6f).GetAbs();
        
        clockTag = d.Attr("stopclockTag", "stopclock");
        Parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));
        StaticScreen = new Vc2(d.Float("screenX", 160f), d.Float("screenY", 90f));

        maxUnit = d.Int("maximumUnit", 3);
        minUnit = d.Int("minimumUnit", 0);

        trimZeros = d.Bool("trimZeros", true);
    }
    private string clockTag;
    private float overrideAlpha, maxAlpha;
    private enum Units { Year = 6, Month = 5, Day = 4, Hour = 3, Minute = 2, Second = 1, Millisecond = 0 }
    private int maxUnit, minUnit;
    private bool trimZeros;
    
    public override string ParseRenderTarget()
    {
        if (!clockTag.GetStopclock(out Stopclock clock)) { return ""; }

        clock.GetClampedTimeData(out int[] data, minUnit, maxUnit);

        string renderTarget = "";
        for (int i = 0; i < data.Length; i++)
        {
            if (i == 0)
            {
                renderTarget = minUnit == 0 ? $"{data[i]:000}" : $"{data[i]:00}";
                continue;
            }
            else
            {
                renderTarget = $"{data[i]:00}:" + renderTarget;
            }
        }

        if (trimZeros)
        {
            renderTarget = Regex.Replace(renderTarget, "0+:+", "");
        }

        return renderTarget;
    }

    public override int Reflection(char c)
    {
        return $"{c}".ParseInt(c == ':' ? 10 : 0);
    }

    public override Vc2 SetRenderPosition(Vc2 position, Vc2 parallax, Vc2 staticScreen)
    {
        return ParseGlobalPositionToHDPosition(Position, Parallax, StaticScreen) * HDScale;
    }

    public override void Update()
    {
        base.Update();

        if (!clockTag.GetStopclock(out Stopclock clock)) { return; }

        image.color.alpha = overrideAlpha = Calc.Approach(overrideAlpha, clock.completed? 0f: maxAlpha, 2 * Engine.DeltaTime);
    }
}
