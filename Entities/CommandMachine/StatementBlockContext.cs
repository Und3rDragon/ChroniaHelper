using ChroniaHelper.Entities;
using ChroniaHelper.Entities.CommandMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Entities.CommandMachine.DataStructure;
using ChroniaHelper.Components;

// 代码块 用于流程控制 wip 不确定怎么实现(草草
// 用(do/if/for/while)...(end)标识代码块 和 lua 语法类似
public class StatementBlockContext
{
    public StatementBlockType Type;
    public CommandPrefix Prefix;
    public string Expression;

    public CommandEntry[] CommandEntries { get; }
    public VariableScopeContext Variables { get; }
    public List<InstantCoroutine> ActiveAsyncCoroutines { get; private set; }
}