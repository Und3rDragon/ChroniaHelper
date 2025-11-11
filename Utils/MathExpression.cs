using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ChroniaHelper.Utils;

public static class MathExpression
{
    /// <summary>
    /// 计算数学表达式的值，支持变量、函数（格式：func[arg]）、运算符等。
    /// </summary>
    public static float GetMathExpression(this string exp)
    {
        if (string.IsNullOrWhiteSpace(exp) || exp.IsNullOrEmpty())
            throw new ArgumentException("Expression is null or empty.");

        var lexer = new Lexer(exp);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        return parser.Parse();
    }
    
    public static float GetNumber(this string variable)
    {
        if (variable == "e") { return (float)Math.E; }
        if (new string[]{ "pi", "PI", "Pi" }.Contains(variable)) { return (float)Math.PI; }
        
        return 0f; // In progress
    }
}

// 内部辅助类

public enum TokenType
{
    Number,
    Variable,
    Function,       // 函数名（后跟 [）
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
    Modulo,
    LeftParen,      // (
    RightParen,     // )
    LeftBracket,    // [
    RightBracket,   // ]
    Comma,          // ,
    End
}

public class Token
{
    public TokenType Type;
    public string Value;
    public float NumberValue;

    public Token(TokenType type, string value = null, float num = 0)
    {
        Type = type;
        Value = value;
        NumberValue = num;
    }
}

public class Lexer
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
                    default:
                        throw new InvalidOperationException($"Unexpected character: '{c}'");
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
            else
                break;
        }
        if (!float.TryParse(sb.ToString(), out float num))
            throw new FormatException("Invalid number format.");
        return new Token(TokenType.Number, null, num);
    }

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
            else
                break;
        }

        string name = sb.ToString();

        // 检查下一个非空白字符是否是 '[' → 判定为函数
        int tempPos = _pos;
        while (tempPos < _input.Length && char.IsWhiteSpace(_input[tempPos]))
            tempPos++;

        if (tempPos < _input.Length && _input[tempPos] == '[')
        {
            return new Token(TokenType.Function, name);
        }
        else
        {
            return new Token(TokenType.Variable, name);
        }
    }
}

public class Parser
{
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    private Token Peek() => _tokens[_current];
    private Token Consume() => _tokens[_current++];

    public float Parse()
    {
        float result = ParseExpression();
        if (Peek().Type != TokenType.End)
            throw new InvalidOperationException($"Unexpected token after expression: {Peek().Type}");
        return result;
    }

    private float ParseExpression() => ParseAddition();

    private float ParseAddition()
    {
        float left = ParseMultiplication();
        while (Peek().Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Consume();
            float right = ParseMultiplication();
            left = op.Type == TokenType.Plus ? left + right : left - right;
        }
        return left;
    }

    private float ParseMultiplication()
    {
        float left = ParsePower();
        while (Peek().Type is TokenType.Multiply or TokenType.Divide or TokenType.Modulo)
        {
            var op = Consume();
            float right = ParsePower();
            switch (op.Type)
            {
                case TokenType.Multiply:
                    left *= right;
                    break;
                case TokenType.Divide:
                    if (right == 0) throw new DivideByZeroException("Division by zero.");
                    left /= right;
                    break;
                case TokenType.Modulo:
                    left %= right;
                    break;
            }
        }
        return left;
    }

    private float ParsePower()
    {
        float left = ParseFactor();
        if (Peek().Type == TokenType.Power)
        {
            Consume(); // consume '^'
            float right = ParsePower(); // right-associative
            return (float)Math.Pow(left, right);
        }
        return left;
    }

    private float ParseFactor()
    {
        var token = Peek();
        switch (token.Type)
        {
            case TokenType.Number:
                Consume();
                return token.NumberValue;

            case TokenType.Variable:
                Consume();
                return token.Value.GetNumber(); // 调用变量获取方法

            case TokenType.Function:
                return ParseFunctionCall();

            case TokenType.LeftParen:
                Consume();
                float expr = ParseExpression();
                if (Consume().Type != TokenType.RightParen)
                    throw new InvalidOperationException("Expected ')'.");
                return expr;

            case TokenType.Minus:
                Consume();
                return -ParseFactor();

            case TokenType.Plus:
                Consume();
                return ParseFactor();

            default:
                throw new InvalidOperationException($"Unexpected token in factor: {token.Type}");
        }
    }

    private float ParseFunctionCall()
    {
        var funcToken = Consume(); // Function
        string funcName = funcToken.Value;

        if (Consume().Type != TokenType.LeftBracket)
            throw new InvalidOperationException("Expected '[' after function name.");

        var args = new List<float>();
        if (Peek().Type != TokenType.RightBracket)
        {
            args.Add(ParseExpression());
            while (Peek().Type == TokenType.Comma)
            {
                Consume(); // skip comma
                args.Add(ParseExpression());
            }
        }

        if (Consume().Type != TokenType.RightBracket)
            throw new InvalidOperationException("Expected ']' after function arguments.");

        return EvaluateFunction(funcName, args);
    }

    private float EvaluateFunction(string name, List<float> args)
    {
        string lower = name.ToLowerInvariant();
        switch (lower)
        {
            case "sin":
                ValidateArgCount(args, 1, "sin");
                return (float)Math.Sin(args[0]);

            case "cos":
                ValidateArgCount(args, 1, "cos");
                return (float)Math.Cos(args[0]);

            case "tan":
                ValidateArgCount(args, 1, "tan");
                return (float)Math.Tan(args[0]);

            case "ln":
                ValidateArgCount(args, 1, "ln");
                if (args[0] <= 0) throw new ArgumentException("ln[x] undefined for x <= 0.");
                return (float)Math.Log(args[0]);

            case "exp":
                ValidateArgCount(args, 1, "exp");
                return (float)Math.Exp(args[0]);

            case "rand":
                ValidateArgCount(args, 2, "rand");
                float a = args[0], b = args[1];
                float min = Math.Min(a, b);
                float max = Math.Max(a, b);
                return RandomUtils.RandomFloat(min, max); // 调用随机数生成

            default:
                throw new ArgumentException($"Unknown function: {name}");
        }
    }

    private static void ValidateArgCount(List<float> args, int expected, string funcName)
    {
        if (args.Count != expected)
            throw new ArgumentException($"{funcName}[...] requires exactly {expected} argument(s), got {args.Count}.");
    }
}