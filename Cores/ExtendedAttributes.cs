using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class LoadHook : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class UnloadHook : Attribute
{
}
