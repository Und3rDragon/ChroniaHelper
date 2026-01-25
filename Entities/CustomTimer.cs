using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.StopwatchSystem;
using YamlDotNet.Core.Tokens;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/CustomTimer")]
public class CustomTimer : Entity
{
    private Level level;
    private bool isValidRun;

    private string text;
    private string completeFlag;
    private long timeLimit;
    private bool useRawTime;
    private bool checkTimeLimit;
    private bool checkIsValidRun;
    private bool showMilliseconds;
    private bool showUnits;

    private Color overTimeLimitColor;
    private Color underTimeLimitColor;
    private Color invaildColor;
    private Color timerColor;

    public CustomTimer(Vector2 position, Vector2 size)
        : base(position)
    {
        Collider = new Hitbox(16f, 16f);
        Tag = TagsExt.SubHUD | Tags.PauseUpdate | Tags.TransitionUpdate;
    }
    
    public CustomTimer(EntityData data, Vector2 offset)
        : this(data.Position + offset, new Vector2(data.Width, data.Height))
    {
        text = data.Attr("text");
        completeFlag = data.Attr("completeFlag");
        timeLimit = ConvertStringToTicks(data.Attr("timeLimit"));

        overTimeLimitColor = Calc.HexToColor(data.Attr("overTimeLimitColor"));
        underTimeLimitColor = Calc.HexToColor(data.Attr("underTimeLimitColor"));
        invaildColor = Calc.HexToColor(data.Attr("invaildColor"));
        timerColor = Calc.HexToColor(data.Attr("defaultTextColor","ffffff"));

        useRawTime = data.Bool("useRawTime");
        checkTimeLimit = data.Bool("checkTimeLimit");
        checkIsValidRun = data.Bool("checkIsValidRun");
        showMilliseconds = data.Bool("showMilliseconds");
        showUnits = data.Bool("showUnits");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        isValidRun = CheckIsValidRun();
        if (checkIsValidRun && !isValidRun)
        {
            timerColor = invaildColor;
            return;
        }

        if (checkTimeLimit && IsUnderTimeLimit())
        {
            timerColor = underTimeLimitColor;
            return;
        }

        if (checkTimeLimit && !IsUnderTimeLimit())
        {
            timerColor = overTimeLimitColor;
            return;
        }
    }

    public override void Update()
    {
        if (!Md.Session.CustomTimer_TimerStarted || Md.Session.CustomTimer_TimerCompleted)
            return;

        if (checkIsValidRun && !isValidRun)
        {
            timerColor = invaildColor;
            return;
        }

        if (checkTimeLimit && IsUnderTimeLimit())
        {
            timerColor = underTimeLimitColor;
            return;
        }

        if (checkTimeLimit && !IsUnderTimeLimit())
        {
            timerColor = overTimeLimitColor;
            return;
        }
        base.Update();
    }

    public override void Render()
    {
        Camera camera = SceneAs<Level>().Camera;
        Vector2 position = new Vector2((Position.X - camera.X) * 6f, (Position.Y - camera.Y) * 6f);
        Vector2 vector = position + new Vector2(Width / 2f * 6f, Height * 6f - 12f);
        Vector2 vector2 = position + new Vector2(Width / 2f * 6f, 12f);

        TimeSpan timeSpan = useRawTime ? TimeSpan.FromTicks(Md.Session.CustomTimer_RawTime) : TimeSpan.FromTicks(Md.Session.CustomTimer_Time);
        string time = ConvertTimeToString(timeSpan, showMilliseconds, showUnits);

        //DrawTimer
        ActiveFont.DrawOutline(time, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, Color.Black, 1.5f, Color.Black);
        ActiveFont.Draw(time, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, timerColor);

        //DrawText
        ActiveFont.DrawOutline(text, vector2, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black, 1.5f, Color.Black);
        ActiveFont.Draw(text, vector2, new Vector2(0.5f, 0.5f), Vector2.One, timerColor);

        base.Render();
    }
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.UpdateTime += Level_UpdateTime;
    }
    [UnloadHook]
    public static void UnLoad()
    {
        On.Celeste.Level.UpdateTime -= Level_UpdateTime;
    }

    private static void Level_UpdateTime(On.Celeste.Level.orig_UpdateTime orig, Level self)
    {
        if (Md.Session.CustomTimer_TimerCompleted || !Md.Session.CustomTimer_TimerStarted)
        {
            orig(self);
            return;
        }

        if (self.InCredits || self.Session.Area.ID == 8 || Md.Session.CustomTimer_TimerPaused)
        {
            orig(self);
            return;
        }

        orig(self);

        if (!self.Completed && Md.Session.CustomTimer_TimerStarted)
        {
            long ticks = TimeUtils.CalculateInterval(Engine.RawDeltaTime, 1000).Ticks;
            Md.Session.CustomTimer_RawTime += ticks;
            Md.Session.CustomTimer_Time += ticks;
        }
    }

    public void ResetColor()
    {
        timerColor = underTimeLimitColor;
    }

    private string ConvertTimeToString(TimeSpan timeSpan, bool showMilliseconds = false, bool showUnits = false)
    {
        if (!showUnits)
            return timeSpan.ToString(timeSpan.Hours >= 1 ? @"hh\:mm\:ss" + (showMilliseconds ? @"\.fff" : "") : @"mm\:ss" + (showMilliseconds ? @"\.fff" : ""));

        int hours = (int)timeSpan.TotalHours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;
        int milliseconds = timeSpan.Milliseconds;

        string formattedTime = timeSpan.Hours >= 1 ? $"{hours:D2}h{minutes:D2}min{seconds:D2}s" : $"{minutes:D2}min{seconds:D2}s";

        if (showMilliseconds)
            formattedTime += $"{milliseconds:D3}ms";

        return formattedTime;
    }

    private long ConvertStringToTicks(string time)
    {
        TimeSpan timeSpan;
        if (TimeSpan.TryParseExact(time, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out timeSpan))
            return timeSpan.Ticks;
        else
            throw new FormatException("Invalid time string format. Expected format is 'hh:mm:ss.fff'.");
    }

    private bool IsUnderTimeLimit()
    {
        Level obj = level;
        return obj != null && Md.Session.CustomTimer_Time < timeLimit;
    }

    private bool CheckIsValidRun()
    {
        if (level == null)
            return false;

        return level.Session.StartedFromBeginning || level.Session.RestartedFromGolden || PlayerHasGolden();
    }

    private bool PlayerHasGolden()
    {
        Player entity = level.Tracker.GetEntity<Player>();
        if (entity == null)
            return false;

        foreach (Follower follower in entity.Leader.Followers)
        {
            if (follower.Entity is Strawberry && (follower.Entity as Strawberry).Golden && !(follower.Entity as Strawberry).Winged)
                return true;
        }
        return false;
    }
}
