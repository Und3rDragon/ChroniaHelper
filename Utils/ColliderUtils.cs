using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using ChroniaHelper.Triggers.PolygonSeries;

namespace ChroniaHelper.Utils;

public enum ColliderSide
{
    None,
    Top,
    Right,
    Bottom,
    Left,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public static class ColliderUtils
{
    public static ColliderList GenerateColliderGrid(bool[,] tilemap)
    {
        bool[,] copy = tilemap.Clone() as bool[,];

        ColliderList colliders = new();

        int sx = copy.GetLength(0), sy = copy.GetLength(1);
        for (int x = 0; x < sx; x++)
        {
            List<Hitbox> prevColliders = new();
            Hitbox currentPrevCollider = null;
            for (int y = 0; y <= sy; y++)
            {
                if (y == sy)
                {
                    if (currentPrevCollider is not null)
                        prevColliders.Add(currentPrevCollider);
                    break;
                }

                // basic vertical expansion of the colliders.
                if (copy[x, y])
                {
                    copy[x, y] = false;

                    if (currentPrevCollider == null)
                        currentPrevCollider = new Hitbox(8, 8, x * 8, y * 8);
                    else
                        currentPrevCollider.Height += 8;

                }
                else if (currentPrevCollider != null)
                {
                    prevColliders.Add((Hitbox)currentPrevCollider.Clone());
                    currentPrevCollider = null;
                }
            }

            // once we are done with them, we can extend them horizontally to the right as much as possible.
            foreach (Hitbox prevCollider in prevColliders)
            {
                int cx = (int)prevCollider.Position.X / 8;
                int cy = (int)prevCollider.Position.Y / 8;
                int cw = (int)prevCollider.Width / 8;
                int ch = (int)prevCollider.Height / 8;

                while (cx + cw < sx)
                {
                    bool canExtend = true;

                    for (int j = cy; j < cy + ch; j++)
                    {
                        if (!copy[cx + cw, j])
                        {
                            canExtend = false;
                            break;
                        }
                    }

                    if (canExtend)
                    {
                        for (int j = cy; j < cy + ch; j++)
                        {
                            copy[cx + cw, j] = false;
                        }
                        prevCollider.Width += 8;
                        cw++;
                    }
                    else break;
                }

                colliders.Add(prevCollider);
            }
        }

        return colliders.colliders.Length > 0 ? colliders : null;
    }

    /// <summary>
    /// "r,width,height,x,y;c,radius,x,y;..." => ColliderList
    /// "p,x1,y1,x2,y2,x3,y3,...bool,bool" => ColliderList
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    public static ColliderList ParseColliderList(this string attribute, out bool success)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = new(); success = true;
        for (int i = 0; i < p.Length; i++)
        {
            Collider c = p[i].ParseCollider(true);
            if (c.IsNotNull()) { cl.Add(c); }
            else { success = false; }
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute, Entity master, bool postProcess, out bool success)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = new(); success = true;
        for (int i = 0; i < p.Length; i++)
        {
            Collider c = p[i].ParseCollider(master, postProcess, true);
            if (c.IsNotNull()) { cl.Add(c); }
            else { success = false; }
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute)
    {
        return ParseColliderList(attribute, out bool a);
    }

    public static ColliderList ParseColliderList(this string attribute, Entity master, bool postProcess)
    {
        return ParseColliderList(attribute, master, postProcess, out bool a);
    }

    public static ColliderList ParseColliderList(this string attribute, ColliderList defaultSet, out bool success)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = defaultSet; success = true;
        for (int i = 0; i < p.Length; i++)
        {
            Collider c = p[i].ParseCollider(true);
            if (c.IsNotNull()) { cl.Add(c); }
            else { success = false; }
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute, ColliderList defaultSet, Entity master, bool postProcess, out bool success)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = defaultSet; success = true;
        for (int i = 0; i < p.Length; i++)
        {
            Collider c = p[i].ParseCollider(master, postProcess, true);
            if (c.IsNotNull()) { cl.Add(c); }
            else { success = false; }
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute, ColliderList defaultSet)
    {
        return ParseColliderList(attribute, defaultSet, out bool a);
    }

    public static ColliderList ParseColliderList(this string attribute, ColliderList defaultSet, Entity master, bool postProcess)
    {
        return ParseColliderList(attribute, defaultSet, master, postProcess, out bool a);
    }

    public static ColliderList ListToCollider(this List<Collider> colliders)
    {
        ColliderList cl = new();
        foreach (var collider in colliders)
        {
            cl.Add(collider);
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute, List<Collider> defaultSet, out bool success)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = defaultSet.ListToCollider(); success = true;
        for (int i = 0; i < p.Length; i++)
        {
            Collider c = p[i].ParseCollider(true);
            if (c.IsNotNull()) { cl.Add(c); }
            else { success = false; }
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute, List<Collider> defaultSet, Entity master, bool postProcess,out bool success)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = defaultSet.ListToCollider(); success = true;
        for (int i = 0; i < p.Length; i++)
        {
            Collider c = p[i].ParseCollider(master, postProcess, true);
            if (c.IsNotNull()) { cl.Add(c); }
            else { success = false; }
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute, List<Collider> defaultSet)
    {
        return ParseColliderList(attribute, out bool a);
    }
    
    public static ColliderList ParseColliderList(this string attribute, List<Collider> defaultSet, Entity master, bool postProcess)
    {
        return ParseColliderList(attribute, master, postProcess, out bool a);
    }

    /// <summary>
    /// "width,height,x,y" => Rectangle Hitbox
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static Hitbox ParseRectangleCollider(this string attribute)
    {
        string[] p = attribute.Split(",", StringSplitOptions.TrimEntries);
        Hitbox rec = new Hitbox(16f, 16f);
        if (p.Length >= 2)
        {
            float w, h, x = 0, y = 0;
            float.TryParse(p[0], out w);
            float.TryParse(p[1], out h);
            if (p.Length >= 3)
            {
                float.TryParse(p[2], out x);
            }
            if (p.Length >= 4)
            {
                float.TryParse(p[3], out y);
            }

            if (w > 0 && h > 0) { rec = new Hitbox(w, h, x, y); }
        }

        return rec;
    }

    public static Hitbox ParseRectangleCollider(this string attribute, Hitbox defaultValue)
    {
        string[] p = attribute.Split(",", StringSplitOptions.TrimEntries);
        Hitbox rec = defaultValue;
        if (p.Length >= 2)
        {
            float w, h, x = rec.Position.X, y = rec.Position.Y;
            float.TryParse(p[0], out w);
            float.TryParse(p[1], out h);
            if (p.Length >= 3)
            {
                float.TryParse(p[2], out x);
            }
            if (p.Length >= 4)
            {
                float.TryParse(p[3], out y);
            }

            if (w > 0 && h > 0) { rec = new Hitbox(w, h, x, y); }
        }
        
        return rec;
    }

    public static Collider ParseCollider(this string singleColliderData, bool abandonSafeSetting = false)
    {
        return ParseCollider(singleColliderData, out bool a, abandonSafeSetting);
    }

    public static Collider ParseCollider(this string singleColliderData, Entity master, bool postProcess, bool abandonSafeSetting = false)
    {
        return ParseCollider(singleColliderData, master, postProcess, out bool a, abandonSafeSetting);
    }

    public static Collider ParseCollider(this string singleColliderData,out bool success, bool abandonSafeSetting = false)
    {
        Collider safeSetting = abandonSafeSetting? null : new Hitbox(8f, 8f);
        success = false;

        string[] ps = singleColliderData.Split(",", StringSplitOptions.TrimEntries);
        if (ps.Length <= 1) { return safeSetting; }

        bool isRect = ps[0].ToLower() == "r", isCir = ps[0].ToLower() == "c",
            isPoly = ps[0].ToLower() == "p";
        if (isRect && ps.Length < 3) { return safeSetting; }
        if (isCir && ps.Length < 2) { return safeSetting; }
        if (isPoly && ps.Length < 7) { return safeSetting; }
        if (!isRect && !isCir && !isPoly) { return safeSetting; }

        if (isRect)
        {
            float w, h, x = 0, y = 0;
            float.TryParse(ps[1], out w);
            if (w <= 0) { return safeSetting; }
            float.TryParse(ps[2], out h);
            if (h <= 0) { return safeSetting; }
            if (ps.Length >= 4) { float.TryParse(ps[3], out x); }
            if (ps.Length >= 5) { float.TryParse(ps[4], out y); }

            success = true;
            return new Hitbox(w, h, x, y);
        }
        else if (isCir)
        {
            float r, x = 0, y = 0;
            float.TryParse(ps[1], out r);
            if (r <= 0) { return safeSetting; }
            if (ps.Length >= 3) { float.TryParse(ps[2], out x); }
            if (ps.Length >= 4) { float.TryParse(ps[3], out y); }

            success = true;
            return new Circle(r, x, y);
        }
        else if (isPoly)
        {
            Vector2 p1 = new Vector2(ps[1].ParseFloat(), ps[2].ParseFloat()),
                p2 = new Vector2(ps[3].ParseFloat(), ps[4].ParseFloat()),
                p3 = new Vector2(ps[5].ParseFloat(), ps[6].ParseFloat());
            HashSet<Vector2> points = new() { p1, p2, p3 };
            if (ps.Length > 7)
            {
                Vector2[] additional = new Vector2[ps.Length - 7 > 0 ? ps.Length - 7 : 1];
                for (int j = 0; j * 2 + 7 < ps.Length; j++)
                {
                    additional[j] = new Vector2(ps[j * 2 + 8].ParseFloat(),
                        ps[j * 2 + 9 < ps.Length ? j * 2 + 9 : ps.Length - 1].ParseFloat());
                }

                foreach (var v in additional)
                {
                    points.Enter(v);
                }
            }

            if (points.Count < 3) { return safeSetting; }

            success = true;
            return new PolygonCollider(points.ToArray());
        }

        return safeSetting;
    }

    public static Collider ParseCollider(this string singleColliderData, Entity master, bool postProcess, out bool success, bool abandonSafeSetting = false)
    {
        Collider safeSetting = abandonSafeSetting ? null : new Hitbox(8f, 8f);
        success = false;

        string[] ps = singleColliderData.Split(",", StringSplitOptions.TrimEntries);
        if (ps.Length <= 1) { return safeSetting; }

        bool isRect = ps[0].ToLower() == "r", isCir = ps[0].ToLower() == "c",
            isPoly = ps[0].ToLower() == "p";
        if (isRect && ps.Length < 3) { return safeSetting; }
        if (isCir && ps.Length < 2) { return safeSetting; }
        if (isPoly && ps.Length < 7) { return safeSetting; }
        if (!isRect && !isCir && !isPoly) { return safeSetting; }

        if (isRect)
        {
            float w, h, x = 0, y = 0;
            float.TryParse(ps[1], out w);
            if (w <= 0) { return safeSetting; }
            float.TryParse(ps[2], out h);
            if (h <= 0) { return safeSetting; }
            if (ps.Length >= 4) { float.TryParse(ps[3], out x); }
            if (ps.Length >= 5) { float.TryParse(ps[4], out y); }

            success = true;
            return new Hitbox(w, h, x, y);
        }
        else if (isCir)
        {
            float r, x = 0, y = 0;
            float.TryParse(ps[1], out r);
            if (r <= 0) { return safeSetting; }
            if (ps.Length >= 3) { float.TryParse(ps[2], out x); }
            if (ps.Length >= 4) { float.TryParse(ps[3], out y); }

            success = true;
            return new Circle(r, x, y);
        }
        else if (isPoly)
        {
            Vector2 p1 = new Vector2(ps[1].ParseFloat(), ps[2].ParseFloat()),
                p2 = new Vector2(ps[3].ParseFloat(), ps[4].ParseFloat()),
                p3 = new Vector2(ps[5].ParseFloat(), ps[6].ParseFloat());
            HashSet<Vector2> points = new() { p1, p2, p3 };
            if (ps.Length > 7)
            {
                Vector2[] additional = new Vector2[ps.Length - 7 > 0 ? ps.Length - 7 : 1];
                for (int j = 0; j * 2 + 7 < ps.Length; j++)
                {
                    additional[j] = new Vector2(ps[j * 2 + 8].ParseFloat(),
                        ps[j * 2 + 9 < ps.Length ? j * 2 + 9 : ps.Length - 1].ParseFloat());
                }

                foreach (var v in additional)
                {
                    points.Enter(v);
                }
            }

            if (points.Count < 3) { return safeSetting; }

            success = true;
            
            PolygonCollider pc = new PolygonCollider(points.ToArray());
            if (postProcess)
            {
                for(int i = 0; i < pc.Points.Length; i++)
                {
                    pc.Points[i] += master.Position;
                }
                Triangulator.Triangulator.Triangulate(pc.Points, Triangulator.WindingOrder.Clockwise, out pc.TriangulatedPoints, out pc.Indices);
            }
            return pc;
        }

        return safeSetting;
    }
}
