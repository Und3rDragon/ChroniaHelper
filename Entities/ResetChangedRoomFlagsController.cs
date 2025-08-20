using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/ResetChangedRoomFlagsController")]
public class ResetChangedRoomFlagsController : Entity
{
    public static void Load()
    {
        On.Celeste.Session.SetFlag += OnSetFlag;
    }

    public static void Unload()
    {
        On.Celeste.Session.SetFlag -= OnSetFlag;
    }

    public static void OnSetFlag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool set)
    {
        if (set)
        {
            if (Md.Session.decreasedFlags.Contains(flag))
            {
                Md.Session.decreasedFlags.Remove(flag);
            }
            Md.Session.increasedFlags.Add(flag);
        }
        else
        {
            if (Md.Session.increasedFlags.Contains(flag))
            {
                Md.Session.increasedFlags.Remove(flag);
            }
            Md.Session.decreasedFlags.Add(flag);
        }

        orig(self, flag, set);
    }

    public ResetChangedRoomFlagsController(EntityData data, Vector2 offset): base(data.Position + offset)
    {
        inspectMode = (Inspection)data.Int("inspectMode", 0);
        resetMethod = (Method)data.Int("resetMethod", 0);
    }
    public enum Inspection { Any, On, Off}
    public enum Method { False, True, Invert}
    private Inspection inspectMode;
    private Method resetMethod;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Md.Session.increasedFlags = new();
        Md.Session.decreasedFlags = new();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);

        if(inspectMode == Inspection.On)
        {
            Md.Session.decreasedFlags = new();
        }
        if(inspectMode == Inspection.Off)
        {
            Md.Session.increasedFlags = new();
        }

        if (resetMethod == Method.Invert)
        {
            Md.Session.decreasedFlags.EachDo(item => item.SetFlag(true));
            Md.Session.increasedFlags.EachDo(item => item.SetFlag(false));
        }
        else if (resetMethod == Method.True)
        {
            Md.Session.increasedFlags.EachDo(item => item.SetFlag(true));
            Md.Session.decreasedFlags.EachDo(item => item.SetFlag(true));
        }
        else
        {
            Md.Session.increasedFlags.EachDo(item => item.SetFlag(false));
            Md.Session.decreasedFlags.EachDo(item => item.SetFlag(false));
        }
    }

    public override void Update()
    {
        base.Update();

        //Log.Each(Md.Session.increasedFlags);
        //Log.Each(Md.Session.decreasedFlags);
        //Log.Error("____________________");
    }
}
