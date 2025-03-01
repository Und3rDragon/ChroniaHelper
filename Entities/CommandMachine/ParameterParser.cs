using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using static ChroniaHelper.Entities.CommandMachine.DataStructure;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities.CommandMachine;

// 枚举转换器注册测试用例
public enum Direction { Up, Down, Left, Right }

// 命令参数解析器 把原始字符串转换为类型化数据
public static class ParameterParser
{
    // 类型转换器字典
    private static readonly Dictionary<Type, (int requiredParamCount, Func<string[], object> converter)> converters = new Dictionary<Type, (int, Func<string[], object>)>();

    // 注册内置类型转换器
    static ParameterParser()
    {
        RegisterConverter<int>(1, args => int.Parse(args[0]));
        RegisterConverter<float>(1, args => float.Parse(args[0]));
        RegisterConverter<bool>(1, args => bool.Parse(args[0]));
        RegisterConverter<string>(1, args => args[0].Trim());

        RegisterConverter<Vector2>(2, args => new Vector2(float.Parse(args[0]), float.Parse(args[1])));
        RegisterConverter<Color>(1, args => Calc.HexToColor(args[0].Trim()));

        RegisterConverter<Direction>(1, args => Enum.Parse<Direction>(args[0], true));
    }

    // 注册自定义类型转换器
    public static void RegisterConverter<T>(int requiredParamCount, Func<string[], T> converter)
    {
        converters[typeof(T)] = (requiredParamCount, args => converter(args));
    }

    // 解析参数列表
    public static object[] Parse(string[] rawArgs, ParameterDefinition[] paramDefs)
    {
        Logger.Log(LogLevel.Verbose, "CommandMachine/ParameterParser", $"Command parameter list: {FormatParameterDefinitions(paramDefs)}");

        List<object> values = new List<object>();
        int rawArgsIndex = 0;
        int paramsIndex = 0;

        // 预处理 把 "_" 替换为 null 标记为弃元
        string[] processedArgs = rawArgs.Select(arg => arg.Trim().Equals("_", StringComparison.OrdinalIgnoreCase) ? null : arg).ToArray();

        // 逐个解析参数
        foreach (var def in paramDefs)
        {
            int startIndex = rawArgsIndex;
            paramsIndex++;

            object value;

            if (def.IsVariable)
            {
                value = ParseVariable(def, processedArgs, ref rawArgsIndex);
                values.Add(value);

                string[] usedParams = rawArgs.Skip(startIndex).Take(rawArgsIndex - startIndex).ToArray();
                Logger.Log(LogLevel.Verbose, "CommandMachine/ParameterParser", $"Parsing parameter succeeded: [{paramsIndex}/{paramDefs.Length}] {def.ParameterType.Name} => {string.Join(", ", CommandBulider.FormatValue(value))}, Used parameters: '{string.Join(",", usedParams)}'");
                break;
            }

            value = ParseSingleParam(def, processedArgs, ref rawArgsIndex);
            values.Add(value);

            string[] usedParamsSingle = rawArgs.Skip(startIndex).Take(rawArgsIndex - startIndex).ToArray();
            Logger.Log(LogLevel.Verbose, "CommandMachine/ParameterParser", $"Parsing parameter succeeded: [{paramsIndex}/{paramDefs.Length}] {def.ParameterType.Name} => {CommandBulider.FormatValue(value)}, Used parameters: '{string.Join(",", usedParamsSingle)}'");
        }

        // 检查是否有多余参数
        if (rawArgsIndex < processedArgs.Length)
            throw new ArgumentException($"Too many parameters at index {rawArgsIndex}. Unprocessed: {rawArgs.Length - rawArgsIndex}");

        return values.ToArray();
    }

    // 解析单个参数
    private static object ParseSingleParam(ParameterDefinition def, string[] args, ref int rawArgsIndex)
    {
        // 处理可选参数 如果参数索引超出原始字符串参数数组长度或是弃元并且是可选参数则使用默认值
        if (rawArgsIndex >= args.Length || args[rawArgsIndex] == null)
        {
            if (def.IsOptional)
            {
                rawArgsIndex++;
                return def.DefaultValue;
            }

            if (args.Length == 0 || args[rawArgsIndex] != null)
                throw new ArgumentException($"Missing required parameter at index {rawArgsIndex}: {def.ParameterType.Name}");
            else
                throw new ArgumentException($"Cannot discard a required parameter at index {rawArgsIndex}: {def.ParameterType.Name}");
        }

        // 获取类型转换器
        if (!converters.TryGetValue(def.ParameterType, out var converter))
            throw new NotSupportedException($"No registered converter for type: {def.ParameterType.Name} at index {rawArgsIndex}");

        // 检查参数数量是否足够
        if (rawArgsIndex + converter.requiredParamCount > args.Length)
            throw new ArgumentException($"Insufficient parameters for {def.ParameterType.Name} at index {rawArgsIndex}. Need {converter.requiredParamCount}");

        // 执行类型转换
        try
        {
            string[] subArgs = args.Skip(rawArgsIndex).Take(converter.requiredParamCount).ToArray();
            object value = converter.converter(subArgs);



            rawArgsIndex += converter.requiredParamCount;
            return value;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Conversion error for '{string.Join(" ", args.Skip(rawArgsIndex).Take(converter.requiredParamCount))}' at index {rawArgsIndex}: {ex.Message}");
        }
    }

    // 解析可变参数
    private static object ParseVariable(ParameterDefinition def, string[] args, ref int rawArgsIndex)
    {
        // 获取数组元素类型
        Type elementType = def.ParameterType.GetElementType() ??
            throw new ArgumentException("Variable parameter must be array type");

        // 获取元素类型转换器
        if (!converters.TryGetValue(elementType, out var converter))
            throw new NotSupportedException($"No registered converter for element type: {elementType.Name}");

        int requiredPerElement = converter.requiredParamCount;
        int remainingArgs = args.Length - rawArgsIndex;
        Logger.Log(LogLevel.Verbose, "CommandMachine/ParameterParser", $"Variable parameter requirements: {requiredPerElement} params/element");

        // 验证参数数量是否合法
        if (remainingArgs % requiredPerElement != 0)
            throw new ArgumentException($"Variable parameters count must be multiple of {requiredPerElement}");

        // 创建目标数组
        Array array = Array.CreateInstance(elementType, remainingArgs / requiredPerElement);
        for (int i = 0; i < array.Length; i++)
        {
            string[] elementArgs = args.Skip(rawArgsIndex).Take(requiredPerElement).ToArray();

            // 验证可变参数是否有弃元 
            if (elementArgs.Any(a => a == null))
                throw new ArgumentException($"Cannot discard elements in variable parameters at index {rawArgsIndex}");

            // 转换并填充元素
            try
            {
                array.SetValue(converter.converter(elementArgs), i);
                rawArgsIndex += requiredPerElement;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Conversion error for '{string.Join(" ", elementArgs)}': {ex.Message}");
            }
        }

        return array;
    }

    // 格式化参数定义
    private static string FormatParameterDefinitions(ParameterDefinition[] parameters)
    {
        var sb = new StringBuilder();
        foreach(var param in parameters)
        {
            string typeName = param.ParameterType.Name;

            if (param.IsOptional)
                sb.Append($"{typeName}(Optional: {CommandBulider.FormatValue(param.DefaultValue)}), ");
            else if (param.IsVariable)
                sb.Append($"{typeName}(Variable), ");
            else sb.Append($"{typeName}, ");
        }

        if (sb.Length > 0)
            sb.Remove(sb.Length - 2, 2);

        return sb.ToString();
    }
}
