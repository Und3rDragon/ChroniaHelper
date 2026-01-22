using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Components;

public class SimpleTimers
{
    public class TimeSpanTimer : BaseComponent
    {
        public TimeSpan Timer => DateTime.Now - _start;

        private DateTime _start = DateTime.Now;

        public void Restart()
        {
            _start = DateTime.Now;
        }
    }

    public class ClassicTimer : BaseComponent
    {
        public float Seconds = 0f;
        
        public bool UseRawDeltaTime = false;

        public TimeSpan Timer => TimeSpan.FromMilliseconds(Seconds.ClampMin(0f) * 1000);

        public ClassicTimer(bool useRaw = false)
        {
            Seconds = 0f;
            UseRawDeltaTime = useRaw;
        }

        public override void Update()
        {
            Seconds += (UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime);
        }
    }
}
