using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NCalc;

public class MathExpression
{
    private readonly string originalExpression;
    private readonly string cleanedExpression;
    private readonly HashSet<string> variables = new HashSet<string>();

    public MathExpression(string expression)
    {
        originalExpression = expression;

        // 提取所有 #xxx 格式的变量名
        var matches = Regex.Matches(expression, @"#([a-zA-Z_]\w*)");
        foreach (Match match in matches)
        {
            string varName = match.Groups[1].Value;
            variables.Add(varName);
        }

        // 替换 #xxx => xxx（让 NCalc 可以识别）
        cleanedExpression = Regex.Replace(expression, @"#([a-zA-Z_]\w*)", "$1");

        // 替换 ^ 为 POW 函数（NCalc 不支持 ^ 运算符）
        cleanedExpression = cleanedExpression.Replace("^", "POW");
    }

    /// <summary>
    /// 解析并计算表达式结果
    /// </summary>
    /// <param name="getValue">获取变量值的方法</param>
    /// <returns>表达式计算结果</returns>
    public object Evaluate(Func<string, object> getValue)
    {
        var expr = new Expression(cleanedExpression);

        // 注册变量
        foreach (var variable in variables)
        {
            expr.Parameters[variable] = GetValueClosure(getValue, variable);
        }

        // 注册 POW 函数用于替代 ^
        expr.EvaluateFunction += delegate (string name, FunctionArgs args)
        {
            if (name == "POW" && args != null && args.Parameters.Length == 2)
            {
                double baseVal = Convert.ToDouble(args.Parameters[0].Evaluate());
                double exponent = Convert.ToDouble(args.Parameters[1].Evaluate());
                args.Result = Math.Pow(baseVal, exponent);
            }
        };

        return expr.Evaluate();
    }

    // 封装 GetValue 到委托中
    private Func<object> GetValueClosure(Func<string, object> getValue, string variable)
    {
        return () => getValue(variable);
    }

    /// <summary>
    /// 获取表达式中涉及的所有变量名
    /// </summary>
    public IEnumerable<string> GetVariables() => variables;

    /*
     string exprStr = "#x3 + 35 - (#y) ^ 2";
        var evaluator = new MathExpression(exprStr);

        // 模拟 GetValue 方法
        Func<string, object> GetValue = (varName) =>
        {
            switch (varName)
            {
                case "x3": return 10.0;
                case "y": return 3.0;
                default: throw new KeyNotFoundException($"变量 {varName} 未定义。");
            }
        };

        try
        {
            var result = evaluator.Evaluate(GetValue);
            Console.WriteLine($"表达式结果：{result}"); // 输出 10 + 35 - 9 = 36
        }
        catch (Exception ex)
        {
            Console.WriteLine("计算出错：" + ex.Message);
        }
     */
}