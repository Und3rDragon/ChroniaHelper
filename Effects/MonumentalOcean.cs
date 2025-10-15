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
        areas.ApplyTo(out this.areas);
    }

    public Vc2[] randomPoints;
    public void GenerateMaps()
    {
        foreach(var area in areas)
        {
            defaultArea.ApplyTo(out float[] p);
            area.Split(',', StringSplitOptions.TrimEntries).EachDoWithIndex((item, n) =>
            { 
                if(n >= 4) { return; }
                p[n] = item.ParseFloat(defaultArea[n]);
            });

            GeometryUtils.GenerateRandomPoints(new Rectangle((int)p[0], (int)p[1], (int)p[2], (int)p[3]), 100)
                .ApplyTo(out randomPoints);
            
        }
    }
        
}
