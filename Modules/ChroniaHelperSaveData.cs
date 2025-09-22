using System;
using System.Collections.Generic;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
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
}
