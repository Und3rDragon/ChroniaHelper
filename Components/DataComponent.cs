using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class DataComponent<T> : BaseComponent
{
    public DataComponent(string tag, T value)
    {
        Tag = tag;
        Value = value;
    }
    public string Tag { get; set; }
    public T Value { get; set; }
}

public class DataComponentPrs
{
    public class Int : DataComponent<int>
    {
        public Int(string tag, int value) : base(tag, value) { }

        private float duration = -1f, timer = -1f;
        private int target = 0, source = 0;
        private EaseMode easer = EaseMode.Linear;
        /// <summary>
        /// Disable by setting duration below 0
        /// </summary>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        public void FadeTo(int target, float duration, EaseMode ease = EaseMode.Linear)
        {
            source = Value;
            this.target = target;
            this.duration = this.timer = duration;
            easer = ease;
        }

        public override void Update()
        {
            // Fader
            if (timer > 0f)
            {
                timer = Calc.Approach(timer, 0f, Engine.DeltaTime);
                Value = timer.LerpValue(duration, 0f, source, target, easer);
            }
        }
    }

    public class Float : DataComponent<float>
    {
        public Float(string tag, float value) : base(tag, value) { }

        private float duration = -1f, timer = -1f, target = 0f, source = 0f;
        private EaseMode easer = EaseMode.Linear;
        /// <summary>
        /// Disable by setting duration below 0
        /// </summary>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        public void FadeTo(float target, float duration, EaseMode ease = EaseMode.Linear)
        {
            source = Value;
            this.target = target;
            this.duration = this.timer = duration;
            easer = ease;
        }

        public override void Update()
        {
            // Fader
            if (timer > 0f)
            {
                timer = Calc.Approach(timer, 0f, Engine.DeltaTime);
                Value = timer.LerpValue(duration, 0f, source, target, easer);
            }
        }
    }

    public class Double : DataComponent<double>
    {
        public Double(string tag, double value) : base(tag, value) { }

        private float duration = -1f, timer = -1f;
        private double target = 0, source = 0;
        private EaseMode easer = EaseMode.Linear;
        /// <summary>
        /// Disable by setting duration below 0
        /// </summary>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        public void FadeTo(double target, float duration, EaseMode ease = EaseMode.Linear)
        {
            source = Value;
            this.target = target;
            this.duration = this.timer = duration;
            easer = ease;
        }

        public override void Update()
        {
            // Fader
            if (timer > 0f)
            {
                timer = Calc.Approach(timer, 0f, Engine.DeltaTime);
                Value = timer.LerpValue(duration, 0f, source, target, easer);
            }
        }
    }
}
