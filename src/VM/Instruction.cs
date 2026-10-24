namespace VM;

public readonly record struct Instruction(OpCodes Operation, int? Operand = null);