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
[CustomEntity("ChroniaHelper/FlagArrayTrigger")]
public class FlagArrayTrigger : FlagManageTrigger
{
    public FlagArrayTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;

        flags = data.Attr("flags").Split(",", StringSplitOptions.TrimEntries);
        string[] t = data.Attr("intervals").Split(",", StringSplitOptions.TrimEntries);
        if (t.Length == 0 || string.IsNullOrEmpty(data.Attr("intervals"))) { t = new string[]{ "0.1" }; }
        intervals = new float[t.Length];
        for(int i = 0; i < t.Length; i++)
        {
            float d = 0.1f;
            float.TryParse(t[i], out d);
            intervals[i] = d;
        }
        posMode = data.Enum("positionMode", PositionModes.NoEffect);
        //staircase = string.IsNullOrEmpty(data.Attr("staircase")) ? false : data.Bool("staircase", false);
        staircase = Util.PrepareData(data.Attr("staircase"), false);
    }
    private int ID;
    private string[] flags;
    private float[] intervals;
    private PositionModes posMode;
    private bool staircase;

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        // clear all array flags when enter
        for (int i = 0; i < flags.Length; i++)
        {
            FlagUtils.SetFlag(flags[i], false);
        }

        if (posMode != PositionModes.NoEffect)
        {
            yield break;
        }
        // will not execute if position mode enabled
        
        for(int i = 0; i < flags.Length; i++)
        {
            if (!staircase)
            {
                FlagUtils.SetFlag(flags[Math.Clamp(i - 1, 0, flags.Length - 1)], false);
            }
            FlagUtils.SetFlag(flags[i], true);

            yield return intervals[Math.Clamp(i, 0, intervals.Length - 1)];
        }
    }

    protected override void OnStayExecute(Player player)
    {
        if(posMode == PositionModes.NoEffect)
        {
            return;
        }
        // will not execute if position mode disabled

        float lerp = GetPositionLerp(player, posMode);
        int index = (int)Math.Floor(lerp * flags.Length);
        index = Math.Clamp(index, 0, flags.Length - 1);

        // this has been done when enter
        //for (int i = 0; i < flags.Length; i++)
        //{
        //    FlagUtils.SetFlag(flags[i], false);
        //}

        if (staircase)
        {
            for(int i = 0; i < flags.Length; i++)
            {
                FlagUtils.SetFlag(flags[i], i <= index);
            }
        }
        else
        {
            for (int i = 0; i < flags.Length; i++)
            {
                FlagUtils.SetFlag(flags[i], i == index);
            }
        }
    }
}
