using System.Collections;
using System.Globalization;
using System.Reflection;
using Celeste.Mod.Helpers;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.References;

namespace ChroniaHelper.Utils;

// This class was moved from CommunalHelperModule, so let's keep the same namespace.
public static class Miscs
{

    public static int ToBitFlag(params bool[] b)
    {
        int ret = 0;
        for (int i = 0; i < b.Length; i++)
            ret |= BoolUtils.ToInt(b[i]) << i;
        return ret;
    }

    public static Vector2 RandomDir(float length)
    {
        return Calc.AngleToVector(Calc.Random.NextAngle(), length);
    }

    public static string StrTrim(string str)
    {
        return str.Trim();
    }

    public static Vector2 Min(Vector2 a, Vector2 b)
    {
        return new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }

    public static Vector2 Max(Vector2 a, Vector2 b)
    {
        return new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    public static Rectangle Rectangle(Vector2 a, Vector2 b)
    {
        Vector2 min = Min(a, b);
        Vector2 size = Max(a, b) - min;
        return new((int) min.X, (int) min.Y, (int) size.X, (int) size.Y);
    }

    /// <summary>
    /// Triangle wave function.
    /// </summary>
    public static float TriangleWave(float x)
    {
        return (2 * Math.Abs(NumberUtils.Mod(x, 2) - 1)) - 1;
    }

    /// <summary>
    /// Triangle wave between mapped between two values.
    /// </summary>
    /// <param name="x">The input value.</param>
    /// <param name="from">The ouput when <c>x</c> is an even integer.</param>
    /// <param name="to">The output when <c>x</c> is an odd integer.</param>
    public static float MappedTriangleWave(float x, float from, float to)
    {
        return ((from - to) * Math.Abs(NumberUtils.Mod(x, 2) - 1)) + to;
    }

    public static float PowerBounce(float x, float p)
    {
        return -(float) Math.Pow(Math.Abs(2 * (NumberUtils.Mod(x, 1) - .5f)), p) + 1;
    }

    public static bool Blink(float time, float duration)
    {
        return time % (duration * 2) < duration;
    }

    /// <summary>
    /// Checks if two line segments are intersecting.
    /// </summary>
    /// <param name="p0">The first end of the first line segment.</param>
    /// <param name="p1">The second end of the first line segment.</param>
    /// <param name="p2">The first end of the second line segment.</param>
    /// <param name="p3">The second end of the second line segment.</param>
    /// <returns>The result of the intersection check.</returns>
    public static bool SegmentIntersection(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float sax = p1.X - p0.X; float say = p1.Y - p0.Y;
        float sbx = p3.X - p2.X; float sby = p3.Y - p2.Y;

        float s = (-say * (p0.X - p2.X) + sax * (p0.Y - p2.Y)) / (-sbx * say + sax * sby);
        float t = (sbx * (p0.Y - p2.Y) - sby * (p0.X - p2.X)) / (-sbx * say + sax * sby);

        return s is >= 0 and <= 1
            && t is >= 0 and <= 1;
    }

    public static IEnumerator Interpolate(float duration, Action<float> action)
    {
        float t = duration;
        while (t > 0.0f)
        {
            action(1 - t / duration);
            t = Calc.Approach(t, 0.0f, Engine.DeltaTime);
            yield return null;
        }
        action(1.0f);
    }

    public static Rectangle GetBounds(this Camera camera)
    {
        int top = (int)camera.Top;
        int bottom = (int)camera.Bottom;
        int left = (int)camera.Left;
        int right = (int)camera.Right;

        return new(left, top, right - left, bottom - top);
    }

    public static float OZMTime(bool isTimeUnit, float param, bool isReturn)
    {
        if (!isTimeUnit)
        {
            return isReturn ? param * 0.5f * Engine.DeltaTime : param * 2f * Engine.DeltaTime;
        }

        if(param <= Engine.DeltaTime)
        {
            return 1f;
        }

        if(param > 1000000f)
        {
            return 0;
        }

        return Engine.DeltaTime / param;
    }

    public static void RenderProgressRectangle(Vector2 Position, float width, float height, float progress, Color color, float expansion = 0f, bool average = false)
    {
        expansion = expansion < 0 && expansion.GetAbs() >= Calc.Min(width, height) / 2f ? -Calc.Min(width, height) / 2f : expansion;
        float newWidth = width + 2 * expansion, newHeight = height + 2 * expansion;
        Vector2 p0 = Position + new Vector2(-expansion, -expansion),
            p1 = Position + new Vector2(width, 0f) + new Vector2(expansion, -expansion),
            p2 = Position + new Vector2(width, height) + new Vector2(expansion, expansion),
            p3 = Position + new Vector2(0f, height) + new Vector2(-expansion, expansion);

        if (average)
        {
            float C = 2 * width + 2 * height + 8 * expansion;
            float L = progress * C;

            if (L >= 0)
            {
                Draw.Line(p0, p0 + new Vector2(Calc.Min(L, newWidth), 0f), color);
            }
            if (L >= newWidth)
            {
                Draw.Line(p1, p1 + new Vector2(0f, Calc.Min(L - newWidth, newWidth + newHeight)), color);
            }
            if (L >= newWidth + newHeight)
            {
                Draw.Line(p2, p2 + new Vector2(-Calc.Min(L - newWidth - newHeight, 0f)), color);
            }
            if (L >= newWidth * 2 + newHeight)
            {
                Draw.Line(p3, p3 + new Vector2(0f, -Calc.Min(L - newWidth * 2 - newHeight)), color);
            }
        }
        else
        {
            Vector2 d1 = p1 - p0, d2 = p2 - p1, d3 = p3 - p2, d4 = p0 - p3;

            if (progress >= 0)
            {
                Draw.Line(p0, p0 + d1 * Calc.Min(progress, 0.25f) / 0.25f, color);
            }
            if (progress >= 0.25f)
            {
                Draw.Line(p1, p1 + d2 * Calc.Min(progress - 0.25f, 0.25f) / 0.25f, color);
            }
            if (progress >= 0.5f)
            {
                Draw.Line(p2, p2 + d3 * Calc.Min(progress - 0.5f, 0.25f) / 0.25f, color);
            }
            if (progress >= 0.75f)
            {
                Draw.Line(p3, p3 + d4 * Calc.Min(progress - 0.75f, 0.25f) / 0.25f, color);
            }
        }
    }
    
    public static bool TryGetSubTextures(this Atlas atlas, string path, out List<MTexture> texture)
    {
        texture = new();
        
        if (atlas.HasAtlasSubtextures(path))
        {
            texture = GFX.Game.GetAtlasSubtextures(path);
            return true;
        }

        return false;
    }

    public static List<MTexture> TryGetSubTextures(this Atlas atlas, string path)
    {
        if (atlas.HasAtlasSubtextures(path))
        {
            return atlas.GetAtlasSubtextures(path);
        }

        return new List<MTexture>();
    }

    public static bool InView(this Vc2 pos, float extension = 16f, Vc2? cameraPos = null)
    {
        extension = extension.GetAbs();
        
        Vc2 camera = cameraPos == null ? MaP.cameraPos : (Vc2)cameraPos;
        if (pos.X > camera.X - extension && pos.Y > camera.Y - extension && pos.X < camera.X + 320f + extension)
        {
            return pos.Y < camera.Y + 180f + extension;
        }

        return false;
    }

    public static bool InView(this Vc2 pos, Vc2 size, float extension = 16f)
    {
        Camera camera = MaP.level.Camera;
        Vc2 cameraSize = new Vc2(320f, 180f);
        if (Md.MaddieLoaded)
        {
            cameraSize.X = RefMaxHelpingHand.CameraWidth;
            cameraSize.Y = RefMaxHelpingHand.CameraHeight;
        }
        return pos.X + size.X > camera.X - 16f && pos.Y + size.Y > camera.Y - 16f && pos.X < camera.X + cameraSize.X && pos.Y < camera.Y + cameraSize.Y;
    }

    [Credits("VivHelper")]
    public static bool GridRectIntersection(Grid grid, Rectangle rect, out Grid ret, out Rectangle scope)
    {
        ret = null;
        scope = new Rectangle();
        if (!rect.Intersects(grid.Bounds))
            return false;
        int x = (int)(((float)rect.Left - grid.AbsoluteLeft) / grid.CellWidth);
        int y = (int)(((float)rect.Top - grid.AbsoluteTop) / grid.CellHeight);
        int width = (int)(((float)rect.Right - grid.AbsoluteLeft - 1f) / grid.CellWidth) - x + 1;
        int height = (int)(((float)rect.Bottom - grid.AbsoluteTop - 1f) / grid.CellHeight) - y + 1;
        if (x < 0)
        {
            width += x;
            x = 0;
        }
        if (y < 0)
        {
            height += y;
            y = 0;
        }
        if (x + width > grid.CellsX)
        {
            width = grid.CellsX - x;
        }
        if (y + height > grid.CellsY)
        {
            height = grid.CellsY - y;
        }
        bool[,] map = new bool[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = grid.Data[x + i, y + j];
            }
        }
        ret = new Grid(grid.CellWidth, grid.CellHeight, map);
        scope = new Rectangle((int)(x * grid.CellWidth + grid.AbsoluteLeft), (int)(y * grid.CellHeight + grid.AbsoluteTop), width, height);
        return true;
    }

    [Credits("VivHelper")]
    public static void AddOrAddToSolidModifierComponent(this Solid entity, SolidModifierComponent smc, out SolidModifierComponent smc2)
    {
        if (entity.Get<SolidModifierComponent>() == null)
        {
            smc2 = smc;
            entity.Add(smc);
            return;
        }
        SolidModifierComponent main = entity.Get<SolidModifierComponent>();
        main.bufferClimbJump |= smc.bufferClimbJump;
        main.triggerClimbOnTouch |= smc.triggerClimbOnTouch;
        // If A has default and B doesn't, prioritize B (A|B)
        // If A has a specific integer value (positive) and B has a behavior integer value (negative), prioritize the negative
        // If A and B have specific integer values (positive), choose the greater of the two
        // If A and B have behavior integer values (negative), behavior is A|B
        if (main.CornerBoostBlock == 0)
        {
            main.CornerBoostBlock = smc.CornerBoostBlock; // functionally 0|B === B
        }
        else if (smc.CornerBoostBlock != 0)
        {
            if (main.CornerBoostBlock < 0)
            { // if A is behavioral
                if (smc.CornerBoostBlock < 0)
                {
                    main.CornerBoostBlock = main.CornerBoostBlock | smc.CornerBoostBlock; // A | B
                } // else do nothing, because A is already prioritized over B
            }
            else
            { // if A is specific integer value (positive)
                if (smc.CornerBoostBlock > 0)
                { // if both A and B are specific integer values, choose the greater of the two
                    main.CornerBoostBlock = Math.Max(main.CornerBoostBlock, smc.CornerBoostBlock); // choose the greater leniency
                }
                else
                { // if A is specific integer value and B is behavior integer value, prioritize B
                    main.CornerBoostBlock = smc.CornerBoostBlock;
                }
            }
        }
        smc2 = main;
    }

    [Credits("VivHelper")]
    public static void AddOrAddToSolidModifierComponent(this Solid entity, SolidModifierComponent smc)
    {
        AddOrAddToSolidModifierComponent(entity, smc, out SolidModifierComponent _);
    }

    /// <summary>
    /// Create a bird tutorial GUI for a certain entity
    /// </summary>
    /// <param name="entity">The entity that it appears on</param>
    /// <param name="offsetX">Tutorial Display offset</param>
    /// <param name="offsetY">Tutorial Display offset</param>
    /// <param name="tutorialTitle">The Dialog ID of the tutorial title</param>
    /// <param name="tutorialText">The commands of the tutorial to display</param>
    /// <returns></returns>
    public static BirdTutorialGui CreateBirdGUI(this Entity entity, float offsetX = 0f, float offsetY = -24f, string tutorialTitle = "", string tutorialText = "")
    {
        string title = string.Empty;
        if (string.IsNullOrEmpty(tutorialTitle))
        {
            title = Dialog.Clean("tutorial_carry");
        }
        else
        {
            title = Dialog.Clean(tutorialTitle);
        }

        List<object> texts = new();
        if (string.IsNullOrEmpty(tutorialText))
        {
            texts.Add(Dialog.Clean("tutorial_hold"));
            texts.Add(Input.Grab);
        }
        else
        {
            string[] contents = tutorialText.Split(',', StringSplitOptions.TrimEntries);
            for (int i = 0; i < contents.Length; i++)
            {
                if (CustomBirdGUIAssets.ContainsKey(contents[i].ToLower()))
                {
                    texts.Add(CustomBirdGUIAssets[contents[i].ToLower()]);
                }
                else if (GFX.Gui.Has(contents[i]))
                {
                    texts.Add(GFX.Gui[contents[i]]);
                }
                else if (GFX.Game.Has(contents[i]))
                {
                    texts.Add(GFX.Game[contents[i]]);
                }
                else if (contents[i].Contains(';'))
                {
                    var o = contents[i].Split(';', StringSplitOptions.TrimEntries);
                    Vc2 v = Vc2.Zero;
                    if (o.Length >= 1)
                    {
                        float.TryParse(o[0], out v.X);
                    }
                    if (o.Length >= 2)
                    {
                        float.TryParse(o[1], out v.Y);
                    }

                    texts.Add(v);
                }
                else
                {
                    texts.Add(Dialog.Clean(contents[i]));
                }
            }
        }

        return new BirdTutorialGui(entity, new Vc2(offsetX, offsetY), title, texts.ToArray());
    }

    public static BirdTutorialGui CreateBirdGUI(this Entity entity, Vc2 offset, string tutorialTitle = "", string tutorialText = "")
    {
        return CreateBirdGUI(entity, offset.X, offset.Y, tutorialTitle, tutorialText);
    }

    public static Dictionary<string, object> CustomBirdGUIAssets = new()
    {
        {"esc", Input.ESC },
        {"pause", Input.Pause },
        {"left", -Vc2.UnitX },
        {"menuleft", -Vc2.UnitX },
        {"menu_left", -Vc2.UnitX },
        {"right", Vc2.UnitX },
        {"menuright", Vc2.UnitX },
        {"menu_right", Vc2.UnitX },
        {"up", -Vc2.UnitY },
        {"menuup", -Vc2.UnitY },
        {"menu_up", -Vc2.UnitY },
        {"down", Vc2.UnitY },
        {"menu_down", Vc2.UnitY },
        {"menudown", Vc2.UnitY },
        {"confirm", Input.MenuConfirm },
        {"menuconfirm", Input.MenuConfirm },
        {"menu_confirm", Input.MenuConfirm },
        {"journal", Input.MenuJournal },
        {"menujournal", Input.MenuJournal },
        {"menu_journal", Input.MenuJournal },
        {"restart", Input.QuickRestart },
        {"quickrestart", Input.QuickRestart },
        {"quick_restart", Input.QuickRestart },
        {"jump", Input.Jump },
        {"dash", Input.Dash },
        {"grab", Input.Grab },
        {"talk", Input.Talk },
        {"crouch", Input.CrouchDash },
        {"crouchdash", Input.CrouchDash },
        {"crouch_dash", Input.CrouchDash },
    };

    /// <summary>
    /// Reference from Kosei Helper, the function that process data and
    /// generate an entity based on the inputs
    /// </summary>
    /// <param name="spawnAt"></param>
    /// <param name="node"></param>
    /// <param name="data"></param>
    /// <param name="noNode"></param>
    /// <param name="asBlockWidth"></param>
    /// <param name="asBlockHeight"></param>
    /// <param name="fullEntityClassName"></param>
    /// <param name="entityDataKeys"></param>
    /// <param name="entityDataValues"></param>
    /// <param name="assignNewID"></param>
    /// <returns></returns>
    [Credits("KoseiHelper")]
    public static Entity CreateEntity(Vector2 spawnAt, Vector2 node, LevelData data, 
        bool noNode, int asBlockWidth, int asBlockHeight, string fullEntityClassName,
        List<string> entityDataKeys, List<string> entityDataValues,
        int assignNewID)
    {
        Log.Info($"Spawning entity at position: {spawnAt}");
        EntityData entityData;
        if (noNode)
        {
            entityData = new()
            {
                Position = spawnAt,
                Width = asBlockWidth,
                Height = asBlockHeight,
                Level = data,
                Values = new()
            };
        }
        else
        {
            entityData = new()
            {
                Position = spawnAt,
                Width = asBlockWidth,
                Height = asBlockHeight,
                Nodes = new Vector2[] { node },
                Level = data,
                Values = new()
            };
        }
        for (int i = 0; i < entityDataKeys.Count; i++)
        {
            entityData.Values[entityDataKeys[i]] = entityDataValues.ElementAtOrDefault(i);
        }
        EntityID newID = new EntityID(data.Name, assignNewID++);
        Type entityType;
        try
        {
            entityType = FakeAssembly.GetFakeEntryAssembly().GetType(fullEntityClassName); // This is where it gets the type, like Celeste.Strawberry
        }
        catch
        {
            Log.Error("Failed to get entity: Requested type does not exist");
            return null;
        }

        ConstructorInfo[] ctors = entityType.GetConstructors();
        try
        {
            foreach (ConstructorInfo ctor in ctors)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                List<object> ctorParams = new List<object>();

                foreach (ParameterInfo param in parameters)
                {
                    if (param.ParameterType == typeof(EntityData))
                    {
                        ctorParams.Add(entityData);
                    }
                    else if (param.ParameterType == typeof(Vector2))
                    { //Needs to check if the entity has just the position or position + offset
                        if (param.Name.Contains("position", StringComparison.OrdinalIgnoreCase))
                        {
                            ctorParams.Add(spawnAt);
                        }
                        else
                        {
                            Vector2 vec = Vector2.Zero;
                            if (entityData.Values.TryGetValue(param.Name, out object val) && val is string s)
                            {
                                s = s.Trim();
                                if ((s.StartsWith("[") && s.EndsWith("]")) || (s.StartsWith("(") && s.EndsWith(")")))
                                    s = s.Substring(1, s.Length - 2);

                                string[] parts = s.Split(',');
                                if (parts.Length == 2 &&
                                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                                {
                                    vec = new Vector2(x, y);
                                }
                            }
                            if (vec == Vector2.Zero)
                            {
                                float x = entityData.Float(param.Name + "X", 0f);
                                float y = entityData.Float(param.Name + "Y", 0f);
                                vec = new Vector2(x, y);
                            }

                            ctorParams.Add(vec);
                        }
                    }
                    else if (param.ParameterType.IsEnum)
                    {
                        if (entityData.Values.FirstOrDefault(kv => kv.Key == param.Name).Value is string enumValue)
                        {
                            Type enumType = param.ParameterType;
                            Object enumParsed = Enum.Parse(enumType, enumValue);
                            ctorParams.Add(enumParsed);
                        }
                        else
                        {
                            ctorParams.Add(Enum.GetValues(param.ParameterType).GetValue(0));
                        }
                    }
                    else if (param.ParameterType == typeof(bool))
                    {
                        ctorParams.Add(entityData.Bool(param.Name, defaultValue: false));
                    }
                    else if (param.ParameterType == typeof(int))
                    {
                        ctorParams.Add(entityData.Int(param.Name, defaultValue: 0));
                    }
                    else if (param.ParameterType == typeof(float))
                    {
                        ctorParams.Add(entityData.Float(param.Name, defaultValue: 0f));
                    }
                    else if (param.ParameterType == typeof(EntityID))
                    {
                        ctorParams.Add(new EntityID(data.Name, assignNewID++));
                    }
                    else if (param.ParameterType == typeof(string))
                    {
                        ctorParams.Add(entityData.Attr(param.Name, ""));
                    }
                    else if (param.ParameterType == typeof(Vector2[])) // hope the naming is correct but idk might work for a few entities
                    {
                        ctorParams.Add(entityData.Nodes != null && entityData.Nodes.Length > 0
                            ? new Vector2[] { entityData.Position }.Concat(entityData.Nodes).ToArray() : new Vector2[] { entityData.Position });
                    }
                    else if (param.ParameterType == typeof(Hitbox)) // hope the naming is correct but idk might work for a few entities
                    {
                        string prefix = param.Name;

                        float width = entityData.Float($"{prefix}Width", 16f);
                        float height = entityData.Float($"{prefix}Height", 16f);
                        float x = entityData.Float($"{prefix}X", 0f);
                        float y = entityData.Float($"{prefix}Y", 0f);
                        if (!entityData.Values.ContainsKey($"{prefix}X") && entityData.Values.ContainsKey($"{prefix}XOffset"))
                            x = entityData.Float($"{prefix}XOffset", 0f);

                        if (!entityData.Values.ContainsKey($"{prefix}Y") && entityData.Values.ContainsKey($"{prefix}YOffset"))
                            y = entityData.Float($"{prefix}YOffset", 0f);

                        ctorParams.Add(new Hitbox(width, height, x, y));
                    }
                    else
                    {
                        Log.Warn($"Unhandled parameter type: {param.ParameterType}");
                        ctorParams.Add(param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType) : null);
                    }
                }
                return (Entity)ctor.Invoke(ctorParams.ToArray());
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to instantiate entity: {ex.Message}");
        }
        return null;
    }

    [Credits("KoseiHelper")]
    public static Entity DuplicateEntity(this Entity original, Vector2 spawnAt, LevelData levelData)
    {
        Log.Info($"=== Duplicating: {original.GetType().Name} ===");
        Log.Info($"Spawning entity at position: {spawnAt}");

        EntityData entityData = original.SourceData;
        if (entityData == null)
        {
            //Log.Error("SourceData 为 null");
            return null;
        }

        EntityID newID = new EntityID(levelData.Name, ++MaP.totalMaxID);
        Type entityType = original.GetType();

        //Log.Info($"Successfully get type: {entityType}");

        ConstructorInfo[] ctors = entityType.GetConstructors();
        //Log.Info($"找到 {ctors.Length} 个构造函数:");
        foreach (var c in ctors)
        {
            var parameters = c.GetParameters();
            //Log.Info($"  - {string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}");
        }

        foreach (ConstructorInfo ctor in ctors)
        {
            ParameterInfo[] parameters = ctor.GetParameters();
            List<object> ctorParams = new List<object>();
            bool canConstruct = true;

            //Log.Info($"--- 尝试构造函数: {ctor.Name} ---");

            foreach (ParameterInfo param in parameters)
            {
                //Log.Info($"Parsing Parameter: {param.ParameterType} - {param.Name}");

                if (param.ParameterType == typeof(EntityData))
                {
                    ctorParams.Add(entityData);
                    //Log.Info($"  添加 EntityData");
                }
                else if (param.ParameterType == typeof(Vector2))
                {
                    if (param.Name.Contains("position", StringComparison.OrdinalIgnoreCase))
                    {
                        ctorParams.Add(spawnAt);
                        //Log.Info($"  → 添加 position: {spawnAt}");
                    }
                    else if (param.Name.Contains("offset", StringComparison.OrdinalIgnoreCase))
                    {
                        // 正确计算 offset：让 entityData.Position + offset = spawnAt
                        Vector2 offset = spawnAt - entityData.Position;
                        ctorParams.Add(offset);
                        //Log.Info($"  → 添加 offset: {offset} (spawnAt - entityData.Position)");
                    }
                    else
                    {
                        // 其他 Vector2 参数，尝试从 entityData 读取
                        Vector2 vec = Vector2.Zero;
                        // ... 你的原有逻辑
                        ctorParams.Add(vec);
                    }
                }
                else if (param.ParameterType == typeof(Player))
                {
                    Player player = PUt.player;
                    if (player != null)
                    {
                        ctorParams.Add(player);
                        //Log.Info($"  添加 Player: {player}");
                    }
                    else
                    {
                        //Log.Error($"  Player 为 null，跳过此构造函数");
                        canConstruct = false;
                        break;
                    }
                }
                else if (param.ParameterType.IsEnum)
                {
                    ctorParams.Add(Enum.GetValues(param.ParameterType).GetValue(0));
                    //Log.Info($"  添加 Enum 默认值");
                }
                else if (param.ParameterType == typeof(bool))
                {
                    ctorParams.Add(entityData.Bool(param.Name, false));
                    //Log.Info($"  添加 bool: {entityData.Bool(param.Name, false)}");
                }
                else if (param.ParameterType == typeof(int))
                {
                    ctorParams.Add(entityData.Int(param.Name, 0));
                    //Log.Info($"  添加 int: {entityData.Int(param.Name, 0)}");
                }
                else if (param.ParameterType == typeof(float))
                {
                    ctorParams.Add(entityData.Float(param.Name, 0f));
                    //Log.Info($"  添加 float: {entityData.Float(param.Name, 0f)}");
                }
                else if (param.ParameterType == typeof(EntityID))
                {
                    ctorParams.Add(newID);
                    //Log.Info($"  添加 EntityID");
                }
                else if (param.ParameterType == typeof(string))
                {
                    ctorParams.Add(entityData.Attr(param.Name, ""));
                    //Log.Info($"  添加 string: {entityData.Attr(param.Name, "")}");
                }
                else
                {
                    Log.Warn($"Unprocessed Type: {param.ParameterType}");
                    ctorParams.Add(param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType) : null);
                }
            }

            if (canConstruct)
            {
                try
                {
                    //Log.Info($"调用构造函数，参数: {string.Join(", ", ctorParams.Select(p => p?.GetType().Name ?? "null"))}");
                    Entity entity = (Entity)ctor.Invoke(ctorParams.ToArray());
                    Log.Info($"Entity duplicated successfully: {entity.GetType().Name}");
                    return entity;
                }
                catch (Exception ex)
                {
                    Log.Error($"ctor function error: {ex.Message}");
                    Log.Error($"stack trace: {ex.StackTrace}");
                    // 继续尝试下一个构造函数
                }
            }
        }

        Log.Error($"All ctor failed, return null");
        return null;
    }

}
