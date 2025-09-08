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
using YamlDotNet.Serialization;

namespace ChroniaHelper.Triggers.TriggerExtension;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TriggerExtension")]
public class TriggerExtension : BaseTrigger
{
    public TriggerExtension(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;
        extensionTag = data.Attr("extensionTag");
        extensionID = data.Int("overrideID", -1);
    }
    public string extensionTag;
    public int extensionID;
    public int ID;

    private Trigger masterTrigger;
    private void FindMasterTrigger()
    {
        bool overrided = false;
        HashSet<Trigger> triggers = new();
        foreach (var i in MaP.level.Tracker.GetEntities<Trigger>())
        {
            Trigger trigger = i as Trigger;

            if (trigger.SourceData.Has("extensionTag") && !(trigger is TriggerExtension))
            {
                if (trigger.SourceData.Attr("extensionTag") == extensionTag && !extensionTag.IsNullOrEmpty())
                {
                    triggers.Enter(trigger);
                }
            }

            if (trigger.SourceData.ID == extensionID && !(trigger is TriggerExtension))
            {
                masterTrigger = trigger;
                overrided = true;
                break;
            }
        }

        if (!overrided)
        {
            masterTrigger = triggers.GetMaxItem((trigger) => trigger.SourceData.ID); // checked
        }
    }

    public override void Awake(Scene scene)
    {
        FindMasterTrigger();

        base.Awake(scene);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        if (CollideOther(player)) { return; }

        masterTrigger.OnEnter(player);
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);

        masterTrigger.OnStay(player);
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);

        if (CollideOther(player)) { return; }

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

            return;
        }

        masterTrigger.OnLeave(player);
    }

    private HashSet<Trigger> GetExtensionsList()
    {
        HashSet<Trigger> triggers = new();
        foreach (var i in MaP.level.Tracker.GetEntities<TriggerExtension>())
        {
            TriggerExtension trigger = i as TriggerExtension;

            if (extensionID >= 0)
            {
                if(trigger.extensionID == extensionID)
                {
                    triggers.Enter(trigger);
                }
            }
            else if (trigger.extensionTag == extensionTag && !extensionTag.IsNullOrEmpty())
            {
                triggers.Enter(trigger);
            }
        }

        return triggers;
    }
    private bool Collide(Player player)
    {
        var triggers = GetExtensionsList();
        triggers.Replace(this, masterTrigger);

        foreach (var i in triggers)
        {
            if (player.CollideCheck(i)) { return true; }
        }

        return false;
    }

    private bool CollideOther(Player player)
    {
        var triggers = GetExtensionsList();
        triggers.SafeRemove(this);
        
        foreach (var i in triggers)
        {
            if (player.CollideCheck(i)) { return true; }
        }

        return false;
    }
}
