using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.LakeSideCode;
using Celeste.Mod.LakeSideCode.FishDefs;
using ChroniaHelper.Utils;

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

    public class FishPrices
    {
        public Dictionary<int, int> PriceList = new()
        {
            { (int)FishType.Nothing, 0 },
            { (int)FishType.Coin, 20 },
            { (int)FishType.Bass, 100 },
            { (int)FishType.Trout, 100 },
            { (int)FishType.Spring, 150 },
            { (int)FishType.Stone, 180 },
            { (int)FishType.StoneEater, 200 },
            { (int)FishType.Blahaj, 220 },
            { (int)FishType.Bomb, 250 },
            { (int)FishType.Leaf, 280 },
            { (int)FishType.Angel, 350 },
            { (int)FishType.Devil, 400 },
            { (int)FishType.Cooked, 600 },
            { (int)FishType.Mythic, 1000 },
        };

        public Dictionary<int, int> Variations = new()
        {
            { (int)FishType.Nothing, 0 },
            { (int)FishType.Coin, 0 },
            { (int)FishType.Bass, 10 },
            { (int)FishType.Trout, 10 },
            { (int)FishType.Spring, 20 },
            { (int)FishType.Stone, 20 },
            { (int)FishType.StoneEater, 40 },
            { (int)FishType.Blahaj, 40 },
            { (int)FishType.Bomb, 50 },
            { (int)FishType.Leaf, 80 },
            { (int)FishType.Angel, 100 },
            { (int)FishType.Devil, 100 },
            { (int)FishType.Cooked, 150 },
            { (int)FishType.Mythic, 200 },
        };

        public Dictionary<int, List<int>> PriceEntries = new();

        public void GeneratePriceList(Dictionary<FishType, int> counter)
        {
            foreach(var item in counter)
            {
                int type = (int)item.Key;
                int count = item.Value;

                for(int i = 0; i < count; i++)
                {
                    PriceEntries[type].Create(i, 
                        PriceList[type] + RandomUtils.RandomInt(Variations[type]) - Variations[type] / 2,
                        PriceList[type] + RandomUtils.RandomInt(Variations[type]) - Variations[type] / 2
                    );
                }
            }
        }

        public int GetTotalCredits()
        {
            int total = 0;
            foreach(var item in PriceEntries)
            {
                int credits = 0;
                foreach(var entry in item.Value)
                {
                    credits += entry;
                }

                total += credits;
            }

            return total;
        }

        public int GetToralCredits(FishType fish)
        {
            int total = 0;
            foreach (var item in PriceEntries.GetValueOrDefault((int)fish, new()))
            {
                total += item;
            }

            return total;
        }
    }
}
