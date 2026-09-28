namespace ExampleLib;

public class Token(TokenType type, string value, SourcePosition position)
{
    public TokenType Type { get; } = type;

    public string Value { get; } = value;

    public SourcePosition Position { get; } = position;

    public int Line => Position.Line;

    public int Column => Position.Column;
}