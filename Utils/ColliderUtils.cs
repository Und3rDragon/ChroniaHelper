using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

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

    public static Collider ParseCollider(this string singleColliderData, out bool success, bool abandonSafeSetting = false)
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
            Vector2 p1 = new Vector2(ps[1].Parse(0f), ps[2].Parse(0f)),
                p2 = new Vector2(ps[3].Parse(0f), ps[4].Parse(0f)),
                p3 = new Vector2(ps[5].Parse(0f), ps[6].Parse(0f));
            HashSet<Vector2> points = new() { p1, p2, p3 };
            if (ps.Length > 7)
            {
                Vector2[] additional = new Vector2[ps.Length - 7 > 0 ? ps.Length - 7 : 1];
                for (int j = 0; j * 2 + 7 < ps.Length; j++)
                {
                    additional[j] = new Vector2(ps[j * 2 + 8].Parse(0f),
                        ps[j * 2 + 9 < ps.Length ? j * 2 + 9 : ps.Length - 1].Parse(0f));
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
            Vector2 p1 = new Vector2(ps[1].Parse(0f), ps[2].Parse(0f)),
                p2 = new Vector2(ps[3].Parse(0f), ps[4].Parse(0f)),
                p3 = new Vector2(ps[5].Parse(0f), ps[6].Parse(0f));
            HashSet<Vector2> points = new() { p1, p2, p3 };
            if (ps.Length > 7)
            {
                Vector2[] additional = new Vector2[ps.Length - 7 > 0 ? ps.Length - 7 : 1];
                for (int j = 0; j * 2 + 7 < ps.Length; j++)
                {
                    additional[j] = new Vector2(ps[j * 2 + 8].Parse(0f),
                        ps[j * 2 + 9 < ps.Length ? j * 2 + 9 : ps.Length - 1].Parse(0f));
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
                Triangulate(pc.Points, true, out pc.TriangulatedPoints, out pc.Indices);
            }
            return pc;
        }

        return safeSetting;
    }
    /// <summary>
    /// 对多边形进行三角化（耳剪裁算法）
    /// </summary>
    /// <param name="inputVertices">输入顶点数组（按顺序）</param>
    /// <param name="clockwiseWinding">是否要求顺时针输出</param>
    /// <param name="outputVertices">输出顶点数组</param>
    /// <param name="indices">三角形索引数组（每3个一组）</param>
    public static void Triangulate(
        Vector2[] inputVertices,
        bool clockwiseWinding,
        out Vector2[] outputVertices,
        out int[] indices)
    {
        // 1. 验证输入
        if (inputVertices == null || inputVertices.Length < 3)
        {
            outputVertices = Array.Empty<Vector2>();
            indices = Array.Empty<int>();
            return;
        }

        // 2. 确保顶点顺序符合要求
        bool isClockwise = GeometryUtils.IsClockwise(inputVertices);
        outputVertices = clockwiseWinding == isClockwise 
            ? (Vector2[])inputVertices.Clone() 
            : ReverseWinding(inputVertices);

        // 3. 构建双向链表节点列表
        List<GeometryUtils.Vertex> vertexList = new List<GeometryUtils.Vertex>();
        for (int i = 0; i < outputVertices.Length; i++)
        {
            vertexList.Add(new GeometryUtils.Vertex(outputVertices[i], i));
        }

        // 4. 构建链表连接关系
        for (int i = 0; i < vertexList.Count; i++)
        {
            int prev = (i - 1 + vertexList.Count) % vertexList.Count;
            int next = (i + 1) % vertexList.Count;
            // 使用 LinkedListNode 来管理关系
        }

        // 使用 LinkedList 来方便移除节点
        LinkedList<GeometryUtils.Vertex> polygon = new LinkedList<GeometryUtils.Vertex>(vertexList);

        // 5. 三角化
        List<GeometryUtils.Triangle> triangles = new List<GeometryUtils.Triangle>();
        TriangulatePolygon(polygon, triangles);

        // 6. 构建索引数组
        indices = new int[triangles.Count * 3];
        
        if (clockwiseWinding)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                indices[i * 3] = triangles[i].A.Index;
                indices[i * 3 + 1] = triangles[i].B.Index;
                indices[i * 3 + 2] = triangles[i].C.Index;
            }
        }
        else
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                indices[i * 3] = triangles[i].C.Index;
                indices[i * 3 + 1] = triangles[i].B.Index;
                indices[i * 3 + 2] = triangles[i].A.Index;
            }
        }
    }

    /// <summary>
    /// 耳剪裁算法主循环
    /// </summary>
    private static void TriangulatePolygon(LinkedList<GeometryUtils.Vertex> polygon, 
        List<GeometryUtils.Triangle> triangles)
    {
        if (polygon.Count < 3) return;

        // 如果正好是三角形，直接添加
        if (polygon.Count == 3)
        {
            var nodes = polygon.ToList();
            triangles.Add(new GeometryUtils.Triangle(nodes[0], nodes[1], nodes[2]));
            return;
        }

        int maxIterations = polygon.Count * polygon.Count;
        int iteration = 0;

        while (polygon.Count > 3 && iteration < maxIterations)
        {
            iteration++;
            bool earFound = false;

            var current = polygon.First;
            while (current != null)
            {
                var next = current.Next ?? polygon.First;
                var prev = current.Previous ?? polygon.Last;

                GeometryUtils.Vertex a = prev.Value;
                GeometryUtils.Vertex b = current.Value;
                GeometryUtils.Vertex c = next.Value;

                // 检查是否是凸顶点
                if (IsConvex(a.Position, b.Position, c.Position))
                {
                    // 检查三角形内是否包含其他顶点（使用已有的 ContainsPoint）
                    bool containsOther = false;
                    var checker = polygon.First;
                    while (checker != null)
                    {
                        if (!checker.Value.Equals(a) && !checker.Value.Equals(b) && !checker.Value.Equals(c))
                        {
                            // 使用 Triangle.ContainsPoint 检测
                            if (GeometryUtils.Triangle.ContainsPoint(a, b, c, checker.Value))
                            {
                                containsOther = true;
                                break;
                            }
                        }
                        checker = checker.Next;
                    }

                    if (!containsOther)
                    {
                        // 找到耳尖，添加三角形
                        triangles.Add(new GeometryUtils.Triangle(a, b, c));

                        // 移除耳尖
                        var toRemove = current;
                        current = current.Next;
                        polygon.Remove(toRemove);
                        earFound = true;
                        break;
                    }
                }

                current = current.Next;
            }

            // 如果没有找到耳尖，使用备用策略
            if (!earFound && polygon.Count > 3)
            {
                var nodes = polygon.ToList();
                // 取前三个顶点强制三角化
                if (nodes.Count >= 3)
                {
                    triangles.Add(new GeometryUtils.Triangle(nodes[0], nodes[1], nodes[2]));
                    polygon.Remove(nodes[1]);
                }
                else
                {
                    break;
                }
            }
        }

        // 处理剩余三角形
        if (polygon.Count == 3)
        {
            var nodes = polygon.ToList();
            triangles.Add(new GeometryUtils.Triangle(nodes[0], nodes[1], nodes[2]));
        }
    }

    /// <summary>
    /// 判断三个点是否构成凸顶点
    /// </summary>
    private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        float cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        // 对于逆时针多边形，cross > 0 表示凸顶点
        // 注意：这里假设多边形已经是逆时针方向
        return cross > 0.0001f;
    }

    /// <summary>
    /// 反转顶点顺序
    /// </summary>
    private static Vector2[] ReverseWinding(Vector2[] vertices)
    {
        Vector2[] reversed = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            reversed[i] = vertices[vertices.Length - 1 - i];
        }
        return reversed;
    }

    /// <summary>
    /// 将顶点列表转换为 Vector2 数组（用于调试）
    /// </summary>
    public static Vector2[] VerticesToVector2Array(List<GeometryUtils.Vertex> vertices)
    {
        Vector2[] result = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            result[i] = vertices[i].Position;
        }
        return result;
    }
    
    /// <summary>
    /// 判断两个 Collider 集合是否完全一致（包括顺序、类型、属性）。
    /// 支持 IEnumerable<Collider>，因此兼容 ColliderList, List<Collider>, Collider[] 等。
    /// </summary>
    public static bool CollidersEqual(this IEnumerable<Collider> a, IEnumerable<Collider> b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        var listA = a.ToList();
        var listB = b.ToList();

        if (listA.Count != listB.Count) return false;

        for (int i = 0; i < listA.Count; i++)
        {
            if (!CollidersEqual(listA[i], listB[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 判断两个 Collider 实例是否完全一致。
    /// </summary>
    private static bool CollidersEqual(this Collider c1, Collider c2)
    {
        if (c1 == null && c2 == null) return true;
        if (c1 == null || c2 == null) return false;

        // 类型必须一致
        if (c1.GetType() != c2.GetType()) return false;

        // 分类型比较
        switch (c1)
        {
            case Hitbox h1 when c2 is Hitbox h2:
                return h1.Position == h2.Position &&
                       h1.Width == h2.Width &&
                       h1.Height == h2.Height;

            case Circle c1Circle when c2 is Circle c2Circle:
                return c1Circle.Position == c2Circle.Position &&
                       c1Circle.Radius == c2Circle.Radius;

            // 可根据需要添加更多 Collider 类型（如 GridCollider, PerimeterCollider 等）
            default:
                // 对于未知或未实现的类型，回退到引用相等（或抛出异常）
                // 这里保守处理：仅当引用相同时认为相等
                return ReferenceEquals(c1, c2);
        }
    }

    /// <summary>
    /// 判断两个 Collider 集合是否在几何上覆盖完全相同的区域（支持 Hitbox 和 Circle）。
    /// 自动忽略面积为 0 的 Collider。
    /// 使用整数像素光栅化（每个 (x,y) 代表 [x,x+1) × [y,y+1) 的单位方格）。
    /// 当 threshold > 0 时，将每个集合的覆盖区域向外膨胀 threshold 像素（切比雪夫距离）。
    /// </summary>
    public static bool CollidersGeometricallyEquivalent(
        this IEnumerable<Collider> a,
        IEnumerable<Collider> b,
        int threshold = 0)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        try
        {
            var regionA = ExtractAndDilate(a, threshold);
            var regionB = ExtractAndDilate(b, threshold);
            return regionA.SetEquals(regionB);
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    /// 提取并膨胀区域。
    /// </summary>
    private static HashSet<(int x, int y)> ExtractAndDilate(IEnumerable<Collider> colliders, int threshold)
    {
        var baseRegion = ExtractCoveredPixels(colliders);
        return threshold <= 0 ? baseRegion : DilateRegion(baseRegion, threshold);
    }

    /// <summary>
    /// 对点集进行方形膨胀（切比雪夫距离 ≤ threshold）。
    /// </summary>
    private static HashSet<(int x, int y)> DilateRegion(HashSet<(int x, int y)> region, int threshold)
    {
        if (region.Count == 0) return new HashSet<(int, int)>();

        var dilated = new HashSet<(int x, int y)>();
        foreach (var (x, y) in region)
        {
            for (int dx = -threshold; dx <= threshold; dx++)
            {
                for (int dy = -threshold; dy <= threshold; dy++)
                {
                    dilated.Add((x + dx, y + dy));
                }
            }
        }
        return dilated;
    }

    /// <summary>
    /// 提取所有有效 Collider 覆盖的整数像素点集合。
    /// 忽略面积 <= 0 的 Collider。
    /// </summary>
    private static HashSet<(int x, int y)> ExtractCoveredPixels(this IEnumerable<Collider> colliders)
    {
        var pixels = new HashSet<(int x, int y)>();

        foreach (var c in colliders)
        {
            if (c == null) continue;

            switch (c)
            {
                case Hitbox h:
                    if (h.Width <= 0f || h.Height <= 0f) continue;

                    // 注意：Celeste 的 Hitbox.Position 是 Vector2
                    float left = Math.Min(h.Position.X, h.Position.X + h.Width);
                    float right = Math.Max(h.Position.X, h.Position.X + h.Width);
                    float top = Math.Min(h.Position.Y, h.Position.Y + h.Height);
                    float bottom = Math.Max(h.Position.Y, h.Position.Y + h.Height);

                    int minX = (int)Math.Floor(left);
                    int maxX = (int)Math.Ceiling(right);
                    int minY = (int)Math.Floor(top);
                    int maxY = (int)Math.Ceiling(bottom);

                    for (int x = minX; x < maxX; x++)
                        for (int y = minY; y < maxY; y++)
                            pixels.Add((x, y));

                    break;

                case Circle circ:
                    if (circ.Radius <= 0f) continue;

                    float cx = circ.Position.X;
                    float cy = circ.Position.Y;
                    float r = circ.Radius;
                    float rSq = r * r;

                    int boundMinX = (int)Math.Floor(cx - r);
                    int boundMaxX = (int)Math.Ceiling(cx + r);
                    int boundMinY = (int)Math.Floor(cy - r);
                    int boundMaxY = (int)Math.Ceiling(cy + r);

                    for (int x = boundMinX; x < boundMaxX; x++)
                    {
                        for (int y = boundMinY; y < boundMaxY; y++)
                        {
                            float dx = (x + 0.5f) - cx;
                            float dy = (y + 0.5f) - cy;
                            if (dx * dx + dy * dy <= rSq)
                            {
                                pixels.Add((x, y));
                            }
                        }
                    }

                    break;

                default:
                    throw new NotSupportedException(
                        $"Geometric equivalence only supports Hitbox and Circle. Found: {c.GetType()}");
            }
        }

        return pixels;
    }
}
