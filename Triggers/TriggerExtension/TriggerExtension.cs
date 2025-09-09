using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers.PolygonSeries;
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

            if (trigger.SourceData.Has("extensionTag") && !trigger.ExtensionBlacklisted())
            {
                if (trigger.SourceData.Attr("extensionTag") == extensionTag && !extensionTag.IsNullOrEmpty())
                {
                    triggers.Enter(trigger);
                }
            }

            if (trigger.SourceData.ID == extensionID && !trigger.ExtensionBlacklisted())
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

[Tracked(true)]
[CustomEntity("ChroniaHelper/TriggerExtensionTarget")]
public class TriggerExtensionTarget : BaseTrigger
{
    public TriggerExtensionTarget(EntityData data, Vector2 offset) : base(data, offset)
    {
        extensionTag = data.Attr("extensionTag");
    }
    public string extensionTag;

    public HashSet<Trigger> coveredTriggers = new();

    public override void Added(Scene scene)
    {
        base.Added(scene);

        foreach(var i in MaP.level.Tracker.GetEntities<Trigger>())
        {
            if (CollideCheck(i) && !(i is TriggerExtensionTarget))
            {
                coveredTriggers.Enter(i as Trigger);
            }
        }
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        foreach(var i in coveredTriggers)
        {
            i.OnEnter(player);
        }
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);

        foreach (var i in coveredTriggers)
        {
            i.OnStay(player);
        }
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);

        foreach (var i in coveredTriggers)
        {
            i.OnLeave(player);
        }
    }
}
