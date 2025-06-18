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
            item.SetFlag(set, saves);
        }
        foreach (var item in falseFlags)
        {
            item.SetFlag(!set, saves);
        }
    }

    private Dictionary<string, ChroniaFlag> RecordedStates = new();
    private List<string> Unchanged = new();
    public void RecordState()
    {
        foreach (var item in trueFlags)
        {
            RecordedStates.Enter(item, new(item, item.GetFlag(), item.PullFlag().Global));
            if(item.GetFlag() == set)
            {
                Unchanged.Enter(item);
            }
        }
        foreach (var item in falseFlags)
        {
            RecordedStates.Enter(item, new(item, item.GetFlag(), item.PullFlag().Global));
            if(item.GetFlag() == !set)
            {
                Unchanged.Enter(item);
            }
        }
    }

    public void LoadState(bool filtering)
    {
        if (!filtering)
        {
            foreach (var item in trueFlags)
            {
                item.SetFlag(!set, saves);
            }
            foreach (var item in falseFlags)
            {
                item.SetFlag(set, saves);
            }
        }
        else
        {
            foreach (var item in RecordedStates.Values)
            {
                if (filtering && !Unchanged.Contains(item.Name))
                {
                    item.Name.SetFlag(item.Active, item.Global);
                }
            }
        }

        RecordedStates.Clear();
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

        NormalSetup();

        if (temp)
        {
            foreach (var item in trueFlags)
            {
                if (item.Check())
                {
                    ChroniaHelperSaveData.ChroniaFlags[item].Temporary = true;
                }
            }
            foreach(var item in falseFlags)
            {
                if (item.Check())
                {
                    ChroniaHelperSaveData.ChroniaFlags[item].Temporary = true;
                }
            }
            foreach (var item in RecordedStates.Values)
            {
                item.Temporary = true;
            }
        }
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
            LoadState(filtering);
        }
    }
}
