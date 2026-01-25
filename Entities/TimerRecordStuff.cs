using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/TimerRecordStuff")]
public class TimerRecordStuff : Entity
{
    private long recordTIme;

    private string text;
    private string recordID;
    private Color recordColor;
    private bool showMilliseconds;
    private bool showUnits;
    public TimerRecordStuff(Vector2 position)
        : base(position)
    {
        Collider = new Hitbox(16f, 16f);
        Tag = TagsExt.SubHUD | Tags.PauseUpdate | Tags.TransitionUpdate;
    }

    public TimerRecordStuff(EntityData data, Vector2 offset)
        : this(data.Position + offset)
    {
        text = data.Attr("text");
        recordID = data.Attr("recordID");
        recordColor = Calc.HexToColor(data.Attr("recordColor"));
        showMilliseconds = data.Bool("showMilliseconds");
        showUnits = data.Bool("showUnits");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        LoadRecords();
    }

    public override void Render()
    {
        base.Render();

        Camera camera = SceneAs<Level>().Camera;
        Vector2 position = new Vector2((Position.X - camera.X) * 6f, (Position.Y - camera.Y) * 6f);
        Vector2 vector = position + new Vector2(Width / 2f * 6f, Height * 6f - 12f);
        Vector2 vector2 = position + new Vector2(Width / 2f * 6f, 12f);

        string time = Md.Session.CustomTimer_TimeRecords.ContainsKey(recordID) ? ConvertTimeToString(TimeSpan.FromTicks(recordTIme), showMilliseconds, showUnits) : "--:--";

        //DrawTimer
        ActiveFont.DrawOutline(time, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, Color.Black, 1.5f, Color.Black);
        ActiveFont.Draw(time, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, recordColor);

        //DrawText
        ActiveFont.DrawOutline(text, vector2, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black, 1.5f, Color.Black);
        ActiveFont.Draw(text, vector2, new Vector2(0.5f, 0.5f), Vector2.One, recordColor);
    }

    public void LoadRecords()
    {
        long time;
        Md.Session.CustomTimer_TimeRecords.TryGetValue(recordID, out time);
        if (time != default)
            recordTIme = time;
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
}
