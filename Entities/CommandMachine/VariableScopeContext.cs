using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities.CommandMachine;

// 变量作用域上下文 用于存储变量与变量作用域管理 wip
public class VariableScopeContext
{
    // 外层作用域
    private readonly VariableScopeContext parent;
    private readonly Dictionary<string, object> variables = new Dictionary<string, object>();

    public VariableScopeContext(VariableScopeContext parent = null)
    {
        this.parent = parent;
    }

    // 索引器 用于访问变量
    public object this[string key]
    {
        get => variables.TryGetValue(key, out var val) ? val : parent?[key];
        set => variables[key] = value;
    }
}
