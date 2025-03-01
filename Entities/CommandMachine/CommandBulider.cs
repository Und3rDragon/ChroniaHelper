using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Entities.CommandMachine.DataStructure;


namespace ChroniaHelper.Entities.CommandMachine;

// 命令构建器 提供链式调用以定义命令参数结构并绑定实现
public sealed class CommandBulider
{
    private CommandMachine commandMachine;
    private readonly List<ParameterDefinition> parameters;
    private string commandName;

    public CommandBulider(CommandMachine commandMachine, string commandName)
    {
        this.commandMachine = commandMachine;
        this.commandName = commandName;
        parameters = new List<ParameterDefinition>();
    }

    // 必选参数
    public CommandBulider Required<T>()
    {
        parameters.Add(new ParameterDefinition(typeof(T)));
        return this;
    }

    // 可选参数 没有则使用类型默认值
    public CommandBulider Optional<T>(T defaultValue)
    {
        parameters.Add(new ParameterDefinition(typeof(T), isOptional: true, defaultValue: defaultValue));
        return this;
    }

    // 可变参数
    public CommandBulider Variable<TArray>() where TArray : IEnumerable
    {
        if (!typeof(TArray).IsArray)
        {
            Logger.Log(LogLevel.Error, "CommandMachine/CommandBuilder", "Variable parameters must be array type");
            throw new ArgumentException("Variable parameters must be array type");
        }

        parameters.Add(new ParameterDefinition(typeof(TArray), isVariable: true, defaultValue: Array.Empty<TArray>()));
        return this;
    }

    // 完成命令构建并注册
    public void Handle<TDelegate>(TDelegate handler) where TDelegate : Delegate
    {
        // 验证参数类型与数量是否合法
        ValidateParameter(handler);

        // 包装原始实现 添加参数解析逻辑
        Func<string[], IEnumerator> wrappedHandler = args =>
        {
            object[] parsedParams;

            // 解析参数
            try
            {
                Logger.Log(LogLevel.Debug, "CommandMachine", $"Prasing command entry: '{commandName},{string.Join(",", args)}'");
                parsedParams = ParameterParser.Parse(args, parameters.ToArray());
                Logger.Log(LogLevel.Debug, "CommandMachine", $"Parsing command entry from '{commandName},{string.Join(",", args)}' to {handler.Method.Name}({FormatConvertedParameters(parsedParams)}) successed");
            }
            catch (Exception ex)
            {
                throw new Exception($"Parsing parameters failed for command '{commandName}, {string.Join(", ", args)}'. Error: {ex.Message}");
            }

            // 调用原始实现
            return (IEnumerator)handler.DynamicInvoke(parsedParams);
        };

        // 注册命令
        commandMachine.RegisterCommand(commandName, parameters.ToArray(), wrappedHandler);
        Logger.Log(LogLevel.Debug, "CommandMachine", $"Command '{commandName}' registered.");
    }

    // 验证参数类型与数量是否合法
    private void ValidateParameter<TDelegate>(TDelegate handler) where TDelegate : Delegate
    {
        MethodInfo method = handler.GetMethodInfo();
        ParameterInfo[] handlerParams = method.GetParameters();

        // 验证参数数量是否匹配
        if (handlerParams.Length != parameters.Count)
            throw new ArgumentException($"Parameter count mismatch. Defined: {parameters.Count}, Handler: {handlerParams.Length}");

        // 验证参数类型是否匹配
        for (int i = 0; i < handlerParams.Length; i++)
        {
            Type handlerType = handlerParams[i].ParameterType;
            Type definedType = parameters[i].ParameterType;

            if (handlerType != definedType && !(handlerType.IsArray && definedType.IsArray))
                throw new ArgumentException($"Parameter {i} type mismatch. Defined: {definedType.Name}, Handler: {handlerType.Name}");
        }

        bool foundOptional = false;
        bool foundVariable = false;

        // 验证参数顺序
        foreach (var parameter in parameters)
        {
            // 可变参数必须是最后一个
            if (parameter.IsVariable && parameter != parameters.Last())
                throw new ArgumentException("Variable parameter must be the last parameter");

            // 不能有多个可变参数
            if (parameter.IsVariable && foundVariable)
                throw new ArgumentException("Only one variable parameter is allowed");

            // 可选参数后不能有必需参数
            if (parameter.IsOptional)
                foundOptional = true;
            else if (!parameter.IsVariable && foundOptional)
                throw new ArgumentException("Required parameter cannot come after optional parameters");

            if (parameter.IsVariable)
                foundVariable = true;
        }
    }

    // 格式化转换后的参数
    private string FormatConvertedParameters(object[] values)
    {
        var sb = new StringBuilder();
        foreach (var value in values)
            sb.Append($"{FormatValue(value)}, ");

        if (sb.Length > 0)
            sb.Remove(sb.Length - 2, 2);

        return sb.ToString();
    }

    // 格式化单个参数
    public static string FormatValue(object value)
    {
        if (value == null)
            return "null";

        if (value is string str)
            return $"\"{str}\"";

        if (value is float f)
            return $"{f}f";

        if (value is Enum e)
            return $"{e.GetType().Name}.{e}";

        if (value is Array)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var element in (Array)value)
                sb.Append($"{FormatValue(element)}, ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append("]");
            return sb.ToString();
        }

        return value.ToString();
    }
}
