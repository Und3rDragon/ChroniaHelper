using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagSerialTrigger")]
public class FlagSerialTrigger : FlagManageTrigger
{
    public FlagSerialTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;

        serialFlag = data.Attr("serialFlag");
        targetSymbol = data.Attr("targetSymbol", "&");
        startIndex = data.Int("startIndex", 0);
        totalIndexes = data.Int("steps", 10);
        interval = data.Float("interval", 0.1f);
        posMode = data.Enum("positionMode", PositionModes.NoEffect);
        staircase = string.IsNullOrEmpty(data.Attr("staircase")) ? false : data.Bool("staircase", false);
    }
    private int ID;
    private string serialFlag, targetSymbol;
    private float interval;
    private int startIndex, totalIndexes;
    private PositionModes posMode;
    private bool staircase;

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        // clear all array flags when enter
        for (int j = startIndex; j < startIndex + totalIndexes; j++)
        {
            FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, j.ToString()), false);
        }

        if (posMode != PositionModes.NoEffect)
        {
            yield break;
        }
        // will not execute if position mode is active

        for (int i = startIndex; i < startIndex + totalIndexes; i++)
        {
            if (!staircase)
            {
                FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, (Math.Max(i - 1, startIndex)).ToString()), false);
            }

            FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, i.ToString()), true);

            yield return interval;
        }

    }

    protected override void OnStayExecute(Player player)
    {
        if(posMode == PositionModes.NoEffect)
        {
            return;
        }
        // will not execute if position mode is disabled

        float lerp = GetPositionLerp(player, posMode);

        int index = startIndex + (int)Math.Floor(lerp * totalIndexes);
        index = Math.Clamp(index, startIndex, startIndex + totalIndexes - 1);

        // this has been done when enter
        //for (int j = startIndex; j < startIndex + totalIndexes; j++)
        //{
        //    FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, j.ToString()), false);
        //}

        if (staircase)
        {
            for(int i = startIndex; i < startIndex + totalIndexes; i++)
            {
                FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, i.ToString()), i <= index);
            }
        }
        else
        {
            for (int i = startIndex; i < startIndex + totalIndexes; i++)
            {
                FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, i.ToString()), i == index);
            }
        }
    }
}
