using VM;

using Xunit;

using static VM.OpCodes;

namespace Lexer.UnitTests;

public class VmTests
{
    [Fact]
    public void Push_WithoutHaltCorrectly()
    {
        Assert.Equal([2, 1], RunStack(
            "",
            new Instruction(Push, 1),
            new Instruction(Push, 2)));
    }

    [Fact]
    public void Halt_StopsBeforeNextInstruction()
    {
        string output = Run(
            "",
            new Instruction(Push, 42),
            new Instruction(Halt),
            new Instruction(Writeln));
        Assert.Equal("", output);
    }

    [Fact]
    public void Halt_OnEmptyProgram_LeavesStackEmpty()
    {
        Assert.Equal([], RunStack(
            "",
            new Instruction(Halt)));
    }

    [Fact]
    public void Push_WithoutOperand_Throws()
        => AssertVmThrows(
            "",
            new Instruction(Push),
            new Instruction(Halt));

    [Fact]
    public void Pop_FromEmptyStack_Throws()
        => AssertVmThrows(
            "",
            new Instruction(Add),
            new Instruction(Halt));

    public static TheoryData<OpCodes, int, int, int> BinaryOpCases() => new()
    {
        { OpCodes.Add, 2, 3, 5 },
        { OpCodes.Sub, 10, 3, 7 },
        { OpCodes.Mul, 6, 7, 42 },
        { OpCodes.Div, 20, 5, 4 },
        { OpCodes.Mod, 10, 3, 1 },
    };

    [Theory]
    [MemberData(nameof(BinaryOpCases))]
    public void BinaryOp_ComputesCorrectly(OpCodes op, int a, int b, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Push, b),
            new Instruction(op));
        Assert.Equal([expected], stack);
    }

    public static TheoryData<int, int> NegFlipSignCases() => new() { { -5, 5 }, { 5, -5 }, };

    [Theory]
    [MemberData(nameof(NegFlipSignCases))]
    public void Neg_FlipSign(int a, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Neg));
        Assert.Equal([expected], stack);
    }

    [Fact]
    public void Div_ByZero_Throws()
        => AssertVmThrows(
            "",
            new Instruction(Push, 10),
            new Instruction(Push, 0),
            new Instruction(Div));

    [Fact]
    public void Mod_ByZero_Throws()
        => AssertVmThrows(
            "",
            new(Push, 10),
            new Instruction(Push, 0),
            new Instruction(Mod));

    [Fact]
    public void CalculateExpression_ComplexExpression()
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, 10),
            new Instruction(Push, 5),
            new Instruction(Add),
            new Instruction(Push, 2),
            new Instruction(Mul),
            new Instruction(Push, 10),
            new Instruction(Push, 5),
            new Instruction(Div),
            new Instruction(Sub));
        Assert.Equal([28], stack);
    }

    public static TheoryData<OpCodes, int, int, int> ComparisonCases() => new()
    {
        { Eq, 5, 5, 1 },
        { Eq, 5, 6, 0 },
        { Ne, 5, 6, 1 },
        { Ne, 5, 5, 0 },
        { Lt, 3, 5, 1 },
        { Lt, 5, 5, 0 },
        { Le, 5, 5, 1 },
        { Le, 6, 5, 0 },
        { Gt, 6, 5, 1 },
        { Gt, 5, 5, 0 },
        { Ge, 5, 5, 1 },
        { Ge, 3, 5, 0 },
        { Le, 10 - 3, 5, 0 },
    };

    [Theory]
    [MemberData(nameof(ComparisonCases))]
    public void Comparison_Works(OpCodes op, int a, int b, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Push, b),
            new Instruction(op));
        Assert.Equal([expected], stack);
    }

    public static TheoryData<OpCodes, int, int, int> BinaryBitOpsCases() => new()
    {
        { And, 5, 3, 1 },
        { And, 6, 3, 2 },
        { And, 1, 0, 0 },
        { Or, 5, 2, 7 },
        { Or, 8, 1, 9 },
        { Or, 0, 0, 0 },
    };

    [Theory]
    [MemberData(nameof(BinaryBitOpsCases))]
    public void BinaryBitOps_Works(OpCodes op, int a, int b, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Push, b),
            new Instruction(op));
        Assert.Equal([expected], stack);
    }

    public static TheoryData<int, int> BinaryOpNotCases() => new() { { 0, -1 }, { 1, -2 }, { 5, -6 } };

    [Theory]
    [MemberData(nameof(BinaryOpNotCases))]
    public void BinaryOpNot_Works(int a, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Not));
        Assert.Equal([expected], stack);
    }

    public static TheoryData<OpCodes, int, int, int> MinMaxCases() => new()
    {
        { Min, 2, 1, 1 },
        { Min, -2, -1, -2 },
        { Min, -1, 1, -1 },
        { Max, 2, 1, 2 },
        { Max, -2, -1, -1 },
        { Max, -1, 1, 1 },
    };

    [Theory]
    [MemberData(nameof(MinMaxCases))]
    public void MinMax_Works(OpCodes op, int a, int b, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Push, b),
            new Instruction(op));
        Assert.Equal([expected], stack);
    }

    public static TheoryData<int, int> AbsCases() => new() { { 1, 1 }, { -1, 1 }, { 0, 0 }, };

    [Theory]
    [MemberData(nameof(AbsCases))]
    public void Abs_Works(int a, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Abs));
        Assert.Equal([expected], stack);
    }

    [Fact]
    public void Abs_OverflowIntException()
    {
        Assert.Throws<OverflowException>(() =>
            RunStack("", new Instruction(Push, int.MinValue), new Instruction(Abs)));
    }

    [Fact]
    public void UnknownOpcode_Throws()
    {
        AssertVmThrows(
            "",
            new Instruction((OpCodes)999),
            new Instruction(Halt));
    }

    [Fact]
    public void Write_PrintsWithoutNewLine()
    {
        Assert.Equal("12", Run(
            "",
            new Instruction(Push, 1),
            new Instruction(Write),
            new Instruction(Push, 2),
            new Instruction(Write)));
    }

    [Fact]
    public void Writeln_PrintsWithNewLine()
    {
        Assert.Equal("1\n", Run(
            "",
            new Instruction(Push, 1),
            new Instruction(Operation: Writeln)));
    }

    [Fact]
    public void Writeln_NegativeNumber()
    {
        Assert.Equal("-1\n", Run(
            "",
            new Instruction(Push, -1),
            new Instruction(Operation: Writeln)));
    }

    [Fact]
    public void Writeln_PrintsWithNewLineTwice()
    {
        Assert.Equal("1\n2\n", Run(
            "",
            new Instruction(Push, 1),
            new Instruction(Operation: Writeln),
            new Instruction(Operation: Push, 2),
            new Instruction(Writeln)));
    }

    [Fact]
    public void Write_FromEmptyStackThrows()
    {
        AssertVmThrows(
            "",
            new Instruction(Write));
    }

    public static TheoryData<int, string> ReadBorderCases() => new()
    {
        { 42, "42" }, { -2147483648, "-2147483648" }, { 1, "\n\t 1" }, { 2147483647, "2147483647" },
    };

    [Theory]
    [MemberData(nameof(ReadBorderCases))]
    public void Read_ParsesInt(int exp, string act)
    {
        Assert.Equal([exp], RunStack(
            act,
            new Instruction(Read),
            new Instruction(Halt)));
    }

    [Fact]
    public void Read_SkipsWhitespaceAndHandlesSigns()
    {
        IReadOnlyList<int> stack = RunStack(
            "  -7  3  ",
            new Instruction(Read),
            new Instruction(Read),
            new Instruction(Halt));
        Assert.Equal([3, -7], stack);
    }

    [Fact]
    public void Readln_ConsumesRestOfLine()
    {
        IReadOnlyList<int> stack = RunStack(
            "10 99\n20\n",
            new Instruction(Readln),
            new Instruction(Readln),
            new Instruction(Halt));
        Assert.Equal([20, 10], stack);
    }

    [Fact]
    public void Readln_BeforeRead()
    {
        IReadOnlyList<int> stack = RunStack(
            "10 30\r\n20\r\n",
            new Instruction(Readln),
            new Instruction(Read),
            new Instruction(Halt));
        Assert.Equal([20, 10], stack);
    }

    [Fact]
    public void Readln_AfterRead()
    {
        IReadOnlyList<int> stack = RunStack(
            "10 30\r\n20\r\n",
            new Instruction(Read),
            new Instruction(Readln),
            new Instruction(Halt));
        Assert.Equal([30, 10], stack);
    }

    [Fact]
    public void Readln_OnEofAfterNumber_IsOk()
    {
        IReadOnlyList<int> stack = RunStack(
            "10",
            new Instruction(Readln),
            new Instruction(Halt));
        Assert.Equal([10], stack);
    }

    [Fact]
    public void Read_AfterEOFThrow()
    {
        AssertVmThrows(
            "10",
            new Instruction(Read),
            new Instruction(Read));
    }

    public static TheoryData<string> ReadInvalidInputCases() =>
    [
        " ",
        "abc",
        "12ab",
        "--5",
        "- 5",
        "-",
        "2147483648",
        "-2147483649"
    ];

    [Theory]
    [MemberData(nameof(ReadInvalidInputCases))]
    public void Read_InvalidInput_Throws(string input)
    {
        AssertVmThrows(
            input,
            new Instruction(Read),
            new Instruction(Halt));
    }

    [Fact]
    public void ReadThenWrite_EchoesInput()
    {
        string output = Run(
            "7",
            prog:
            [
                new Instruction(Read),
                new Instruction(Writeln),
                new Instruction(Halt)
            ]);
        Assert.Equal("7\n", output);
    }

    [Fact]
    public void Program_FullExpression()
    {
        string output = Run("1 2 5 15", [
            new Instruction(Read),
            new Instruction(Read),
            new Instruction(Add),
            new Instruction(Read),
            new Instruction(Mul),
            new Instruction(Read),
            new Instruction(Div),
            new Instruction(Writeln)
        ]);
        Assert.Equal("1\n", output);
    }

    [Fact]
    public void Program_AfterHaltNotWorks()
    {
        string output = Run("1 2 5", [
            new Instruction(Read),
            new Instruction(Read),
            new Instruction(Add),
            new Instruction(Writeln),
            new Instruction(Halt),
            new Instruction(Read),
            new Instruction(Add),
            new Instruction(Writeln)
        ]);
        Assert.Equal("3\n", output);
    }

    [Fact]
    public void Constructor_EmptyVmConstructorCorrectly()
    {
        Console.SetIn(new StringReader(""));
        Console.SetOut(new StringWriter());
        VirtualMachine vm = new([new Instruction(Halt)]);
        vm.Run();
        Assert.Empty(vm.StackSnapshot);
    }

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
}