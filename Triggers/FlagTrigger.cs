using System;
using System.Diagnostics.Tracing;
using System.Xml;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;
using YoctoHelper.Cores;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagTrigger")]
public class FlagTrigger : Trigger
{
    public bool set;
    public string[] flagList;
    public List<string> trueFlags, falseFlags;
    public bool temp;
    public bool saves;
    public bool reset;
    public bool filtering;
    public bool onStay;

    private int ID;

    public FlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;
        
        set = data.Bool("set", true);
        temp = data.Bool("temporary", false);
        saves = data.Bool("global", false);
        reset = data.Bool("resetOnLeave", false);
        filtering = data.Bool("ignoreUnchanged", true);
        onStay = data.Bool("onStay", false);

        // flag processing
        string input = data.Attr("flag", "Flag");
        flagList = input.Split(',', StringSplitOptions.TrimEntries);
        trueFlags = new List<string>(); 
        falseFlags = new List<string>();
        foreach (var item in flagList)
        {
            bool s = item.StartsWith('!');
            if (!s)
            {
                trueFlags.Add(item);
            }
            else
            {
                falseFlags.Add(item.TrimStart('!'));
            }
        }
    }

    public void NormalSetup()
    {
        foreach (var item in trueFlags)
        {
            Utils.FlagUtils.SetFlag(item, set, saves);
        }
        foreach (var item in falseFlags)
        {
            Utils.FlagUtils.SetFlag(item, !set, saves);
        }
    }

    private Dictionary<string, ChroniaHelperSession.ChroniaFlag> RecordedStates = new();
    public void RecordState()
    {
        foreach(var item in flagList)
        {
            bool state = Utils.FlagUtils.GetFlag(item);
            bool global = Utils.FlagUtils.GetFlag(item, Utils.FlagUtils.CheckFlag.CheckGlobal);
            ChroniaHelperSession.ChroniaFlag chroniaflag = new()
            {
                flagID = item,
                flagState = state,
                isGlobal = global
            };

            if (RecordedStates.ContainsKey(item))
            {
                RecordedStates[item] = chroniaflag;
            }
            else
            {
                RecordedStates.Add(item, chroniaflag);
            }
        }
    }

    public void LoadState()
    {
        foreach(var item in RecordedStates.Keys)
        {
            bool state = RecordedStates[item].flagState;
            bool global = RecordedStates[item].isGlobal;

            Utils.FlagUtils.SetFlag(item, state, global);
        }

        RecordedStates.Clear();
    }

    public void ReverseSetup(bool filtering)
    {
        if (filtering)
        {
            LoadState();
        }
        else
        {
            foreach (var item in trueFlags)
            {
                Utils.FlagUtils.SetFlag(item, !set, saves);
            }
            foreach (var item in falseFlags)
            {
                Utils.FlagUtils.SetFlag(item, set, saves);
            }
        }
        
    }

    private void ListAsTemporary()
    {
        foreach (var item in flagList)
        {
            bool state = Utils.FlagUtils.GetFlag(item);
            bool global = saves;

            ChroniaHelperSession.ChroniaFlag chroniaflag = new()
            {
                flagID = item,
                flagState = state,
                isGlobal = global,
            };

            if (ChroniaHelperSession.TemporaryFlags.ContainsKey(item))
            {
                ChroniaHelperSession.TemporaryFlags[item] = chroniaflag;
            }
            else
            {
                ChroniaHelperSession.TemporaryFlags.Add(item, chroniaflag);
            }
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        RecordState();

        if (temp)
        {
            ListAsTemporary();
        }

        NormalSetup();
    }

    public override void OnStay(Player player)
    {
        if (onStay)
        {
            NormalSetup();
        }
    }

    public override void OnLeave(Player player)
    {
        if (reset)
        {
            ReverseSetup(filtering);
        }
    }
}
