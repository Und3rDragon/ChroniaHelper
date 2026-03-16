using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public abstract class SelectiveSessionValue : BaseComponent
{
    public string Expression;

    public float GeneralValue => DefaultGetValue();

    public abstract float DefaultGetValue();
}
