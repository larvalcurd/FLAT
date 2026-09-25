namespace ExampleLib;

public class LexerException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public LexerException(string message, int line, int column)
        : base($"Lexical error at {line}:{column}: {message}")
    {
        Line = line;
        Column = column;
    }
}