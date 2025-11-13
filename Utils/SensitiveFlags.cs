using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AsmResolver.DotNet.Memory;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Utils;

public static class SensitiveFlags
{
    public static Dictionary<string, Sensitivity> Flags = new()
    {
        { "ChroniaHelper_PlayerCollidingBGTiles", Sensitivity.None },
        { "ChroniaHelper_PlayerTouchingTriggers", Sensitivity.None },
        { "ChroniaHelper_PlayerCollidingEntitiesAbove", Sensitivity.None },
        { "ChroniaHelper_PlayerCollidingEntitiesBelow", Sensitivity.None },
        { "ChroniaHelper_PlayerCollidingEntitiesWithSameDepth", Sensitivity.None },
    };

    public static Dictionary<string, Sensitivity> Regexs = new()
    {
        { @"^ChroniaHelper_Input_.*", Sensitivity.None },
        { @"^ChroniaHelper_ConnectedRefill_.*_triggered$", Sensitivity.None },
        { @"^ChroniaHelper_ConnectedRefill_.*_consumed$" , Sensitivity.None },
        { @"^ChroniaHelper_ConnectedRefill_.*_queue$", Sensitivity.None },
        { @"^ChroniaHelper_ConnectedRefill_.*_collect$" , Sensitivity.None },
        { @"^ChroniaHelper_Stopclock_.*", Sensitivity.None },
    };
    
    public static bool SensitiveFlagged(this string name, RegexOptions regexExpansion = RegexOptions.None)
    {
        if (Flags.Keys.Contains(name)) { return true; }
        
        foreach(var regex in Regexs.Keys)
        {
            if(Regex.Match(name, regex, regexExpansion).Success)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public static Sensitivity GetSensitivity(this string name, RegexOptions regexExpansion = RegexOptions.None)
    {
        if (Flags.Keys.Contains(name)) { return Flags[name]; }

        foreach (var regex in Regexs.Keys)
        {
            if (Regex.Match(name, regex, regexExpansion).Success)
            {
                return Regexs[regex];
            }
        }

        return Sensitivity.None;
    }

    /// <summary>
    /// ChroniaHelper Sensitive Flag level
    /// </summary>
    /// 
    [Flags]
    public enum Sensitivity
    {
        None = 0,
        AllowNoRegister = 1 << 0, // Pow(2,n) == 1 << n, bit形式
        AllowNoSetFlag = 1 << 1,
    }
}
