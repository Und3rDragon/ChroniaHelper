using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class InnerData : BaseComponent
{
    public override bool Equals(object obj)
    {
        if(obj is InnerData data)
        {
            return string.Equals(data.Tag, Tag);
        }

        return false;
    }

    public InnerData(string tag)
    {
        Tag = tag;
    }
    public string Tag { get; set; }

    public class Bool : InnerData
    {
        public Bool(string tag, bool value) : base(tag)
        {
            Value = value;
        }
        public bool Value { get; set; }
    }

    public class Int : InnerData
    {
        public Int(string tag, int value) : base(tag)
        {
            Value = value;
        }
        public int Value { get; set; }
    }

    public class Float : InnerData
    {
        public Float(string tag, float value) : base(tag)
        {
            Value = value;
        }
        public float Value { get; set; }
    }

    public class String : InnerData
    {
        public String(string tag, string value) : base(tag)
        {
            Value = value;
        }
        public string Value { get; set; }
    }

    public class Double : InnerData
    {
        public Double(string tag, double value) : base(tag)
        {
            Value = value;
        }
        public double Value { get; set; }
    }

    public class General<T> : InnerData
    {
        public General(string tag, T value) : base(tag)
        {
            Value = value;
        }
        public T Value { get; set; }
    }

    public class InList<T> : InnerData
    {
        public InList(string tag, List<T> value) : base(tag)
        {
            Value = value;
        }
        public List<T> Value { get; set; }
    }

    public class InDic<TKey, TValue> : InnerData
    {
        public InDic(string tag, Dictionary<TKey, TValue> value) : base(tag)
        {
            Value = value;
        }
        public Dictionary<TKey, TValue> Value { get; set; }
    }
}
