using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public static class AlignUtils
{
    public enum Aligns { None, TopLeft, TopCenter, TopRight, MiddleLeft, Center, MiddleRight, BottomLeft, BottomCenter, BottomRight }
    public static Dictionary<Aligns, Vector2> AlignToJustify = new()
    {
        { (Aligns)0, new Vector2(0.5f, 0.5f) },
        { (Aligns)1, new Vector2(0f, 0f) },
        { (Aligns)2, new Vector2(0.5f, 0f) },
        { (Aligns)3, new Vector2(1f, 0f) },
        { (Aligns)4, new Vector2(0f, 0.5f) },
        { (Aligns)5, new Vector2(0.5f, 0.5f) },
        { (Aligns)6, new Vector2(1f, 0.5f) },
        { (Aligns)7, new Vector2(0f, 1f) },
        { (Aligns)8, new Vector2(0.5f, 1f) },
        { (Aligns)9, new Vector2(1f,1f) }
    };

    public static Vc2 ToJustify(this Aligns align)
    {
        return AlignToJustify[align];
    }
}
