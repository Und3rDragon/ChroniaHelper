using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Utils.AlignUtils;

namespace ChroniaHelper.Components.Graphical;

public class ActiveFontRenderPack : BaseComponent
{
    public ActiveFontRenderPack(string targetText = "")
    {
        TargetText = targetText;
    }
    public string TargetText = string.Empty;
    public Vc2 RelativePosition = Vc2.Zero;
    public Vc2? OverridePosition = null;
    public bool Outlined = false;
    public Vc2 Alignment = Alignments.Center;
    public Vc2 Scale = new(1f, 1f);
    public ChroniaColor Color = ChroniaColor.White;
    public float Stroke = 0f;
    public ChroniaColor StrokeColor = ChroniaColor.Black;
    public float EdgeDepth = 0f;
    public ChroniaColor EdgeColor = ChroniaColor.White;

    public bool Rendering = false;
    
    public override void Render()
    {
        base.Render();

        if (!Rendering) { return; }

        if (Outlined)
        {
            ActiveFont.DrawOutline(TargetText, OverridePosition ?? ((Entity?.Position ?? Vc2.Zero) + RelativePosition), Alignment, Scale, Color.Parsed(), 2f, StrokeColor.Parsed());
        }
        else
        {
            ActiveFont.Draw(TargetText, OverridePosition ?? ((Entity?.Position ?? Vc2.Zero) + RelativePosition), Alignment, Scale, Color.Parsed(), EdgeDepth, EdgeColor.Parsed(), Stroke, StrokeColor.Parsed());
        }
    }
}
