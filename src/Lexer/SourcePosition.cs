namespace ExampleLib;

/// <summary>
/// Позиция в исходном тексте: строка, столбец.
/// </summary>
public readonly record struct SourcePosition(int Line, int Column)
{
}