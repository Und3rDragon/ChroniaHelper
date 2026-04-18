using ChroniaHelper.Cores;
using ChroniaHelper.Cores.Graphical;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components.Graphical;

public class SerialImageComponent : BaseComponent
{
    public SerialImageComponent(string path = "ChroniaHelper/DisplayFonts/font")
    {
        if(!path.HasValidContent())
        {
            path = "ChroniaHelper/DisplayFonts/font";
        }

        Main = new(path);
    }
    public SerialImage Main;
    public string TargetText;
    public Vc2 RelativePosition = Vc2.Zero;
    public Func<char, int> TextureSelector = (c) =>
    {
        return Cons.DisplayFontsReference.Contains(c) ? Cons.DisplayFontsReference.IndexOf(c) : Cons.DisplayFontsReference.IndexOf(" ");
    };

    public override void Render()
    {
        base.Render();

        Main.Render(TargetText, TextureSelector, Entity.Position + RelativePosition);
    }
}
