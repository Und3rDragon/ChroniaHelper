using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

[ModImportName("FrostHelper")] // registered in Module
public static class FrostHelperImports
{
    /// <summary>
    /// Creates an object which can evaluate a Session Expression.
    /// The returned object can be passed to <see cref="GetSessionExpressionValue"/>
    /// Refer to https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions
    /// </summary>
    public delegate bool _TryCreateSessionExpression(string str, [NotNullWhen(true)] out object? expression);
    public static _TryCreateSessionExpression TryCreateSessionExpression;
    public static object FrostHelper_TryCreateSessionExpression(this string str)
    {
        TryCreateSessionExpression(str, out object expression);
        return expression;
    }

    /// <summary>
    /// Returns the current value of a Session Expression.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public delegate object _GetSessionExpressionValue(object expression, Session session);
    public static _GetSessionExpressionValue GetSessionExpressionValue;
    public static object FrostHelper_GetSessionExpressionValue(this object expression)
    {
        return GetSessionExpressionValue(expression, MaP.level.Session);
    }

    /// <summary>
    /// Returns the type that the given session expression will return, or typeof(object) if that's unknown.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public delegate Type _GetSessionExpressionReturnedType(object expression);
    public static _GetSessionExpressionReturnedType GetSessionExpressionReturnedType;
    public static Type FrostHelper_GetSessionExpressionReturnedType(this object expression)
    {
        return GetSessionExpressionReturnedType(expression);
    }

    /// <summary>
    /// Returns the current value of a Session Expression as an integer, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public delegate int _GetIntSessionExpressionValue(object expression, Session session);
    public static _GetIntSessionExpressionValue GetIntSessionExpressionValue;
    public static int FrostHelper_GetIntSessionExpressionValue(this object expression)
    {
        return GetIntSessionExpressionValue(expression, MaP.level.Session);
    }

    /// <summary>
    /// Returns the current value of a Session Expression as a float, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public delegate float _GetFloatSessionExpressionValue(object expression, Session session);
    public static _GetFloatSessionExpressionValue GetFloatSessionExpressionValue;
    public static float FrostHelper_GetFloatSessionExpressionValue(this object expression)
    {
        return GetFloatSessionExpressionValue(expression, MaP.level.Session);
    }
    
    /// <summary>
    /// Returns the current value of a Session Expression as a boolean, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public delegate bool _GetBoolSessionExpressionValue(object expression, Session session);
    public static _GetBoolSessionExpressionValue GetBoolSessionExpressionValue;
    public static bool FrostHelper_GetBoolSessionExpressionValue(this object expression)
    {
        return GetBoolSessionExpressionValue(expression, MaP.level.Session);
    }

    /// <summary>
    /// Creates a Session Expression Context object, which can be passed to <see cref="TryCreateSessionExpression(string,object,out object?)"/>
    /// This allows you to register custom commands for specific entities.
    /// A context should be created once, and reused as much as possible.
    /// <br />
    /// Do not capture entity instances into the Func objects, instead pass a `userdata` object when calling GetSessionExpressionValue,
    /// which will be passed as the 2nd argument your functions.
    /// <br />
    /// Dictionary keys are names under which the commands will be available. For example, if your key is 'coolValue',
    /// then it will be accessed as `$coolValue` in Session Expressions created using this context.
    /// </summary>
    public delegate object _CreateSessionExpressionContext(
        Dictionary<string, Func<Session, object? /* userdata */, object>>? simpleCommands,
        Dictionary<string, Func<Session, object? /* userdata */, IReadOnlyList<object>, object>>? functionCommands);
    public static _CreateSessionExpressionContext CreateSessionExpressionContext;

    public static Func<Color> GetBloomColor;
    public static Color FrostHelper_GetBloomColor()
    {
        return GetBloomColor();
    }
    public static CColor FrostHelper_GetBloomChroniaColor()
    {
        return GetBloomColor().GetChroniaColor();
    }
}