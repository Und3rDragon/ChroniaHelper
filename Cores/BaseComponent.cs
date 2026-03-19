using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public class BaseComponent : Component
{
    public BaseComponent(bool active, bool visible) : base(active, visible) { }
    
    public BaseComponent() : base(true, true) { }
}
