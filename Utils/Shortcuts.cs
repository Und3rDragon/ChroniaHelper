global using FI = ChroniaHelper.Imports.FrostHelperImports;
global using MaP = ChroniaHelper.Cores.MapProcessor;
global using Md = ChroniaHelper.ChroniaHelperModule;
global using Sav = ChroniaHelper.Modules.ChroniaHelperSaveData;
global using Ses = ChroniaHelper.Modules.ChroniaHelperSession;
global using Sh = ChroniaHelper.Utils.Shortcuts;
global using PUt = ChroniaHelper.Utils.PlayerUtils;
using System.Runtime.InteropServices.Marshalling;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Utils;

public static class Shortcuts
{
    public static Dictionary<string, ChroniaFlag> CFlags => Md.SaveData.ChroniaFlags;
    public static Dictionary<string, ChroniaSlider> CSliders => Md.SaveData.ChroniaSliders;
    public static Dictionary<string, ChroniaCounter> CCounters => Md.SaveData.ChroniaCounters;

    public static Sav Sav => Md.SaveData;
    public static Ses Ses => Md.Session;
}