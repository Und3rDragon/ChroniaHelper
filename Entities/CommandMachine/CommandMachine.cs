using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Entities.CommandMachine.DataStructure;
using ChroniaHelper.Components;

namespace ChroniaHelper.Entities.CommandMachine;

// ToDo 流程控制(if while) 变量存储 预处理命名流(构建代码块)



// 命令机 提供命令的注册 解析 执行
public sealed class CommandMachine : Component
{
    private readonly Dictionary<string, CommandDefinition> commands = new Dictionary<string, CommandDefinition>(StringComparer.OrdinalIgnoreCase);
    private InstantCoroutine executionCoroutine;
    private List<InstantCoroutine> globalActiveAsyncCoroutines;
    
    public CommandMachine(bool logging)
        : base(true, false)
    {
        if (logging)
        {
            Logger.SetLogLevel("CommandMachine", LogLevel.Verbose);
            Logger.SetLogLevel("CommandMachine/CommandBuilder", LogLevel.Verbose);
            Logger.SetLogLevel("CommandMachine/ParameterParser", LogLevel.Verbose);
        }

        globalActiveAsyncCoroutines = new List<InstantCoroutine>();
    }

    // 通过链式调用注册命令
    public CommandBulider RegisterCommand(string name)
    {
        return new CommandBulider(this, name);
    }

    // 把命令注册到字典 由 CommandBulider 调用
    internal void RegisterCommand(string name, ParameterDefinition[] parameters, Func<string[], IEnumerator> handler)
    {
        commands[name] = new CommandDefinition
        {
            Name = name,
            Parameters = parameters,
            Handler = handler
        };
    }

    // 执行命令流
    public void Execute(string commandStream)
    {
        executionCoroutine = new InstantCoroutine(ExecutionCoroutine(commandStream), false, true);
    }

    // 
    public override void Update()
    {
        base.Update();

        foreach (var coroutine in globalActiveAsyncCoroutines)
        {
            if (coroutine != null && coroutine.Active)
            {
                coroutine.PreUpdate(null);
                coroutine.Update();
            }
        }
        globalActiveAsyncCoroutines.RemoveAll(c => c.Completed);

        if (executionCoroutine != null && executionCoroutine.Active)
        {
            executionCoroutine.PreUpdate(null);
            executionCoroutine.Update();
            if (executionCoroutine.Completed)
            {
                executionCoroutine = null;
                globalActiveAsyncCoroutines.Clear();
            }
        }
    }

    // 主执行协程
    private IEnumerator ExecutionCoroutine(string rawCommandStream)
    {
        Logger.Log(LogLevel.Debug, "CommandMachine", $"Starting execute command stream: '{rawCommandStream}'");

        // 分割命令 预期格式 command1,param1,param2,...;command2,param1,param2,...;commandN...,param1,paramN
        string[] commandEntries = rawCommandStream.Split([';'], StringSplitOptions.RemoveEmptyEntries);
        //string[] commandEntries = PreProcessCommandStrem(commandStream);

        foreach (string entry in commandEntries)
        {
            string trimmedEntry = entry.Trim();
            if (string.IsNullOrEmpty(trimmedEntry)) continue;

            // 解析单个命令
            ParseCommandEntry(trimmedEntry, out string commandName, out string[] args, out CommandPrefix prefix);

            if (commands.TryGetValue(commandName, out CommandDefinition def))
            {
                // 使用可以捕获指令运行时抛出异常的协程进行包装
                var safeHandler = SafeExecutionWrapper(commandName, def.Handler(args), trimmedEntry, prefix);

                switch (prefix)
                {           
                // async前缀 启动异步协程 避免阻塞主流程 
                case CommandPrefix.Async:
                    var asyncCorotine = new InstantCoroutine(safeHandler, true);
                    globalActiveAsyncCoroutines.Add(asyncCorotine);
                    asyncCorotine.Update();
                    break;

                // sync前缀 等待所有异步协程执行完成再执行
                case CommandPrefix.Sync:
                    // 等待所有异步执行完成
                    // 目前不保证能能正确获取异步协程在这一帧是否完成 wip
                    while (globalActiveAsyncCoroutines.Any(c => c.Active))
                        yield return null;

                    yield return safeHandler;
                    break;

                // 无前缀 直接执行并等待执行完成
                case CommandPrefix.None:
                    yield return safeHandler;
                    break;
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "CommandMachine", $"Unregistered command: '{trimmedEntry}'. Skipped \n");
            }
        }

        // 等待所有异步执行完成
        // 目前不保证能能正确获取异步协程在这一帧是否完成 wip
        while (globalActiveAsyncCoroutines.Any(c => c.Active))
        {
            yield return null;
        }

        Logger.Log(LogLevel.Debug, "CommandMachine", $"Command stream execution completed: '{rawCommandStream}'");
    }

    // 用于捕获命令执行时抛出的异常的协程包装
    private IEnumerator SafeExecutionWrapper(string commandName, IEnumerator handler, string commandEntry, CommandPrefix prefix)
    {
        Logger.Log(LogLevel.Debug, "CommandMachine", $"{(prefix == CommandPrefix.Async ? "Async" : "")} command execution started: '{commandEntry}'");

        while (true)
        {
            bool moveNext;

            // 尝试推进迭代器 有异常抛出时进行捕获并结束命令的执行 
            try
            {
                moveNext = handler.MoveNext();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CommandMachine", $"{(prefix == CommandPrefix.Async ? "Async" : "")} command execution failed: {commandName}, Skipped. Error: {ex.Message}\n");
                yield break;
            }
            
            // 迭代器结束时退出循环
            if (!moveNext)
                break;

            // 命令内部 yield return 的值
            var current = handler.Current;

            yield return current;
        }
        Logger.Log(LogLevel.Debug, "CommandMachine", $"{(prefix == CommandPrefix.Async ? "Async" : "")} command execution completed: '{commandEntry}'\n");
    }

    // 解析单个命令 需要重构以支持 StatementBlockContext
    // 预期格式 command,param1,param2,...,paramN
    private void ParseCommandEntry(string commandEntry, out string command, out string[] args, out CommandPrefix prefix)
    {
        // 获取前缀
        if (commandEntry.StartsWith("async", StringComparison.OrdinalIgnoreCase) || commandEntry.StartsWith("passby", StringComparison.OrdinalIgnoreCase))
        {
            commandEntry = commandEntry[6..];
            prefix = CommandPrefix.Async;
        }
        else if (commandEntry.StartsWith("sync", StringComparison.OrdinalIgnoreCase))
        {
            commandEntry = commandEntry[4..];
            prefix = CommandPrefix.Sync;
        }
        else
        {
            prefix= CommandPrefix.None;
        }

        // 分割命令与参数
        string[] parts = commandEntry.Split([','], StringSplitOptions.RemoveEmptyEntries);
        command = parts[0].Trim();
        args = parts.Skip(1).Select(p => p.Trim()).ToArray();
    }

/*    // 预处理命令流 wip
    private string[] PreProcessCommandStrem(string rawCommandStream)
    {
        List<string> processedEntries = new List<string>();

        return processedEntries.ToArray();
    }

    // 解析命令流 wip
    private CommandEntry[] ParseCommandEntry(string[] commandEntries)
    {
        List<CommandEntry> parsedEntries = new List<CommandEntry>();

        return parsedEntries.ToArray();
    }*/
}
