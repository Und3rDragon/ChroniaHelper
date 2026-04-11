using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Utils;

public static class ConditionUtils
{
    public enum ConditionMode { Flags = 0, ChroniaMathExpression = 1, FrostSessionExpression = 2 }
    public static bool CheckCondition(this string condition, ConditionMode mode = 0)
    {
        if(mode == ConditionMode.FrostSessionExpression)
        {
            if (Md.FrostHelperLoaded)
            {
                return condition.tryCreateSessionExpression().getBoolSessionExpressionValue();
            }
            else
            {
                return false;
            }
        }
        else if(mode == ConditionMode.ChroniaMathExpression)
        {
            return condition.ParseMathExpression() != 0;
        }
        else
        {
            return condition.GetGeneralFlags();
        }
    }

    public static float Calculate(this string condition, ConditionMode mode = ConditionMode.ChroniaMathExpression, Func<string, float> getVariable = null, Func<string, float> getFlag = null)
    {
        if (mode == ConditionMode.Flags)
        {
            return condition.GetGeneralFlags() ? 1f : 0f;
        }
        else if (mode == ConditionMode.FrostSessionExpression && Md.FrostHelperLoaded)
        {
            return condition.getFloatSessionExpressionValue();
        }
        else
        {
            return condition.ParseMathExpression(getVariable, getFlag);
        }
    }
    
    public static float Calculate(this string condition, int mode = 1, 
        Func<string, float> getVariable = null, Func<string, float> getFlag = null,
        Dictionary<string, Func<Session, object? /* userdata */, object>>? simpleCommands = null,
        Dictionary<string, Func<Session, object? /* userdata */, IReadOnlyList<object>, object>>? functionCommands = null)
    {
        if ((ConditionMode)mode == ConditionMode.Flags)
        {
            return condition.GetGeneralFlags() ? 1f : 0f;
        }
        else if ((ConditionMode)mode == ConditionMode.FrostSessionExpression && Md.FrostHelperLoaded)
        {
            object context = APIFrostHelper.createSessionExpressionContext(simpleCommands, functionCommands);
            object exp = condition.tryCreateSessionExpression(context);
            return exp.getFloatSessionExpressionValue();
        }
        else
        {
            return condition.ParseMathExpression(getVariable, getFlag);
        }
    }
}
