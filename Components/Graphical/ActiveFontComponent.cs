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

public class ActiveFontComponent : BaseComponent
{
    public ActiveFontComponent(string targetText = "")
    {
        TargetText = targetText;
    }
    public string TargetText = string.Empty;
    public Vc2 RelativePosition = Vc2.Zero;
    public bool Outlined = false;
    public Vc2 Alignment = Alignments.Center;
    public Vc2 Scale = new(1f, 1f);
    public ChroniaColor Color = ChroniaColor.White;
    public float Stroke = 0f;
    public ChroniaColor StrokeColor = ChroniaColor.White;
    public float EdgeDepth = 0f;
    public ChroniaColor EdgeColor = ChroniaColor.White;

    public override void Render()
    {
        base.Render();

        if (Outlined)
        {
            ActiveFont.DrawOutline(TargetText, Entity.Position + RelativePosition, Alignment, Scale, Color.Parsed(), 2f, StrokeColor.Parsed());
        }
        else
        {
            ActiveFont.Draw(TargetText, Entity.Position + RelativePosition, Alignment, Scale, Color.Parsed(), EdgeDepth, EdgeColor.Parsed(), Stroke, StrokeColor.Parsed());
        }
    }
}
