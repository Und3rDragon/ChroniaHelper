using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.WIPs.Entities;

[WorkingInProgress("Current Seamless Spinner is working fine, so this is not a urgent work",
    "but the fact is there are many useless paths and some features are hard to optimize",
    "therefore it's necessary to build a new one with simplified file settings",
    "(Yes, file settings, all the old features should be kept)")]
public class CustomSpinner : BaseEntity
{
    public CustomSpinner(EntityData d, Vc2 o) : base(d, o)
    {

    }
}
