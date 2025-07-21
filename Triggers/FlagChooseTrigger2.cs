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
[CustomEntity("ChroniaHelper/FlagChooseTrigger2", "ChroniaHelper/FlagDictionaryTrigger")]
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

    // new: "xxx#xxx, flag >> flag_#" ...
    public void Listen()
    {
        foreach(var pairs in flagPairs)
        {
            string[] conditions = pairs.Key.Split(",",StringSplitOptions.TrimEntries), 
                results = pairs.Value.Split(",", StringSplitOptions.TrimEntries);

            bool flag = true;
            Dictionary<int, List<string>> wildNames = new();
            for(int x = 0; x < conditions.Length; x++)
            {
                string name = conditions[x];
                List<string> wildName_x = wildNames.ContainsKey(x) ? wildNames[x] : new();

                bool reverse = name.Contains("!"), wild = name.Contains("#");
                string basicName = name.RemoveAll("!").RemoveAll("*");
                //flag.TryNegative(name.Contains("!") ? !ChroniaFlagUtils.GetFlag(name.RemoveAll("!"))
                //    : ChroniaFlagUtils.GetFlag(name));

                if (wild)
                {
                    bool hasWildMatch = false;
                    foreach (var item in 0.LevelFlags())
                    {
                        string[] s1 = basicName.Split("#", StringSplitOptions.TrimEntries);
                        string s2 = item;
                        bool match = true;
                        for (int i = 0; i < s1.Length; i++)
                        {
                            if (!s2.Contains(s1[i])) { match = false; break; }

                            s2 = s2.Remove(0, s2.IndexOf(s1[i]) + s1[i].Length);
                        }
                        if (match)
                        {
                            hasWildMatch = true;

                            s2 = item;
                            for(int i = 0; i < s1.Length; i++)
                            {
                                s2 = s2.Remove(s2.IndexOf(s1[i]), s1[i].Length);
                            }
                            
                            wildName_x.Enter(s2);
                        }
                    }

                    if (hasWildMatch)
                    {
                        if (!reverse)
                        {
                            wildNames.Enter(x, wildName_x);
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        if (!reverse)
                        {
                            flag = false;
                        }
                    }

                }
                else
                {
                    flag.TryNegative(reverse? !basicName.GetFlag() : basicName.GetFlag());
                }
            }

            //foreach(var item in wildNames.Keys)
            //{
            //    foreach (var item1 in wildNames[item])
            //    {
            //        Log.Info(item, item1);
            //    }
            //}
            //Log.Warn("------------------------");

            if (flag)
            {
                // cross reference lists
                var tList = wildNames.Count > 0 ? wildNames.First().Value.ToArray() : Array.Empty<string>();
                var cList = wildNames.Values.ToArray();
                List<int> ignoreIndexes = new();
                for(int i = 0; i < tList.Length; i++)
                {
                    foreach(var item in cList)
                    {
                        if (!item.Contains(tList[i]))
                        {
                            ignoreIndexes.Enter(i);
                        }
                    }
                }
                List<string> fList = new();
                for (int i = 0; i < tList.Length; i++)
                {
                    if (!ignoreIndexes.Contains(i))
                    {
                        fList.Enter(tList[i]);
                    }
                }


                foreach (string target in results)
                {
                    bool reverse = target.Contains("!"), global = target.Contains("*"), wild = target.Contains("#");
                    string basicTarget = target.RemoveAll("!").RemoveAll("*");
                    if (wild)
                    {
                        foreach(var item in fList)
                        {
                            ChroniaFlagUtils.SetFlag(basicTarget.Replace("#", item), !reverse, global);
                        }
                    }
                    else
                    {
                        ChroniaFlagUtils.SetFlag(basicTarget, !reverse, global);
                    }
                }
            }
        }
    }
}
