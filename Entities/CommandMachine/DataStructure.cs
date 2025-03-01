using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities.CommandMachine;
public static class DataStructure
{
    // 命令定义
    public sealed record CommandDefinition
    {
        public string Name;
        public ParameterDefinition[] Parameters;
        public Func<string[], IEnumerator> Handler;
    }

    // 命令参数定义
    public sealed record ParameterDefinition
    {
        public Type ParameterType;
        public bool IsOptional;
        public bool IsVariable;
        public object DefaultValue;

        public ParameterDefinition(Type parameterType, bool isOptional = false, bool isVariable = false, object defaultValue = default)
        {
            ParameterType = parameterType;
            IsOptional = isOptional;
            IsVariable = isVariable;
            DefaultValue = defaultValue;
        }
    }

    // 解析后的命令定义 wip
    public record CommandEntry
    {
        public string CommandName;
        public string[] Args;
        public CommandPrefix Prefix;
        public StatementBlockContext Inner;
    }

    // 命令前缀定义
    public enum CommandPrefix
    {
        None,
        Async,
        Sync
    }

    // 代码块类型定义
    public enum StatementBlockType
    {
        Do,
        If,
        For,
        While
    }
}
