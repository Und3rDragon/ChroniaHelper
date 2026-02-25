using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Celeste.Mod.LakeSideCode.FishDefs;
using ChroniaHelper.Cores;
using ChroniaHelper.References;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/FishPriceController")]
public class FishPriceController : BaseEntity
{
    public FishPriceController(EntityData data, Vc2 offset):base(data, offset)
    {
        prices = new();

        prices.PriceList = new()
        {
            { 0, data.Int("DefaultPrize", 0) },
            { 1, data.Int("FishCoinPrize", 20) },
            { 2, data.Int("FishBassPrize", 100) },
            { 3, data.Int("FishTroutPrize", 100) },
            { 4, data.Int("FishSpringPrize", 150) },
            { 5, data.Int("FishStonePrize", 180) },
            { 6, data.Int("FishStoneEaterPrize", 200) },
            { 7, data.Int("FishBlahajPrize", 220) },
            { 8, data.Int("FishBombPrize", 250) },
            { 9, data.Int("FishLeafPrize", 280) },
            { 10, data.Int("FishAngelPrize", 350) },
            { 11, data.Int("FishDevilPrize", 400) },
            { 12, data.Int("FishCookedPrize", 600) },
            { 13, data.Int("FishMythicPrize", 1000) },
        };

        prices.Variations = new()
        {
            { 0, data.Int("DefaultRandomness", 0) },
            { 1, data.Int("FishCoinRandomness", 0) },
            { 2, data.Int("FishBassRandomness", 10) },
            { 3, data.Int("FishTroutRandomness", 10) },
            { 4, data.Int("FishSpringRandomness", 20) },
            { 5, data.Int("FishStoneRandomness", 20) },
            { 6, data.Int("FishStoneEaterRandomness", 40) },
            { 7, data.Int("FishBlahajRandomness", 40) },
            { 8, data.Int("FishBombRandomness", 50) },
            { 9, data.Int("FishLeafRandomness", 80) },
            { 10, data.Int("FishAngelRandomness", 100) },
            { 11, data.Int("FishDevilRandomness", 100) },
            { 12, data.Int("FishCookedRandomness", 150) },
            { 13, data.Int("FishMythicRandomness", 200) },
        };
    }
    private RefLakeside.FishPrices prices;
    public const string LakeSideCreditsCounter = "ChroniaHelper_LakesideFishCredits";

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if(scene.Tracker.GetEntities<FishPriceController>().Count != 0)
        {
            RemoveSelf();
            return;
        }
    }

    public override void Update()
    {
        base.Update();

        if (!Md.LakeSideLoaded) { return; }

        prices.GeneratePriceList(RefLakeside.FishCounters);
        LakeSideCreditsCounter.SetCounter(prices.GetTotalCredits());
    }
}
