using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Utils;

public static class TimeUtils
{
    public static long deltaTicks => CalculateInterval(Engine.RawDeltaTime, 1000).Ticks;
    public static TimeSpan CalculateInterval(double value, int scale)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("TimeSpan does not accept floating point Not-a-Number values.");
        }
        double millis = value * (double)scale + ((value >= 0.0) ? 0.5 : (-0.5));
        if (millis > 922337203685477.0 || millis < -922337203685477.0)
        {
            throw new OverflowException("TimeSpan overflowed because the duration is too long.");
        }
        return new TimeSpan((long)millis * 10000);
    }
}

public class Stopclock
{
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
                watches.Value.UpdateTime(TimeUtils.deltaTicks);
            }
            else
            {
                watches.Value.IsolatedUpdateTime();
            }

            if (watches.Value.completed && watches.Value.removeWhenCompleted) { toRemove.Add(watches.Key); }

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

                if (!watches.Value.isolatedUpdate)
                {
                    watches.Value.UpdateTime(TimeUtils.deltaTicks);
                }
                else
                {
                    watches.Value.IsolatedUpdateTime();
                }

                if (watches.Value.completed && watches.Value.removeWhenCompleted) { toRemove.Add(watches.Key); }

                //Log.Info(watches.Key, watches.Value.FormattedTime);
            }
            foreach (var item in toRemove)
            {
                Md.Session.sessionStopwatches.SafeRemove(item);
            }
        }
    }
    
    public static void Debug(string additionalTagInfo = "")
    {
        Md.Session.sessionStopwatches.EachDo((e) =>
        {
            Log.Info($"[{additionalTagInfo}] Session ({e.Key}): {e.Value.completed}, {e.Value.FormattedTime}");
        });
        Md.SaveData.globalStopwatches.EachDo((e) =>
        {
            Log.Info($"[{additionalTagInfo}] Global ({e.Key}): {e.Value.completed}, {e.Value.FormattedTime}");
        });
        Log.Error("_____________________________");
    }
    
    public int year = 0;
    public int month = 0;
    public int day = 0;
    public int hour = 0;
    public int minute = 0;
    public int second = 0;
    public int millisecond = 0;

    public bool countdown = false;
    public bool global = false;
    public bool completed { get; private set; } = false;
    public bool removeWhenCompleted = true;
    public bool running { get; private set; } = false;
    public bool isolatedUpdate = false; 
    
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

    // 信号系统
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
    /// <summary>
    /// 仅用于外部接收信号, 使用信号请调用UseSignal()
    /// </summary>
    public int signal { get; private set; } = 0;
    /// <summary>
    /// 调用信号
    /// </summary>
    public void UseSignal() { signal = -1; }
    /// <summary>
    /// 检查信号
    /// </summary>
    public bool HasSignal => signal > 0;
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
        this.countdown = false;
        this.global = false;
        this.removeWhenCompleted = true;
        this.isolatedUpdate = false;
        this._lastUpdateTime = DateTime.Now;
        
        Initialize();
    }

    /// <summary>
    /// 如果是倒计时, 倒计时默认事件为5分钟, 设置时注意清零
    /// </summary>
    /// <param name="countdown"></param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="second"></param>
    /// <param name="millisecond"></param>
    /// <param name="global"></param>
    /// <param name="initialYear"></param>
    /// <param name="initialMonth"></param>
    /// <param name="initialDay"></param>
    /// <param name="initialHour"></param>
    /// <param name="initialMinute"></param>
    /// <param name="initialSecond"></param>
    /// <param name="initialMillisecond"></param>
    /// <param name="removeWhenCompleted"></param>
    /// <param name="isolatedUpdate"></param>
    public Stopclock(bool countdown, int year = 0, int month = 0, int day = 0, int hour = 0,
        int minute = 0, int second = 0, int millisecond = 0, bool global = false,
        int initialYear = 0, int initialMonth = 0, int initialDay = 0, int initialHour = 0, int initialMinute = 5,
        int initialSecond = 0, int initialMillisecond = 0, bool removeWhenCompleted = true, bool isolatedUpdate = false)
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
        this.initialYear = initialYear;
        this.initialMonth = initialMonth;
        this.initialDay = initialDay;
        this.initialHour = initialHour;
        this.initialMinute = initialMinute;
        this.initialSecond = initialSecond;
        this.initialMillisecond = initialMillisecond;
        this.removeWhenCompleted = removeWhenCompleted;
        this.isolatedUpdate = isolatedUpdate;
        this._lastUpdateTime = DateTime.Now;

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
    }

    #region Operations
    
    /// <summary>
    /// 开始计时
    /// </summary>
    public void Start()
    {
        if (running) return;

        running = true;
        completed = false;
        _accumulatedTicks = 0;
        _lastUpdateTime = DateTime.Now; // 重置更新时间

        signal = (int)Signal.Start;

        onStart?.Invoke();
        OnStart();
    }

    /// <summary>
    /// 停止/暂停计时
    /// </summary>
    public void Stop()
    {
        running = false;

        signal = (int)Signal.Stop;

        onStop?.Invoke();
        OnStop();
    }
    
    public void Restart()
    {
        Reset();
        Start();
    }

    /// <summary>
    /// 重置为默认值
    /// </summary>
    public void Reset()
    {
        Stop();

        if (countdown)
        {
            // 倒计时模式：恢复到初始值
            year = initialYear;
            month = initialMonth;
            day = initialDay;
            hour = initialHour;
            minute = initialMinute;
            second = initialSecond;
            millisecond = initialMillisecond;
        }
        else
        {
            // 正计时模式：重置为0
            year = 0;
            month = 0;
            day = 0;
            hour = 0;
            minute = 0;
            second = 0;
            millisecond = 0;
        }

        completed = false;
        _accumulatedTicks = 0;
        _lastUpdateTime = DateTime.Now;

        // 重置后刷新单位
        RefreshUnits();

        signal = (int)Signal.Reset;

        onReset?.Invoke();
        OnReset();
    }
    
    public void Initialize()
    {
        Reset();

        signal = (int)Signal.Initial;
    }

    #endregion

    /// <summary>
    /// 更新时间 - 使用 ticks 同步（非独立更新模式）
    /// </summary>
    /// <param name="ticks">时间间隔的 ticks 数</param>
    public void UpdateTime(long ticks)
    {
        if (!running || completed || isolatedUpdate) { return; }
        
        // 将 ticks 转换为毫秒 (1 tick = 100 纳秒 = 0.0001 毫秒)
        long deltaMilliseconds = ticks / 10000; // 10000 ticks = 1 毫秒

        _accumulatedTicks += deltaMilliseconds;

        if (countdown)
        {
            // 倒计时逻辑 - 减少对应的毫秒数
            millisecond -= (int)deltaMilliseconds;
        }
        else
        {
            // 正计时逻辑 - 增加对应的毫秒数
            millisecond += (int)deltaMilliseconds;
        }

        // 更新后刷新时间单位
        RefreshUnits();

        // 再次检查倒计时是否完成
        if (countdown && ZeroState)
        {
            completed = true;
            running = false;

            onComplete?.Invoke();
            OnComplete();
        }
    }

    /// <summary>
    /// 独立更新时间 - 基于真实电脑时间（独立更新模式）
    /// </summary>
    public void IsolatedUpdateTime()
    {
        if (!running || completed || !isolatedUpdate) { return; }
        
        DateTime currentTime = DateTime.Now;
        TimeSpan elapsed = currentTime - _lastUpdateTime;
        _lastUpdateTime = currentTime;

        // 转换为毫秒
        long deltaMilliseconds = (long)elapsed.TotalMilliseconds;

        _accumulatedTicks += deltaMilliseconds;

        if (countdown)
        {
            // 倒计时逻辑 - 减少对应的毫秒数
            millisecond -= (int)deltaMilliseconds;
        }
        else
        {
            // 正计时逻辑 - 增加对应的毫秒数
            millisecond += (int)deltaMilliseconds;
        }

        // 更新后刷新时间单位
        RefreshUnits();

        // 再次检查倒计时是否完成
        if (countdown && ZeroState)
        {
            completed = true;
            running = false;

            onComplete?.Invoke();
            OnComplete();
        }
    }
    
    /// <summary>
    /// 刷新时间单位，处理进位和借位
    /// </summary>
    private void RefreshUnits()
    {
        if (countdown)
        {
            // 倒计时模式 - 处理负数
            HandleNegativeTime();
        }
        else
        {
            // 正计时模式 - 处理溢出
            HandleOverflowTime();
        }
    }

    /// <summary>
    /// 处理正计时的时间溢出
    /// </summary>
    private void HandleOverflowTime()
    {
        // 处理毫秒溢出
        while (millisecond >= 1000)
        {
            millisecond -= 1000;
            second++;
        }

        while (millisecond < 0)
        {
            millisecond += 1000;
            second--;
        }

        // 处理秒溢出
        while (second >= 60)
        {
            second -= 60;
            minute++;
        }

        while (second < 0)
        {
            second += 60;
            minute--;
        }

        // 处理分溢出
        while (minute >= 60)
        {
            minute -= 60;
            hour++;
        }

        while (minute < 0)
        {
            minute += 60;
            hour--;
        }

        // 处理时溢出
        while (hour >= 24)
        {
            hour -= 24;
            day++;
        }

        while (hour < 0)
        {
            hour += 24;
            day--;
        }

        // 处理天溢出（每月按30天计算）
        while (day >= 30)
        {
            day -= 30;
            month++;
        }

        while (day < 0)
        {
            day += 30;
            month--;
        }

        // 处理月溢出
        while (month >= 12)
        {
            month -= 12;
            year++;
        }

        while (month < 0)
        {
            month += 12;
            year--;
        }

        // 确保没有负数（正计时不应该有负数）
        if (year < 0) year = 0;
        if (month < 0) month = 0;
        if (day < 0) day = 0;
        if (hour < 0) hour = 0;
        if (minute < 0) minute = 0;
        if (second < 0) second = 0;
        if (millisecond < 0) millisecond = 0;
    }

    /// <summary>
    /// 处理倒计时的时间负数
    /// </summary>
    private void HandleNegativeTime()
    {
        // 先检查是否已经归零，如果归零就直接返回
        if (ZeroState)
        {
            return;
        }

        // 处理毫秒负数
        while (millisecond < 0)
        {
            millisecond += 1000;
            second--;
            // 检查是否归零，如果归零就返回
            if (ZeroState) return;
        }

        // 处理秒负数
        while (second < 0)
        {
            second += 60;
            minute--;
            // 检查是否归零，如果归零就返回
            if (ZeroState) return;
        }

        // 处理分负数
        while (minute < 0)
        {
            minute += 60;
            hour--;
            // 检查是否归零，如果归零就返回
            if (ZeroState) return;
        }

        // 处理时负数
        while (hour < 0)
        {
            hour += 24;
            day--;
            // 检查是否归零，如果归零就返回
            if (ZeroState) return;
        }

        // 处理天负数（每月按30天计算）
        while (day < 0)
        {
            day += 30;
            month--;
            // 检查是否归零，如果归零就返回
            if (ZeroState) return;
        }

        // 处理月负数
        while (month < 0)
        {
            month += 12;
            year--;
            // 检查是否归零，如果归零就返回
            if (ZeroState) return;
        }

        // 确保没有负数（倒计时到0就停止）
        // 这里应该只处理负数，不应该重置为0，因为借位逻辑已经处理了
        if (year < 0 || month < 0 || day < 0 || hour < 0 || minute < 0 || second < 0 || millisecond < 0)
        {
            // 如果还有负数，说明时间已经归零但借位逻辑有问题
            year = 0;
            month = 0;
            day = 0;
            hour = 0;
            minute = 0;
            second = 0;
            millisecond = 0;
        }
    }

    /// <summary>
    /// 检查时间是否归零
    /// </summary>
    public bool ZeroState => year == 0 && month == 0 && day == 0 && 
        hour == 0 && minute == 0 && second == 0 && millisecond == 0;

    /// <summary>
    /// 获取格式化时间字符串
    /// </summary>
    public string FormattedTime => 
        $"{year:000}Years {month:00}Months {day:00}Days {hour:00}:{minute:00}:{second:00}.{millisecond:000}";

    /// <summary>
    /// 获取总毫秒数（近似值）
    /// </summary>
    public long TotalMilliseconds => _accumulatedTicks;

    /// <summary>
    /// 获取简化的时间字符串（适合显示）
    /// </summary>
    public string GetSimpleTimeString()
    {
        if (year > 0)
            return $"{year}年{month}月{day}天 {hour:00}:{minute:00}:{second:00}";
        else if (month > 0)
            return $"{month}月{day}天 {hour:00}:{minute:00}:{second:00}";
        else if (day > 0)
            return $"{day}天 {hour:00}:{minute:00}:{second:00}";
        else if (hour > 0)
            return $"{hour:00}:{minute:00}:{second:00}";
        else
            return $"{minute:00}:{second:00}.{millisecond:000}";
    }
}