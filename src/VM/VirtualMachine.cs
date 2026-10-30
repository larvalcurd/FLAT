namespace VM;

/// <summary>
/// Стековая виртуальная машина для MicroPascal.
/// </summary>
public class VirtualMachine(List<Instruction> program, TextReader? input = null, TextWriter? output = null)
{
    /// <summary>
    /// Программа, т.е. список инструкций, которая будет выполнять ВМ
    /// </summary>
    private readonly List<Instruction> _program = program;

    /// <summary>
    /// Входящий поток
    /// </summary>
    private readonly InputReader _input = new(input ?? Console.In);

    /// <summary>
    /// Выходящий поток
    /// </summary>
    private readonly OutputWriter _output = new(output ?? Console.Out);

    /// <summary>
    /// Стэк для вычислений
    /// </summary>
    private readonly Stack<int> _stack = new();

    /// <summary>
    /// Указатель на инструкцию, что выполняется в данный момент
    /// </summary>
    private int _ip;

    public IReadOnlyList<int> StackSnapshot => [.. _stack];

    public void Run()
    {
        while (_ip < _program.Count)
        {
            Instruction instruction = _program[_ip++];
            Execute(instruction);
        }
    }

    /// <summary>
    /// Реализация поведения инструкций из разрешенного списка.
    /// </summary>
    /// <param name="instruction">Инструкция ВМ, которая будет выполняться в данный момент</param>
    /// <exception cref="VmException">Исключение возникает в случае попытки деления на 0</exception>
    private void Execute(Instruction instruction)
    {
        switch (instruction.Operation)
        {
            case OpCodes.Halt:
                {
                    _ip = _program.Count;
                    break;
                }

            case OpCodes.Push:
                {
                    _stack.Push(instruction.Operand ?? throw new VmException("Operand is null"));
                    break;
                }

            case OpCodes.Add:
                {
                    Binary((a, b) => a + b);
                    break;
                }

            case OpCodes.Sub:
                {
                    Binary((a, b) => a - b);
                    break;
                }

            case OpCodes.Mul:
                {
                    Binary((a, b) => a * b);
                    break;
                }

            case OpCodes.Div:
                {
                    Binary((a, b) => b == 0 ? throw new VmException("Division by zero") : a / b);
                    break;
                }

            case OpCodes.Mod:
                {
                    Binary((a, b) => b == 0 ? throw new VmException("Division by zero") : a % b);
                    break;
                }

            case OpCodes.Neg:
                {
                    _stack.Push(-Pop());
                    break;
                }

            case OpCodes.Eq:
                {
                    Compare((a, b) => a == b);
                    break;
                }

            case OpCodes.Ne:
                {
                    Compare((a, b) => a != b);
                    break;
                }

            case OpCodes.Lt:
                {
                    Compare((a, b) => a < b);
                    break;
                }

            case OpCodes.Le:
                {
                    Compare((a, b) => a <= b);
                    break;
                }

            case OpCodes.Gt:
                {
                    Compare((a, b) => a > b);
                    break;
                }

            case OpCodes.Ge:
                {
                    Compare((a, b) => a >= b);
                    break;
                }

            case OpCodes.And:
                {
                    Logical((a, b) => a && b);
                    break;
                }

            case OpCodes.Or:
                {
                    Logical((a, b) => a || b);
                    break;
                }

            case OpCodes.Not:
                {
                    _stack.Push(Pop() == 0 ? 1 : 0);
                    break;
                }

            case OpCodes.Read:
                {
                    _stack.Push(_input.ReadInt());
                    break;
                }

            case OpCodes.Readln:
                {
                    _stack.Push(_input.ReadlnInt());
                    break;
                }

            case OpCodes.Write:
                {
                    _output.Write(Pop());
                    break;
                }

            case OpCodes.Writeln:
                {
                    _output.WriteLine(Pop());
                    break;
                }

            default:
                throw new VmException($"Unsupported opcode: {instruction.Operation}");
        }
    }

    /// <summary>
    /// Обертка для бинарных операций.
    /// </summary>
    /// <param name="op">Передаваемая операция вместе с операндами</param>
    private void Binary(Func<int, int, int> op)
    {
        int b = Pop();
        int a = Pop();
        _stack.Push(op(a, b));
    }

    /// <summary>
    /// Обертка для операций сравнения.
    /// </summary>
    /// <param name="op">Передаваемая операция вместе с операндами</param>
    private void Compare(Func<int, int, bool> op)
    {
        int b = Pop();
        int a = Pop();
        _stack.Push(op(a, b) ? 1 : 0);
    }

    /// <summary>
    /// Обертка для логических операций.
    /// </summary>
    /// <param name="op">Передаваемая операция вместе с операндами</param>
    private void Logical(Func<bool, bool, bool> op)
    {
        int b = Pop();
        int a = Pop();
        _stack.Push(op(a != 0, b != 0) ? 1 : 0);
    }

    /// <summary>
    /// Модифицированная инструкция POP для стека вычислений.
    /// </summary>
    /// <returns>Значение с вершины стека</returns>
    /// <exception cref="VmException">Исключение возникает в случае попытки извлечь значение из пустого стека </exception>
    private int Pop()
    {
        return _stack.Count == 0 ? throw new VmException("Stack is empty") : _stack.Pop();
    }
}