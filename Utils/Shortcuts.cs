global using FI = ChroniaHelper.Imports.FrostHelperImports;
global using MaP = ChroniaHelper.Cores.MapProcessor;
global using Md = ChroniaHelper.ChroniaHelperModule;
global using Sav = ChroniaHelper.Modules.ChroniaHelperSaveData;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Shortcuts;

public static class Shortcuts
{
    public static Dictionary<string, ChroniaFlag> ChroniaFlags(this object obj)
    {
        return Md.SaveData.ChroniaFlags;
    }
}