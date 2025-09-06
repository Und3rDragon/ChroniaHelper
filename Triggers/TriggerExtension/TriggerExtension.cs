using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers.TriggerExtension;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TriggerExtension")]
public class TriggerExtension : BaseTrigger
{
    public TriggerExtension(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;
        extensionTag = data.Attr("extensionTag");
    }
    public string extensionTag;
    public int ID;

    private Trigger masterTrigger;
    protected override void AwakeExecute(Scene scene)
    {
        // Find master trigger
        HashSet<Trigger> triggers = new();
        foreach(var i in MaP.level.Tracker.GetEntities<Trigger>())
        {
            Trigger trigger = i as Trigger;

            if (trigger.SourceData.Has("extensionTag") && !(trigger is TriggerExtension))
            {
                if(trigger.SourceData.Attr("extensionTag") == extensionTag)
                {
                    triggers.Add(trigger);
                }
            }
        }

        masterTrigger = triggers.GetMaxItem((trigger) => trigger.SourceData.ID); // checked
    }

    protected override void OnEnterExecute(Player player)
    {
        if (CollideOther(player)) { return; }

        masterTrigger.OnEnter(player);
    }

    protected override void OnStayExecute(Player player)
    {
        masterTrigger.OnStay(player);
    }

    protected override void OnLeaveExecute(Player player)
    {
        if (CollideOther(player)) { return; }

        masterTrigger.OnLeave(player);

        // standalone arguments
        if (masterTrigger is FlagTrigger)
        {
            FlagTrigger ft = masterTrigger as FlagTrigger;
            if (ft.reset)
            {
                foreach (var item in ft.records.Keys)
                {
                    item.SetFlag(!ft.records[item], ft.saves, ft.temp);
                }
                ft.records.Clear();
            }
        }
    }

    private bool Collide(Player player)
    {
        HashSet<Trigger> triggers = new();
        foreach (var i in MaP.level.Tracker.GetEntities<Trigger>())
        {
            Trigger trigger = i as Trigger;

            if (trigger.SourceData.Has("extensionTag"))
            {
                if (trigger.SourceData.Attr("extensionTag") == extensionTag)
                {
                    triggers.Add(trigger);
                }
            }
        }

        foreach (var i in triggers)
        {
            if (player.CollideCheck(i)) { return true; }
        }

        return false;
    }

    private bool CollideOther(Player player)
    {
        HashSet<Trigger> triggers = new();
        foreach (var i in MaP.level.Tracker.GetEntities<Trigger>())
        {
            Trigger trigger = i as Trigger;

            if (trigger.SourceData.Has("extensionTag"))
            {
                if (trigger.SourceData.Attr("extensionTag") == extensionTag && trigger.SourceData.ID != ID)
                {
                    triggers.Add(trigger);
                }
            }
        }
        
        foreach (var i in triggers)
        {
            if (player.CollideCheck(i)) { return true; }
        }

        return false;
    }
}
