using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

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

    private HashSet<string> last_flags;
    private List<string> increasedFlags = new(), decreasedFlags = new();

    public override void Added(Scene scene)
    {
        base.Added(scene);

        increasedFlags = new();
        decreasedFlags = new();

        last_flags = MaP.level.Session.Flags;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);

        if (resetMethod == Method.Invert)
        {
            decreasedFlags.EachDo(item => item.SetFlag(true));
            increasedFlags.EachDo(item => item.SetFlag(false));
        }
        else if (resetMethod == Method.True)
        {
            increasedFlags.EachDo(item => item.SetFlag(true));
            decreasedFlags.EachDo(item => item.SetFlag(true));
        }
        else
        {
            increasedFlags.EachDo(item => item.SetFlag(false));
            decreasedFlags.EachDo(item => item.SetFlag(false));
        }
    }

    public override void Update()
    {
        base.Update();

        last_flags.Compare(MaP.level.Session.Flags, out List<string> i, out List<string> d);
        if (inspectMode != Inspection.Off)
        {
            i.AddTo(ref increasedFlags);
        }
        if (inspectMode != Inspection.On)
        {
            d.AddTo(ref decreasedFlags);
        }

        //Log.Each(increasedFlags, Log.LogMode.Info);
        //Log.Each(decreasedFlags, Log.LogMode.Warn);
        //Log.Info("_________________");
    }
}
