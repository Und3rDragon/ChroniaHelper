using ChroniaHelper.Cores;
using MonoMod.Utils;

namespace ChroniaHelper.Components;

public class DynamicDataWatcher : BaseComponent
{
    public override bool Equals(object obj)
    {
        if (obj is DynamicDataWatcher watcher)
        {
            return this.Field == watcher.Field;
        }
        
        return base.Equals(obj);
    }
    
    public override int GetHashCode()
    {
        return Field.GetHashCode();
    }

    public static bool operator ==(DynamicDataWatcher a, DynamicDataWatcher b)
    {
        return a.Field.Equals(b.Field);
    }
    
    public static bool operator !=(DynamicDataWatcher a, DynamicDataWatcher b)
    {
        return !(a == b);
    }

    public DynamicDataWatcher(string field)
    {
        Field = field;
    }

    public string Field;
    
    private DynamicData data = null;

    // public override void EntityAdded(Scene scene)
    // {
    //     if (Entity != null)
    //     {
    //         data = new DynamicData(Entity);
    //     }
    // }

    public override void EntityAwake()
    {
        if (Entity != null)
        {
            data = new DynamicData(Entity);
        }
    }

    public object Value => data.Get(Field);
    
    public class Float : DynamicDataWatcher
    {
        public Float(string field) : base(field){}

        public float Parsed => (float)Value;

        public Action<float> Applier = null;

        public override void Update()
        {
            Applier?.Invoke(Parsed);
        }
    }

    public class Int : DynamicDataWatcher
    {
        public Int(string field) : base(field){}
        
        public int Parsed => (int)Value;
        
        public Action<int> Applier = null;

        public override void Update()
        {
            Applier?.Invoke(Parsed);
        }
    }

    public class Double : DynamicDataWatcher
    {
        public Double(string field) : base(field){}
        
        public double Parsed => (double)Value;
        
        public Action<double> Applier = null;

        public override void Update()
        {
            Applier?.Invoke(Parsed);
        }
    }

    public class String : DynamicDataWatcher
    {
        public String(string field) : base(field){}
        
        public string Parsed => Value.ToString();
        
        public Action<string> Applier = null;

        public override void Update()
        {
            Applier?.Invoke(Parsed);
        }
    }

    public class Vec2 : DynamicDataWatcher
    {
        public Vec2(string field) : base(field){}
        
        public Vector2 Parsed => (Vector2)Value;
        
        public Action<Vc2> Applier = null;

        public override void Update()
        {
            Applier?.Invoke(Parsed);
        }
    }
}

public class GeneralDynamicDataWatcher<T> : BaseComponent where T : IConvertible
{
    public override bool Equals(object obj)
    {
        if (obj is GeneralDynamicDataWatcher<T> watcher)
        {
            return this.Field == watcher.Field;
        }
        
        return base.Equals(obj);
    }
    
    public override int GetHashCode()
    {
        return Field.GetHashCode();
    }

    public static bool operator ==(GeneralDynamicDataWatcher<T> a, GeneralDynamicDataWatcher<T> b)
    {
        return a.Field.Equals(b.Field);
    }
    
    public static bool operator !=(GeneralDynamicDataWatcher<T> a, GeneralDynamicDataWatcher<T> b)
    {
        return !(a == b);
    }

    public GeneralDynamicDataWatcher(string field)
    {
        Field = field;
    }

    public string Field;
    
    private DynamicData data = null;

    // public override void EntityAdded(Scene scene)
    // {
    //     if (Entity != null)
    //     {
    //         data = new DynamicData(Entity);
    //     }
    // }

    public override void EntityAwake()
    {
        if (Entity != null)
        {
            data = new DynamicData(Entity);
        }
    }

    public T Value => (T)data.Get(Field);
}