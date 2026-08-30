using ChroniaHelper.Utils;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Imports;

[ModImportName("FrostHelper")] // registered in Module
public static class APIFrostHelper
{
    public delegate bool TryCreateSessionExpressionDelegate(string str, object context, out object expression);
    public static TryCreateSessionExpressionDelegate TryCreateSessionExpression;
    /// <summary>
    /// Creates an object which can evaluate a Session Expression.
    /// The returned object can be passed to <see cref="GetSessionExpressionValue"/>
    /// Refer to https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions
    /// </summary>
    public static bool tryCreateSessionExpression(this string str, object context, out object expression)
        => TryCreateSessionExpression(str, context, out expression);
    public static bool tryCreateSessionExpression(this string str, out object expression)
        => TryCreateSessionExpression(str, null, out expression);

    public static Func<object, Session, object> GetSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static object getSessionExpressionValue(this object expression, Session session)
         => GetSessionExpressionValue(expression, session);
    public static object getSessionExpressionValue(this object expression)
         => GetSessionExpressionValue(expression, MaP.level.Session);


    public static Func<object, Type> GetSessionExpressionReturnedType;
    /// <summary>
    /// Returns the type that the given session expression will return, or typeof(object) if that's unknown.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static Type getSessionExpressionReturnedType(this object expression)
        => GetSessionExpressionReturnedType(expression);

    public static Func<object, Session, int> GetIntSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as an integer, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static int getIntSessionExpressionValue(this object expression, Session session)
        => GetIntSessionExpressionValue(expression, session);
    public static int getIntSessionExpressionValue(this object expression)
        => GetIntSessionExpressionValue(expression, MaP.level.Session);

    public static Func<object, Session, float> GetFloatSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as a float, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static float getFloatSessionExpressionValue(this object expression)
        => GetFloatSessionExpressionValue(expression, MaP.level.Session);
    public static float getFloatSessionExpressionValue(this object expression, Session session)
        => GetFloatSessionExpressionValue(expression, session);

    public static Func<object, Session, bool> GetBoolSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as a boolean, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="TryCreateSessionExpression"/>
    /// </summary>
    public static bool getBoolSessionExpressionValue(this object expression)
        => GetBoolSessionExpressionValue(expression, MaP.level.Session);
    public static bool getBoolSessionExpressionValue(this object expression, Session session)
        => GetBoolSessionExpressionValue(expression, session);

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
        => CreateSessionExpressionContext(simpleCommands, functionCommands);

    public static Func<Color> GetBloomColor;
    public static Action<Color> SetBloomColor;
}