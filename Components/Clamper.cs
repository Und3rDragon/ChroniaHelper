using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public abstract class Clamper : BaseComponent
{
    public bool Enabled = false;

    public class Float : Clamper
    {
        private float Minimum = 0f;
        private float Maximum = 1f;
        public Float(float n1, float n2)
        {
            Minimum = float.Min(n1, n2);
            Maximum = float.Max(n1, n2);
            Enabled = true;
        }

        public Float()
        {
            Enabled = false;
        }

        public float Operate(float value)
        {
            if (!Enabled) { return value; }

            if (value < Minimum) { return Minimum; }

            if (value > Maximum) { return Maximum; }

            return value;
        }
    }

    public class FloatTracker : Clamper
    {
        private float Minimum = 0f;
        private float Maximum = 1f;
        private Func<float> Getter;
        private Action<float> Setter;
        private float Fallback;
        public FloatTracker(float n1, float n2, Func<float> getter, Action<float> setter, float fallback = 0f)
        {
            Minimum = float.Min(n1, n2);
            Maximum = float.Max(n1, n2);
            
            Getter = getter;
            Setter = setter;
            Fallback = fallback;

            Enabled = true;
        }

        public FloatTracker()
        {
            Enabled = false;
        }

        private float f = 0f;
        public override void Update()
        {
            if (!Enabled) { return; }

            f = Getter?.Invoke() ?? Fallback;

            if (f < Minimum) { Setter?.Invoke(Minimum); }

            if (f > Maximum) { Setter?.Invoke(Maximum); }

            Setter?.Invoke(f);
        }
    }

    public class Int : Clamper
    {
        private int Minimum = 0;
        private int Maximum = 1;
        public Int(int n1, int n2)
        {
            Minimum = int.Min(n1, n2);
            Maximum = int.Max(n1, n2);
            Enabled = true;
        }

        public Int()
        {
            Enabled = false;
        }

        public int Operate(int value)
        {
            if (!Enabled) { return value; }

            if (value < Minimum) { return Minimum; }

            if (value > Maximum) { return Maximum; }

            return value;
        }
    }

    public class IntTracker : Clamper
    {
        private int Minimum = 0;
        private int Maximum = 1;
        private Func<int> Getter;
        private Action<int> Setter;
        private int Fallback;
        public IntTracker(int n1, int n2, Func<int> getter, Action<int> setter, int fallback = 0)
        {
            Minimum = int.Min(n1, n2);
            Maximum = int.Max(n1, n2);

            Getter = getter;
            Setter = setter;
            Fallback = fallback;

            Enabled = true;
        }

        public IntTracker()
        {
            Enabled = false;
        }

        private int i = 0;
        public override void Update()
        {
            if (!Enabled) { return; }

            i = Getter?.Invoke() ?? Fallback;

            if (i < Minimum) { Setter?.Invoke(Minimum); }

            if (i > Maximum) { Setter?.Invoke(Maximum); }

            Setter?.Invoke(i);
        }
    }

    public class General<T> : Clamper
        where T : IComparable<T>
    {
        private T Minimum;
        private T Maximum;
        public General(T n1, T n2)
        {
            Minimum = n1.CompareTo(n2) > 0 ? n2 : n1;
            Maximum = n1.CompareTo(n2) > 0 ? n1 : n2;
            Enabled = true;
        }

        public General()
        {
            Enabled = false;
        }

        public T Operate(T value)
        {
            if (!Enabled) { return value; }

            if (value.CompareTo(Minimum) < 0) { return Minimum; }

            if (value.CompareTo(Maximum) > 0) { return Maximum; }

            return value;
        }
    }

    public class GeneralTracker<T> : Clamper
        where T : IComparable<T>
    {
        private T Minimum;
        private T Maximum;
        private Func<T> Getter;
        private Action<T> Setter;
        private T Fallback;
        public GeneralTracker(T n1, T n2, Func<T> getter, Action<T> setter, T fallback)
        {
            Minimum = n1.CompareTo(n2) > 0 ? n2 : n1;
            Maximum = n1.CompareTo(n2) > 0 ? n1 : n2;

            Getter = getter;
            Setter = setter;
            Fallback = fallback;

            Enabled = true;
        }

        public GeneralTracker()
        {
            Enabled = false;
        }

        private T i = default;
        public override void Update()
        {
            if (!Enabled) { return; }

            i = (Getter is null) ? Fallback : Getter.Invoke();

            if (i.CompareTo(Minimum) < 0) { Setter?.Invoke(Minimum); }

            if (i.CompareTo(Maximum) > 0) { Setter?.Invoke(Maximum); }

            Setter?.Invoke(i);
        }
    }
}
