using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public static class AlignUtils
{
    public static class Alignments
    {
        public static Vc2 TopLeft = new Vc2(0f, 0f);
        public static Vc2 TopCenter = new Vc2(0.5f, 0f);
        public static Vc2 TopRight = new Vc2(1f, 0f);
        public static Vc2 MiddleLeft = new Vc2(0f, 0.5f);
        public static Vc2 Center = new Vc2(0.5f, 0.5f);
        public static Vc2 MiddleRight = new Vc2(1f, 0.5f);
        public static Vc2 BottomLeft = new Vc2(0f, 1f);
        public static Vc2 BottomCenter = new Vc2(0.5f, 1f);
        public static Vc2 BottomRight = new Vc2(1f, 1f);
    }

    public enum Aligns { None, TopLeft, TopCenter, TopRight, MiddleLeft, Center, MiddleRight, BottomLeft, BottomCenter, BottomRight }
    public static Dictionary<Aligns, Vector2> AlignToJustify = new()
    {
        { Aligns.None, Alignments.Center },
        { Aligns.TopLeft, Alignments.TopLeft },
        { Aligns.TopCenter, Alignments.TopCenter },
        { Aligns.TopRight, Alignments.TopRight },
        { Aligns.MiddleLeft, Alignments.MiddleLeft },
        { Aligns.Center, Alignments.Center },
        { Aligns.MiddleRight, Alignments.MiddleRight },
        { Aligns.BottomLeft, Alignments.BottomLeft },
        { Aligns.BottomCenter, Alignments.BottomCenter },
        { Aligns.BottomRight, Alignments.BottomRight },
    };

    public static Vc2 ToJustify(this Aligns align)
    {
        return AlignToJustify[align];
    }
    
    public static Vc2 ToJustify(this int align)
    {
        return AlignToJustify[(Aligns)(align < 1 || align > 9 ? 0 : align)];
    }
}
