namespace VM;

public readonly record struct Instruction(OpCode OpCode, int? Operand);