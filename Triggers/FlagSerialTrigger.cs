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
        if(posMode != PositionModes.NoEffect)
        {
            yield break;
        }

        for (int i = startIndex; i < startIndex + totalIndexes; i++)
        {
            if (!staircase || i == startIndex)
            {
                for (int j = startIndex; j < startIndex + totalIndexes; j++)
                {
                    FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, j.ToString()), false);
                }
            }

            FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, i.ToString()), true);

            yield return interval;
        }

    }

    protected override void OnStayExecute(Player player)
    {
        float lerp = GetPositionLerp(player, posMode);

        if(posMode == PositionModes.NoEffect)
        {
            return;
        }

        int index = startIndex + (int)Math.Floor(lerp * totalIndexes);

        for (int j = startIndex; j < startIndex + totalIndexes; j++)
        {
            FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, j.ToString()), false);
        }

        if (staircase)
        {
            for(int i = startIndex; i <= index; i++)
            {
                FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, Math.Clamp(i, 0, startIndex + totalIndexes - 1).ToString()), true);
            }
        }
        else
        {
            FlagUtils.SetFlag(serialFlag.Replace(targetSymbol, Math.Clamp(index, 0, startIndex + totalIndexes - 1).ToString()), true);
        }
    }
}
