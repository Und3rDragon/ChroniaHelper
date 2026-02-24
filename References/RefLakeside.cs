using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.LakeSideCode;
using Celeste.Mod.LakeSideCode.FishDefs;

namespace ChroniaHelper.References;

public static class RefLakeside
{
    public static LakeSideCodeModule GetModule()
    {
        return LakeSideCodeModule.Instance;
    }

    public static LakeSideCodeModuleSaveData GetSaveData()
    {
        return LakeSideCodeModule.SaveData;
    }

    public static LakeSideCodeModuleSession GetSession()
    {
        return LakeSideCodeModule.Session;
    }

    public static LakeSideCodeModuleSettings GetSettings()
    {
        return LakeSideCodeModule.Settings;
    }

    public static Dictionary<FishType, int> FishCounters => GetSession().CatchCounter;

    public static int FishPrice(FishType fish) => (fish) switch 
    { 
        FishType.Nothing => 0,
        FishType.Coin => 100,
        FishType.Bass => 120,
        FishType.Trout => 140,
        FishType.Spring => 160,
        FishType.Stone => 180,
        FishType.StoneEater => 200,
        FishType.Blahaj => 220,
        FishType.Bomb => 250,
        FishType.Leaf => 280,
        FishType.Angel => 350,
        FishType.Devil => 400,
        FishType.Cooked => 600,
        FishType.Mythic => 1000,
        _ => 0,
    };
}
