namespace VM;

/// <summary>
/// Инструкции, реализуемые виртуальной машиной
/// </summary>
public enum OpCodes
{
    Halt,
    Push,
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Neg,
    Eq,
    Ne,
    Lt,
    Le,
    Gt,
    Ge,
    And,
    Or,
    Not,
    Read,
    Readln,
    Write,
    Writeln,
}