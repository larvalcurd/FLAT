namespace ExampleLib;


/// <summary>
/// Класс TextHandler, отвечает за считывание и хранение текста из файла,
/// а также хранения позиции нахождения "каретки". 
/// </summary>
public class SourceScaner
{
    private readonly string _inputText;
    private int _position;
    private int _line = 1;
    private int _column = 0;
    
    public SourceScaner(string inputText)
    {
        _inputText = inputText;
        _position = 0;
    }
    
    
    /// <summary>
    /// Функция позволяет увидеть символ, который находится на расстоянии от текущего положения каретки,
    /// без смещения ее позиции. Без передачи параметра смотрит текущий символ.
    /// </summary>
    /// <param name="offset">Расстояние от текущей позиции каретки.
    ///  Если не передавать параметр, то = 0.</param>
    /// <returns>Символ, который мы в данный момент,
    /// который мы "увидели" на offset от текущей позиции каретки</returns>
    public char Peek(int offset = 0)
    {
        int pos = _position + offset;
        return pos < _inputText.Length ? _inputText[pos] : '\0';
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
        char c = _inputText[_position];
        _position++;

        switch (c)
        {
            case '\n':
                _line++;
                _column = 0;
                break;
            case '\r':
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
    public bool IsEof() => _position >= _inputText.Length;
    public int Line => _line;
    public int Column => _column;
    public int Position => _position;
}