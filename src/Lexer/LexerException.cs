using System.Diagnostics.CodeAnalysis;

namespace ExampleLib;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors", Justification = "Лексическая ошибка всегда имеет позицию в исходном тексте.")]
public class LexerException(string message, SourcePosition position) : Exception($"{message} at {position}")
{
    public SourcePosition Position { get; } = position;

    public int Line => Position.Line;

    public int Column => Position.Column;
}