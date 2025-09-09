using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Triggers.PolygonSeries;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers.TriggerExtension;

public static class TriggerExtensionUtils
{
    public static HashSet<Type> extensionBlacklist = new()
    {
        typeof(TriggerExtension),
        typeof(PolygonTrigger),
    };

    public static bool ExtensionBlacklisted(this object obj) => extensionBlacklist.Contains(obj.GetType());

    public static HashSet<TriggerExtension> GetExtensions(this string extensionTag)
    {
        HashSet<TriggerExtension> triggers = new();
        foreach (var i in MaP.level.Tracker.GetEntities<TriggerExtension>())
        {
            TriggerExtension trigger = i as TriggerExtension;

            if (!trigger.extensionTag.IsNullOrEmpty())
            {
                if (trigger.extensionTag == extensionTag)
                {
                    triggers.Add(trigger);
                }
            }
        }

        return triggers;
    }

    public static bool CollideExtensions(this Player player, string extensionTag)
    {
        foreach (var extension in extensionTag.GetExtensions())
        {
            if (player.CollideCheck(extension)) { return true; }
        }

        return false;
    }
}
