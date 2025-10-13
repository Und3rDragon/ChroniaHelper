using System;
using System.Collections.Generic;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSaveData : EverestModuleSaveData
{
    // Flag Timer Trigger
    public Dictionary<string, float> FlagTimerS = new();

    // New flag system
    public Dictionary<string, ChroniaFlag> ChroniaFlags = new();
    public Dictionary<string, ChroniaCounter> ChroniaCounters = new();
    public Dictionary<string, ChroniaSlider> ChroniaSliders = new();

    // Stopwatch
    public Dictionary<string, Stopclock> globalStopwatches = new();

    // Backup Simplified Flag System (Unused)
    //public HashSet<string> GlobalFlags = new();
    //public HashSet<string> ForcedFlags = new();
    //public Dictionary<string, float> FlagTimers = new();
}
