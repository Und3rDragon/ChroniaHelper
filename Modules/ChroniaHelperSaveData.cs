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

    [Note("For Chronia Flag-Counter-Slider System")]
    public HashSet<string> flags = new();
    [Note("For Chronia Flag-Counter-Slider System")]
    public Dictionary<string, int> counters = new();
    [Note("For Chronia Flag-Counter-Slider System")]
    public Dictionary<string, float> sliders = new();
    [Note("For Chronia Flag-Counter-Slider System")]
    public Dictionary<string, string> keystrings = new();
    [Note("For Chronia Flag-Counter-Slider System")]
    public Dictionary<string, ChroniaColor> chroniaColors = new();

    // Flag Packer
    public Dictionary<string, List<string>> PackedFlags = new();
    public Dictionary<string, List<string>> CurrentPackedFlags = new();

    // Stopwatch
    public Dictionary<string, Stopclock> stopclocks = new();
}
