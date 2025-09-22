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

        Md.Session.flagsWhenEnter = new();
    }

    public override void Removed(Scene scene)
    {
        MaP.level.Session.Flags.Compare(Md.Session.flagsWhenEnter, out HashSet<string> added, out HashSet<string> removed);
        
        if(inspectMode != Inspection.On)
        {
            foreach(var flag in removed)
            {
                if (resetMethod == Method.Invert) { flag.SetFlag(!flag.GetFlag()); }
                else if (resetMethod == Method.True) { flag.SetFlag(true); }
                else { flag.SetFlag(false); }
            }
        }
        if(inspectMode != Inspection.Off)
        {
            foreach (var flag in added)
            {
                if (resetMethod == Method.Invert) { flag.SetFlag(!flag.GetFlag()); }
                else if (resetMethod == Method.True) { flag.SetFlag(true); }
                else { flag.SetFlag(false); }
            }
        }

        base.Removed(scene);
    }
}
