using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

public partial class Stopclock
{

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

            signal = (int)Signal.Complete;

            onComplete?.Invoke();
            OnComplete();
        }
    }

    /// <summary>
    /// 启动自动更新定时器
    /// </summary>
    private void StartAutoUpdate()
    {
        if (_updateTimer != null) return;

        _lastUpdateTime = DateTime.Now;
        _updateTimer = new Timer(_ =>
        {
            if (!_disposed && running && !completed)
            {
                IsolatedUpdateTime();
            }
        }, null, 0, IsolateUpdateInterval);
    }

    /// <summary>
    /// 停止自动更新
    /// </summary>
    private void StopAutoUpdate()
    {
        if (_updateTimer != null)
        {
            _updateTimer.Dispose();
            _updateTimer = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            StopAutoUpdate();

            activeIsolatedClocks--;
            activeIsolatedClocks = activeIsolatedClocks >= 0 ? activeIsolatedClocks : 0;
        }
    }

    /// <summary>
    /// 独立更新时间
    /// </summary>
    private void IsolatedUpdateTime()
    {
        DateTime currentTime = DateTime.Now;
        TimeSpan elapsed = currentTime - _lastUpdateTime;
        _lastUpdateTime = currentTime;

        long deltaMilliseconds = (long)elapsed.TotalMilliseconds;
        _accumulatedTicks += deltaMilliseconds;

        if (countdown)
        {
            millisecond -= (int)deltaMilliseconds;
        }
        else
        {
            millisecond += (int)deltaMilliseconds;
        }

        RefreshUnits();

        if (countdown && ZeroState)
        {
            completed = true;
            running = false;

            signal = (int)Signal.Complete;

            onComplete?.Invoke();
            OnComplete();

            // 完成后停止定时器以节省资源
            StopAutoUpdate();
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

}
