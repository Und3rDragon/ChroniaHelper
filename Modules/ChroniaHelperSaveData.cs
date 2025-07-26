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

    // Database?
    public static Dictionary<string, bool> ChroniaFlag_Active = new();
    public static Dictionary<string, bool> ChroniaFlag_Global = new();
    public static Dictionary<string, bool> ChroniaFlag_Temporary = new();
    public static Dictionary<string, bool> ChroniaFlag_ForceRefresh = new();
    public static Dictionary<string, float> ChroniaFlag_Timed = new();
    public static Dictionary<string, int> ChroniaFlag_DefaultReset = new();
    public static Dictionary<string, List<string>> ChroniaFlag_Tags = new();
    public static Dictionary<string, Dictionary<string, string>> ChroniaFlag_CustomData = new();
    public static List<string> ChroniaFlag_BuiltInTags = new() { "serial" };
    public static Dictionary<string, List<string>> ChroniaFlag_PresetTags = new();

}
