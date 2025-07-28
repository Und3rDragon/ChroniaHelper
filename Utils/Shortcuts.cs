using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;

namespace ChroniaHelper.Utils;

public static class Shortcuts
{
    public static Dictionary<string, ChroniaFlag> ChroniaFlags(this object obj) { return ChroniaHelperSaveData.ChroniaFlags; }
    public static Dictionary<string, float> Floats(this object obj){ return ChroniaHelperSaveData.Floats; }
    public static Level Level(this object obj) { return MapProcessor.level; }
    public static Session Session(this object obj) { return MapProcessor.session; }
    public static HashSet<string> Flags(this object obj) { return MapProcessor.session.Flags; }

    public class Save : ChroniaHelperSaveData
    {
        
    }

    public class Ses : ChroniaHelperSession
    {

    }
}
