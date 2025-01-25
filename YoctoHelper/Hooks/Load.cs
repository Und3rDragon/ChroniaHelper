using System;

namespace YoctoHelper.Hooks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class Load : Attribute
{
}
