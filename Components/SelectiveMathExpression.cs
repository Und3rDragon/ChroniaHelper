using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class SelectiveMathExpression : SelectiveSessionValue
{
    public SelectiveMathExpression(string name, float fallback = 0f,
        Clamper.Float restraints = null) : base(name)
    {
        this.Fallback = fallback;
        this.Limiter = restraints ?? new();
    }
    public float Fallback;
    public Clamper.Float Limiter = new();

    public float Value => Limiter.Operate(GetValue());
    private float GetValue()
    {
        float n = Fallback;

        if (string.IsNullOrEmpty(Expression) || string.IsNullOrWhiteSpace(Expression))
        {
            return n;
        }

        if (float.TryParse(Expression, out n))
        {
            return n;
        }

        return Expression.ParseMathExpression();
    }
}

public static class SelectiveMathExpressionExtension
{
    public static SelectiveMathExpression MathExpression(this EntityData data, string field, float fallback = 0f, Clamper.Float limiter = null)
    {
        return new SelectiveMathExpression(data.Attr(field), fallback, limiter);
    }
}
