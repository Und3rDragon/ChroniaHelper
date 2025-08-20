using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagTimerTrigger")]
public class FlagTimerTrigger : BaseTrigger
{
    private int ID;

    private enum Range { room, map, saves }
    private Range range;
    private enum Mode { set, add, minus }
    private Mode mode;

    public FlagTimerTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;

        string[] setups = data.Attr("setups").Split(';',StringSplitOptions.TrimEntries);
        for(int i = 0; i< setups.Length; i++)
        {
            string[] pairs = setups[i].Split(",", StringSplitOptions.TrimEntries);
            if (pairs.Length < 2) continue;
            if (string.IsNullOrEmpty(pairs[0]) || string.IsNullOrEmpty(pairs[1])) continue;

            float time = 0f;
            if (!float.TryParse(pairs[1], out time)) continue;

            timedFlagset.Enter(pairs[0], time);
        }

        range = (Range)data.Fetch("range", 0);
        mode = (Mode)data.Fetch("mode", 0);
    }
    private Dictionary<string, float> timedFlagset = new();
    private Dictionary<string, float> timedFlags = new();

    protected override void OnEnterExecute(Player player)
    {
        foreach(var flag in timedFlagset.Keys)
        {
            ChroniaFlagUtils.SetFlag(flag, true);
        }

        if(range == Range.room)
        {
            foreach (var item in timedFlagset.Keys)
            {
                if (timedFlags.ContainsKey(item))
                {
                    if(mode == Mode.set)
                    {
                        timedFlags[item] = timedFlagset[item];
                    }
                    else if(mode == Mode.add)
                    {
                        timedFlags[item] += timedFlagset[item];
                    }
                    else if(mode == Mode.minus)
                    {
                        timedFlags[item] -= timedFlagset[item];
                    }
                }
                else
                {
                    if(mode != Mode.minus)
                    {
                        timedFlags.Add(item, timedFlagset[item]);
                    }
                }
            }
        }

        else if(range == Range.map)
        {
            foreach (var flag in timedFlagset.Keys)
            {
                if (Md.Session.FlagTimer.ContainsKey(flag))
                {
                    if(mode == Mode.set)
                    {
                        Md.Session.FlagTimer[flag] = timedFlagset[flag];
                    }
                    else if (mode == Mode.add)
                    {
                        Md.Session.FlagTimer[flag] += timedFlagset[flag];
                    }
                    else if (mode == Mode.minus)
                    {
                        Md.Session.FlagTimer[flag] -= timedFlagset[flag];
                    }
                }
                else
                {
                    if(mode != Mode.minus)
                    {
                        Md.Session.FlagTimer.Add(flag, timedFlagset[flag]);
                    }
                }
            }
        }

        else if (range == Range.saves)
        {
            foreach (var flag in timedFlagset.Keys)
            {
                if (Md.SaveData.FlagTimerS.ContainsKey(flag))
                {
                    if (mode == Mode.set)
                    {
                        Md.SaveData.FlagTimerS[flag] = timedFlagset[flag];
                    }
                    else if (mode == Mode.add)
                    {
                        Md.SaveData.FlagTimerS[flag] += timedFlagset[flag];
                    }
                    else if (mode == Mode.minus)
                    {
                        Md.SaveData.FlagTimerS[flag] -= timedFlagset[flag];
                    }
                }
                else
                {
                    if (mode != Mode.minus)
                    {
                        Md.SaveData.FlagTimerS.Add(flag, timedFlagset[flag]);
                    }
                }
            }
        }
    }

    public override void Removed(Scene scene)
    {
        if(range == Range.room)
        {
            foreach (var flag in timedFlagset.Keys)
            {
                ChroniaFlagUtils.SetFlag(flag, false);
            }
        }
    }

    public override void Update()
    {
        base.Update();

        foreach (var flag in timedFlags.Keys)
        {
            timedFlags[flag] = Calc.Approach(timedFlags[flag], 0f, Engine.DeltaTime);
            if(range == Range.room && timedFlags[flag] == 0f)
            {
                ChroniaFlagUtils.SetFlag(flag, false);
            }
        }
    }
}

