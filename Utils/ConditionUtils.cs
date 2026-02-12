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
                return condition.getBoolSessionExpressionValue();
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
}
