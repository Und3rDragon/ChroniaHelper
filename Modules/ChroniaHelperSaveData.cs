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

    // New flag system
    public static Dictionary<string, ChroniaFlag> ChroniaFlags = new();

    public static Dictionary<string, float> Floats = new();

    // New flag system saving
    public struct ChroniaFlagData
    {
        public static Dictionary<string, bool> Active = new();
        public static Dictionary<string, bool> Global = new();
        public static Dictionary<string, bool> Temporary = new();
        public static Dictionary<string, bool> Force = new();
        public static Dictionary<string, float> Timed = new();
        public static Dictionary<string, int> DefaultResetState = new();
        public static Dictionary<string, List<string>> Tags = new();
        public static Dictionary<string, Dictionary<string, string>> CustomData = new();
        public static Dictionary<string, List<int>> PresetTags = new();
    }
}
