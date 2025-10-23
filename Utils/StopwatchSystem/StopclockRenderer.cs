using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
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
        Position = d.Position;
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
        
        clockTag = d.Attr("stopclockTag", "stopclock");
        parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));
        staticScreen = new Vc2(d.Float("screenX", 160f), d.Float("screenY", 90f));

        maxUnit = d.Int("maximumUnit", 3);
        minUnit = d.Int("minimumUnit", 0);

        trimZeros = d.Bool("trimZeros", true);
    }
    private string clockTag;
    private float overrideAlpha, maxAlpha;
    private enum Units { Year = 6, Month = 5, Day = 4, Hour = 3, Minute = 2, Second = 1, Millisecond = 0 }
    private int maxUnit, minUnit;
    private bool trimZeros;

    public override void Render()
    {
        base.Render();

        if (!clockTag.GetStopclock(out Stopclock clock)) { return; }

        clock.GetTimeData(out int[] data, minUnit, maxUnit);

        string renderTarget = "";
        for (int i = 0; i < data.Length; i++)
        {
            if (i == 0)
            {
                renderTarget = $":{data[i]:000}";
                continue;
            }
            if (i != data.Length - 1)
            {
                renderTarget = $":{data[i]:00}" + renderTarget;
                continue;
            }
            renderTarget = $"{data[i]}" + renderTarget;
        }
        
        if (trimZeros)
        {
            renderTarget = renderTarget.TrimStart('0').TrimStart(':').RemoveAll("00:");
        }

        Vc2 levelPos = new Vc2(MaP.level.Bounds.Left, MaP.level.Bounds.Top);
        Vc2 c = MaP.level.Camera.Position + new Vc2(160f, 90f);
        Vc2 _basePosition = c + (Position - c) * parallax + levelPos;
        Vc2 basePosition = new Vc2(parallax.X == 0 ? MaP.level.Camera.Position.X + staticScreen.X : _basePosition.X,
            parallax.Y == 0 ? MaP.level.Camera.Position.Y + staticScreen.Y : _basePosition.Y);

        image.Render(renderTarget, (c) =>
        {
            return $"{c}".ParseInt(c == ':' ? 10 : 0);
        }, basePosition + new Vc2(4f, 4f));
    }

    public override void Update()
    {
        base.Update();

        if (!clockTag.GetStopclock(out Stopclock clock)) { return; }

        image.color.alpha = overrideAlpha = Calc.Approach(overrideAlpha, clock.completed? 0f: maxAlpha, 2 * Engine.DeltaTime);
    }
}
