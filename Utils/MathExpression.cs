using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.LogicExpression;

namespace ChroniaHelper.Utils.MathExpression;
public static class MathExpression 
{
    /// <summary>
    /// 缓存已解析的表达式词法序列，避免重复扫描字符串。
    /// </summary>
    private static readonly Dictionary<string, List<Token>> _compileCache = new();

    /// <summary>
    /// 计算数学表达式的值（返回 float），支持变量、函数、运算符等。
    /// </summary>
    public static float ParseMathExpression(this string exp) 
    {
        return (float)ParseMathExpressionRaw(exp);
    }

    public static float ParseMathExpression(this string exp, Func<string, float> getVariable = null, Func<string, float> getFlag = null) 
    {
        Func<string, double> getVarDouble = null;
        Func<string, double> getFlagDouble = null;
        
        if (getVariable != null) 
            getVarDouble = s => getVariable(s);
        if (getFlag != null) 
            getFlagDouble = s => getFlag(s);

        return (float)ParseMathExpressionRaw(exp, getVarDouble, getFlagDouble);
    }

    /// <summary>
    /// 计算数学表达式的值（返回 double，更高精度）。
    /// </summary>
    public static double ParseMathExpressionRaw(this string exp) 
    {
        if (!exp.HasValidContent())
        {
            return 0;
        }
        
        return new Parser(Compile(exp)).Parse(null, null);
    }

    public static double ParseMathExpressionRaw(this string exp, Func<string, double> getVariable = null, Func<string, double> getFlag = null) 
    {
        if (!exp.HasValidContent())
        {
            return 0;
        }
        return new Parser(Compile(exp)).Parse(getVariable, getFlag);
    }

    /// <summary>
    /// 解析表达式并返回缓存后的词法序列。
    /// </summary>
    private static List<Token> Compile(string exp)
    {
        if (_compileCache.TryGetValue(exp, out var tokens))
        {
            return tokens;
        }
        tokens = new Lexer(exp).Tokenize();
        _compileCache[exp] = tokens;
        return tokens;
    }

    public static double GetVariable(this string variable) 
    {
        if (variable == "e") 
        {
            return Math.E; // double
        }
        if (new string[]{ "pi", "PI", "Pi" }.Contains(variable)) 
        {
            return Math.PI; // double
        }
        if (variable == "time" || variable == "Time") 
        {
            return (new DateTime(MaP.level?.Session.Time ?? 0) - new DateTime(0)).TotalMilliseconds / 1000; // double
        }
        if (new string[] { "playerX", "playerx", "player_x"}.Contains(variable))
        {
            if(PUt.TryGetPlayer(out Player p))
            {
                return p.Position.X;
            }

            return 0;
        }
        if (new string[] { "playerY", "playery", "player_y" }.Contains(variable))
        {
            if (PUt.TryGetPlayer(out Player p))
            {
                return p.Position.Y;
            }

            return 0;
        }
        if (new string[] { "playerSpeedX", "playerspeedx", "player_speed_x" }.Contains(variable))
        {
            if (PUt.TryGetPlayer(out Player p))
            {
                return p.Speed.X;
            }

            return 0;
        }
        if (new string[] { "playerSpeedY", "playerspeedy", "player_speed_y" }.Contains(variable))
        {
            if (PUt.TryGetPlayer(out Player p))
            {
                return p.Speed.Y;
            }

            return 0;
        }

        return MaP.level?.Session.GetSlider(variable) ?? 0.0;
    }
}

// 内部辅助类
public enum TokenType 
{
    Number, Variable, Function, Flag, Plus, Minus, Multiply, Divide, Power, Modulo, 
    LessThan, GreaterThan, LessEqual, GreaterEqual, EqualEqual, 
    LeftParen, RightParen, LeftBracket, RightBracket, LeftBrace, RightBrace, Comma, End,
    ExternalBlock
}

internal class Token 
{
    public TokenType Type;
    public string Value;
    public double NumberValue;
    /// <summary>
    /// 外部表达式块的额外内容（如主体表达式）。
    /// </summary>
    public string Extra;
    public Token(TokenType type, string value = null, double num = 0)
    {
        Type = type;
        Value = value;
        NumberValue = num;
    }
}

internal class Lexer 
{
    private readonly string _input;
    private int _pos = 0;
    public Lexer(string input) 
    {
        _input = input ?? "";
    }

    public List<Token> Tokenize() 
    {
        var tokens = new List<Token>();
        while (_pos < _input.Length) 
        {
            char c = _input[_pos];
            if (char.IsWhiteSpace(c)) 
            {
                _pos++;
                continue;
            }
            // {x}的flag检测
            if (c == '{') 
            {
                tokens.Add(ReadFlagContent());
                continue;
            }
            if (char.IsDigit(c) || c == '.') 
            {
                tokens.Add(ReadNumber());
            }
            else if (char.IsLetter(c) || c == '_') 
            {
                tokens.Add(ReadIdentifier());
            }
            else 
            {
                switch (c) 
                {
                    case '+': tokens.Add(new Token(TokenType.Plus)); break;
                    case '-': tokens.Add(new Token(TokenType.Minus)); break;
                    case '*': tokens.Add(new Token(TokenType.Multiply)); break;
                    case '/': tokens.Add(new Token(TokenType.Divide)); break;
                    case '^': tokens.Add(new Token(TokenType.Power)); break;
                    case '%': tokens.Add(new Token(TokenType.Modulo)); break;
                    case '(': tokens.Add(new Token(TokenType.LeftParen)); break;
                    case ')': tokens.Add(new Token(TokenType.RightParen)); break;
                    case '[': tokens.Add(new Token(TokenType.LeftBracket)); break;
                    case ']': tokens.Add(new Token(TokenType.RightBracket)); break;
                    case ',': tokens.Add(new Token(TokenType.Comma)); break;
                    case '<': 
                        _pos++;
                        if (_pos < _input.Length && _input[_pos] == '=') 
                        {
                            _pos++;
                            tokens.Add(new Token(TokenType.LessEqual));
                        }
                        else 
                        {
                            tokens.Add(new Token(TokenType.LessThan));
                        }
                        break;
                    case '>': 
                        _pos++;
                        if (_pos < _input.Length && _input[_pos] == '=') 
                        {
                            _pos++;
                            tokens.Add(new Token(TokenType.GreaterEqual));
                        }
                        else 
                        {
                            tokens.Add(new Token(TokenType.GreaterThan));
                        }
                        break;
                    case '=': 
                        _pos++;
                        if (_pos < _input.Length && _input[_pos] == '=') 
                        {
                            _pos++;
                            tokens.Add(new Token(TokenType.EqualEqual));
                        }
                        else 
                        {
                            throw new InvalidOperationException("Single '=' is not allowed. Use '==' for equality.");
                        }
                        break;
                    case '{': tokens.Add(new Token(TokenType.LeftBrace)); break;
                    case '}': tokens.Add(new Token(TokenType.RightBrace)); break;
                    default: throw new InvalidOperationException($"Unexpected character: '{c}'");
                }
                _pos++;
            }
        }
        tokens.Add(new Token(TokenType.End));
        return tokens;
    }

    private Token ReadNumber() 
    {
        var sb = new StringBuilder();
        while (_pos < _input.Length) 
        {
            char c = _input[_pos];
            if (char.IsDigit(c) || c == '.') 
            {
                sb.Append(c);
                _pos++;
            }
            else break;
        }
        if (!double.TryParse(sb.ToString(), out double num))
            throw new FormatException("Invalid number format.");
        return new Token(TokenType.Number, null, num);
    }

    /// <summary>
    /// 需要以 externalFunc[]@ 形式调用的外部函数名集合（尾部必须紧跟 '@'）。
    /// </summary>
    private static readonly HashSet<string> ExternalFunctionNames = new()
    {
        "frost", "se", "sessionexpression", "frostse"
    };

    /// <summary>
    /// 主体需要整体原样读取、交由内置引擎求值的函数名集合（不带尾部 '@'）。
    /// 这些函数的主体不是 CH 的数学表达式语法，不能在 CH 层被解析。
    /// </summary>
    private static readonly HashSet<string> RawBodyFunctionNames = new()
    {
        "logic"
    };

    private Token ReadIdentifier() 
    {
        var sb = new StringBuilder();
        while (_pos < _input.Length) 
        {
            char c = _input[_pos];
            if (char.IsLetterOrDigit(c) || c == '_') 
            {
                sb.Append(c);
                _pos++;
            }
            else break;
        }
        string name = sb.ToString();
        // 检查下一个非空白字符是否是 '[' → 判定为函数
        int tempPos = _pos;
        while (tempPos < _input.Length && char.IsWhiteSpace(_input[tempPos])) tempPos++;
        if (tempPos < _input.Length && _input[tempPos] == '[') 
        {
            string lower = name.ToLowerInvariant();
            // 外部函数：读取整个 [...] 主体，并检查末尾是否是 '@'
            if (ExternalFunctionNames.Contains(lower))
            {
                return ReadExternalFunctionBody(name, tempPos, true);
            }
            // 主体需整体原样读取的内置函数（如 logic）：不含尾部 '@'
            if (RawBodyFunctionNames.Contains(lower))
            {
                return ReadExternalFunctionBody(name, tempPos, false);
            }
            return new Token(TokenType.Function, name);
        }
        else 
        {
            return new Token(TokenType.Variable, name);
        }
    }

    /// <summary>
    /// 读取 externalFunc[...]@ 或 rawFunc[...] 形式的函数调用，主体内容原样保留。
    /// requireAt 为 true 时，右括号后必须紧跟（无空白）一个 '@' 才视为外部函数格式。
    /// </summary>
    private Token ReadExternalFunctionBody(string name, int bracketPos, bool requireAt)
    {
        _pos = bracketPos + 1; // skip '['

        var bodySb = new StringBuilder();
        while (_pos < _input.Length && _input[_pos] != ']')
        {
            bodySb.Append(_input[_pos]);
            _pos++;
        }
        if (_pos >= _input.Length)
        {
            throw new InvalidOperationException("Unclosed '[' in external function call.");
        }
        _pos++; // skip ']'

        if (requireAt)
        {
            // 检查紧跟的字符是否是 '@'（不允许空白）
            if (_pos < _input.Length && _input[_pos] == '@')
            {
                _pos++; // skip '@'
                var token = new Token(TokenType.ExternalBlock, name, 0);
                token.Extra = bodySb.ToString();
                return token;
            }

            // 没有 '@' → 不是外部函数格式，退回普通函数（内容不会被 CH 解析，最终按未知函数返回 0）
            var fallback = new Token(TokenType.Function, name);
            fallback.Extra = bodySb.ToString();
            return fallback;
        }

        var rawToken = new Token(TokenType.ExternalBlock, name, 0);
        rawToken.Extra = bodySb.ToString();
        return rawToken;
    }

    private Token ReadFlagContent() 
    {
        var sb = new StringBuilder();
        _pos++; // skip '{'
        while (_pos < _input.Length) 
        {
            char c = _input[_pos];
            if (c == '}') 
            {
                _pos++; // skip '}'
                string content = sb.ToString();
                return new Token(TokenType.Flag, content);
            }
            sb.Append(c);
            _pos++;
        }
        throw new InvalidOperationException("Unclosed '{' in flag expression.");
    }
}

internal class Parser 
{
    private readonly List<Token> _tokens;
    private int _current = 0;

    /// <summary>
    /// 缓存已编译的外部表达式（如 Frost Session Expression），避免重复编译。
    /// key 为表达式主体字符串，value 为编译后的表达式对象。
    /// </summary>
    internal static readonly Dictionary<string, object> _externalExpressionCache = new();

    private Func<string, double> _getVariableFunc = MathExpression.GetVariable;
    private Func<string, double> _getFlagFunc = (s) => s.GetFlag() ? 1.0 : 0.0;

    public Parser(List<Token> tokens) 
    {
        _tokens = tokens;
    }

    public Parser(List<Token> tokens, Func<string, double> getVariable, Func<string, double> getFlag) 
    {
        _tokens = tokens;
        if(getVariable != null) 
        {
            _getVariableFunc = getVariable;
        }
        if(getFlag != null) 
        {
            _getFlagFunc = getFlag;
        }
    }

    private Token Peek() => _tokens[_current];
    private Token Consume() => _tokens[_current++];


    public double Parse() 
    {
        double result = ParseExpression();
        if (Peek().Type != TokenType.End) throw new InvalidOperationException($"Unexpected token after expression: {Peek().Type}");
        return result;
    }

    /// <summary>
    /// 求值解析结果，支持传入变量/flag 取值委托。
    /// </summary>
    public double Parse(Func<string, double> getVariable, Func<string, double> getFlag) 
    {
        var origVar = _getVariableFunc;
        var origFlag = _getFlagFunc;
        if (getVariable != null) _getVariableFunc = getVariable;
        if (getFlag != null) _getFlagFunc = getFlag;
        try 
        {
            double result = ParseExpression();
            if (Peek().Type != TokenType.End) throw new InvalidOperationException($"Unexpected token after expression: {Peek().Type}");
            return result;
        }
        finally 
        {
            _getVariableFunc = origVar;
            _getFlagFunc = origFlag;
        }
    }

    private double ParseExpression() => ParseComparison();
    
    private double ParseComparison() 
    {
        double left = ParseAddition();
        var token = Peek().Type;
        if (token is TokenType.LessThan or TokenType.GreaterThan or TokenType.LessEqual or TokenType.GreaterEqual or TokenType.EqualEqual) 
        {
            var op = Consume();
            double right = ParseAddition();
            bool result = op.Type switch 
            {
                TokenType.LessThan => left < right,
                TokenType.GreaterThan => left > right,
                TokenType.LessEqual => left <= right,
                TokenType.GreaterEqual => left >= right,
                TokenType.EqualEqual => Math.Abs(left - right) < 1e-9,
                _ => false
            };
            return result ? 1d : 0d;
        }
        return left;
    }

    private double ParseAddition() 
    {
        double left = ParseMultiplication();
        while (Peek().Type is TokenType.Plus or TokenType.Minus) 
        {
            var op = Consume();
            double right = ParseMultiplication();
            left = op.Type == TokenType.Plus ? left + right : left - right;
        }
        return left;
    }

    private double ParseMultiplication() 
    {
        double left = ParsePower();
        while (Peek().Type is TokenType.Multiply or TokenType.Divide or TokenType.Modulo) 
        {
            var op = Consume();
            double right = ParsePower();
            switch (op.Type) 
            {
                case TokenType.Multiply: left *= right; break;
                case TokenType.Divide: 
                    if (right == 0) throw new DivideByZeroException("Division by zero.");
                    left /= right; 
                    break;
                case TokenType.Modulo: left %= right; break;
            }
        }
        return left;
    }

    private double ParsePower() 
    {
        double left = ParseFactor();
        if (Peek().Type == TokenType.Power) 
        {
            Consume();
            double right = ParsePower();
            return Math.Pow(left, right); // Math.Pow 返回 double
        }
        return left;
    }

    private double ParseFactor() 
    {
        var token = Peek();
        switch (token.Type) 
        {
            case TokenType.Number: Consume(); return token.NumberValue;
            case TokenType.Variable: Consume(); return _getVariableFunc(token.Value);
            case TokenType.Function: return ParseFunctionCall();
            case TokenType.LeftParen: 
                Consume(); 
                double expr = ParseExpression();
                if (Consume().Type != TokenType.RightParen) throw new InvalidOperationException("Expected ')'.");
                return expr;
            case TokenType.Minus: Consume(); return -ParseFactor();
            case TokenType.Plus: Consume(); return ParseFactor();
            case TokenType.Flag: Consume(); return _getFlagFunc(token.Value);
            case TokenType.ExternalBlock: Consume(); return EvaluateExternalBlock(token.Value, token.Extra);
            default: throw new InvalidOperationException($"Unexpected token in factor: {token.Type}");
        }
    }

    /// <summary>
    /// 求值一个外部/原样函数调用。
    /// 目前支持：
    /// - frost / sessionexpression / se / frostse（需 externalFunc[]@ 形式）：调用 FrostHelper 的 Session Expression。
    /// - logic（rawFunc[] 形式）：调用本项目的逻辑表达式。
    /// 名字不匹配时直接返回 0。
    /// </summary>
    private double EvaluateExternalBlock(string name, string expression)
    {
        switch (name.ToLowerInvariant())
        {
            case "frost":
            case "sessionexpression":
            case "se":
            case "frostse":
                return EvaluateFrostExpression(expression);
            case "logic":
                return expression.ParseLogicExpression() ? 1d : 0d;
            default:
                // 名字不匹配，整段作废返回 0
                return 0d;
        }
    }

    /// <summary>
    /// 求值一段 FrostHelper Session Expression 表达式，返回 float。
    /// 编译结果会被缓存以便复用。
    /// </summary>
    private double EvaluateFrostExpression(string expression)
    {
        if (!_externalExpressionCache.TryGetValue(expression, out var compiled))
        {
            // Frost 未安装或编译失败时返回 false
            if (!expression.tryCreateSessionExpression(out var exp))
            {
                return 0d;
            }
            compiled = exp;
            _externalExpressionCache[expression] = compiled;
        }

        return compiled.getFloatSessionExpressionValue();
    }

    private double ParseFunctionCall() 
    {
        var funcToken = Consume();
        string funcName = funcToken.Value;
        if (Consume().Type != TokenType.LeftBracket) throw new InvalidOperationException("Expected '[' after function name.");
        
        var args = new List<double>();
        if (Peek().Type != TokenType.RightBracket) 
        {
            args.Add(ParseExpression());
            while (Peek().Type == TokenType.Comma) 
            {
                Consume();
                args.Add(ParseExpression());
            }
        }
        if (Consume().Type != TokenType.RightBracket) throw new InvalidOperationException("Expected ']' after function arguments.");
        
        return EvaluateFunction(funcName, args);
    }

    private double EvaluateFunction(string name, List<double> args)
    {
        try 
        {
            string lower = name.ToLowerInvariant();
            switch (lower) 
            {
                // 单参数标准数学函数
                case "sin": ValidateArgCount(args, 1, "sin"); return Math.Sin(args[0]);
                case "cos": ValidateArgCount(args, 1, "cos"); return Math.Cos(args[0]);
                case "tan": ValidateArgCount(args, 1, "tan"); return Math.Tan(args[0]);
                case "ln": 
                    ValidateArgCount(args, 1, "ln"); 
                    if (args[0] <= 0f) throw new ArgumentException("ln[x] undefined for x <= 0.");
                    return Math.Log(args[0]); // Math.Log 返回 double
                case "log": // alias for ln
                    ValidateArgCount(args, 1, "log"); 
                    if (args[0] <= 0f) throw new ArgumentException("log[x] undefined for x <= 0.");
                    return Math.Log(args[0]);
                case "exp": ValidateArgCount(args, 1, "exp"); return Math.Exp(args[0]);
                case "abs": ValidateArgCount(args, 1, "abs"); return Math.Abs(args[0]);
                case "ceiling": ValidateArgCount(args, 1, "ceiling"); return Math.Ceiling(args[0]);
                case "floor": ValidateArgCount(args, 1, "floor"); return Math.Floor(args[0]);
                case "round": ValidateArgCount(args, 1, "round"); return Math.Round(args[0]);
                
                case "rand": 
                    ValidateArgCount(args, 2, "rand"); 
                    double a = args[0], b = args[1]; 

                    return Rd.RandomDouble(Math.Min(a, b), Math.Max(a, b)); 
                
                // 可变参数函数
                case "min": if (args.Count == 0) return 0d; return args.Min(); 
                case "max": if (args.Count == 0) return 0d; return args.Max(); 
                
                // clamp[value, min, max]
                case "clamp": 
                    if (args.Count == 0) return 0d;
                    if (args.Count == 1) return args[0];
                    if (args.Count == 2) return Math.Max(args[0], args[1]);
                    if (args.Count == 3) return Math.Max(args[1], Math.Min(args[0], args[2]));
                    throw new ArgumentException("Clamp[] accepts 1 to 3 arguments.");
                
                // if[arg, x, y]
                case "if":
                    if (args.Count < 1) return 0d;
                    double argIf = args[0];
                    if (args.Count >= 2) 
                    {
                        double x = args[1];
                        if (args.Count >= 3) 
                        {
                            double y = args[2];
                            return argIf != 0d ? x : y;
                        }
                        else 
                        {
                            return argIf != 0d ? x : 0d;
                        }
                    }
                    else 
                    {
                        return 0d;
                    }
                
                // ifx[arg, x, y]
                case "ifx":
                    if (args.Count < 1) return 0d;
                    double argIfx = args[0];
                    if (args.Count >= 2) 
                    {
                        double x = args[1];
                        if (args.Count >= 3) 
                        {
                            double y = args[2];
                            return argIfx >= 0d ? x : y;
                        }
                        else 
                        {
                            return argIfx >= 0d ? x : 0d;
                        }
                    }
                    else 
                    {
                        return 0d;
                    }
                
                default: throw new ArgumentException($"Unknown function: {name}");
            }
        }
        catch 
        {
            return 0d;
        }
    }

    private void ValidateArgCount(List<double> args, int expected, string funcName)
    {
        if (args.Count != expected) throw new ArgumentException($"{funcName}[] requires exactly {expected} argument(s).");
    }
}