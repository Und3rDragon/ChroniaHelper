using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagChooseTrigger")]
public class FlagChooseTrigger : FlagManageTrigger
{

    private Dictionary<string[], string[]> flagDictionary;

    private string[] defaultFlag;

    private bool deleteKeyFlag;

    private bool multiExecute;

    private List<string[]> addFlags;

    private List<string[]> removeFlags;

    private bool isEnterExecute;

    public FlagChooseTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.flagDictionary = FlagUtils.ParseChoose(data.Attr("flagDictionary", null));
        this.defaultFlag = FlagUtils.Parse(data.Attr("defaultFlag", null));
        this.deleteKeyFlag = data.Bool("deleteKeyFlag", false);
        this.multiExecute = data.Bool("multiExecute", false);
        this.addFlags = new List<string[]>();
        this.removeFlags = new List<string[]>();
    }

    protected override void OnEnterExecute(Player player)
    {
        int execute = 0;
        foreach (KeyValuePair<string[], string[]> dictionary in this.flagDictionary)
        {
            if (!base.Contains(dictionary.Key))
            {
                continue;
            }
            base.Add(dictionary.Value);
            this.addFlags.Add(dictionary.Value);
            execute++;
            this.isEnterExecute = true;
            if (this.deleteKeyFlag)
            {
                base.Remove(dictionary.Key);
                this.removeFlags.Add(dictionary.Key);
            }
            if (!this.multiExecute)
            {
                return;
            }
        }
        if (execute == 0 && this.defaultFlag.Length > 0)
        {
            base.Add(this.defaultFlag);
            this.addFlags.Add(this.defaultFlag);
        }
    }

    public override void OnLeave(Player player)
    {
        if (this.isEnterExecute)
        {
            base.OnLeave(player);
        }
    }

    protected override void LeaveReset(Player player)
    {
        foreach (string[] flag in this.addFlags)
        {
            base.Remove(flag);
        }
        foreach (string[] flag in this.removeFlags)
        {
            base.Add(flag);
        }
    }

}
