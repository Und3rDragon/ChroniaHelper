using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using ChroniaHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class DataComponent : BaseComponent
{
    public override bool Equals(object obj)
    {
        if(obj is DataComponent data)
        {
            return string.Equals(data.Tag, Tag);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Tag.GetHashCode();
    }

    public static bool operator ==(DataComponent a, DataComponent b)
    {
        return a.Tag.Equals(b.Tag);
    }

    public static bool operator !=(DataComponent a, DataComponent b)
    {
        return !(a == b);
    }

    public DataComponent(string tag)
    {
        Tag = tag;
    }
    public string Tag { get; set; }

    public class Bool : DataComponent
    {
        public Bool(string tag, bool value) : base(tag)
        {
            Value = value;
        }
        public bool Value { get; set; }
    }

    public class Int : DataComponent
    {
        public Int(string tag, int value) : base(tag)
        {
            Value = value;
        }
        public int Value { get; set; }

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

    public class Float : DataComponent
    {
        public Float(string tag, float value) : base(tag)
        {
            Value = value;
        }
        public float Value { get; set; }

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
            if(timer > 0f)
            {
                timer = Calc.Approach(timer, 0f, Engine.DeltaTime);
                Value = timer.LerpValue(duration, 0f, source, target, easer);
            }
        }
    }

    public class String : DataComponent
    {
        public String(string tag, string value) : base(tag)
        {
            Value = value;
        }
        public string Value { get; set; }
    }

    public class Double : DataComponent
    {
        public Double(string tag, double value) : base(tag)
        {
            Value = value;
        }
        public double Value { get; set; }

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

    public class General<T> : DataComponent
    {
        public General(string tag, T value) : base(tag)
        {
            Value = value;
        }
        public T Value { get; set; }
    }

    public class InList<T> : DataComponent
    {
        public InList(string tag, List<T> value) : base(tag)
        {
            Value = value;
        }
        public List<T> Value { get; set; }
    }

    public class InDic<TKey, TValue> : DataComponent
    {
        public InDic(string tag, Dictionary<TKey, TValue> value) : base(tag)
        {
            Value = value;
        }
        public Dictionary<TKey, TValue> Value { get; set; }
    }
}
