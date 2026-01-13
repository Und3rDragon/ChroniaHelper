using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

public partial class Stopclock
{
    public class Countdown : Stopclock
    {
        public Countdown(params int[] n)
            : base(countdown: true,
                  initialYear: n.Length > 6 ? n[6] : 0,
                  initialMonth: n.Length > 5 ? n[5] : 0,
                  initialDay: n.Length > 4 ? n[4] : 0,
                  initialHour: n.Length > 3 ? n[3] : 0,
                  initialMinute: n.Length > 2 ? n[2] : 0,
                  initialSecond: n.Length > 1 ? n[1] : 0,
                  initialMillisecond: n.Length > 0 ? n[0] : 0,
                  followPause: false,
                  removeWhenCompleted: true,
                  isolatedUpdate: false,
                  removeRequireSignalUsed: true)
        {

        }

        public Countdown(bool followPause = false,
            bool removeWhenCompleted = true,
            bool removeRequireSignalUsed = true,
            params int[] n)
            : base(countdown: true,
                  initialYear: n.Length > 6 ? n[6] : 0,
                  initialMonth: n.Length > 5 ? n[5] : 0,
                  initialDay: n.Length > 4 ? n[4] : 0,
                  initialHour: n.Length > 3 ? n[3] : 0,
                  initialMinute: n.Length > 2 ? n[2] : 0,
                  initialSecond: n.Length > 1 ? n[1] : 0,
                  initialMillisecond: n.Length > 0 ? n[0] : 0,
                  followPause: followPause,
                  removeWhenCompleted: removeWhenCompleted,
                  isolatedUpdate: false,
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {

        }

        public Countdown(int initialYear = 0, 
            int initialMonth = 0, 
            int initialDay = 0, 
            int initialHour = 0, 
            int initialMinute = 5, 
            int initialSecond = 0, 
            int initialMillisecond = 0,
            bool followPause = false, 
            bool removeWhenCompleted = true,
            bool removeRequireSignalUsed = true) 
            : base(countdown: true, 
                  initialYear: initialYear, 
                  initialMonth: initialMonth,
                  initialDay: initialDay, 
                  initialHour: initialHour, 
                  initialMinute: initialMinute,
                  initialSecond: initialSecond, 
                  initialMillisecond: initialMillisecond,
                  followPause: followPause, 
                  removeWhenCompleted: removeWhenCompleted,
                  isolatedUpdate: false, 
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {
            
        }
        
        public Countdown(string time, 
            bool followPause = false,
            bool removeWhenCompleted = true, 
            bool removeRequireSignalUsed = true) 
            : base(true, time, followPause, removeWhenCompleted, false, removeRequireSignalUsed)
        {
            
        }
    }
    
    public class Stopwatch : Stopclock
    {
        public Stopwatch(params int[] n)
            : base(countdown: false,
                  year: n.Length > 6 ? n[6] : 0,
                  month: n.Length > 5 ? n[5] : 0,
                  day: n.Length > 4 ? n[4] : 0,
                  hour: n.Length > 3 ? n[3] : 0,
                  minute: n.Length > 2 ? n[2] : 0,
                  second: n.Length > 1 ? n[1] : 0,
                  millisecond: n.Length > 0 ? n[0] : 0,
                  followPause: false,
                  removeWhenCompleted: true,
                  isolatedUpdate: false,
                  removeRequireSignalUsed: true)
        {

        }

        public Stopwatch(bool followPause = false,
            bool removeWhenCompleted = true,
            bool removeRequireSignalUsed = true,
            params int[] n)
            : base(countdown: false,
                  year: n.Length > 6 ? n[6] : 0,
                  month: n.Length > 5 ? n[5] : 0,
                  day: n.Length > 4 ? n[4] : 0,
                  hour: n.Length > 3 ? n[3] : 0,
                  minute: n.Length > 2 ? n[2] : 0,
                  second: n.Length > 1 ? n[1] : 0,
                  millisecond: n.Length > 0 ? n[0] : 0,
                  followPause: followPause,
                  removeWhenCompleted: removeWhenCompleted,
                  isolatedUpdate: false,
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {

        }

        public Stopwatch(int year = 0, 
            int month = 0, 
            int day = 0, 
            int hour = 0,
            int minute = 0, 
            int second = 0, 
            int millisecond = 0, 
            bool followPause = false,
            bool removeWhenCompleted = true, 
            bool removeRequireSignalUsed = true)
            : base(countdown: false, 
                  year: year, 
                  month: month, 
                  day: day, 
                  hour: hour,
                  minute: minute, 
                  second: second, 
                  millisecond: millisecond, 
                  followPause: followPause,
                  removeWhenCompleted: removeWhenCompleted, 
                  isolatedUpdate: false,
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {
            
        }
        
        public Stopwatch(string time, 
            bool followPause = false,
            bool removeWhenCompleted = true, 
            bool removeRequireSignalUsed = true)
            : base(false, time, followPause, removeWhenCompleted, false, removeRequireSignalUsed)
        {
            
        }
    }

    public class IsolatedCountdown : Stopclock
    {
        public IsolatedCountdown(params int[] n)
            : base(countdown: true,
                  initialYear: n.Length > 6 ? n[6] : 0,
                  initialMonth: n.Length > 5 ? n[5] : 0,
                  initialDay: n.Length > 4 ? n[4] : 0,
                  initialHour: n.Length > 3 ? n[3] : 0,
                  initialMinute: n.Length > 2 ? n[2] : 0,
                  initialSecond: n.Length > 1 ? n[1] : 0,
                  initialMillisecond: n.Length > 0 ? n[0] : 0,
                  followPause: false,
                  removeWhenCompleted: true,
                  isolatedUpdate: true,
                  removeRequireSignalUsed: true)
        {

        }

        public IsolatedCountdown(bool followPause = false,
            bool removeWhenCompleted = true,
            bool removeRequireSignalUsed = true,
            params int[] n)
            : base(countdown: true,
                  initialYear: n.Length > 6 ? n[6] : 0,
                  initialMonth: n.Length > 5 ? n[5] : 0,
                  initialDay: n.Length > 4 ? n[4] : 0,
                  initialHour: n.Length > 3 ? n[3] : 0,
                  initialMinute: n.Length > 2 ? n[2] : 0,
                  initialSecond: n.Length > 1 ? n[1] : 0,
                  initialMillisecond: n.Length > 0 ? n[0] : 0,
                  followPause: followPause,
                  removeWhenCompleted: removeWhenCompleted,
                  isolatedUpdate: true,
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {

        }

        public IsolatedCountdown(int initialYear = 0, 
            int initialMonth = 0, 
            int initialDay = 0, 
            int initialHour = 0,
            int initialMinute = 5, 
            int initialSecond = 0, 
            int initialMillisecond = 0,
            bool followPause = false, 
            bool removeWhenCompleted = true,
            bool removeRequireSignalUsed = true)
            : base(countdown: true, 
                  initialYear: initialYear, 
                  initialMonth: initialMonth,
                  initialDay: initialDay, 
                  initialHour: initialHour, 
                  initialMinute: initialMinute,
                  initialSecond: initialSecond, 
                  initialMillisecond: initialMillisecond,
                  followPause: followPause, 
                  removeWhenCompleted: removeWhenCompleted,
                  isolatedUpdate: true, 
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {

        }

        public IsolatedCountdown(string time, 
            bool followPause = false,
            bool removeWhenCompleted = true, 
            bool removeRequireSignalUsed = true)
            : base(true, time, followPause, removeWhenCompleted, true, removeRequireSignalUsed)
        {

        }
    }

    public class IsolatedStopwatch : Stopclock
    {
        public IsolatedStopwatch(params int[] n)
            : base(countdown: false,
                  year: n.Length > 6 ? n[6] : 0,
                  month: n.Length > 5 ? n[5] : 0,
                  day: n.Length > 4 ? n[4] : 0,
                  hour: n.Length > 3 ? n[3] : 0,
                  minute: n.Length > 2 ? n[2] : 0,
                  second: n.Length > 1 ? n[1] : 0,
                  millisecond: n.Length > 0 ? n[0] : 0,
                  followPause: false,
                  removeWhenCompleted: true,
                  isolatedUpdate: true,
                  removeRequireSignalUsed: true)
        {

        }

        public IsolatedStopwatch(bool followPause = false,
            bool removeWhenCompleted = true,
            bool removeRequireSignalUsed = true,
            params int[] n)
            : base(countdown: false,
                  year: n.Length > 6 ? n[6] : 0,
                  month: n.Length > 5 ? n[5] : 0,
                  day: n.Length > 4 ? n[4] : 0,
                  hour: n.Length > 3 ? n[3] : 0,
                  minute: n.Length > 2 ? n[2] : 0,
                  second: n.Length > 1 ? n[1] : 0,
                  millisecond: n.Length > 0 ? n[0] : 0,
                  followPause: followPause,
                  removeWhenCompleted: removeWhenCompleted,
                  isolatedUpdate: true,
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {

        }

        public IsolatedStopwatch(int year = 0, 
            int month = 0, 
            int day = 0, 
            int hour = 0,
            int minute = 0, 
            int second = 0, 
            int millisecond = 0, 
            bool followPause = false,
            bool removeWhenCompleted = true, 
            bool removeRequireSignalUsed = true)
            : base(countdown: false, 
                  year: year, 
                  month: month, 
                  day: day, 
                  hour: hour,
                  minute: minute, 
                  second: second, 
                  millisecond: millisecond, 
                  followPause: followPause,
                  removeWhenCompleted: removeWhenCompleted, 
                  isolatedUpdate: true,
                  removeRequireSignalUsed: removeRequireSignalUsed)
        {

        }

        public IsolatedStopwatch(string time, 
            bool followPause = false,
            bool removeWhenCompleted = true, 
            bool removeRequireSignalUsed = true)
            : base(false, time, followPause, removeWhenCompleted, true, removeRequireSignalUsed)
        {

        }
    }
}
