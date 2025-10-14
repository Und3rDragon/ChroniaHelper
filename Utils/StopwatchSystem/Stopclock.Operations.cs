using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

public partial class Stopclock
{

    /// <summary>
    /// 开始计时
    /// </summary>
    public void Start()
    {
        if (running) return;

        running = true;
        completed = false;
        _accumulatedTicks = 0;
        _lastUpdateTime = DateTime.Now;

        // 运行前检查
        if (isolatedUpdate)
        {
            if (maxIsolatedClocks <= activeIsolatedClocks)
            {
                activeIsolatedClocks++;
                Dispose();
            }
        }

        // 确保定时器运行
        if (isolatedUpdate && _updateTimer == null)
        {
            StartAutoUpdate();
            activeIsolatedClocks++;
        }

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

}
