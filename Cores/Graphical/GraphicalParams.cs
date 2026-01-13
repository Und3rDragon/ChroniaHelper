using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores.Graphical;

public static class GraphicalParams
{
    public class SerialImageTemplate
    {
        public Vc2 origin = Vc2.One * 0.5f;
        public Vc2 segmentOrigin = Vc2.One * 0.5f;
        /// <summary>
        /// Compact = 0, EqualDistance = 1
        /// </summary>
        public int renderMode = 0;
        public float distance = 4f;
        public CColor color = CColor.White;
    }
}
