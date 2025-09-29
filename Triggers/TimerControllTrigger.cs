using Celeste.Mod.Entities;
using ChroniaHelper.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/TimerControlTrigger")]
public class TimerControlTrigger : Trigger
{
    private enum TimerControlType
    {
        None,
        Start,
        Pause,
        Unpause,
        Reset,
        Complete,
        Set,
        Add,
        Subtract
    }

    private enum FlagControlType
    {
        None,
        Set,
        Remove,
        SetIfValid
    }

        private enum ConditionType
    {
        None,
        Equel,
        NotEqual,
        Less,
        LessEqual,
        Greater,
        GreaterEqual
    }


    private Level level;
    private TimerControlType controlType;
    private FlagControlType flagControlType;
    private ConditionType conditionType;
    private long time;

    private string timeInput;
    private string recordID;
    private string flag;

    private bool recordTime, once;
    private bool useRecordAsInput;
    private List<TimerRecordStuff> timerRecordEntities;
    private EntityID id;


    public TimerControlTrigger(EntityData data, Vector2 offset, EntityID id)
       : base(data, offset)
    {
        controlType = data.Enum("controlType", TimerControlType.Start);
        flagControlType = data.Enum("flagType", FlagControlType.None);
        conditionType = data.Enum("conditionType", ConditionType.None);

        timeInput = data.Attr("timeInput");
        recordID = data.Attr("recordID");
        flag = data.Attr("flag");

        recordTime = data.Bool("recordTime");
        useRecordAsInput = data.Bool("useRecordAsInput");

        once = data.Bool("onlyOnce",false);
        this.id = id;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        time = ConvertStringToTicks(timeInput);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        var session = ChroniaHelperModule.Session;
        long usedTime;
        if (useRecordAsInput)
        {
            long record = session.TimeRecords.ContainsKey(recordID)
            ? session.TimeRecords[recordID] : throw new Exception($"{recordID} not exist in current time record!");
            usedTime = record;
        }
        else
        {
            usedTime = time;
        }

        switch (controlType)
        {
        case TimerControlType.None:
            break;

        case TimerControlType.Start:
            session.TimerStarted = true;
            break;

        case TimerControlType.Pause:
            session.TimerPauseed = true;
            //Console.WriteLine(session.TimerPauseed);
            break;

        case TimerControlType.Unpause:
            session.TimerPauseed = false;
            //Console.WriteLine(session.TimerPauseed);
            break;

        case TimerControlType.Reset:
            session.TimerStarted = false;
            session.TimerPauseed = false;
            session.TimerCompleted = false;
            session.RawTime = 0L;
            session.Time = 0L;
            var timers = Scene.Tracker.GetEntities<CustomTimer>();
            foreach (var timer in timers)
                if (timer is CustomTimer customTimer)
                    customTimer.ResetColor();
            break;

        case TimerControlType.Complete:
            session.TimerCompleted = true;
            break;

        case TimerControlType.Set:
            session.Time = usedTime;
            break;

        case TimerControlType.Add:
            session.Time += usedTime;
            break;

        case TimerControlType.Subtract:
            session.Time -= usedTime;
            break;

        default:
            break;
        }

        if (recordTime)
        {
            Md.Session.TimeRecords[recordID] = Md.Session.Time;
            var entities = Scene.Tracker.GetEntities<TimerRecordStuff>();
            foreach (var entity in entities)
                if (entity is TimerRecordStuff recordEntity)
                    recordEntity.LoadRecords();
        }

        bool condition = false;
        switch (conditionType)
        {
        case ConditionType.None:
            condition = true;
            break;
        case ConditionType.Equel:
            condition = session.Time == usedTime ? true : false;
            break;
        case ConditionType.NotEqual:
            condition = session.Time != usedTime ? true : false;
            break;
        case ConditionType.Less:
            condition = session.Time < usedTime ? true : false;
            break;
        case ConditionType.LessEqual:
            condition = session.Time <= usedTime ? true : false;
            break;
        case ConditionType.Greater:
            condition = session.Time > usedTime ? true : false;
            break;
        case ConditionType.GreaterEqual:
            condition = session.Time >= usedTime ? true : false;
            break;
        }

        if (flag == null || !condition)
            return;

        switch (flagControlType)
        {
        case FlagControlType.Set:
            level.Session.SetFlag(flag);
            break;

        case FlagControlType.Remove:
            level.Session.SetFlag(flag, false);
            break;

        case FlagControlType.None:
            break;

        case FlagControlType.SetIfValid:
            if(CheckIsValidRun())
                 level.Session.SetFlag(flag);
            break;

        default:
            break;
        }
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

    private long ConvertStringToTicks(string time)
    {
        TimeSpan timeSpan;
        if (TimeSpan.TryParseExact(time, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out timeSpan))
            return timeSpan.Ticks;
        else
            throw new FormatException("Invalid time string format. Expected format is 'hh:mm:ss.fff'.");
    }

    public override void OnLeave(Player player)
    {
        if (once)
        {
            RemoveSelf();
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
    }
}
