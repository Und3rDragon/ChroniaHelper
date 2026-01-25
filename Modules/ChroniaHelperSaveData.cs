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

    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public HashSet<string> flags = new();
    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public Dictionary<string, int> counters = new();
    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public Dictionary<string, float> sliders = new();

    public Dictionary<string, string> keystrings = new();

    // Flag Packer
    public Dictionary<string, List<string>> PackedFlags = new();
    public Dictionary<string, List<string>> CurrentPackedFlags = new();

    // Stopwatch
    public Dictionary<string, Stopclock> stopclocks = new();
}
