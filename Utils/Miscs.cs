using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.DotNet.Code.Cil;
using Celeste.Mod.MaxHelpingHand.Module;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using Microsoft.VisualBasic;
using YamlDotNet.Serialization;
using YoctoHelper.Cores;

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
            cameraSize.X = MaxHelpingHandModule.CameraWidth;
            cameraSize.Y = MaxHelpingHandModule.CameraHeight;
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
                if (ButtonReferences.ContainsKey(contents[i].ToLower()))
                {
                    VirtualButton selected = InputButtons[ButtonReferences[contents[i].ToLower()]];
                    //for(int j = 0; j < selected.Nodes.Count; j++)
                    //{
                    //    if (selected.Nodes[j] is VirtualButton.KeyboardKey)
                    //    {
                    //        string path = (selected.Nodes[j] as VirtualButton.KeyboardKey).Key.ToString();

                    //        texts.Add(GFX.Gui["controls/keyboard/" + path]);
                    //    }
                    //}
                    texts.Add(selected);
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

    public static Dictionary<string, int> ButtonReferences = new()
    {
        {"esc", 0 },
        {"pause", 1 },
        {"left", 2 },
        {"menuleft", 2 },
        {"menu_left", 2 },
        {"right", 3 },
        {"menuright", 3 },
        {"menu_right", 3 },
        {"up", 4 },
        {"menuup", 4 },
        {"menu_up", 4 },
        {"down", 5 },
        {"menu_down", 5 },
        {"menudown", 5 },
        {"confirm", 6 },
        {"menuconfirm", 6 },
        {"menu_confirm", 6 },
        {"journal", 7 },
        {"menujournal", 7 },
        {"menu_journal", 7 },
        {"restart", 8 },
        {"quickrestart", 8 },
        {"quick_restart", 8 },
        {"jump", 9 },
        {"dash", 10 },
        {"grab", 11 },
        {"talk", 12 },
        {"crouch", 13 },
        {"crouchdash", 13 },
        {"crouch_dash", 13 },
    };

    public static List<VirtualButton> InputButtons = new()
    {
        Input.ESC,
        Input.Pause,
        Input.MenuLeft,
        Input.MenuRight,
        Input.MenuUp,
        Input.MenuDown,
        Input.MenuConfirm,
        Input.MenuJournal,
        Input.QuickRestart,
        Input.Jump,
        Input.Dash,
        Input.Grab,
        Input.Talk,
        Input.CrouchDash,
    };
}
