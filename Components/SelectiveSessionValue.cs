using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public abstract class SelectiveSessionValue : BaseComponent
{
    public SelectiveSessionValue(string expression)
    {
        Expression = expression;
    }
    public string Expression;
}
