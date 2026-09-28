namespace ExampleLib;

public class LexerException(string message, SourcePosition position) : Exception($"{message} at {position}")
{
    public SourcePosition Position { get; } = position;

    public int Line => Position.Line;

    public int Column => Position.Column;
}