namespace VM;

/// <summary>
/// Стековая виртуальная машина.
/// </summary>
public class VirtualMachine(List<Instruction> program)
{
    private List<Instruction> _program { get; } = program;

    private readonly Stack<int> _stack = new();
    private int _ip;

    public void Run()
    {
        while (_ip < _program.Count)
        {
            Instruction instruction = _program[_ip++];
            Execute(instruction);
        }
    }

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
                int b = Pop();
                int a = Pop();
                _stack.Push(a + b);
                break;
            case OpCodes.Sub:
                int b = Pop();
                int a = Pop();
                _stack.Push(a - b);
                break;
            case OpCodes.Mul:
                int b = Pop();
                int a = Pop();
                _stack.Push(a * b);
            case OpCodes.Div:
                int b = Pop();
                int a = Pop();
                if (b == 0)
                {
                    throw new VmException("Division by zero");
                }

                _stack.Push(a / b);
            case OpCodes.Mod:
                int b = Pop();
                int a = Pop();
                if (b == 0)
                {
                    throw new VmException("Division by zero");
                }

                _stack.Push(a % b);
            case OpCodes.Neg:
                _stack.Push(-Pop());
                break;
            case OpCodes.Eq:
                int b = Pop();
                int a = Pop();
                _stack.Push(a == b ? 1 : 0);
                break;
            case OpCodes.Lt:
                int b = Pop();
                int a = Pop();
                _stack.Push(a > b ? 1 : 0);
                break;
            case OpCodes.Le:
                int b = Pop();
                int a = Pop();
                _stack.Push(a >= b ? 1 : 0);
                break;
            case OpCodes.Gt:
                int b = Pop();
                int a = Pop();
                _stack.Push(a < b ? 1 : 0);
                break;
            case OpCodes.Ge:
                int b = Pop();
                int a = Pop();
                _stack.Push(a >= b ? 1 : 0);
                break;
            case OpCodes.And:
                int b = Pop();
                int a = Pop();
                _stack.Push(a != 0 && b != 0 ? 1 : 0);
                break;
            case OpCodes.Or:
                int b = Pop();
                int a = Pop();
                _stack.Push(a != 0 || b != 0 ? 1 : 0);
                break;
            case OpCodes.Not:
                _stack.Push(Pop() == 0 ? 1 : 0);
                break;
            case OpCodes.Read:
            case OpCodes.Readln:
            case OpCodes.Write:
                Console.Write(Pop());
                break;
            case OpCodes.Writeln:
                Console.WriteLine(Pop());
                break;
            default:
                throw new VmException($"Unsupported opcode: {instruction.Operation}");
        }
    }

    private int Pop()
    {
        if (_stack.Count == 0)
        {
            throw new VmException("Stack is empty");
        }

        return _stack.Pop();
    }
}