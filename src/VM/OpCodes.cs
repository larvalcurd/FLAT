namespace VM;

/// <summary>
/// Инструкции, реализуемые виртуальной машиной
/// </summary>
/// FIXME: переписать бинарные логические операции на побитный вид
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
    Abs,
    Min,
    Max,
    Read,
    Readln,
    Write,
    Writeln,
}