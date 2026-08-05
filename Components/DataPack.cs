using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class DataPack<T> : BaseComponent
{
    public DataPack(string tag, T baseValue)
    {
        Tag = tag;
        _Value = Value = baseValue;
    }
    public string Tag { get; set; }
    public T Value { get; set; }

    public T _Value { get; private set; }
    public void SetValue(T newValue)
    {
        _Value = Value;
        Value = newValue;
    }
}

public class DataPackPreset
{
    public class Int : DataPack<int>
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

    public class Float : DataPack<float>
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

    public class Double : DataPack<double>
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
