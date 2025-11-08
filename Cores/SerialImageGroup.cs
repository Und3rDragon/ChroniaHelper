using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using YoctoHelper.Cores;

namespace ChroniaHelper.Cores;

public class SerialImageGroup
{
    public List<SerialImage> members = new();
    /// <summary>
    /// An empty template defining member parameters
    /// </summary>
    public SerialImage template = new SerialImage("ChroniaHelper/DisplayFonts/font");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="template">An empty template defining member parameters</param>
    /// <param name="paths"></param>
    public SerialImageGroup(SerialImage template, params string[] paths)
    {
        this.template = template;
        foreach (var p in paths)
        {
            if (p.IsNullOrEmpty()) { continue; }

            path.Add(p);
        }
    }
    
    public SerialImageGroup(params string[] paths)
    {
        foreach(var p in paths)
        {
            if (p.IsNullOrEmpty()) { continue;  }

            path.Add(p);
        }
    }
    public Vc2 groupOrigin = Vc2.Zero;
    public float memberDistance = 2f;
    public Vc2 groupPosition = Vc2.Zero;
    public Vc2 groupOffset = Vc2.Zero;
    public List<string> path = new();
    public List<float> scales = new();
    public List<float> depths = new();
    
    public string SafeGetPath(int i)
    {
        if (path.IsNull()) { return "ChroniaHelper/DisplayFonts/font"; }
        if (path.Count == 0) { return "ChroniaHelper/DisplayFonts/font"; }

        return path[i.Clamp(0, path.Count - 1)];
    }

    public Vc2 groupSize = Vc2.Zero;
    public Vc2 groupTopleft, groupBottomRight;
    public List<Vc2> memberPosition = new();
    public Vc2 memberStart = Vc2.Zero;
    /// <summary>
    /// Measuring the size of the to-be-rendered texts
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source should be a list of classes, if it's string,
    /// the source should be a List of strings </param>
    /// <param name="selector">The deepest reflection of the function returning integers as the texture index</param>
    public void Measure<T>(IList<IList<T>> source, Func<T, int> selector)
    {
        members = new();
        
        for(int i = 0; i < source.Count; i++)
        {
            SerialImage image = new SerialImage(SafeGetPath(i));
            image.origin = template.origin;
            image.segmentOrigin = template.segmentOrigin;
            image.overallOffset = groupOffset;
            image.renderMode = template.renderMode;
            image.distance = template.distance;
            image.color = template.color;
            if(scales.TryGetOrGetLast(i, out float? scale))
            {
                image.scale = scale?? 1f;
            }
            else
            {
                image.scale = 1f;
            }
            if(depths.TryGetOrGetLast(i, out float? depth))
            {
                image.depth = depth?? 0f;
            }
            else
            {
                image.depth = 0f;
            }
            image.Measure(source[i], (item) => selector(item));
            members.Add(image);
        }

        // Mapping members
        Vc2 cal = groupTopleft = groupBottomRight = Vc2.Zero;
        memberPosition = new();
        groupSize = new();
        
        for(int i = 0; i < members.Count; i++)
        {
            if(i == 0)
            {
                memberPosition.Add(cal);

                groupTopleft = -1f * members[i].overallSize * template.origin;
                groupBottomRight = members[i].overallSize * (Vc2.One - template.origin);

                continue;
            }

            cal.Y += members[i].overallSize.Y * template.origin.Y + members[i - 1].overallSize.Y * (1f - template.origin.Y) + memberDistance;
            memberPosition.Add(cal);
            
            groupTopleft.X = groupTopleft.X.ClampMax(members[i].overallSize.X * template.origin.X * -1f);
            groupTopleft.Y = groupTopleft.Y.ClampMax(cal.Y + members[i].overallSize.Y * template.origin.Y * -1f);
            groupBottomRight.X = groupBottomRight.X.ClampMin(members[i].overallSize.X * (1f - template.origin.X));
            groupBottomRight.Y = groupBottomRight.Y.ClampMin(cal.Y + members[i].overallSize.Y * (1f - template.origin.Y));
        }

        groupSize = groupBottomRight - groupTopleft;
        memberStart = -groupTopleft;
    }
    
    public void Measure(IList<string> source, Func<char, int> selector)
    {
        members = new();

        for (int i = 0; i < source.Count; i++)
        {
            SerialImage image = new SerialImage(SafeGetPath(i));
            image.origin = template.origin;
            image.segmentOrigin = template.segmentOrigin;
            image.overallOffset = groupOffset;
            image.renderMode = template.renderMode;
            image.distance = template.distance;
            image.color = template.color;
            if (scales.TryGetOrGetLast(i, out float? scale))
            {
                image.scale = scale ?? 1f;
            }
            else
            {
                image.scale = 1f;
            }
            if (depths.TryGetOrGetLast(i, out float? depth))
            {
                image.depth = depth ?? 0f;
            }
            else
            {
                image.depth = 0f;
            }
            image.Measure(source[i], (item) => selector(item));
            members.Add(image);
        }

        // Mapping members
        Vc2 cal = groupTopleft = groupBottomRight = Vc2.Zero;
        memberPosition = new();
        groupSize = new();
        
        for (int i = 0; i < members.Count; i++)
        {
            if (i == 0)
            {
                memberPosition.Add(cal);

                groupTopleft = -1f * members[i].overallSize * template.origin;
                groupBottomRight = members[i].overallSize * (Vc2.One - template.origin);

                continue;
            }

            cal.Y += members[i].overallSize.Y * template.origin.Y + members[i - 1].overallSize.Y * (1f - template.origin.Y) + memberDistance;
            memberPosition.Add(cal);

            groupTopleft.X = groupTopleft.X.ClampMax(members[i].overallSize.X * template.origin.X * -1f);
            groupTopleft.Y = groupTopleft.Y.ClampMax(cal.Y + members[i].overallSize.Y * template.origin.Y * -1f);
            groupBottomRight.X = groupBottomRight.X.ClampMin(members[i].overallSize.X * (1f - template.origin.X));
            groupBottomRight.Y = groupBottomRight.Y.ClampMin(cal.Y + members[i].overallSize.Y * (1f - template.origin.Y));
        }

        groupSize = groupBottomRight - groupTopleft;
        memberStart = -groupTopleft;
    }

    public void Render(IList<string> source, Func<char, int> selector, Vc2 renderPosition)
    {
        Measure(source, selector);

        for (int i = 0; i < members.Count; i++)
        {
            Vc2 dPos = groupSize * groupOrigin * -1f + memberStart + memberPosition[i] + groupOffset;
            
            members[i].Render(source[i], selector, renderPosition + new Vc2((int)dPos.X, (int)dPos.Y));
        }
    }
    
    public void Render<T>(IList<IList<T>> source, Func<T, int> selector, Vc2 renderPosition)
    {
        Measure(source, selector);

        for(int i = 0; i < members.Count; i++)
        {
            Vc2 dPos = groupSize * groupOrigin * -1f + memberStart + memberPosition[i] + groupOffset;
            
            members[i].Render(source[i], selector, renderPosition + new Vc2((int)dPos.X, (int)dPos.Y));
        }
    }
}
