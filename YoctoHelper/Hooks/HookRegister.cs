using System;

namespace YoctoHelper.Hooks;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class HookRegister : Attribute
{

    public HookId id { get; private set; }

    public bool useData { get; private set; }

    public HookRegister(HookId id, bool useData = false)
    {
        this.id = id;
        this.useData = useData;
    }

}
