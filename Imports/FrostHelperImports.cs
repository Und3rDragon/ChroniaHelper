#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

[ModImportName("FrostHelper")] // registered in Module
public static class FrostHelperImports
{
    public static Func<string, Type?> EntityNameToTypeOrNull;

    public static Func<string, Type[]> GetTypes;
}