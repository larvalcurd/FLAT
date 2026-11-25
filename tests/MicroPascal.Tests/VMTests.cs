using VM;

using Xunit;

using static VM.OpCodes;

namespace Lexer.UnitTests;

public class VmTests
{
    [Fact]
    public void Run_PushWithoutHalt_LeavesValuesOnStack()
    {
        Assert.Equal([2, 1], RunStack(
            "",
            new Instruction(Push, 1),
            new Instruction(Push, 2)));
    }

    [Fact]
    public void Run_HaltBeforeWriteln_PrintsNothing()
    {
        string output = Run(
            "",
            new Instruction(Push, 42),
            new Instruction(Halt),
            new Instruction(Writeln));
        Assert.Equal("", output);
    }

    [Fact]
    public void Run_HaltOnEmptyStack_LeavesStackEmpty()
    {
        Assert.Equal([], RunStack(
            "",
            new Instruction(Halt)));
    }

    [Fact]
    public void Run_PushWithoutOperand_Throws()
        => AssertVmThrows(
            "",
            new Instruction(Push),
            new Instruction(Halt));

    [Fact]
    public void Run_AddOnEmptyStack_Throws()
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
    public void Run_ArithmeticOperation_PushesResult(OpCodes op, int a, int b, int expected)
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
    public void Run_NegInstruction_ChangesSign(int a, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Neg));
        Assert.Equal([expected], stack);
    }

    [Fact]
    public void Run_DivisionByZero_Throws()
        => AssertVmThrows(
            "",
            new Instruction(Push, 10),
            new Instruction(Push, 0),
            new Instruction(Div));

    [Fact]
    public void Run_ModuloByZero_Throws()
        => AssertVmThrows(
            "",
            new(Push, 10),
            new Instruction(Push, 0),
            new Instruction(Mod));

    [Fact]
    public void Run_ArithmeticExpression_LeavesResultOnStack()
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
    public void Run_Comparison_PushesIntegerBoolean(OpCodes op, int a, int b, int expected)
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
    public void Run_BitwiseOperation_PushesResult(OpCodes op, int a, int b, int expected)
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
    public void Run_NotInstruction_InvertsBits(int a, int expected)
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
    public void Run_MinOrMax_PushesSelectedValue(OpCodes op, int a, int b, int expected)
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
    public void Run_AbsInstruction_PushesAbsoluteValue(int a, int expected)
    {
        IReadOnlyList<int> stack = RunStack(
            "",
            new Instruction(Push, a),
            new Instruction(Abs));
        Assert.Equal([expected], stack);
    }

    [Fact]
    public void Run_AbsOfMinInteger_ThrowsOverflowException()
    {
        Assert.Throws<OverflowException>(() =>
            RunStack("", new Instruction(Push, int.MinValue), new Instruction(Abs)));
    }

    [Fact]
    public void Run_UnknownOpcode_Throws()
    {
        AssertVmThrows(
            "",
            new Instruction((OpCodes)999),
            new Instruction(Halt));
    }

    [Fact]
    public void Run_WriteInstruction_PrintsWithoutNewLine()
    {
        Assert.Equal("12", Run(
            "",
            new Instruction(Push, 1),
            new Instruction(Write),
            new Instruction(Push, 2),
            new Instruction(Write)));
    }

    [Fact]
    public void Run_WritelnInstruction_PrintsWithNewLine()
    {
        Assert.Equal("1\n", Run(
            "",
            new Instruction(Push, 1),
            new Instruction(Operation: Writeln)));
    }

    [Fact]
    public void Run_WritelnNegativeNumber_PrintsSignAndValue()
    {
        Assert.Equal("-1\n", Run(
            "",
            new Instruction(Push, -1),
            new Instruction(Operation: Writeln)));
    }

    [Fact]
    public void Run_TwoWritelnInstructions_PrintSeparateLines()
    {
        Assert.Equal("1\n2\n", Run(
            "",
            new Instruction(Push, 1),
            new Instruction(Operation: Writeln),
            new Instruction(Operation: Push, 2),
            new Instruction(Writeln)));
    }

    [Fact]
    public void Run_WriteOnEmptyStack_Throws()
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
    public void Run_ReadInteger_PushesValue(int exp, string act)
    {
        Assert.Equal([exp], RunStack(
            act,
            new Instruction(Read),
            new Instruction(Halt)));
    }

    [Fact]
    public void Run_ReadSignedIntegers_SkipsWhitespace()
    {
        IReadOnlyList<int> stack = RunStack(
            "  -7  3  ",
            new Instruction(Read),
            new Instruction(Read),
            new Instruction(Halt));
        Assert.Equal([3, -7], stack);
    }

    [Fact]
    public void Run_ReadlnInstruction_SkipsRestOfLine()
    {
        IReadOnlyList<int> stack = RunStack(
            "10 99\n20\n",
            new Instruction(Readln),
            new Instruction(Readln),
            new Instruction(Halt));
        Assert.Equal([20, 10], stack);
    }

    [Fact]
    public void Run_ReadAfterReadln_ReadsNextLine()
    {
        IReadOnlyList<int> stack = RunStack(
            "10 30\r\n20\r\n",
            new Instruction(Readln),
            new Instruction(Read),
            new Instruction(Halt));
        Assert.Equal([20, 10], stack);
    }

    [Fact]
    public void Run_ReadlnAfterRead_ReadsCurrentLine()
    {
        IReadOnlyList<int> stack = RunStack(
            "10 30\r\n20\r\n",
            new Instruction(Read),
            new Instruction(Readln),
            new Instruction(Halt));
        Assert.Equal([30, 10], stack);
    }

    [Fact]
    public void Run_ReadlnAtEofAfterNumber_PushesValue()
    {
        IReadOnlyList<int> stack = RunStack(
            "10",
            new Instruction(Readln),
            new Instruction(Halt));
        Assert.Equal([10], stack);
    }

    [Fact]
    public void Run_ReadAfterEof_Throws()
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
    public void Run_ReadInvalidInput_Throws(string input)
    {
        AssertVmThrows(
            input,
            new Instruction(Read),
            new Instruction(Halt));
    }

    [Fact]
    public void Run_ReadThenWriteln_PrintsInputValue()
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
    public void Run_ExpressionWithInput_PrintsResult()
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
    public void Run_HaltAfterOutput_StopsFurtherExecution()
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
    public void Run_DefaultInputAndOutput_LeavesStackEmpty()
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