using System;

namespace YoctoHelper.Hooks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class DefaultValue : Attribute
{
}
