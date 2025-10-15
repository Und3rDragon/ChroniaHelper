using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using Triangulator;

namespace ChroniaHelper.Effects;

public class MonumentalOcean : Backdrop
{
    public float[] defaultArea = new float[4]{ -20f, 90f, 340f, 180f };
    public string[] areas;
    public MonumentalOcean(BinaryPacker.Element e) 
    {
        string[] areas = e.Attr("areas", "-20,90,340,180").Split(";", StringSplitOptions.TrimEntries);
        areas.ApplyTo(ref this.areas);
    }
        
}
