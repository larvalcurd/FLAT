using System.Diagnostics.CodeAnalysis;

using Lexer;

namespace Parsing;

[SuppressMessage(
    "Roslynator",
    "RCS1194:Implement exception constructors",
    Justification = "Ошибка парсера всегда имеет позицию")]
public class ParserException(string message, SourcePosition position)
    : Exception($"{message} at line {position.Line}, column {position.Column}")
{
    public SourcePosition Position { get; } = position;
}