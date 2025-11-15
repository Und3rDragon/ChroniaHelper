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
    // 默认保存到 ChroniaHelperGlobalSaveData.xml
    //public string SomeEntry = "hahaha";
    //public int SomeNumber = 1;
    //public float SomeFloat = 2f;

    // 保存到 Saves/ChroniaHelper/ChroniaGaming/SomeData.xml
    //[ChroniaGlobalSavePath("ChroniaGaming/SomeData.xml")]
    //public Dictionary<string, int> AchievementProgress = new();

    //[ChroniaGlobalSavePath("PlayerStats/Points.xml")]
    //public BigInteger TotalPoints = BigInteger.Zero;
}
