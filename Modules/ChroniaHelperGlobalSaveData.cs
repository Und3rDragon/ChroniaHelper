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
    [ChroniaGlobalSavePath("MapHider.xml")]
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
