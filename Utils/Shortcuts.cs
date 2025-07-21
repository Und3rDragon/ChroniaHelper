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
    public static Dictionary<string, ChroniaFlag> ChroniaFlags(this object obj)
    {
        return ChroniaHelperSaveData.ChroniaFlags;
    }

    public static ChroniaHelperSaveData SaveData(this object obj)
    {
        return (ChroniaHelperSaveData)ChroniaHelperModule.Instance._SaveData;
    }

    public static ChroniaHelperSession Session(this object obj)
    {
        return (ChroniaHelperSession)ChroniaHelperModule.Instance._Session;
    }

    public static Level Level(this object obj)
    {
        return MapProcessor.level;
    }

    public static Session LevelSession(this object obj)
    {
        return MapProcessor.session;
    }

    public static HashSet<string> LevelFlags(this object obj)
    {
        return 0.LevelSession().Flags;
    }

}
