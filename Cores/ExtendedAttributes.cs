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

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ChroniaGlobalSavePathAttribute : Attribute
{
    public string RelativePath { get; }

    public ChroniaGlobalSavePathAttribute(string relativePath = "ChroniaHelperGlobalSaveData.xml")
    {
        RelativePath = relativePath;
    }
}
