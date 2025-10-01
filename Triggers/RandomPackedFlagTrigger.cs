using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/RandomPackedFlagTrigger")]
public class RandomPackedFlagTrigger : BaseTrigger
{
    public RandomPackedFlagTrigger(EntityData e, Vector2 offset):base(e, offset)
    {
        label = e.Attr("label");
        onlyOnce = e.Bool("onlyOnce", false);
    }
    private string label;
    private HashSet<string> targets;
    private string[] idle;

    protected override void AddedExecute(Scene scene)
    {
        targets = new();
        
        foreach (var entry in Md.SaveData.ChroniaFlags)
        {
            if (!entry.Value.PresetTags.Contains(Utils.ChroniaSystem.Labels.Packed)) { continue; }
            if (entry.Value.CustomData.Count == 0) { continue; }
            if (!entry.Value.CustomData.ContainsKey("packed_label")) { continue; }

            if (entry.Value.CustomData["packed_label"] == label)
            {
                targets.Enter(entry.Key);
            }
        }
        
    }

    protected override void OnEnterExecute(Player player)
    {
        if (targets.IsNull() || targets.Count == 0) { return; }
        
        Random random = new(DateTime.Now.Day * 1000000 + DateTime.Now.Hour * 10000 + DateTime.Now.Minute * 100 + DateTime.Now.Second);
        
        idle = new string[targets.Count];
        int count = 0;
        var a = targets.ToArray();
        for(int i = 0, j = 0; i < a.Length; i++)
        {
            if (!Md.SaveData.ChroniaFlags[a[i]].CustomData.ContainsKey("packed_triggered"))
            {
                idle[j] = a[i];
                j++;
                count++;
                continue;
            }
            
            if (!Md.SaveData.ChroniaFlags[a[i]].CustomData["packed_triggered"].ParseBool(false))
            {
                idle[j] = a[i];
                j++;
                count++;
            }
        }
        
        if(count == 0)
        {
            for(int i = 0; i < a.Length; i++)
            {
                Md.SaveData.ChroniaFlags[a[i]].CustomData.Enter("packed_triggered", "false");
                idle[i] = a[i];
            }
            
            int m = random.Next(0, count);
            
            Md.SaveData.ChroniaFlags[idle[m]].Active = true;
            Md.SaveData.ChroniaFlags[idle[m]].CustomData.Enter("packed_triggered", "true");
            Md.SaveData.ChroniaFlags[idle[m]].PushFlag(idle[m]);

            return;
        }

        int selection = random.Next(0, count);

        Md.SaveData.ChroniaFlags[idle[selection]].Active = true;
        Md.SaveData.ChroniaFlags[idle[selection]].CustomData.Enter("packed_triggered", "true");
        Md.SaveData.ChroniaFlags[idle[selection]].PushFlag(idle[selection]);
    }
}
