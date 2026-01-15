using System;
using System.Collections.Generic;
using System.Formats.Tar;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/CustomCoreMessage = Load0", "ChroniaHelper/CustomCoreMessage2 = Load1")]
public class ColoredCustomCoreMessage : Entity
{
    public enum PauseRenderTypes
    {
        Hidden = 0,
        Shown = 1,
        Fade = 2
    }
    public static Entity Load0(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new ColoredCustomCoreMessage(entityData, offset, 0);
    public static Entity Load1(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new ColoredCustomCoreMessage(entityData, offset, 1);

    private string text;
    public float alpha, defaultFadedValue;
    private bool outline, alwaysRender, lockPosition;
    private float RenderDistance, alphaMult;
    private Vector2 scale;
    private Ease.Easer EaseType;
    private ChroniaColor color, outlineColor;
    private Vector2[] nodes;
    private bool CustomPositionRange;
    private float MoveSpeed;
    //AlwaysHidden = 0, AlwaysShown = 1, Fade = 2 
    private PauseRenderTypes pausetype;

    private string dialog, se_line;
    private int line, fLine;
    private object se;

    private Level level;

    private bool wholeDialog, useSE, vanillaBehaviour;

    public string timerStatic, framesStatic;

    private float parallax, screenX, screenY;

    public ColoredCustomCoreMessage(EntityData data, Vector2 offset, int legacy)
        : base(data.Position + offset)
    {
        base.Tag = data.Bool("ShowInTransition", false) ? Tags.HUD | Tags.PauseUpdate | Tags.TransitionUpdate : Tags.HUD | Tags.PauseUpdate;

        this.dialog = data.Attr("dialog", "app_ending");
        int.TryParse(data.Attr("line", "0"), out line);
        fLine = line;

        useSE = data.Bool("detectSessionExpression", false);
        vanillaBehaviour = data.Bool("vanillaRenderDistanceBehaviour", true);

        // line process
        // FrostHelper SessionExpression support
        if (useSE && Md.FrostHelperLoaded)
        {
            se_line = data.Attr("line");
            se = se_line.TryCreateSessionExpression();
        }

        this.wholeDialog = data.Bool("wholeDialog", false);

        parallax = data.Float("parallax", 1.2f);
        screenX = data.Float("screenPosX", 160f);
        screenY = data.Float("screenPosY", 90f);

        TextProcess();

        outlineColor = new(data.Attr("OutlineColor", "000000"));
        outline = data.Has("outline") ? data.Bool("outline") : outlineColor.Parsed() != Color.Transparent;
        pausetype = data.Enum<PauseRenderTypes>("PauseType", PauseRenderTypes.Hidden);
        scale = Vector2.One * data.Float("Scale", 1.25f);
        RenderDistance = data.Float("RenderDistance", 128f);
        EaseType = EaseUtils.EaseMatch[data.Enum<EaseMode>("EaseType", EaseMode.CubeInOut)];
        color = new(data.Attr("TextColor1", "ffffff"));
        alwaysRender = data.Bool("AlwaysRender");

        lockPosition = data.Bool("LockPosition", false);
        nodes = data.NodesOffset(offset);

        switch (legacy)
        {
            case 0:
                CustomPositionRange = false;
                break;
            case 1:
                CustomPositionRange = nodes.Length % 2 == 0 && nodes.Length > 1;
                break;
        }

        defaultFadedValue = data.Float("DefaultFadedValue", 0f);
        alphaMult = data.Float("AlphaMultiplier", 1f).Clamp(0f, 1f);

        // align
        align = (AlignUtils.Aligns)data.Fetch("align", 5);
    }
    AlignUtils.Aligns align;

    public void TextProcess()
    {
        var t1 = this.dialog;
        var b = false;

        if (dialog.StartsWith('"') && dialog.EndsWith('"'))
        {
            text = dialog.Replace('"', ' ').Trim();
            return;
        }

        if (dialog.StartsWith('#'))
        {
            text = Md.Session.sessionKeys.GetValueOrDefault(dialog.TrimStart('#'), "");
            return;
        }

        if (this.wholeDialog)
        {
            text = Dialog.Clean(t1);
        }
        else
        {
            if (t1.StartsWith("*§")) { text = t1.Substring(2); b = true; } else text = Dialog.Clean(t1);
            if (text.Contains("\n") || text.Contains("\r"))
            {
                var t2 = text.Split(new char[2]
                {
                '\n',
                '\r'
                }, StringSplitOptions.RemoveEmptyEntries);
                if (t2.Length > 0)
                    text = t2[this.line];
                else if (!b)
                    text = "{" + t1 + "}";
            }
        }

    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
        timerStatic = Md.Session.timer;
        framesStatic = Md.Session.timerFrames.ToString();
    }

    public override void Update()
    {
        if (level.Session.GetFlag("ChroniaHelperTimer.reset"))
        {
            TimerReset();
            level.Session.SetFlag("ChroniaHelperTimer.reset", false);
        }
        Timer();

        Player entity = base.Scene.Tracker.GetEntity<Player>();
        if (base.Scene.Paused)
        {
            switch (pausetype)
            {
                case PauseRenderTypes.Hidden:
                    alpha = 0f;
                    break;
                case PauseRenderTypes.Shown:
                    break;
                case PauseRenderTypes.Fade:
                    alpha = Calc.Approach(alpha, 0f, 0.05f);
                    break;
            }
        }
        else
        {
            float q;
            if (alwaysRender) { q = alphaMult; }
            else if (!CustomPositionRange)
            {
                if (entity != null)
                {
                    float dx = Math.Abs(base.X - entity.X);
                    float dy = Math.Abs(base.Y - entity.Y);
                    q = alphaMult * (defaultFadedValue + (1 - defaultFadedValue) * EaseType(Calc.ClampedMap(vanillaBehaviour ? dx : new Vector2(dx, dy).Length(), 
                        0f, RenderDistance, 1f, 0f)));
                }
                else { q = alpha; }
            }
            else
            {
                List<float> f = new List<float>();
                float dx = Math.Abs(base.X - entity.X);
                float dy = Math.Abs(base.Y - entity.Y);
                f.Add(Calc.ClampedMap(vanillaBehaviour ? dx : new Vector2(dx, dy).Length(), 
                    0f, RenderDistance, 1f, 0f));
                for (int i = 0; i < nodes.Length; i += 2)
                {
                    Vector2 v = Vector2.Lerp(nodes[i], nodes[i + 1], 0.5f);
                    f.Add(Calc.ClampedMap(Math.Abs(v.X - entity.X), 0f, nodes[i + 1].X - v.X, 1f, 0f));
                }
                q = alphaMult * (defaultFadedValue + (1 - defaultFadedValue) * EaseType(Calc.Max(f.ToArray())));
            }
            if (pausetype == PauseRenderTypes.Fade)
            {
                alpha = Calc.Approach(alpha, q, 0.05f);
            }
            else
            {
                alpha = q;
            }
        }
        base.Update();
    }

    public override void Render()
    {
        if (useSE && Md.FrostHelperLoaded)
        {
            fLine = se.GetIntSessionExpressionValue();
            if (fLine != line)
            {
                line = fLine;
                TextProcess();
            }
        }

        if (dialog == "ChroniaHelperTimerStatic")
        {
            text = timerStatic;
        }
        if (dialog == "ChroniaHelperTimer")
        {
            text = Md.Session.timer;
        }
        if (dialog == "ChroniaHelperFrames")
        {
            text = Md.Session.timerFrames.ToString();
        }
        if (dialog == "ChroniaHelperFramesStatic")
        {
            text = framesStatic;
        }
        if (dialog.StartsWith("ChroniaHelperRealTime_"))
        {
            // example: ChroniaHelperRealTime_password
            // in-game: set flag "password_aaa" true
            // text: "aaa"
            string tag = dialog.Remove(0, "ChroniaHelperRealTime_".Length);
            foreach (var item in level.Session.Flags)
            {
                if (item.StartsWith($"{tag}_"))
                {
                    text = item.Remove(0, $"{tag}_".Length);
                    break;
                }
                text = "NO MATCHING CODE";
            }
        }
        if (dialog.StartsWith("keyboardSync_"))
        {
            string tag = dialog.Remove(0, "keyboardSync_".Length);
            bool valid = Md.Session.Passwords.ContainsKey(tag);
            text = valid ? Md.Session.Passwords[tag] : string.Empty;
        }

        Vector2 position = ((Level)base.Scene).Camera.Position;
        Vector2 value = position + new Vector2(screenX, screenY);
        Vector2 position2 = lockPosition ? (Position - position) * 6f : (Position - position + (Position - value) * (parallax - 1f)) * 6f; // parallax
        if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
        {
            position2.X = 1920f - position2.X;
        }
        if (outline)
        {
            ActiveFont.DrawOutline(text, position2, AlignUtils.AlignToJustify[align], scale, color.color * alpha, 2f, outlineColor.color * alpha);
        }
        else
        {
            ActiveFont.Draw(text, position2, AlignUtils.AlignToJustify[align], scale, color.color * alpha);
        }
    }

    private void Timer()
    {
        if (!level.Session.GetFlag("ChroniaHelperTimer.pause"))
        {
            Md.Session.timerA++;
            Md.Session.timerFrames++;
        }

        if (Md.Session.timerA >= 1 / Engine.DeltaTime)
        {
            Md.Session.timerA = 0;
            Md.Session.timerB++;
        }
        if (Md.Session.timerB >= 60)
        {
            Md.Session.timerB = 0;
            Md.Session.timerC++;
        }
        if (Md.Session.timerC >= 60)
        {
            Md.Session.timerC = 0;
            Md.Session.timerD++;
        }
        Md.Session.timer = $"{Md.Session.timerD}:{Md.Session.timerC}:{Md.Session.timerB}:{Md.Session.timerA}";
    }

    private void TimerReset()
    {
        Md.Session.timerA = 0;
        Md.Session.timerB = 0;
        Md.Session.timerC = 0;
        Md.Session.timerD = 0;
        Md.Session.timerFrames = 0;
    }
}
