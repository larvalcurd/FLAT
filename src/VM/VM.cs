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
                return;
            case OpCodes.Push:
            case OpCodes.Add:
            case OpCodes.Sub:
            case OpCodes.Mul:
            case OpCodes.Div:
            case OpCodes.Mod:
            case OpCodes.Neg:
            case OpCodes.Eq:
            case OpCodes.Lt:
            case OpCodes.Le:
            case OpCodes.Gt:
            case OpCodes.Ge:
            case OpCodes.And:
            case OpCodes.Or:
            case OpCodes.Not:
            case OpCodes.Read:
            case OpCodes.Readln:
            case OpCodes.Write:
            case OpCodes.Writeln:
            default:
                throw new VMException($"Unsupported opcode: {instruction.Operation}");
        }
    }

    private int Pop()
    {
        if (_stack.Count == 0)
        {
            throw new VMException("Stack is empty");
        }

        return _stack.Pop();
    }
}