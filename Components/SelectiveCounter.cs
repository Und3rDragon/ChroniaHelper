using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class SelectiveCounter : BaseComponent
{
    public SelectiveCounter(string name, int fallback = 0, Clamper.Int restraints = null) : base()
    {
        Expression = name;
        this.Fallback = fallback;
        this.Limiter = restraints ?? new();
    }
    public string Expression;
    public int Fallback;
    public Clamper.Int Limiter = new();

    public int Value => Limiter.Operate(GetValue());
    private int GetValue()
    {
        int n = Fallback;

        if (string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression))
        {
            return n;
        }
        
        if(!int.TryParse(Expression, out n))
        {
            return Expression.GetCounter();
        }

        return n;
    }
}

public static class SelectiveCounterExtension
{
    public static SelectiveCounter Counter(this EntityData data, string field, int fallback = 0)
    {
        return new SelectiveCounter(data.Attr(field), fallback);
    }
}
