using System;
using System.Collections.Generic;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.StopwatchSystem;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSaveData : EverestModuleSaveData
{
    // Flag Timer Trigger
    public Dictionary<string, float> FlagTimerS = new();

    public HashSet<string> flags = new();
    public Dictionary<string, int> counters = new();
    public Dictionary<string, float> sliders = new();

    // Stopwatch
    public Dictionary<string, Stopclock> globalStopwatches = new();
}
