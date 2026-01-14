using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores.Graphical;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.StopwatchSystem;

namespace ChroniaHelper.Cores;

/// <summary>
/// Normal Render mode Serial Image Renderer
/// </summary>
public class SerialImageRenderer : BaseEntity
{
    public SerialImageRenderer(EntityData d, Vc2 o) : base(d, o)
    {
        Depth = d.Int("depth", -10000000);
    }
    public string source = "ChroniaHelper/DisplayFonts/font";
    public SerialImage image = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"));

    /// <summary>
    /// index,x,y => segment offset
    /// </summary>
    public void ParseSegmentOffset(string str)
    {
        str.Split(';', StringSplitOptions.TrimEntries).ApplyTo(out string[] _segOffset);
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
    }
    
    public Vc2 Parallax = Vc2.One;
    public Vc2 StaticScreen = new Vc2(160f, 80f);
}
