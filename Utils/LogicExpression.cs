using System;
using System.Collections.Generic;
using System.Text;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Utils.LogicExpression;

/// <summary>
/// 用于解析和计算由变量、逻辑与（&&）、逻辑或（||）及括号组成的布尔表达式。
/// 表达式示例: "flagA || (flagB && flagC)"
/// </summary>
public static class LogicExpression
{
    /// <summary>
    /// 解析并计算给定的逻辑表达式。
    /// </summary>
    /// <param name="expression">要计算的逻辑表达式字符串。</param>
    /// <param name="getVariableValue">一个函数，用于根据变量名获取其布尔值。如果变量未定义，默认返回 false。</param>
    /// <returns>表达式的布尔计算结果。</returns>
    public static bool ParseLogicExpression(this string expression, Func<string, bool> getVariableValue = null)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("表达式不能为空。", nameof(expression));

        var lexer = new Lexer(expression);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, getVariableValue);
        return parser.Parse();
    }
}

// =============== 内部实现 ===============

internal enum TokenType
{
    Identifier,
    And, // &&
    Or,  // ||
    LeftParen,
    RightParen,
    End
}

internal class Token
{
    public TokenType Type;
    public string Value; // 仅用于 Identifier

    public Token(TokenType type, string value = null)
    {
        Type = type;
        Value = value;
    }
}

internal class Lexer
{
    private readonly string _input;
    private int _position;

    public Lexer(string input)
    {
        _input = input ?? string.Empty;
        _position = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_position < _input.Length)
        {
            char c = _input[_position];
            if (char.IsWhiteSpace(c))
            {
                _position++;
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                tokens.Add(ReadIdentifier());
            }
            else
            {
                switch (c)
                {
                    case '(':
                        tokens.Add(new Token(TokenType.LeftParen));
                        _position++;
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.RightParen));
                        _position++;
                        break;
                    case '&':
                        _position++;
                        if (_position < _input.Length && _input[_position] == '&')
                        {
                            _position++;
                            tokens.Add(new Token(TokenType.And));
                        }
                        else
                        {
                            throw new FormatException("期望 '&&'。");
                        }
                        break;
                    case '|':
                        _position++;
                        if (_position < _input.Length && _input[_position] == '|')
                        {
                            _position++;
                            tokens.Add(new Token(TokenType.Or));
                        }
                        else
                        {
                            throw new FormatException("期望 '||'。");
                        }
                        break;
                    default:
                        throw new FormatException($"意外字符: '{c}'。");
                }
            }
        }
        tokens.Add(new Token(TokenType.End));
        return tokens;
    }

    private Token ReadIdentifier()
    {
        var start = _position;
        while (_position < _input.Length)
        {
            char c = _input[_position];
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                _position++;
            }
            else
            {
                break;
            }
        }
        string text = _input.Substring(start, _position - start);
        return new Token(TokenType.Identifier, text);
    }
}

internal class Parser
{
    private readonly List<Token> _tokens;
    private int _currentIndex;
    private readonly Func<string, bool> _getVariableValue = (s) => s.GetFlag();

    public Parser(List<Token> tokens, Func<string, bool> getVariableValue)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _getVariableValue = getVariableValue ?? (name => name.GetFlag());
        _currentIndex = 0;
    }

    private Token Current => _tokens[_currentIndex];
    private Token Consume() => _tokens[_currentIndex++];

    public bool Parse()
    {
        bool result = ParseOr();
        if (Current.Type != TokenType.End)
            throw new FormatException($"表达式后存在意外内容: {Current.Type}。");
        return result;
    }

    // OR 表达式 (优先级最低)
    private bool ParseOr()
    {
        bool left = ParseAnd();
        while (Current.Type == TokenType.Or)
        {
            Consume(); // 消费 '||'
            bool right = ParseAnd();
            left = left || right;
        }
        return left;
    }

    // AND 表达式 (优先级较高)
    private bool ParseAnd()
    {
        bool left = ParseFactor();
        while (Current.Type == TokenType.And)
        {
            Consume(); // 消费 '&&'
            bool right = ParseFactor();
            left = left && right;
        }
        return left;
    }

    // 因子 (优先级最高: 变量 或 括号表达式)
    private bool ParseFactor()
    {
        Token token = Current;
        switch (token.Type)
        {
            case TokenType.Identifier:
                Consume();
                return _getVariableValue(token.Value);

            case TokenType.LeftParen:
                Consume(); // 消费 '('
                bool exprValue = ParseOr();
                if (Consume().Type != TokenType.RightParen)
                    throw new FormatException("期望 ')'。");
                return exprValue;

            default:
                throw new FormatException($"在因子位置遇到意外符号: {token.Type}。");
        }
    }
}