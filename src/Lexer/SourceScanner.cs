namespace Lexer;

/// <summary>
/// Класс SourceScanner, отвечает за считывание и хранение текста из файла,
/// а также хранения позиции нахождения "каретки".
/// </summary>
public class SourceScanner(string inputText)
{
    public int Position { get; private set; }

    public int Line { get; private set; } = 1;

    public int Column { get; private set; }

    public SourcePosition CurrentPosition => new(Line, Column);

    /// <summary>
    /// Функция позволяет увидеть символ, который находится на расстоянии от текущего положения каретки,
    /// без смещения ее позиции. Без передачи параметра смотрит текущий символ.
    /// </summary>
    /// <param name="offset">Расстояние от текущей позиции каретки.
    ///  Если не передавать параметр, то = 0.</param>
    /// <returns>Символ, который мы в данный момент считываем,
    /// на offset от текущей позиции каретки</returns>
    public char Peek(int offset = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        int pos = Position + offset;
        return pos < inputText.Length ? inputText[pos] : '\0';
    }

    /// <summary>
    /// Сдвигает позицию чтения вперед на один символ и возвращает его.
    /// Обновляет счетчики строк и столбцов.
    /// </summary>
    public void Advance()
    {
        if (IsEof())
        {
            return;
        }

        char c = inputText[Position++];

        switch (c)
        {
            case '\n':
                Line++;
                Column = 0;
                break;
            case '\r':
                if (Peek() != '\n')
                {
                    Line++;
                    Column = 0;
                }

                break;
            default:
                Column++;
                break;
        }
    }

    /// <summary>
    /// Проверяет, достигнут ли конец файла относительно переданной позиции.
    /// </summary>
    /// <returns>Возвращает флаг, указывающий на факт возвращения или нет</returns>
    public bool IsEof() => Position >= inputText.Length;
}