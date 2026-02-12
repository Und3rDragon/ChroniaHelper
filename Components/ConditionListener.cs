using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.LogicExpression;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Components;

public class ConditionListener : StateListener
{
    public ConditionListener(string condition, ConditionType expression)
    {
        this.condition = condition;
        this.conditionType = expression;
    }
    
    public string condition;
    public enum ConditionType { Flags = 0, ChroniaMathExpression = 1, FrostSessionExpression = 2, ChroniaLogicExpression = 3 }
    public ConditionType conditionType;

    protected override bool GetState()
    {
        if (conditionType == ConditionType.FrostSessionExpression)
        {
            if (Md.FrostHelperLoaded)
            {
                return condition.getBoolSessionExpressionValue();
            }
            else
            {
                return condition.ParseMathExpression() != 0f;
            }
        }
        else if (conditionType == ConditionType.ChroniaMathExpression)
        {
            return condition.ParseMathExpression() != 0f;
        }
        else if(conditionType == ConditionType.ChroniaLogicExpression)
        {
            return condition.ParseLogicExpression((x) => x.GetFlag());
        }
        else
        {
            return condition.GetGeneralFlags();
        }
    }
}
