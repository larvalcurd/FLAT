using System.Text;

namespace ExampleLib;

// TODO: Избавиться от предупреждений в пределах проекта, по имени Лексер
public class Lexer(string source)
{
    private readonly SourceScanner _sourceScanner = new(source);

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "program", TokenType.Program },
        { "var", TokenType.Var },
        { "procedure", TokenType.Procedure },
        { "function", TokenType.Function },
        { "integer", TokenType.Integer },
        { "boolean", TokenType.Boolean },
        { "string", TokenType.String },
        { "record", TokenType.Record },
        { "array", TokenType.Array },
        { "of", TokenType.Of },
        { "begin", TokenType.Begin },
        { "end", TokenType.End },
        { "if", TokenType.If },
        { "then", TokenType.Then },
        { "else", TokenType.Else },
        { "while", TokenType.While },
        { "do", TokenType.Do },
        { "write", TokenType.Write },
        { "read", TokenType.Read },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "and", TokenType.And },
        { "or", TokenType.Or },
        { "not", TokenType.Not },
        { "div", TokenType.Div },
        { "mod", TokenType.Mod },
    };

    private static readonly Dictionary<char, TokenType> SingleCharTokens = new()
    {
        { '+', TokenType.Plus },
        { '-', TokenType.Minus },
        { '*', TokenType.Multiply },
        { '=', TokenType.Equal },
        { '(', TokenType.OpenParen },
        { ')', TokenType.CloseParen },
        { '[', TokenType.OpenBracket },
        { ']', TokenType.CloseBracket },
        { ',', TokenType.Comma },
        { ';', TokenType.Semicolon },
    };

    public Token NextToken()
    {
        SkipWhitespaceAndComments();
        if (_sourceScanner.IsEof())
        {
            return new Token(TokenType.Unknown, "", _sourceScanner.Line, _sourceScanner.Column);
        }

        char current = _sourceScanner.Peek();
        if (char.IsLetter(current) || current == '_')
        {
            return ReadIdentifierOrKeyword();
        }

        if (char.IsDigit(current))
        {
            return ReadNumericLiteral();
        }

        if (current == '\'')
        {
            return ReadStringLiteral();
        }

        if (SingleCharTokens.TryGetValue(current, out TokenType type) && !SingleCharTokens.TryGetValue(_sourceScanner.Peek(1), out TokenType typeNext))
        {
            _sourceScanner.Advance();
            return new Token(type, current.ToString(), _sourceScanner.Line, _sourceScanner.Column);
        }

        return ReadComplexOperator(current);
    }

    private Token ReadIdentifierOrKeyword()
    {
        int startLine = _sourceScanner.Line;
        int startCol = _sourceScanner.Column;
        StringBuilder sb = new();
        while (!_sourceScanner.IsEof() && (char.IsLetter(_sourceScanner.Peek()) ||
                                           char.IsDigit(_sourceScanner.Peek()) || _sourceScanner.Peek() == '_'))
        {
            sb.Append(_sourceScanner.Advance());
        }

        string text = sb.ToString();
        string lowerText = text.ToLowerInvariant();
        if (Keywords.TryGetValue(lowerText, out TokenType type))
        {
            return new Token(type, text, startLine, startCol);
        }

        return new Token(TokenType.Identifier, text, startLine, startCol);
    }

    /// <summary>
    /// Считывание числового литерала.
    /// </summary>
    private Token ReadNumericLiteral()
    {
        int startLine = _sourceScanner.Line;
        int startCol = _sourceScanner.Column;
        StringBuilder sb = new();
        if (_sourceScanner.Peek() == '0' && char.IsDigit(_sourceScanner.Peek(1)))
        {
            throw new Exception(
                $"Lexical error: invalid numeric literal at {startLine}:{startCol}. " +
                $"Identifier cannot start with a 0.");
        }

        while (!_sourceScanner.IsEof() && char.IsDigit(_sourceScanner.Peek()))
        {
            sb.Append(_sourceScanner.Advance());
        }

        if (!_sourceScanner.IsEof())
        {
            char nextChar = _sourceScanner.Peek();
            if (char.IsLetter(nextChar) || nextChar == '_')
            {
                throw new Exception(
                    $"Lexical error: invalid numeric literal at {startLine}:{startCol}. " +
                    $"Identifier cannot start with a digit.");
            }
        }

        return new Token(TokenType.IntegerLiteral, sb.ToString(), startLine, startCol);
    }

    /// <summary>
    /// Считывание строкового литерала
    /// </summary>
    private Token ReadStringLiteral()
    {
        int startLine = _sourceScanner.Line;
        int startCol = _sourceScanner.Column;
        StringBuilder sb = new();

        _sourceScanner.Advance();

        while (!_sourceScanner.IsEof())
        {
            char c = _sourceScanner.Peek();

            if (c == '\'')
            {
                if (_sourceScanner.Peek(1) == '\'')
                {
                    sb.Append('\'');
                    _sourceScanner.Advance();
                    _sourceScanner.Advance();
                    continue;
                }

                _sourceScanner.Advance();
                return new Token(TokenType.StringLiteral, sb.ToString(), startLine, startCol);
            }

            if (c == '\n' || c == '\r')
            {
                throw new Exception(
                    $"Lexical error: unclosed string literal starting at {startLine}:{startCol}");
            }

            if (IsInvalidControlChar(c))
            {
                throw new Exception(
                    $"Lexical error: invalid control character in string literal at {_sourceScanner.Line}:{_sourceScanner.Column}");
            }

            sb.Append(_sourceScanner.Advance());
        }

        throw new Exception(
            $"Lexical error: unclosed string literal starting at {startLine}:{startCol}");
    }

    /// <summary>
    /// Обработка сложных(комплексных) операторов состоящих из 2 символов.
    /// </summary>
    private Token ReadComplexOperator(char c)
    {
        int line = _sourceScanner.Line;
        int col = _sourceScanner.Column;
        switch (c)
        {
            case ':':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() == '=')
                {
                    _sourceScanner.Advance();
                    return new Token(TokenType.Assign, ":=", line, col);
                }

                return new Token(TokenType.Colon, ":", line, col);
            case '<':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() == '>')
                {
                    _sourceScanner.Advance();
                    return new Token(TokenType.NotEqual, "<>", line, col);
                }

                if (_sourceScanner.Peek() == '=')
                {
                    _sourceScanner.Advance();
                    return new Token(TokenType.LessOrEqual, "<=", line, col);
                }

                return new Token(TokenType.Less, "<", line, col);
            case '>':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() == '=')
                {
                    _sourceScanner.Advance();
                    return new Token(TokenType.GreaterOrEqual, ">=", line, col);
                }

                return new Token(TokenType.Greater, ">", line, col);
            case '.':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() == '.')
                {
                    _sourceScanner.Advance();
                    return new Token(TokenType.DoubleDot, "..", line, col);
                }

                return new Token(TokenType.Dot, ".", line, col);
            default:
                throw new Exception($"Lexical error: unexpected character '{c}' at {line}:{col}");
        }
    }

    /// <summary>
    /// Пропуск комментариев, а также пробельных символов
    /// </summary>
    private void SkipWhitespaceAndComments()
    {
        while (!_sourceScanner.IsEof())
        {
            char c = _sourceScanner.Peek();
            if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f')
            {
                _sourceScanner.Advance();
                continue;
            }

            if (c == '/' && _sourceScanner.Peek(1) == '/')
            {
                _sourceScanner.Advance();
                _sourceScanner.Advance();
                SkipOneLineComment();
                continue;
            }

            if (c == '{')
            {
                _sourceScanner.Advance();
                SkipMultilineComment();
                continue;
            }

            break;
        }
    }

    /// <summary>
    /// Пропуск однострочных комментариев
    /// </summary>
    private void SkipOneLineComment()
    {
        while (!_sourceScanner.IsEof())
        {
            char commentChar = _sourceScanner.Peek();
            if (commentChar == '\n' || commentChar == '\r')
            {
                break;
            }

            if (IsInvalidControlChar(commentChar))
            {
                throw new Exception(
                    $"Lexical error: invalid control character in comment at {_sourceScanner.Line}:{_sourceScanner.Column}");
            }

            _sourceScanner.Advance();
        }
    }

    /// <summary>
    /// Пропуск многострочных комментариев
    /// </summary>
    private void SkipMultilineComment()
    {
        bool closed = false;
        while (!_sourceScanner.IsEof())
        {
            char commentChar = _sourceScanner.Advance();

            if (commentChar == '}')
            {
                closed = true;
                break;
            }

            if (IsInvalidControlChar(commentChar))
            {
                throw new Exception($"Lexical error: invalid control character in comment at {_sourceScanner.Line}:{_sourceScanner.Column}");
            }
        }

        if (!closed)
        {
            throw new Exception($"Lexical error: unclosed block comment starting at {_sourceScanner.Line}:{_sourceScanner.Column}");
        }
    }

    /// <summary>
    /// Проверяет, является ли символ запрещенным управляющим символом внутри комментария или строки.
    /// Разрешены: HT (0x09), LF (0x0A), CR (0x0D).
    /// Запрещены: 0x00–0x08, 0x0B, 0x0C, 0x0E–0x1F, 0x7F.
    /// </summary>
    private static bool IsInvalidControlChar(char c)
    {
        if (c >= 0x20)
        {
            return false;
        }

        if (c == '\t' || c == '\n' || c == '\r')
        {
            return false;
        }

        return true;
    }
}