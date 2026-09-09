using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class SelectiveFlag : SelectiveSessionValue
{
    public SelectiveFlag(string name, bool fallback = false) : base(name)
    {
        Fallback = fallback;

        _valid = !(string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression));
        if (_valid)
        {
            string s = Expression.ToLower().Trim();
            if (TrueSyntax.Contains(s))
            {
                _cached = true;
            }

            if (FalseSyntax.Contains(s))
            {
                _cached = true;
            }
        }
    }
    private bool Fallback;

    private bool _valid;
    private bool? _cached = null;

    private List<string> TrueSyntax = new()
    {
        "true", "t", "1",
    };

    private List<string> FalseSyntax = new()
    {
        "false", "f", "0",
    };

    public bool Value => GetValue();
    private bool GetValue()
    {
        if (!_valid) { return Fallback; }

        if(_cached != null) { return (bool)_cached; }

        return Expression.GetFlag();
    }
}

public static class SelectiveFlagExtension
{
    public static SelectiveFlag Flag(this EntityData data, string field, bool fallback = false)
    {
        return new SelectiveFlag(data.Attr(field), fallback);
    }
}