global using ApiRenderPart = (string Contents, string ColorId, System.Collections.Generic.IReadOnlyList<(string Contents, string ColorId)>? Tooltip);
using MonoMod.ModInterop;

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

    // Updated FrostHelper API Version 2

    /// <summary>
    /// Creates a delegate which can evaluate a Session Expression efficiently.
    /// This is the recommended way to evaluate session expressions.
    /// This function should only be called once when creating an entity, the delegate should be stored for re-use.
    /// Added in Frost Helper 1.81.0
    /// </summary>
    /// <param name="str">The expression</param>
    /// <param name="context">The Session Expression Context to use, leave null to use the global context. The context object should be generated by <see cref="CreateSessionExpressionContextV2"/>.</param>
    /// <param name="returnType">The type that should be returned by the expression.</param>
    /// <returns>A delegate which evaluates the expression. Will always be of type Func{Session, object?, returnType} and can safely be cast to that type.</returns>
    public static Func<string, object?, Type, Delegate?> CreateTypedSessionExpressionOrNull;
    public static Delegate? createTypedSessionExpressionOrNull(string str, object? context, Type returnType)
    {
        return CreateTypedSessionExpressionOrNull(str, context, returnType);
    }

    /// <summary>
    /// Coerces the given value to the target type, using Session Expression rules.
    /// Added in Frost Helper 1.81.0
    /// </summary>
    public static Func<object, Type, object> CoerceValueInSessionExpression;
    public static object coerceValueInSessionExpression(object value, Type targetType)
    {
        return CoerceValueInSessionExpression(value, targetType);
    }

    /// <summary>
    /// Registers a simple Session Expression command, which will be accessible in Session Expressions via
    /// - $cmdName if a context is provided,
    /// - $modName.cmdName if the command is global (no context is provided).
    /// Added in Frost Helper 1.81.0
    /// </summary>
    /// <param name="modName">Name of the mod which registers this command. Will be used to prefix the command name.</param>
    /// <param name="cmdName">Name of the command</param>
    /// <param name="context">The Expression Context, created via CreateSessionExpressionContextV2, to register this command to. Leave null to register the command globally.</param>
    /// <param name="command">
    /// Function called each time the command needs to be evaluated. Must have a return type.
    /// May accept up to 2 arguments, where the first one is a Session, and the second is userdata.
    /// Userdata is object? by default, it can be typed as a different type if the command is registered non-globally.
    /// If userdata is not typed as object?, and the correct userdata type is not passed when evaluating the command, an exception will be thrown.
    ///
    /// For optimal performance, make sure the delegate is created from a static method, like:
    /// <code>
    /// static void Register() {
    ///     RegisterSimpleSessionExpressionCommandV2("yourMod", "three", null, [], Three);
    /// }
    /// 
    /// static int Three() {
    ///    return 3;
    /// }
    /// </code>
    /// </param>
    /// <param name="description">Description of this command, visible in Mapping Utils. (ApiRenderPart is defined in Api.RenderPart.cs)</param>
    public static Action<string, string, object?, IReadOnlyList<ApiRenderPart>, Delegate> RegisterSimpleSessionExpressionCommandV2;
    public static void registerSimpleSessionExpressionCommandV2(string modName, string cmdName, object? context,
        IReadOnlyList<ApiRenderPart> description, Delegate command) => RegisterSimpleSessionExpressionCommandV2(modName, cmdName, context, description, command);

    /// <summary>
    /// Registers a simple Session Expression function, which will be accessible in Session Expressions via
    /// - $cmdName(...) if a context is provided,
    /// - $modName.cmdName(...) if the command is global (no context is provided).
    /// </summary>
    /// Added in Frost Helper 1.81.0
    /// <param name="modName">Name of the mod which registers this command. Will be used to prefix the command name.</param>
    /// <param name="cmdName">Name of the command</param>
    /// <param name="context">The Expression Context, created via CreateSessionExpressionContextV2, to register this command to. Leave null to register the command globally.</param>
    /// <param name="func">
    /// Function called each time the function needs to be evaluated. Must have a return type.
    /// MUST accept at least 2 arguments, where the first one is a Session, and the second is userdata.
    /// Userdata is object? by default, it can be typed as a different type if the command is registered non-globally.
    /// If userdata is not typed as object?, and the correct userdata type is not passed when evaluating the function, an exception will be thrown.
    ///
    /// The delegate may accept more than 2 arguments, in which case all further arguments will be obtained from the Session Expression.
    /// These arguments may accept any C# type, arguments will be coerced automatically using usual Session Expression rules.
    /// 
    /// For optimal performance, make sure the delegate is created from a static method, like:
    /// <code>
    /// static void Register() {
    ///     RegisterFunctionSessionExpressionCommandV2("yourMod", "sum", null, [], Sum);
    /// }
    /// 
    /// static int Sum(Session session, object? userdata, int a, int b) {
    ///    return a + b;
    /// }
    /// </code>
    /// </param>
    /// <param name="description">Description of this function, visible in Mapping Utils. (ApiRenderPart is defined in Api.RenderPart.cs)</param>
    public static Action<string, string, object?, IReadOnlyList<ApiRenderPart>, Delegate> RegisterFunctionSessionExpressionCommandV2;
    public static void registerFunctionSessionExpressionCommandV2(string modName, string cmdName, object? context, IReadOnlyList<ApiRenderPart> description, Delegate func)
    {
        RegisterFunctionSessionExpressionCommandV2(modName, cmdName, context, description, func);
    }

    /// <summary>
    /// Creates a Session Expression Context object, which can be passed to <see cref="TryCreateSessionExpression(string,object,out object?)"/>
    /// This allows you to register custom commands for specific entities.
    /// A context should be created once, and reused as much as possible.
    ///
    /// Commands and functions can be added to this context via other API functions.
    /// Added in Frost Helper 1.81.0
    /// </summary>
    public static Func<object> CreateSessionExpressionContextV2;
    public static object createSessionExpressionContextV2() => CreateSessionExpressionContextV2();

}