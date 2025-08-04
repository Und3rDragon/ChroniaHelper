using static System.Runtime.InteropServices.JavaScript.JSType;

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
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    public static ColliderList ParseColliderList(this string attribute, out bool success)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = new(); success = false;
        for (int i = 0; i < p.Length; i++)
        {
            string[] ps = p[i].Split(",", StringSplitOptions.TrimEntries);
            if (ps.Length <= 1) { continue; }

            bool isRect = ps[0].ToLower() == "r", isCir = ps[0].ToLower() == "c";
            if (isRect && ps.Length < 3) { continue; }
            if (isCir && ps.Length < 2) { continue; }
            if (!isRect && !isCir) { continue; }

            if (isRect)
            {
                float w, h, x = 0, y = 0;
                float.TryParse(ps[1], out w);
                if (w <= 0) { continue; }
                float.TryParse(ps[2], out h);
                if (h <= 0) { continue; }
                if (ps.Length >= 4) { float.TryParse(ps[3], out x); }
                if (ps.Length >= 5) { float.TryParse(ps[4], out y); }
                cl.Add(new Hitbox(w, h, x, y));
                success = true;
            }
            else if (isCir)
            {
                float r, x = 0, y = 0;
                float.TryParse(ps[1], out r);
                if (r <= 0) { continue; }
                if (ps.Length >= 3) { float.TryParse(ps[2], out x); }
                if (ps.Length >= 4) { float.TryParse(ps[3], out y); }
                cl.Add(new Circle(r, x, y));
                success = true;
            }
        }

        return cl;
    }

    public static ColliderList ParseColliderList(this string attribute)
    {
        string[] p = attribute.Split(";", StringSplitOptions.TrimEntries);
        ColliderList cl = new();
        for (int i = 0; i < p.Length; i++)
        {
            string[] ps = p[i].Split(",", StringSplitOptions.TrimEntries);
            if (ps.Length <= 1) { continue; }

            bool isRect = ps[0].ToLower() == "r", isCir = ps[0].ToLower() == "c";
            if (isRect && ps.Length < 3) { continue; }
            if (isCir && ps.Length < 2) { continue; }
            if (!isRect && !isCir) { continue; }

            if (isRect)
            {
                float w, h, x = 0, y = 0;
                float.TryParse(ps[1], out w);
                if (w <= 0) { continue; }
                float.TryParse(ps[2], out h);
                if (h <= 0) { continue; }
                if (ps.Length >= 4) { float.TryParse(ps[3], out x); }
                if (ps.Length >= 5) { float.TryParse(ps[4], out y); }
                cl.Add(new Hitbox(w, h, x, y));
            }
            else if (isCir)
            {
                float r, x = 0, y = 0;
                float.TryParse(ps[1], out r);
                if (r <= 0) { continue; }
                if (ps.Length >= 3) { float.TryParse(ps[2], out x); }
                if (ps.Length >= 4) { float.TryParse(ps[3], out y); }
                cl.Add(new Circle(r, x, y));
            }
        }

        return cl;
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
                float.TryParse(p[2], out y);
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
            float w, h, x = 0, y = 0;
            float.TryParse(p[0], out w);
            float.TryParse(p[1], out h);
            if (p.Length >= 3)
            {
                float.TryParse(p[2], out x);
            }
            if (p.Length >= 4)
            {
                float.TryParse(p[2], out y);
            }

            if (w > 0 && h > 0) { rec = new Hitbox(w, h, x, y); }
        }

        return rec;
    }
}
