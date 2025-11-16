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
    public string Name { get; set; } = "123";
    public int hhh { get; set; } = 2;

    [ChroniaGlobalSavePath]
    public bool lol { get; set; } = false;
    [ChroniaGlobalSavePath]
    public int hhh2 { get; set; } = 3;

    [ChroniaGlobalSavePath("Gaming/Haha.xml")]
    public int hhh3 { get; set; } = 4;
}
