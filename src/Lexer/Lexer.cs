using System.Text;

namespace ExampleLib;

public class Lexer
{
    private readonly SourceScaner _sourceScaner;
    
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
        { "mod", TokenType.Mod }
    };
    
    public Lexer(string source)
    {
        _sourceScaner = new SourceScaner(source);
    }

    public Token NextToken()
    {
        SkipWhitespaceAndComments();
        if (_sourceScaner.IsEof())
        {
            return new Token(TokenType.Unknown, "", _sourceScaner.Line, _sourceScaner.Column);
        }
        char current = _sourceScaner.Peek();
        if (char.IsLetter(current) || current == '_')
        {
            return ReadIdentifierOrKeyword();
        }
        if (char.IsDigit(current))
        {
            return ReadNumericLiteral();
        }
        throw new Exception($"unexpected character {current}");
    }
    
    private Token ReadIdentifierOrKeyword()
    {
        int startLine = _sourceScaner.Line;
        int startCol = _sourceScaner.Column;
        StringBuilder sb = new();
        while (!_sourceScaner.IsEof() && (char.IsLetter(_sourceScaner.Peek()) || char.IsDigit(_sourceScaner.Peek()) || _sourceScaner.Peek() == '_'))
        {
            sb.Append(_sourceScaner.Advance());
        }

        string text = sb.ToString();
        string lowerText = text.ToLowerInvariant();
        if (Keywords.TryGetValue(lowerText, out TokenType type))
        {
            return new Token(type, text, startLine, startCol);
        }
        return new Token(TokenType.Identifier, text, startLine, startCol);
    }

    private Token ReadNumericLiteral()
    {
        int startLine = _sourceScaner.Line;
        int startCol = _sourceScaner.Column;
        StringBuilder sb = new();
        while (!_sourceScaner.IsEof() && char.IsDigit(_sourceScaner.Peek()))
        {
            sb.Append(_sourceScaner.Advance());
        }

        if (!_sourceScaner.IsEof())
        {
            char nextChar = _sourceScaner.Peek();
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
    /// Пропуск комментариев, а также пробельных символов
    /// </summary>
    private void SkipWhitespaceAndComments()
    {
    while (!_sourceScaner.IsEof())
    {
        char c = _sourceScaner.Peek();
        if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f')
        {
            _sourceScaner.Advance();
            continue;
        }
        if (c == '/' && _sourceScaner.Peek(1) == '/')
        {
            _sourceScaner.Advance(); 
            _sourceScaner.Advance();
            SkipOnelineComment();
            continue;
        }
        if (c == '{')
        {
            _sourceScaner.Advance();
            SkipMultilineComment();            
            continue;
        }
        
        break;
    }
}
    /// <summary>
    /// Пропуск однострочных комментариев
    /// </summary>
    private void SkipOnelineComment()
    {
        while (!_sourceScaner.IsEof())
        {
            char commentChar = _sourceScaner.Peek();
            if (commentChar == '\n' || commentChar == '\r')
                break;
            if (IsInvalidControlChar(commentChar))
                throw new Exception($"Lexical error: invalid control character in comment at {_sourceScaner.Line}:{_sourceScaner.Column}");
            _sourceScaner.Advance();
        }
    }    
    
    /// <summary>
    /// Пропуск многострочных комментариев
    /// </summary>
    private void SkipMultilineComment()
    {
        bool closed = false;
        while (!_sourceScaner.IsEof())
        {
            char commentChar = _sourceScaner.Advance();
                
            if (commentChar == '}')
            {
                closed = true;
                break;
            }
                
            if (IsInvalidControlChar(commentChar))
                throw new Exception($"Lexical error: invalid control character in comment at {_sourceScaner.Line}:{_sourceScaner.Column}");
        }

        if (!closed)
            throw new Exception($"Lexical error: unclosed block comment starting at {_sourceScaner.Line}:{_sourceScaner.Column}");


    }    
    
    
    
    /// <summary>
    /// Проверяет, является ли символ запрещенным управляющим символом внутри комментария или строки.
    /// Разрешены: HT (0x09), LF (0x0A), CR (0x0D). 
    /// Запрещены: 0x00–0x08, 0x0B, 0x0C, 0x0E–0x1F, 0x7F.
    /// </summary>
    private static bool IsInvalidControlChar(char c)
    {
        if (c >= 0x20) return false;
        if (c == '\t' || c == '\n' || c == '\r') return false;
        
        return true;
    }
}