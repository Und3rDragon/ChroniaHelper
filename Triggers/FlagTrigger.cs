using System;
using System.Diagnostics.Tracing;
using System.Xml;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagTrigger")]
public class FlagTrigger : BaseTrigger
{
    public bool set;
    public string[] flagList;
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
    }

    public Dictionary<string, bool> records = new();
    protected override void OnEnterExecute(Player player)
    {
        foreach (var item in flagList)
        {
            bool revert = item.StartsWith("!");
            string name = item.TrimStart('!');
            bool defState = revert ? !set : set;

            if (filtering && defState == name.GetFlag()) { continue; }

            name.SetFlag(defState, saves, temp);
            records.Enter(name, defState);
        }
    }

    protected override void OnStayExecute(Player player)
    {
        if (onStay)
        {
            foreach (var item in flagList)
            {
                bool revert = item.StartsWith("!");
                string name = item.TrimStart('!');
                bool defState = revert ? !set : set;

                if (filtering && defState == name.GetFlag()) { continue; }

                name.SetFlag(defState, saves, temp);
                records.Enter(name, defState);
            }
        }
    }

    protected override void LeaveReset(Player player)
    {
        if (reset)
        {
            foreach(var item in records.Keys)
            {
                item.SetFlag(!records[item], saves, temp);
            }
            records.Clear();
        }
    }

}
