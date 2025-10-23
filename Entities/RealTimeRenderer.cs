using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.StopwatchSystem;
using VivHelper.Entities;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/RealTimeRenderer")]
public class RealTimeRenderer : SerialImageRenderer
{
    public RealTimeRenderer(EntityData d, Vc2 o) : base(d, o)
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
        d.Attr("segmentOffset").Split(';', StringSplitOptions.TrimEntries).ApplyTo(out string[] _segOffset);
        foreach (var s in _segOffset)
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

        parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));
        staticScreen = new Vc2(d.Float("screenX", 160f), d.Float("screenY", 90f));

        showMilliseconds = d.Bool("showMilliseconds", false);
    }
    private bool showMilliseconds = false;

    public override void Render()
    {
        base.Render();

        string format = showMilliseconds ? "HH:mm:ss:fff" : "HH:mm:ss";
        string renderTarget = DateTime.Now.ToString(format);

        Vc2 levelPos = new Vc2(MaP.level.Bounds.Left, MaP.level.Bounds.Top);
        Vc2 c = MaP.level.Camera.Position + new Vc2(160f, 90f);
        Vc2 _basePosition = c + (Position - c) * parallax + levelPos;
        Vc2 basePosition = new Vc2(parallax.X == 0 ? MaP.level.Camera.Position.X + staticScreen.X : _basePosition.X,
            parallax.Y == 0 ? MaP.level.Camera.Position.Y + staticScreen.Y : _basePosition.Y);

        image.Render(renderTarget, (c) =>
        {
            return $"{c}".ParseInt(c == ':' ? 10 : 0);
        }, basePosition);
    }

    public override void Update()
    {
        base.Update();
    }
}
