using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Utils.StopwatchSystem;

public partial class Stopclock : IDisposable
{
    // 线程计数和限制
    private static int activeIsolatedClocks = 0;
    private static int maxIsolatedClocks = 5;
    
    public static void Load()
    {
        On.Celeste.Level.UpdateTime += LevelUpdateTime;
        On.Celeste.Level.LoadLevel += LoadingLevel;
        On.Monocle.Scene.Update += GlobalUpdate;
    }

    public static void Unload()
    {
        On.Celeste.Level.UpdateTime -= LevelUpdateTime;
        On.Celeste.Level.LoadLevel -= LoadingLevel;
        On.Monocle.Scene.Update -= GlobalUpdate;
    }

    public static void LoadingLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool loader)
    {
        orig(self, intro, loader);
    }

    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
    {
        orig(self);

        if (Md.SaveData.IsNull()) { return; }

        HashSet<string> toRemove = new();
        foreach (var watches in Md.SaveData.globalStopwatches)
        {
            // If not global, migrate the timer to session timers
            if (!watches.Value.global)
            {
                toRemove.Add(watches.Key);
                watches.Value.Register(watches.Key);
                continue;
            }

            if (!watches.Value.isolatedUpdate)
            {
                if(!watches.Value.followPause || !self.Paused)
                {
                    watches.Value.UpdateTime(TimeUtils.deltaTicks);
                }
            }

            if (watches.Value.completed && watches.Value.removeWhenCompleted) 
            {
                if (!watches.Value.removeRequireSignalUsed)
                {
                    toRemove.Add(watches.Key);
                }
                else
                {
                    if (!watches.Value.HasValidSignal) { toRemove.Add(watches.Key); }
                }
            }

            //Log.Info(watches.Key, watches.Value.FormattedTime);
        }
        foreach (var item in toRemove)
        {
            Md.SaveData.globalStopwatches.SafeRemove(item);
        }
    }

    public static void LevelUpdateTime(On.Celeste.Level.orig_UpdateTime orig, Level self)
    {
        orig(self);

        if (Md.Session.IsNull()) { return; }

        if (!self.Completed)
        {
            HashSet<string> toRemove = new();
            foreach (var watches in Md.Session.sessionStopwatches)
            {
                if (watches.Value.global)
                {
                    toRemove.Add(watches.Key);
                    watches.Value.Register(watches.Key);
                    continue;
                }
                
                if (Md.SaveData.globalStopwatches.ContainsKey(watches.Key))
                {
                    toRemove.Add(watches.Key);
                    watches.Value.Stop();
                    continue;
                }

                if (!watches.Value.isolatedUpdate)
                {
                    if (!watches.Value.followPause || !self.Paused)
                    {
                        watches.Value.UpdateTime(TimeUtils.deltaTicks);
                    }
                }

                if (watches.Value.completed && watches.Value.removeWhenCompleted)
                {
                    if (!watches.Value.removeRequireSignalUsed)
                    {
                        toRemove.Add(watches.Key);
                    }
                    else
                    {
                        if (!watches.Value.HasValidSignal) { toRemove.Add(watches.Key); }
                    }
                }

                //Log.Info(watches.Key, watches.Value.FormattedTime);
            }
            foreach (var item in toRemove)
            {
                Md.Session.sessionStopwatches.SafeRemove(item);
            }
        }
    }
    
    public int year = 0;
    public int month = 0;
    public int day = 0;
    public int hour = 0;
    public int minute = 0;
    public int second = 0;
    public int millisecond = 0;

    public bool countdown = false;
    public bool global { get; private set; } = false;
    public bool completed { get; private set; } = false;
    public bool removeWhenCompleted = true;
    public bool removeRequireSignalUsed = true;
    public bool running { get; private set; } = false;
    public bool isolatedUpdate = false;
    public bool registered { get; private set; } = false;
    /// <summary>
    /// 是否随游戏内暂停而暂停
    /// </summary>
    public bool followPause = false;
    
    public int initialYear = 0;
    public int initialMonth = 0;
    public int initialDay = 0;
    public int initialHour = 0;
    public int initialMinute = 5;
    public int initialSecond = 0;
    public int initialMillisecond = 0;

    // 时间累积
    private long _accumulatedTicks;
    private DateTime _lastUpdateTime; // 用于独立更新的时间记录
    private Timer _updateTimer = null;
    private bool _disposed = false;
    // 自动更新间隔（毫秒）
    public int IsolateUpdateInterval { get; set; } = 50;
    
    /// <summary>
    /// 注意: 该操作不会被保存到记录中
    /// </summary>
    [YamlIgnore]
    public Action onStart;
    /// <summary>
    /// 注意: 该操作不会被保存到记录中
    /// </summary>
    [YamlIgnore]
    public Action onStop;
    /// <summary>
    /// 注意: 该操作不会被保存到记录中
    /// </summary>
    [YamlIgnore]
    public Action onReset;
    /// <summary>
    /// 注意: 该操作不会被保存到记录中
    /// </summary>
    [YamlIgnore]
    public Action onComplete;
    public virtual void OnStart() { }
    public virtual void OnStop() { }
    public virtual void OnReset() { }
    public virtual void OnComplete() { }
    
    // 信号系统

    /// <summary>
    /// 信号状态, 使用信号请调用FetchSignal()
    /// </summary>
    public int signal { get; private set; } = 0;
    public bool HasValidSignal => signal > 0;
    public bool FetchSignal()
    {
        bool state = signal > 0;
        if (state) { signal = -1; }
        return state;
    }
    public bool FetchSignal(int specificSignal)
    {
        bool state = signal == specificSignal;
        if (state) { signal = -1; }
        return state;
    }
    public bool FetchSignal(Signal specificSignal)
    {
        bool state = signal == (int)specificSignal;
        if (state) { signal = -1; }
        return state;
    }
    public enum Signal 
    { 
        Used = -1,
        Initial = 0, 
        Reset = 1, 
        Start = 2, 
        Stop = 3, 
        Complete = 4,
    }
    
    /// <summary>
    /// 如果是倒计时, 倒计时默认事件为5分钟
    /// </summary>
    public Stopclock()
    {
        countdown = false;
        global = false;
        followPause = false;
        removeWhenCompleted = true;
        isolatedUpdate = false;
        _lastUpdateTime = DateTime.Now;

        registered = false;
        
        Initialize();
    }

    /// <summary>
    /// 如果是倒计时, 倒计时默认事件为5分钟, 设置时注意清零
    /// </summary>
    public Stopclock(bool countdown, int year = 0, int month = 0, int day = 0, int hour = 0,
        int minute = 0, int second = 0, int millisecond = 0, bool global = false, bool followPause = false,
        int initialYear = 0, int initialMonth = 0, int initialDay = 0, int initialHour = 0, int initialMinute = 5,
        int initialSecond = 0, int initialMillisecond = 0, bool removeWhenCompleted = true, bool isolatedUpdate = false,
        bool removeRequireSignalUsed = true)
    {
        this.year = year;
        this.month = month;
        this.day = day;
        this.hour = hour;
        this.minute = minute;
        this.second = second;
        this.millisecond = millisecond;
        this.countdown = countdown;
        this.global = global;
        this.followPause = followPause;
        this.initialYear = initialYear;
        this.initialMonth = initialMonth;
        this.initialDay = initialDay;
        this.initialHour = initialHour;
        this.initialMinute = initialMinute;
        this.initialSecond = initialSecond;
        this.initialMillisecond = initialMillisecond;
        this.removeWhenCompleted = removeWhenCompleted;
        this.isolatedUpdate = isolatedUpdate;
        this.removeRequireSignalUsed = removeRequireSignalUsed;
        _lastUpdateTime = DateTime.Now;

        registered = false;

        Initialize();
    }

    /// <summary>
    /// 创建时自带Register, 如果是倒计时, 倒计时默认事件为5分钟, 设置时注意清零
    /// </summary>
    public Stopclock(string regName, bool countdown = false, int year = 0, int month = 0, int day = 0, int hour = 0,
        int minute = 0, int second = 0, int millisecond = 0, bool global = false, bool followPause = false,
        int initialYear = 0, int initialMonth = 0, int initialDay = 0, int initialHour = 0, int initialMinute = 5,
        int initialSecond = 0, int initialMillisecond = 0, bool removeWhenCompleted = true, bool isolatedUpdate = false,
        bool removeRequireSignalUsed = true)
    {
        this.year = year;
        this.month = month;
        this.day = day;
        this.hour = hour;
        this.minute = minute;
        this.second = second;
        this.millisecond = millisecond;
        this.countdown = countdown;
        this.global = global;
        this.followPause = followPause;
        this.initialYear = initialYear;
        this.initialMonth = initialMonth;
        this.initialDay = initialDay;
        this.initialHour = initialHour;
        this.initialMinute = initialMinute;
        this.initialSecond = initialSecond;
        this.initialMillisecond = initialMillisecond;
        this.removeWhenCompleted = removeWhenCompleted;
        this.isolatedUpdate = isolatedUpdate;
        this.removeRequireSignalUsed = removeRequireSignalUsed;
        _lastUpdateTime = DateTime.Now;

        registered = false;

        Register(regName);

        Initialize();
    }

    /// <summary>
    /// 用xxx:xxx:xxx的字符串创建时间
    /// </summary>
    /// <param name="countdown"></param>
    /// <param name="time"></param>
    /// <param name="global"></param>
    /// <param name="followPause"></param>
    /// <param name="removeWhenCompleted"></param>
    /// <param name="isolatedUpdate"></param>
    public Stopclock(bool countdown, string time, bool global = false, bool followPause = false,
        bool removeWhenCompleted = true, bool isolatedUpdate = false, bool removeRequireSignalUsed = true)
    {
        time.Split(':', StringSplitOptions.TrimEntries).ApplyTo(out string[] t);

        for (int i = 0, n = 0; i < t.Length; i++)
        {
            n = t.Length - 1 - i;
            t[n].ParseInt(out int m);
            m = m.ClampMin(0);

            switch (i)
            {
                case 0:
                    m.AssignTo(countdown, out initialMillisecond, out millisecond);
                    break;
                case 1:
                    m.AssignTo(countdown, out initialSecond, out second);
                    break;
                case 2:
                    m.AssignTo(countdown, out initialMinute, out minute);
                    break;
                case 3:
                    m.AssignTo(countdown, out initialHour, out hour);
                    break;
                case 4:
                    m.AssignTo(countdown, out initialDay, out day);
                    break;
                case 5:
                    m.AssignTo(countdown, out initialMonth, out month);
                    break;
                case 6:
                    m.AssignTo(countdown, out initialYear, out year);
                    break;
                default:
                    m.AssignTo(countdown, out initialMillisecond, out millisecond);
                    break;
            }
        }
        
        this.countdown = countdown;
        this.global = global;
        this.followPause = followPause;
        this.removeWhenCompleted = removeWhenCompleted;
        this.isolatedUpdate = isolatedUpdate;
        this.removeRequireSignalUsed = removeRequireSignalUsed;
        _lastUpdateTime = DateTime.Now;

        registered = false;

        Initialize();
    }

    /// <summary>
    /// 用xxx:xxx:xxx的字符串创建时间, 但是需要带注册名
    /// </summary>
    /// <param name="regsiterName"></param>
    /// <param name="countdown"></param>
    /// <param name="time"></param>
    /// <param name="global"></param>
    /// <param name="followPause"></param>
    /// <param name="removeWhenCompleted"></param>
    /// <param name="isolatedUpdate"></param>
    public Stopclock(string regsiterName, bool countdown, string time, bool global = false, bool followPause = false,
        bool removeWhenCompleted = true, bool isolatedUpdate = false, bool removeRequireSignalUsed = true)
    {
        time.Split(':', StringSplitOptions.TrimEntries).ApplyTo(out string[] t);

        for (int i = 0, n = 0; i < t.Length; i++)
        {
            n = t.Length - 1 - i;
            t[n].ParseInt(out int m);
            m = m.ClampMin(0);

            switch (i)
            {
                case 0:
                    m.AssignTo(countdown, out initialMillisecond, out millisecond);
                    break;
                case 1:
                    m.AssignTo(countdown, out initialSecond, out second);
                    break;
                case 2:
                    m.AssignTo(countdown, out initialMinute, out minute);
                    break;
                case 3:
                    m.AssignTo(countdown, out initialHour, out hour);
                    break;
                case 4:
                    m.AssignTo(countdown, out initialDay, out day);
                    break;
                case 5:
                    m.AssignTo(countdown, out initialMonth, out month);
                    break;
                case 6:
                    m.AssignTo(countdown, out initialYear, out year);
                    break;
                default:
                    m.AssignTo(countdown, out initialMillisecond, out millisecond);
                    break;
            }
        }

        this.countdown = countdown;
        this.global = global;
        this.followPause = followPause;
        this.removeWhenCompleted = removeWhenCompleted;
        this.isolatedUpdate = isolatedUpdate;
        this.removeRequireSignalUsed = removeRequireSignalUsed;
        _lastUpdateTime = DateTime.Now;

        registered = false;

        Register(regsiterName);

        Initialize();
    }

    public void Register(string name)
    {
        if (global)
        {
            Md.SaveData.globalStopwatches.Enter(name, this);
        }
        else
        {
            Md.Session.sessionStopwatches.Enter(name, this);
        }

        registered = true;
    }

    public void Register(string name, bool overrideGlobal)
    {
        global = overrideGlobal;

        if (global)
        {
            Md.SaveData.globalStopwatches.Enter(name, this);
        }
        else
        {
            Md.Session.sessionStopwatches.Enter(name, this);
        }

        registered = true;
    }
    
    public bool Registered()
    {
        CheckRegistry(out bool registered, out string registeredAs, out bool globally);
        return registered;
    }

    public string RegisteredAs()
    {
        CheckRegistry(out bool registered, out string registeredAs, out bool globally);
        return registeredAs;
    }
    
    public bool RegisteredGlobally()
    {
        CheckRegistry(out bool registered, out string registeredAs, out bool globally);
        return globally;
    }
    
    public void CheckRegistry(out bool registered, out string registeredAs, out bool globally)
    {
        registered = Md.Session.sessionStopwatches.ContainsValue(this)
            || Md.SaveData.globalStopwatches.ContainsValue(this);
        this.registered = registered;
        globally = Md.SaveData.globalStopwatches.ContainsValue(this);
        registeredAs = "";
        if (globally)
        {
            foreach(var item in Md.SaveData.globalStopwatches)
            {
                if(item.Value == this)
                {
                    registeredAs = item.Key;
                    break;
                }
            }
        }
        else
        {
            foreach(var item in Md.Session.sessionStopwatches)
            {
                if(item.Value == this)
                {
                    registeredAs = item.Key;
                    break;
                }
            }
        }
    }
    
    public void SetGlobal(bool value)
    {
        CheckRegistry(out bool registered, out string registeredAs, out bool globally);
        
        if (registered && value != globally)
        {
            Register(registeredAs, value);
            if (globally)
            {
                Md.SaveData.globalStopwatches.SafeRemove(registeredAs);
            }
            else
            {
                Md.Session.sessionStopwatches.SafeRemove(registeredAs);
            }
        }

        global = value;
    }
}