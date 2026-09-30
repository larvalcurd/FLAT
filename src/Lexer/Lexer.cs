using System.Text;

namespace ExampleLib;

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
            return new Token(TokenType.Eof, "", _sourceScanner.CurrentPosition);
        }

        char current = _sourceScanner.Peek();
        if (char.IsAsciiLetter(current) || current == '_')
        {
            return ReadIdentifierOrKeyword();
        }

        if (char.IsAsciiDigit(current))
        {
            return ReadNumericLiteral();
        }

        if (current == '\'')
        {
            return ReadStringLiteral();
        }

        if (!SingleCharTokens.TryGetValue(current, out TokenType type))
        {
            return ReadComplexOperator(current);
        }

        SourcePosition pos = _sourceScanner.CurrentPosition;
        _sourceScanner.Advance();
        return new Token(type, current.ToString(), pos);
    }

    private Token ReadIdentifierOrKeyword()
    {
        SourcePosition startPos = _sourceScanner.CurrentPosition;
        StringBuilder sb = new();
        while (!_sourceScanner.IsEof() && (char.IsAsciiLetter(_sourceScanner.Peek()) ||
                                           char.IsAsciiDigit(_sourceScanner.Peek()) || _sourceScanner.Peek() == '_'))
        {
            sb.Append(_sourceScanner.Peek());
            _sourceScanner.Advance();
        }

        string text = sb.ToString();
        string lowerText = text.ToLowerInvariant();
        return Keywords.TryGetValue(lowerText, out TokenType type)
            ? new Token(type, lowerText, startPos)
            : new Token(TokenType.Identifier, lowerText, startPos);
    }

    /// <summary>
    /// Считывание числового литерала.
    /// </summary>
    private Token ReadNumericLiteral()
    {
        SourcePosition startPos = _sourceScanner.CurrentPosition;
        StringBuilder sb = new();
        if (_sourceScanner.Peek() == '0' && char.IsAsciiDigit(_sourceScanner.Peek(1)))
        {
            throw new LexerException(
                "Lexical error: invalid numeric literal. Identifier cannot start with a 0.", startPos);
        }

        while (!_sourceScanner.IsEof() && char.IsAsciiDigit(_sourceScanner.Peek()))
        {
            sb.Append(_sourceScanner.Peek());
            _sourceScanner.Advance();
        }

        if (_sourceScanner.IsEof())
        {
            return new Token(TokenType.IntegerLiteral, sb.ToString(), startPos);
        }

        char nextChar = _sourceScanner.Peek();
        if (char.IsAsciiLetter(nextChar) || nextChar == '_')
        {
            throw new LexerException(
                "Lexical error: invalid numeric literal. " +
                "Identifier cannot start with a digit.",
                startPos);
        }

        return new Token(TokenType.IntegerLiteral, sb.ToString(), startPos);
    }

    /// <summary>
    /// Считывание строкового литерала
    /// </summary>
    private Token ReadStringLiteral()
    {
        SourcePosition startPos = _sourceScanner.CurrentPosition;
        StringBuilder sb = new();

        _sourceScanner.Advance();

        while (!_sourceScanner.IsEof())
        {
            char c = _sourceScanner.Peek();

            switch (c)
            {
                case '\'' when _sourceScanner.Peek(1) == '\'':
                    sb.Append('\'');
                    _sourceScanner.Advance();
                    _sourceScanner.Advance();
                    continue;
                case '\'':
                    _sourceScanner.Advance();
                    return new Token(TokenType.StringLiteral, sb.ToString(), startPos);
                case '\n':
                case '\r':
                    throw new LexerException(
                        "Lexical error: unclosed string literal ", startPos);
            }

            if (IsInvalidControlChar(c))
            {
                throw new LexerException(
                    "Lexical error: invalid control character in string literal.",
                    _sourceScanner.CurrentPosition);
            }

            sb.Append(_sourceScanner.Peek());
            _sourceScanner.Advance();
        }

        throw new LexerException(
            "Lexical error: unclosed string literal.", startPos);
    }

    /// <summary>
    /// Обработка сложных(комплексных) операторов состоящих из 2 символов.
    /// </summary>
    private Token ReadComplexOperator(char c)
    {
        SourcePosition startPos = _sourceScanner.CurrentPosition;
        switch (c)
        {
            case ':':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() != '=')
                {
                    return new Token(TokenType.Colon, ":", startPos);
                }

                _sourceScanner.Advance();
                return new Token(TokenType.Assign, ":=", startPos);

            case '<':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() == '>')
                {
                    _sourceScanner.Advance();
                    return new Token(TokenType.NotEqual, "<>", startPos);
                }

                if (_sourceScanner.Peek() != '=')
                {
                    return new Token(TokenType.Less, "<", startPos);
                }

                _sourceScanner.Advance();
                return new Token(TokenType.LessOrEqual, "<=", startPos);

            case '>':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() != '=')
                {
                    return new Token(TokenType.Greater, ">", startPos);
                }

                _sourceScanner.Advance();
                return new Token(TokenType.GreaterOrEqual, ">=", startPos);

            case '.':
                _sourceScanner.Advance();
                if (_sourceScanner.Peek() != '.')
                {
                    return new Token(TokenType.Dot, ".", startPos);
                }

                _sourceScanner.Advance();
                return new Token(TokenType.DoubleDot, "..", startPos);

            default:
                throw new LexerException($"Lexical error: unexpected character '{c}'", startPos);
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
            switch (c)
            {
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                case '\f':
                    _sourceScanner.Advance();
                    continue;
                case '/' when _sourceScanner.Peek(1) == '/':
                    _sourceScanner.Advance();
                    _sourceScanner.Advance();
                    SkipOneLineComment();
                    continue;
                case '{':
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
                throw new LexerException(
                    "Lexical error: invalid control character in comment",
                    _sourceScanner.CurrentPosition);
            }

            _sourceScanner.Advance();
        }
    }

    /// <summary>
    /// Пропуск многострочных комментариев
    /// </summary>
    private void SkipMultilineComment()
    {
        SourcePosition startPos = _sourceScanner.CurrentPosition;
        bool closed = false;
        while (!_sourceScanner.IsEof())
        {
            char commentChar = _sourceScanner.Peek();
            _sourceScanner.Advance();

            if (commentChar == '}')
            {
                closed = true;
                break;
            }

            if (IsInvalidControlChar(commentChar))
            {
                throw new LexerException(
                    "Lexical error: invalid control character in comment. ",
                    startPos);
            }
        }

        if (!closed)
        {
            throw new LexerException(
                "Lexical error: unclosed block comment starting. ",
                startPos);
        }
    }

    /// <summary>
    /// Проверяет, является ли символ запрещенным управляющим символом внутри комментария или строки.
    /// Разрешены: печатные ASCII (0x20–0x7E), HT (0x09), LF (0x0A), CR (0x0D), символы от 0x80 и выше.
    /// Запрещены: 0x00–0x08, 0x0B, 0x0C, 0x0E–0x1F, 0x7F.
    /// </summary>
    private static bool IsInvalidControlChar(char c)
    {
        if ((c >= 0x20 && c < 0x7F) || c >= 0x80)
        {
            return false;
        }

        if (c is '\t' or '\n' or '\r')
        {
            return false;
        }

        return true;
    }
}