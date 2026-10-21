using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/CoreModeFlagController")]
public class CoreModeFlagController : BaseEntity
{
    public CoreModeFlagController(EntityData data, Vc2 offset) : base(data, offset)
    {
        mode = data.Int("coreMode", 0);
        flag = data.Attr("flag", "flag");

        Tag = Tags.TransitionUpdate | Tags.FrozenUpdate | Tags.PauseUpdate;
    }
    private EnumWrapper<Session.CoreModes> mode;
    private string flag;

    public override void Update()
    {
        base.Update();

        flag.SetFlag(mode == SceneAs<Level>().CoreMode);
    }
}
