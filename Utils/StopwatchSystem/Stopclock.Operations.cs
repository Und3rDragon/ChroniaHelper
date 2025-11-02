using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    
    /// <summary>
    /// 将xxx:xxx:xxx形式的字符串转换为时间
    /// </summary>
    /// <param name="timeFormat"></param>
    public void SetTime(string timeFormat, bool initial = false, bool reset = true)
    {
        timeFormat.Split(':', StringSplitOptions.TrimEntries).ApplyTo(out string[] t);
        
        for(int i = 0, n = 0; i < t.Length; i++)
        {
            n = t.Length - 1 - i;
            t[n].ParseInt(out int m);
            m = m.ClampMin(0);
            
            switch (i)
            {
                case 0:
                    m.AssignTo(initial, out initialMillisecond, out millisecond);
                    break;
                case 1:
                    m.AssignTo(initial, out initialSecond, out second);
                    break;
                case 2:
                    m.AssignTo(initial, out initialMinute, out minute);
                    break;
                case 3:
                    m.AssignTo(initial, out initialHour, out hour);
                    break;
                case 4:
                    m.AssignTo(initial, out initialDay, out day);
                    break;
                case 5:
                    m.AssignTo(initial, out initialMonth, out month);
                    break;
                case 6:
                    m.AssignTo(initial, out initialYear, out year);
                    break;
                default:
                    m.AssignTo(initial, out initialMillisecond, out millisecond);
                    break;
            }
        }

        if (reset)
        {
            Initialize();
        }
    }

    public void SetTime(int[] time, bool initial = false, bool reset = true)
    {
        for (int i = 0, n = 0; i < time.Length; i++)
        {
            n = time.Length - 1 - i;
            int m = time[n].ClampMin(0);

            switch (i)
            {
                case 0:
                    m.AssignTo(initial, out initialMillisecond, out millisecond);
                    break;
                case 1:
                    m.AssignTo(initial, out initialSecond, out second);
                    break;
                case 2:
                    m.AssignTo(initial, out initialMinute, out minute);
                    break;
                case 3:
                    m.AssignTo(initial, out initialHour, out hour);
                    break;
                case 4:
                    m.AssignTo(initial, out initialDay, out day);
                    break;
                case 5:
                    m.AssignTo(initial, out initialMonth, out month);
                    break;
                case 6:
                    m.AssignTo(initial, out initialYear, out year);
                    break;
                default:
                    break;
            }
        }

        if (reset)
        {
            Initialize();
        }
    }

}
