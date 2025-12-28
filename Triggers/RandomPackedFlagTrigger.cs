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
[PrivateFor("SSC2025")]
public class RandomPackedFlagTrigger : BaseTrigger
{
    public RandomPackedFlagTrigger(EntityData e, Vector2 offset):base(e, offset)
    {
        label = e.Attr("label");
        onlyOnce = e.Bool("onlyOnce", false);
    }
    private string label;
    private Random random;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        random = new(DateTime.Now.Day * 1000000 + DateTime.Now.Hour * 10000 + DateTime.Now.Minute * 100 + DateTime.Now.Second);
    }
    
    protected override void OnEnterExecute(Player player)
    {
        if (!Md.SaveData.CurrentPackedFlags.ContainsKey(label))
        {
            if (Md.SaveData.PackedFlags.ContainsKey(label))
            {
                Md.SaveData.PackedFlags[label].ApplyTo(out List<string> list);
                Md.SaveData.CurrentPackedFlags.Enter(label, list);
            }
            else { return; }
        }
        else if (Md.SaveData.CurrentPackedFlags[label].Count == 0)
        {
            if (Md.SaveData.PackedFlags.ContainsKey(label))
            {
                Md.SaveData.PackedFlags[label].ApplyTo(out List<string> list);
                Md.SaveData.CurrentPackedFlags.Enter(label, list);
            }
            else { return; }
        }
        
        string _flag = Md.SaveData.CurrentPackedFlags[label][random.Next(Md.SaveData.CurrentPackedFlags[label].Count)];
        if (_flag.StartsWith("!"))
        {
            _flag.RemoveFirst("!").SetFlag(true);
            Md.Session.flagsPerDeath.Add(_flag.RemoveFirst("!"));
            Md.SaveData.CurrentPackedFlags[label].SafeRemove(_flag);
        }
        else if (_flag.StartsWith("*"))
        {
            _flag.RemoveFirst("*").SetFlag(true);
            Md.SaveData.flags.Add(_flag.RemoveFirst("*"));
            Md.SaveData.CurrentPackedFlags[label].SafeRemove(_flag);
        }
        else
        {
            _flag.SetFlag(true);
            Md.SaveData.CurrentPackedFlags[label].SafeRemove(_flag);
        }
    }
}
