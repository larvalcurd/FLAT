using VM;

using Xunit;

using static VM.OpCodes;

namespace Lexer.UnitTests;

public class VMTests
{
    private static VirtualMachine Vm(string input, TextWriter output, params Instruction[] prog)
        => new([.. prog], new StringReader(input), output);

    private static string Run(string input, params Instruction[] prog)
    {
        StringWriter output = new();
        Vm(input, output, prog).Run();
        return output.ToString();
    }

    private static IReadOnlyList<int> RunStack(string input, params Instruction[] prog)
    {
        VirtualMachine vm = Vm(input, new StringWriter(), prog);
        vm.Run();
        return vm.StackSnapshot;
    }

    private static void AssertVmThrows(string input, params Instruction[] prog)
    {
        VirtualMachine vm = Vm(input, new StringWriter(), prog);
        Assert.Throws<VmException>(vm.Run);
    }

    [Fact]
    public void Push_Halt_ProgramEndsCorrectly()
    {
        Assert.Equal(new[] { 2, 1 }, RunStack(
            "",
            new(Push, 1), new(Push, 2), new(Halt)));
    }

    [Fact]
    public void Halt_StopsBeforeNextInstruction()
    {
        string output = Run("",
            new(Push, 42), new(Halt), new(Writeln));
        Assert.Equal("", output);
    }

    [Fact]
    public void Push_WithoutOperand_Throws()
        => AssertVmThrows("", new(Push), new(Halt));

    [Fact]
    public void Pop_FromEmptyStack_Throws()
        => AssertVmThrows("", new(Add), new(Halt));
    
    [Theory]
    [InlineData(Add, 2, 3, 5)]
    [InlineData(Sub, 10, 3, 7)] // a - b, не b - a
    [InlineData(Mul, 6, 7, 42)]
    [InlineData(Div, 20, 5, 4)] // a / b, не b / a
    [InlineData(Mod, 10, 3, 1)]
    public void BinaryOp_ComputesCorrectly(OpCodes op, int a, int b, int expected)
    {
        IReadOnlyList<int> stack = RunStack("",
            new(Push, a), new(Push, b), new(op));
        Assert.Equal([expected], stack);
    }

    [Fact]
    public void Neg_FlipsSign()
    {
        Assert.Equal(new[] { -5 }, RunStack("",
            new(Push, 5), new(Neg)));
    }

    [Fact]
    public void Div_ByZero_Throws()
        => AssertVmThrows("",
            new(Push, 10), new(Push, 0), new(Div));

    [Fact]
    public void Mod_ByZero_Throws()
        => AssertVmThrows("",
            new(Push, 10), new(Push, 0), new(Mod));

    [Theory]
    [InlineData(Eq, 5, 5, 1)]
    [InlineData(Eq, 5, 6, 0)]
    [InlineData(Ne, 5, 6, 1)]
    [InlineData(Ne, 5, 5, 0)]
    [InlineData(Lt, 3, 5, 1)]
    [InlineData(Lt, 5, 5, 0)]
    [InlineData(Le, 5, 5, 1)]
    [InlineData(Le, 6, 5, 0)]
    [InlineData(Gt, 6, 5, 1)]
    [InlineData(Gt, 5, 5, 0)]
    [InlineData(Ge, 5, 5, 1)]
    [InlineData(Ge, 3, 5, 0)]
    public void Comparison_Works(OpCodes op, int a, int b, int expected)
    {
        IReadOnlyList<int> stack = RunStack("",
            new(Push, a), new(Push, b), new(op));
        Assert.Equal([expected], stack);
    }

    [Theory]
    [InlineData(And, 1, 1, 1)]
    [InlineData(And, 1, 0, 0)]
    [InlineData(And, 0, 0, 0)]
    [InlineData(Or, 1, 0, 1)]
    [InlineData(Or, 0, 0, 0)]
    [InlineData(Or, 5, 0, 1)]
    public void LogicalBinary_Works(OpCodes op, int a, int b, int expected)
    {
        IReadOnlyList<int> stack = RunStack("",
            new(Push, a), new(Push, b), new(op));
        Assert.Equal([expected], stack);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(5, 0)]
    public void Not_Works(int a, int expected)
    {
        IReadOnlyList<int> stack = RunStack("",
            new(Push, a), new(Not));
        Assert.Equal(new[] { expected }, stack);
    }

    [Fact]
    public void Write_PrintsWithoutNewLine()
    {
        Assert.Equal("123", Run("",
            new(Push, 1), new(Write),
            new(Push, 2), new(Write),
            new(Push, 3), new(Write)));
    }

    [Fact]
    public void Writeln_PrintsWithNewLine()
    {
        Assert.Equal("1\n2\n", Run("",
            new(Push, 1), new(Writeln),
            new(Push, 2), new(Writeln)));
    }

    // ---------- Ввод ----------

    [Fact]
    public void Read_ParsesInt()
    {
        Assert.Equal(new[] { 42 }, RunStack("42",
            new(Read), new(Halt)));
    }

    [Fact]
    public void Read_SkipsWhitespaceAndHandlesSigns()
    {
        IReadOnlyList<int> stack = RunStack(
            "  -7  3  ",
            new(Read), new(Read), new(Halt));
        Assert.Equal(new[] { 3, -7 }, stack);
    }

    [Fact]
    public void Readln_ConsumesRestOfLine()
    {
        IReadOnlyList<int> stack = RunStack("10 99\n20\n",
            new(Readln), new(Readln), new(Halt));
        Assert.Equal(new[] { 20, 10 }, stack);
    }

    [Fact]
    public void ReadThenWrite_EchoesInput()
    {
        string output = Run("7",
            new(Read), new(Writeln), new(Halt));
        Assert.Equal("7\n", output);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12ab")]
    [InlineData("- 5")]
    [InlineData("2147483648")]
    public void Read_InvalidInput_Throws(string input)
    {
        AssertVmThrows(input, new(Read), new(Halt));
    }

    [Fact]
    public void Read_MinValue_IsAccepted()
    {
        IReadOnlyList<int> stack = RunStack("-2147483648",
            new(Read), new(Halt));
        Assert.Equal(new[] { int.MinValue }, stack);
    }

    [Fact]
    public void UnknownOpcode_Throws()
    {
        AssertVmThrows("",
            new Instruction((OpCodes)999, null),
            new(Halt));
    }
}