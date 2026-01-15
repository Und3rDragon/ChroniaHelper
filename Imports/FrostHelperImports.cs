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
    public delegate bool _TryCreateSessionExpression(string str, [NotNullWhen(true)] out object? expression);
    public static _TryCreateSessionExpression _tryCreateSessionExpression;
    /// <summary>
    /// Creates an object which can evaluate a Session Expression.
    /// The returned object can be passed to <see cref="_getSessionExpressionValue"/>
    /// Refer to https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions
    /// </summary>
    public static object TryCreateSessionExpression(this string str)
    {
        _tryCreateSessionExpression(str, out object expression);
        return expression;
    }

    public delegate object _GetSessionExpressionValue(object expression, Session session);
    public static _GetSessionExpressionValue _getSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression.
    /// The object passed as the 1st argument needs to be created via <see cref="_tryCreateSessionExpression"/>
    /// </summary>
    public static object GetSessionExpressionValue(this object expression)
    {
        return _getSessionExpressionValue(expression, MaP.level.Session);
    }

    public delegate Type _GetSessionExpressionReturnedType(object expression);
    public static _GetSessionExpressionReturnedType _getSessionExpressionReturnedType;
    /// <summary>
    /// Returns the type that the given session expression will return, or typeof(object) if that's unknown.
    /// The object passed as the 1st argument needs to be created via <see cref="_tryCreateSessionExpression"/>
    /// </summary>
    public static Type GetSessionExpressionReturnedType(this object expression)
    {
        return _getSessionExpressionReturnedType(expression);
    }

    public delegate int _GetIntSessionExpressionValue(object expression, Session session);
    public static _GetIntSessionExpressionValue _getIntSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as an integer, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="_tryCreateSessionExpression"/>
    /// </summary>
    public static int GetIntSessionExpressionValue(this object expression)
    {
        return _getIntSessionExpressionValue(expression, MaP.level.Session);
    }

    public delegate float _GetFloatSessionExpressionValue(object expression, Session session);
    public static _GetFloatSessionExpressionValue _getFloatSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as a float, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="_tryCreateSessionExpression"/>
    /// </summary>
    public static float GetFloatSessionExpressionValue(this object expression)
    {
        return _getFloatSessionExpressionValue(expression, MaP.level.Session);
    }
    
    public delegate bool _GetBoolSessionExpressionValue(object expression, Session session);
    public static _GetBoolSessionExpressionValue _getBoolSessionExpressionValue;
    /// <summary>
    /// Returns the current value of a Session Expression as a boolean, coercing it if needed.
    /// The object passed as the 1st argument needs to be created via <see cref="_tryCreateSessionExpression"/>
    /// </summary>
    public static bool GetBoolSessionExpressionValue(this object expression)
    {
        return _getBoolSessionExpressionValue(expression, MaP.level.Session);
    }

    public delegate object _CreateSessionExpressionContext(
        Dictionary<string, Func<Session, object? /* userdata */, object>>? simpleCommands,
        Dictionary<string, Func<Session, object? /* userdata */, IReadOnlyList<object>, object>>? functionCommands);
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
    public static _CreateSessionExpressionContext _createSessionExpressionContext;

    public static Func<Color> _getBloomColor;
    public static Color GetBloomColor()
    {
        return _getBloomColor();
    }
    public static CColor GetBloomChroniaColor()
    {
        return _getBloomColor().GetChroniaColor();
    }
}