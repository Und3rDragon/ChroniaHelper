using System;
using System.Collections.Generic;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSaveData : EverestModuleSaveData
{
    // Flag Timer Trigger
    public static Dictionary<string, float> FlagTimerS = new();

    // wip: new flag system?
    public static Dictionary<string, ChroniaFlag> ChroniaFlags = new();
}
