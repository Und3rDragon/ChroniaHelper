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

[AttributeUsage(AttributeTargets.All)]
public class WorkingInProgressAttribute : Attribute
{
    public WorkingInProgressAttribute(params string[] note) { }
}

[AttributeUsage(AttributeTargets.All)]
public class NoteAttribute : Attribute
{
    public NoteAttribute(params string[] note) { }
}

[AttributeUsage(AttributeTargets.All)]
public class ObsoletedAttribute : Attribute
{
    public ObsoletedAttribute(params string[] note) { }
}

[AttributeUsage(AttributeTargets.All)]
public class PrivateForAttribute : Attribute
{
    public PrivateForAttribute(params string[] modOrAuthorName) { }
}

[AttributeUsage(AttributeTargets.All)]
public class CreditsAttribute : Attribute
{
    public CreditsAttribute(params string[] creditsInfo) { }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RegistryHandlerAttribute : Attribute
{
    public RegistryHandlerAttribute(params string[] notes)
    {
    }
}

[AttributeUsage(AttributeTargets.All)]
public class TodoAttribute : Attribute
{
    public TodoAttribute(params string[] creditsInfo) { }
}
