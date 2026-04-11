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
public static class APIFrostHelper
{
    public delegate bool TryCreateSessionExpressionDelegate(string str, object context, out object? expression);
    public static TryCreateSessionExpressionDelegate TryCreateSessionExpression;
    /// <summary>
    /// Creates an object which can evaluate a Session Expression.
    /// The returned object can be passed to <see cref="GetSessionExpressionValue"/>
    /// Refer to https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions
    /// </summary>
    public static object tryCreateSessionExpression(this string str, object context = null)
    {
        TryCreateSessionExpression(str, context, out object expression);
        return expression;
    }

    public delegate object GetSessionExpressionValueDelegate(object expression, Session session);
    public static GetSessionExpressionValueDelegate GetSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static object getSessionExpressionValue(this object expression)
    {
        return GetSessionExpressionValue(expression, MaP.level.Session);
    }

    public delegate Type GetSessionExpressionReturnedTypeDelegate(object expression);
    public static GetSessionExpressionReturnedTypeDelegate GetSessionExpressionReturnedType;
    /// <summary>
    /// Returns the type that the given session expression will return, or typeof(object) if that's unknown.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static Type getSessionExpressionReturnedType(this object expression)
    {
        return GetSessionExpressionReturnedType(expression);
    }

    public delegate int GetIntSessionExpressionValueDelegate(object expression, Session session);
    public static GetIntSessionExpressionValueDelegate GetIntSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as an integer, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static int getIntSessionExpressionValue(this object expression)
    {
        return GetIntSessionExpressionValue(expression, MaP.level.Session);
    }

    public delegate float GetFloatSessionExpressionValueDelegate(object expression, Session session);
    public static GetFloatSessionExpressionValueDelegate GetFloatSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as a float, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static float getFloatSessionExpressionValue(this object expression)
    {
        return GetFloatSessionExpressionValue(expression, MaP.level.Session);
    }
    
    public delegate bool GetBoolSessionExpressionValueDelegate(object expression, Session session);
    public static GetBoolSessionExpressionValueDelegate GetBoolSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as a boolean, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static bool getBoolSessionExpressionValue(this object expression)
    {
        return GetBoolSessionExpressionValue(expression, MaP.level.Session);
    }
    
    public static Func<Dictionary<string, Func<Session, object? /* userdata */, object>>?,
        Dictionary<string, Func<Session, object? /* userdata */, IReadOnlyList<object>, object>>?,
        object> CreateSessionExpressionContext;
    /// <summary>
    /// Creates a Session Expression Context object, which can be passed to <see cref="TryCreateSessionExpressionDelegate"/>
    /// This allows you to register custom commands for specific entities.
    /// A context should be created once, and reused as much as possible.
    /// <br />
    /// Do not capture entity instances into the Func objects, instead pass a `userdata` object when calling GetSessionExpressionValue,
    /// which will be passed as the 2nd argument your functions.
    /// <br />
    /// Dictionary keys are names under which the commands will be available. For example, if your key is 'coolValue',
    /// then it will be accessed as `$coolValue` in Session Expressions created using this context.
    /// </summary>
    public static object createSessionExpressionContext(
        Dictionary<string, Func<Session, object? /* userdata */, object>>? simpleCommands,
        Dictionary<string, Func<Session, object? /* userdata */, IReadOnlyList<object>, object>>? functionCommands)
    {
        return CreateSessionExpressionContext(simpleCommands, functionCommands);
    }

    public static Func<Color> GetBloomColor;
    public static Action<Color> SetBloomColor;
}