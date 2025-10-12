using System;

namespace ChroniaHelper.Utils;
public class Stopwatch
{

    public static void Load()
    {
        On.Celeste.Level.UpdateTime += LevelUpdateTime;
    }

    public static void Unload()
    {
        On.Celeste.Level.UpdateTime -= LevelUpdateTime;
    }

    public static void LevelUpdateTime(On.Celeste.Level.orig_UpdateTime orig, Level self)
    {
        orig(self);
        
        foreach(var watches in Md.Session.stopwatches)
        {
            watches.Value.UpdateTime();
        }
    }

    // 公共变量 - 保密性为 public 且可读写
    public int year = 0;
    public int month = 0;
    public int day = 0;
    public int hour = 0;
    public int minute = 0;
    public int second = 0;
    public int millisecond = 0;

    public bool countdown = false;
    public bool completed { get; private set; } = false;
    public bool running { get; private set; } = false;

    // 内部变量用于存储初始值（倒计时用）
    public int initialYear = 0;
    public int initialMonth = 0;
    public int initialDay = 0;
    public int initialHour = 1; // 倒计时初始小时设为1
    public int initialMinute = 0;
    public int initialSecond = 0;
    public int initialMillisecond = 0;

    // 用于时间累积
    private float _accumulatedTime;

    public Stopwatch()
    {
        Reset();
    }

    public Stopwatch(bool countdown)
    {
        this.countdown = countdown;
        Reset();
    }

    /// <summary>
    /// 开始计时
    /// </summary>
    public void Start()
    {
        if (running) return;

        running = true;
        completed = false;
        _accumulatedTime = 0f;
    }

    /// <summary>
    /// 停止/暂停计时
    /// </summary>
    public void Stop()
    {
        running = false;
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
        _accumulatedTime = 0f;

        // 重置后刷新单位
        RefreshUnits();
    }

    /// <summary>
    /// 更新时间 - 在 Celeste.Level.UpdateTime() 中调用
    /// </summary>
    /// <summary>
    /// 更新时间 - 在 Celeste.Level.UpdateTime() 中调用
    /// </summary>
    public void UpdateTime()
    {
        if (!running || completed)
            return;

        // 直接计算本次更新应该增加/减少的毫秒数
        float deltaMilliseconds = Engine.DeltaTime * 1000f;

        if (countdown)
        {
            // 倒计时逻辑 - 直接减少对应的毫秒数
            millisecond -= (int)deltaMilliseconds;
        }
        else
        {
            // 正计时逻辑 - 直接增加对应的毫秒数
            millisecond += (int)deltaMilliseconds;
        }

        // 更新后刷新时间单位
        RefreshUnits();

        // 检查倒计时是否完成
        if (countdown && IsTimeZero())
        {
            completed = true;
            running = false;
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
        // 处理毫秒负数
        while (millisecond < 0)
        {
            millisecond += 1000;
            second--;
        }

        // 处理秒负数
        while (second < 0)
        {
            second += 60;
            minute--;
        }

        // 处理分负数
        while (minute < 0)
        {
            minute += 60;
            hour--;
        }

        // 处理时负数
        while (hour < 0)
        {
            hour += 24;
            day--;
        }

        // 处理天负数（每月按30天计算）
        while (day < 0)
        {
            day += 30;
            month--;
        }

        // 处理月负数
        while (month < 0)
        {
            month += 12;
            year--;
        }

        // 确保没有负数（倒计时到0就停止）
        if (year < 0) year = 0;
        if (month < 0) month = 0;
        if (day < 0) day = 0;
        if (hour < 0) hour = 0;
        if (minute < 0) minute = 0;
        if (second < 0) second = 0;
        if (millisecond < 0) millisecond = 0;
    }

    /// <summary>
    /// 检查时间是否归零
    /// </summary>
    private bool IsTimeZero()
    {
        return year == 0 && month == 0 && day == 0 &&
               hour == 0 && minute == 0 && second == 0 && millisecond == 0;
    }

    /// <summary>
    /// 获取格式化时间字符串
    /// </summary>
    public string GetFormattedTime()
    {
        if (countdown)
        {
            return $"{year:000}年{month:00}月{day:00}天 {hour:00}:{minute:00}:{second:00}.{millisecond:000}";
        }
        else
        {
            return $"{year:000}年{month:00}月{day:00}天 {hour:00}:{minute:00}:{second:00}.{millisecond:000}";
        }
    }

    /// <summary>
    /// 获取总毫秒数（近似值）
    /// </summary>
    public long GetTotalMilliseconds()
    {
        long total = millisecond;
        total += second * 1000L;
        total += minute * 60L * 1000L;
        total += hour * 60L * 60L * 1000L;
        total += day * 24L * 60L * 60L * 1000L;
        total += month * 30L * 24L * 60L * 60L * 1000L; // 每月按30天估算
        total += year * 12L * 30L * 24L * 60L * 60L * 1000L; // 每年按12个月估算

        return total;
    }

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