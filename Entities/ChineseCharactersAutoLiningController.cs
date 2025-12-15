using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/ChineseCharactersAutoLiningController")]
public class ChineseCharactersAutoLiningController :  BaseEntity
{
    public ChineseCharactersAutoLiningController(EntityData d, Vc2 o) : base(d, o)
    {
        state = d.Bool("autoLining", false);
    }
    private bool state;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Md.Settings.ChineseCharactersAutoLining = state;
    }
}
