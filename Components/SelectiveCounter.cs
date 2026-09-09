using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class SelectiveCounter : SelectiveSessionValue
{
    public SelectiveCounter(string name, int fallback = 0, 
        Clamper.Int restraints = null) : base(name)
    {
        this.Fallback = fallback;
        this.Limiter = restraints ?? new();

        _valid = !(string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression));
        if (_valid)
        {
            if (int.TryParse(Expression, out int n))
            {
                _cached = n;
            }
        }
    }
    public int Fallback;
    public Clamper.Int Limiter = new();

    private bool _valid;
    private int? _cached = null;

    public int Value => Limiter.Operate(GetValue());
    private int GetValue()
    {
        if (!_valid) { return Fallback; }

        if(_cached != null) { return (int)_cached; }

        return Expression.GetCounter(Fallback);
    }
}

public static class SelectiveCounterExtension
{
    public static SelectiveCounter Counter(this EntityData data, string field, int fallback = 0, Clamper.Int limiter = null)
    {
        return new SelectiveCounter(data.Attr(field), fallback, limiter);
    }
}
