namespace VM;

/// <summary>
/// Стековая виртуальная машина для MicroPascal.
/// </summary>
// TODO: Дописать поведение на инструкции ввода
// TODO: Убрать предупреждения сборки.
public class VirtualMachine(List<Instruction> program, TextReader? input = null, TextWriter? output = null)
{
    /// <summary>
    /// Программа, т.е. список инструкций, которая будет выполнять ВМ 
    /// </summary>
    private List<Instruction> _program { get; } = program;

    /// <summary>
    /// Стэк для вычислений
    /// </summary>
    private readonly Stack<int> _stack = new();

    /// <summary>
    /// Указатель на инструкцию, что выполняется в данный момент 
    /// </summary>
    private int _ip;

    private readonly InputReader _input = new(input ?? Console.In);
    private readonly OutputWriter _output = new(output ?? Console.Out);

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
    /// TODO: Дописать описание исключения
    /// <exception cref="VmException">Исключение, </exception>
    private void Execute(Instruction instruction)
    {
        switch (instruction.Operation)
        {
            case OpCodes.Halt:
                _ip = _program.Count;
                break;
            case OpCodes.Push:
                _stack.Push(instruction.Operand ?? throw new VmException("Operand is null"));
                break;
            case OpCodes.Add:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a + b);
                    break;
                }

            case OpCodes.Sub:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a - b);
                    break;
                }

            case OpCodes.Mul:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a * b);
                    break;
                }

            case OpCodes.Div:
                {
                    int b = Pop();
                    int a = Pop();
                    if (b == 0)
                    {
                        throw new VmException("Division by zero");
                    }

                    _stack.Push(a / b);
                    break;
                }

            case OpCodes.Mod:
                {
                    int b = Pop();
                    int a = Pop();
                    if (b == 0)
                    {
                        throw new VmException("Division by zero");
                    }

                    _stack.Push(a % b);
                    break;
                }

            case OpCodes.Neg:
                {
                    _stack.Push(-Pop());
                    break;
                }

            case OpCodes.Eq:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a == b ? 1 : 0);
                    break;
                }

            case OpCodes.Ne:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a != b ? 1 : 0);
                    break;
                }

            case OpCodes.Lt:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a < b ? 1 : 0);
                    break;
                }

            case OpCodes.Le:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a <= b ? 1 : 0);
                    break;
                }

            case OpCodes.Gt:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a > b ? 1 : 0);
                    break;
                }

            case OpCodes.Ge:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a >= b ? 1 : 0);
                    break;
                }

            case OpCodes.And:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a != 0 && b != 0 ? 1 : 0);
                    break;
                }

            case OpCodes.Or:
                {
                    int b = Pop();
                    int a = Pop();
                    _stack.Push(a != 0 || b != 0 ? 1 : 0);
                    break;
                }

            case OpCodes.Not:
                {
                    _stack.Push(Pop() == 0 ? 1 : 0);
                    break;
                }

            case OpCodes.Read:
                {
                    break;
                }

            case OpCodes.Readln:
                {
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
    /// Модифицированная инструкция POP для стека вычислений.
    /// </summary>
    /// <returns>Значение с вершины стека</returns>
    /// <exception cref="VmException">возникает в случае попытки извлечь значение из пустого стека </exception>
    private int Pop()
    {
        return _stack.Count == 0 ? throw new VmException("Stack is empty") : _stack.Pop();
    }
}