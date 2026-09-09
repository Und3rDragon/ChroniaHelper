using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.LogicExpression;
using ChroniaHelper.Utils.MathExpression;
using MonoMod.ModInterop;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.API;

[ModExportName("ChroniaHelper")]
public static class API
{
    public static Version APIVersion => new(1, 0);

    /// <summary>
    /// Calculate a Chronia Math Expression 
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="setVariables"></param>
    /// <param name="getFlagValue"></param>
    /// <returns></returns>
    [Note("Included in 1.0")]
    public static double ParseChroniaMathExpression(string expression, Func<string, double> setVariables = null, Func<string, double> getFlagValue = null)
        => expression.ParseMathExpressionRaw(setVariables, getFlagValue);
    public static float ParseChroniaMathExpression(string expression, Func<string, float> setVariables = null, Func<string, float> getFlagValue = null)
        => expression.ParseMathExpression(setVariables, getFlagValue);

    /// <summary>
    /// Calculate a Chronia Logic Expression
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="setVariableValue"></param>
    /// <param name="fallback"></param>
    /// <returns></returns>
    [Note("Included in 1.0")]
    public static bool ParseChroniaFlagLogicExpression(string expression, Func<string, bool> setVariableValue = null, bool fallback = false)
        => expression.ParseLogicExpression(setVariableValue, fallback);

    /// <summary>
    /// Utilize Chronia Flags
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="state"></param>
    /// <param name="global"></param>
    /// <param name="perDeath"></param>
    /// <param name="perRoom"></param>
    [Note("Included in 1.0")]
    public static void SetChroniaFlag(string flag, bool state = true, bool global = false, bool perDeath = false, bool perRoom = false)
        => flag.SetFlag(state, global, perDeath, perRoom);

    /// <summary>
    /// Check if a certain flag is enlisted or checking its state
    /// </summary>
    /// <param name="flag">Flag name</param>
    /// <param name="isGlobal">Check if the flag is enlisted in global flags</param>
    /// <param name="isPerDeath">Check if the flag is enlisted in per-death flags</param>
    /// <param name="isPerRoom">Check if the flag is enlisted in per-room flags</param>
    [Note("Included in 1.0")]
    public static bool GetChroniaFlag(string flag, bool isGlobal = false, bool isPerDeath = false, bool isPerRoom = false)
        => (flag.GetFlag() && !isGlobal && !isPerDeath && !isPerRoom)
        || ((Md.SaveData?.flags.Contains(flag) ?? false) && isGlobal)
        || ((Md.Session?.flagsPerRoom.Contains(flag) ?? false) && isPerRoom)
        || ((Md.Session?.flagsPerDeath.Contains(flag) ?? false) && isPerDeath);
}
