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
    public WorkingInProgressAttribute(string note = "") { }
}

[AttributeUsage(AttributeTargets.All)]
public class NoteAttribute : Attribute
{
    public NoteAttribute(string note = "") { }
}

[AttributeUsage(AttributeTargets.All)]
public class VersionNoteAttribute : Attribute
{
    public VersionNoteAttribute(int x, int y, int z, string note = "") { }
}

[AttributeUsage(AttributeTargets.All)]
public class PrivateForAttribute : Attribute
{
    public PrivateForAttribute(string modOrAuthorName) { }
}
