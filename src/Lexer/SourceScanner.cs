namespace ExampleLib;

/// <summary>
/// Класс TextHandler, отвечает за считывание и хранение текста из файла,
/// а также хранения позиции нахождения "каретки".
/// </summary>
public class SourceScanner(string inputText)
{
    private int _line = 1;
    private int _column;

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
        int pos = Position + offset;
        return pos < inputText.Length ? inputText[pos] : '\0';
    }

    /// <summary>
    /// Сдвигает позицию чтения вперед на один символ и возвращает его.
    /// Обновляет счетчики строк и столбцов.
    /// </summary>
    public char Advance()
    {
        if (IsEof())
        {
            return '\0';
        }

        char c = inputText[Position];
        Position++;

        switch (c)
        {
            case '\n':
                _line++;
                _column = 0;
                break;
            case '\r':
                if (Peek() != '\n')
                {
                    _line++;
                    _column = 0;
                }

                break;
            default:
                _column++;
                break;
        }

        return c;
    }

    /// <summary>
    /// Проверяет, достигнут ли конец файла относительно переданной позиции.
    /// </summary>
    /// <returns>Возвращает флаг, указывающий на факт возвращения или нет</returns>
    public bool IsEof() => Position >= inputText.Length;

    public int Position { get; private set; } = 0;

    public int Line => _line;

    public int Column => _column;
}