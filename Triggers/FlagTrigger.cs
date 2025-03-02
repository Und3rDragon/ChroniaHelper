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
    public List<string> trueFlags, falseFlags;
    private Level level;
    public bool temp;
    public bool saves;

    public FlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        set = data.Bool("set");
        temp = data.Bool("temporary");
        saves = data.Bool("global", false);

        // flag processing
        string input = data.Attr("flag", "Flag");
        string[] flagSet = input.Split(',', StringSplitOptions.TrimEntries);
        trueFlags = new List<string>(); 
        falseFlags = new List<string>();
        foreach (var item in flagSet)
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

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();

        if (this.temp)
        {
            foreach (var item in trueFlags)
            {
                Utils.FlagUtils.SetFlag(item, set ? false : true, saves);
            }
            foreach (var item in falseFlags)
            {
                Utils.FlagUtils.SetFlag(item, set ? true : false, saves);
            }
        }
    }
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        foreach (var item in trueFlags)
        {
            Utils.FlagUtils.SetFlag(item, set? true: false, saves);
        }
        foreach (var item in falseFlags)
        {
            Utils.FlagUtils.SetFlag(item, set ? false : true, saves);
        }
    }

    public override void OnStay(Player player)
    {

    }

    public override void OnLeave(Player player)
    {
        
    }
}
