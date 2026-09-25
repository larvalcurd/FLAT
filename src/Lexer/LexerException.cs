namespace ExampleLib;

public class LexerException(string message, int line, int column)
    : Exception($"Lexical error at {line}:{column}: {message}")
{
    public int Line { get; } = line;

    public int Column { get; } = column;
}