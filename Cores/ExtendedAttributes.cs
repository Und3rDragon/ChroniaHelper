﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public static class ExtendedAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class LoadHook : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class UnloadHook : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SelectiveLoadHook : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SelectiveUnloadHook : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ChroniaGlobalSavePath : Attribute
    {
        public string RelativePath { get; }

        public ChroniaGlobalSavePath(string relativePath = "ChroniaHelperGlobalSaveData.xml")
        {
            RelativePath = relativePath;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class WorkingInProgress : Attribute
    {
        public WorkingInProgress(params string[] note) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class Note : Attribute
    {
        public Note(params string[] note) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class Obsoleted : Attribute
    {
        public Obsoleted(params string[] note) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class PrivateFor : Attribute
    {
        public PrivateFor(params string[] modOrAuthorName) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class Credits : Attribute
    {
        public Credits(params string[] creditsInfo) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RegistryHandler : Attribute
    {
        public RegistryHandler(params string[] notes)
        {
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class Todo : Attribute
    {
        public Todo(params string[] creditsInfo) { }
    }
}
