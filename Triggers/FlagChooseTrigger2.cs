using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagChooseTrigger2")]
public class FlagChooseTrigger2 : BaseTrigger
{
    private EntityID eID;
    private EntityData data;
    public FlagChooseTrigger2(EntityData entityData, Vector2 offset, EntityID entityID) : base(entityData, offset)
    {
        eID = entityID;
        data = entityData;

        onStay = data.Fetch("listenOnStay", false);
        coverScreen = data.Fetch("coverScreen", false);
        // filter input data
        string[] flagGroups = data.Attr("flagDictionary").Split(";", StringSplitOptions.TrimEntries);
        for(int i = 0; i < flagGroups.Length; i++)
        {
            string[] t = flagGroups[i].Split(">>", StringSplitOptions.TrimEntries);
            if (t.Length < 2) { continue; }

            flagPairs[t[0]] = t[1];
        }
    }
    private Dictionary<string, string> flagPairs = new();
    private bool onStay = false, coverScreen = false;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (coverScreen) { Listen(); }
    }

    public override void Update()
    {
        base.Update();

        if (coverScreen && onStay) { Listen(); }
    }

    protected override void OnEnterExecute(Player player)
    {
        Listen();
    }

    protected override void OnStayExecute(Player player)
    {
        if (onStay)
        {
            Listen();
        }
    }

    public void Listen()
    {
        foreach(var pairs in flagPairs)
        {
            string[] conditions = pairs.Key.Split(",",StringSplitOptions.TrimEntries), 
                results = pairs.Value.Split(",", StringSplitOptions.TrimEntries);

            bool flag = true;
            foreach(string name in conditions)
            {
                flag.TryNegative(name.Contains("!") ? !FlagUtils.GetFlag(name.RemoveAll("!"))
                    : FlagUtils.GetFlag(name));
            }

            if (flag)
            {
                foreach(string target in results)
                {
                    bool reverse = target.Contains("!"), global = target.Contains("*");
                    FlagUtils.SetFlag(target.RemoveAll("!").RemoveAll("*"), !reverse, global);
                }
            }
        }
    }
}
