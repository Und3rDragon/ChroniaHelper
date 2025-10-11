using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Reflection;
using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;


namespace ChroniaHelper.Cores;

[TrackedAs(typeof(CornerBoostSolid))]
[Tracked(true)]
public class CornerBoostSolid : BaseSolid
{
    public CornerBoostSolid(EntityData data, Vector2 position, float width, float height, bool safe, bool perfectCB) : base(position, data)
    {
        Add(new SolidModifierComponent(perfectCB ? 2 : 1, false, false));
    }
}
