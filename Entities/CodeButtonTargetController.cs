using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CodeButtonTargetController")]
public class CodeButtonTargetController : BaseEntity
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Update += OnLevelUpdate;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Update -= OnLevelUpdate;
    }

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Celeste.Level self)
    {
        orig(self);

        foreach (var item in Md.Session.codeButtonTargets)
        {
            if (!item.Value.needsEnterCheck)
            {
                if (Md.Session.keystrings.GetValueOrDefault(item.Key, "") == item.Value.codeString)
                {
                    item.Value.flag.SetFlag(true);
                }
                else if (item.Value.deactivateFlagWhenNotStaisfied)
                {
                    item.Value.flag.SetFlag(false);
                }
            }
        }
    }

    public CodeButtonTargetController(EntityData data, Vc2 offset) : base(data, offset)
    {
        sessionKeyID = data.Attr("sessionKeyID", "buttonKey");
        buttonCodeTarget = data.Attr("buttonCodeTarget", "000000");
        hitEnterNeeded = data.Bool("hitEnterToConfirm", true);
        targetFlag = data.Attr("targetFlag", "flag");
        deactivateFlagWhenNotSatisfied = data.Bool("deactivateFlagWhenNotSatisfied", false);
    }
    private string sessionKeyID;
    private string buttonCodeTarget;
    private bool hitEnterNeeded;
    private string targetFlag;
    private bool deactivateFlagWhenNotSatisfied;

    protected override void AddedExecute(Scene scene)
    {
        Ses.CodeButtonTarget target = new()
        {
            codeString = buttonCodeTarget,
            needsEnterCheck = hitEnterNeeded,
            flag = targetFlag,
            deactivateFlagWhenNotStaisfied = deactivateFlagWhenNotSatisfied,
        };
        Md.Session.codeButtonTargets[sessionKeyID] = target;
    }
}
