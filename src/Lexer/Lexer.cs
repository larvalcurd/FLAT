namespace ExampleLib;

public class Lexer
{
    private readonly SourceScaner _sourceScaner;
    
    public Lexer(string source)
    {
        _sourceScaner = new SourceScaner(source);
    }

    public Token NextToken()
    {
        SkipWhitespaceAndComments();
        return new Token();
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