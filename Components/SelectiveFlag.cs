using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class SelectiveFlag : BaseComponent
{
    public SelectiveFlag(string name, bool fallback = false)
    {
        Expression = name;
        Fallback = fallback;
    }
    private string Expression;
    private bool Fallback;

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
        bool b = Fallback;

        if(string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression))
        {
            return b;
        }

        string s = Expression.ToLower().Trim();
        if (TrueSyntax.Contains(s))
        {
            return true;
        }

        if (FalseSyntax.Contains(s))
        {
            return false;
        }

        return Expression.GetFlag();
    }

    protected override void BeforeEntityAdded(Scene scene)
    {
        if (string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression))
        {
            return;
        }
        
        if (!MaP.level?.Session?.Flags.Contains(Expression) ?? true)
        {
            Expression.SetFlag(Fallback);
        }
    }
}

public static class SelectiveFlagExtension
{
    public static SelectiveFlag Flag(this EntityData data, string field, bool fallback = false)
    {
        return new SelectiveFlag(data.Attr(field), fallback);
    }
}