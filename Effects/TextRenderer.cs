using Celeste.Mod.Backdrops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Effects;

[CustomBackdrop("ChroniaHelper/TextRenderer")]
[ExA.WorkingInProgress]
public class TextRenderer : Backdrop
{
    public TextRenderer(BinaryPacker.Element data)
    {
        useActiveFont = data.AttrBool("useActiveFont", true);
        dialog = data.Attr("dialogID");
        texturePath = data.Attr("directory", Cons.DefaultDisplayerFontPath);
        textureReferences = data.Attr("references", Cons.DisplayFontsReference);
    }
    private bool useActiveFont;
    private string dialog;
    private string texturePath;
    private string textureReferences;
}
