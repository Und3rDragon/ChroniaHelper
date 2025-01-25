using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagRandomTrigger")]
public class FlagRandomTrigger : FlagManageTrigger
{

    private Dictionary<string[], float> randomFlagDictionary;

    private List<KeyValuePair<string[], float>> rangeFlagList;

    private float rangeMax;

    private string[] randomFlag;

    public FlagRandomTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.randomFlagDictionary = FlagUtils.ParseRandom(data.Attr("randomFlagDictionary", null));
        this.rangeFlagList = new List<KeyValuePair<string[], float>>();
        this.rangeMax = 0F;
        foreach (KeyValuePair<string[], float> pair in this.randomFlagDictionary)
        {
            this.rangeMax += pair.Value;
            this.rangeFlagList.Add(new KeyValuePair<string[], float>(pair.Key, this.rangeMax));
        }
        this.rangeFlagList.Sort((x, y) => y.Value.CompareTo(x.Value));
    }

    protected override void OnEnterExecute(Player player)
    {
        float randomValue = new Random().NextFloat() * this.rangeMax;
        foreach (KeyValuePair<string[], float> pair in this.rangeFlagList)
        {
            if (randomValue <= pair.Value)
            {
                this.randomFlag = pair.Key;
            }
        }
        base.Add(this.randomFlag);
    }

    protected override void LeaveReset(Player player)
    {
        base.Remove(this.randomFlag);
    }

}
