using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/FlagOperationTrigger")]
[Tracked(true)]

public class FlagOperationTrigger : FlagManageTrigger
{
    public FlagOperationTrigger(EntityData data, Vector2 offset):base(data, offset)
    {
        flag = data.Attr("flag", "flag");
        dataName = data.Attr("dataName", "unknown");
        dataValue = data.Attr("dataValue", "0");
        numExp = data.Attr("numeralExpression", "x");

        op = (Operations)data.Int("operation", 0);
    }
    private string flag, dataName, dataValue, numExp;
    private enum Operations
    {
        Add, Remove, CalcExpression, IfContains, IfNotContains
    }
    private Operations op;

    protected override void OnEnterExecute(Player player)
    {
        base.OnEnterExecute(player);

        ChroniaFlag f = flag.PullFlag();

        if(op == Operations.Add)
        {
            f.CustomData.Enter(dataName, dataValue);
        }
        else if(op == Operations.Remove)
        {
            f.CustomData.SafeRemove(dataName);
        }
        else if(op == Operations.IfContains)
        {
            f.Active = f.CustomData.ContainsKey(dataName);
        }
        else if(op == Operations.IfNotContains)
        {
            f.Active = !f.CustomData.ContainsKey(dataName);
        }
        else if(op == Operations.CalcExpression)
        {
            var expression = new MathExpression(numExp);

            try
            {
                // 将 GetValue 方法作为委托传入 Evaluate
                object result = expression.Evaluate(GetVariable);

                Log.Info("表达式结果：" + result); // 应该输出 36
            }
            catch (Exception ex)
            {
                Log.Error("错误：" + ex.Message);
            }
        }

        f.PushFlag();
    }

    private object GetVariable(string name)
    {
        float m = 0f;
        ChroniaFlag f = flag.PullFlag();
        if (f.CustomData.ContainsKey(name))
        {
            float.TryParse(f.CustomData[name], out m);
        }

        return m;
    }
}
