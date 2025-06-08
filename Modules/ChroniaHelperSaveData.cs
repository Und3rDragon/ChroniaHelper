using System;
using System.Collections.Generic;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSaveData : EverestModuleSaveData
{
    public Dictionary<string, bool> globalflags { get; set; } = new Dictionary<string, bool>();

    // FLag Timer Trigger
    public Dictionary<string, float> FlagTimerS = new();
}
