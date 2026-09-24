using ChroniaHelper.Cores;
using ChroniaHelper.Cores.Graphical;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Components.Graphical;

public class SerialImageRenderPack : BaseComponent
{
    public SerialImage Main = new();
    public string TargetText = string.Empty;
    /// <summary>
    /// If the component is not added to an entity, this refers to the world coordinates
    /// </summary>
    public Vc2 RelativePosition = Vc2.Zero;
    public Func<char, int> TextureSelector = (c) =>
    {
        return Cons.DisplayFontsReference.Contains(c) ? Cons.DisplayFontsReference.IndexOf(c) : Cons.DisplayFontsReference.IndexOf(" ");
    };

    public bool Rendering = false;

    public override void Render()
    {
        base.Render();

        if (!Rendering) { return; }

        Main.Render(TargetText, TextureSelector, (Entity?.Position ?? Vc2.Zero) + RelativePosition);
    }
}
