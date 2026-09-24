using ChroniaHelper.Cores;
using ChroniaHelper.Cores.Graphical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components.Graphical;

public class SerialImageGroupRawRenderPack : BaseComponent
{
    public SerialImageGroupRaw Main = new();
    public List<string> TargetText = new();
    /// <summary>
    /// If the component is not added to an entity, this refers to the world coordinates
    /// </summary>
    public Vc2 RelativePosition = Vc2.Zero;
    public Vc2? OverridePosition = null;
    public Func<char, int> TextureSelector = (c) =>
    {
        return Cons.DisplayFontsReference.Contains(c) ? Cons.DisplayFontsReference.IndexOf(c) : Cons.DisplayFontsReference.IndexOf(" ");
    };

    public bool Rendering = false;

    public override void Render()
    {
        base.Render();

        if (!Rendering) { return; }

        Main.Render(TargetText, TextureSelector,
            OverridePosition ?? ((Entity?.Position ?? Vc2.Zero) + RelativePosition));
    }
}
