using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Modules;

public class ChroniaHelperGlobalSaveData : ChroniaHelperModuleGlobalSaveData
{
    public HashSet<string> flags { get; set; }= new();
    public Dictionary<string, int> counters { get; set; }= new();
    public Dictionary<string, float> sliders { get; set; } = new();
    public Dictionary<string, string> permaKeys { get; set; } = new();

    [ChroniaGlobalSavePath("MapHider.yaml")]
    public List<string> HelperMapsToHide { get; set; } = new List<string> {
        "AltSidesHelper",
        "bitsbolts",
        "BounceHelper",
        "CustomPoints",
        "HonlyHelper",
        "JackalHelper",
        "SusanHelper"
    };
}
