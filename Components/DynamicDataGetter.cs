using ChroniaHelper.Cores;
using MonoMod.Utils;

namespace ChroniaHelper.Components;

public class DynamicDataGetter<T> : BaseComponent
{
    public DynamicDataGetter(string field)
    {
        Field = field;
    }

    public string Field;

    private DynamicData data = null;

    public object Value => data.Get(Field);
    public T Parsed => (T)Value;
    public Action<T> Applier = null;

    public override void Update()
    {
        Applier?.Invoke(Parsed);
    }
}