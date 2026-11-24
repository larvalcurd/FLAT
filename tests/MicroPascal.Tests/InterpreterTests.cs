using Xunit;

namespace Parsing.UnitTests;

public class InterpreterTests
{
    public static TheoryData<string, string> ExpressionPrecedenceCases() =>
        new() { { "2 + 3 * 4", "14" }, { "(2 + 3) * 4", "20" }, { "not 1 + 2", "0" }, };

    [Theory]
    [MemberData(nameof(ExpressionPrecedenceCases))]
    public void Run_Expression_RespectsPrecedence(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    public static TheoryData<string, string> LeftAssociativityCases() => new()
    {
        { "10 - 3 - 2", "5" }, { "20 div 2 div 2", "5" },
    };

    [Theory]
    [MemberData(nameof(LeftAssociativityCases))]
    public void Run_Expression_UsesLeftAssociativity(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    public static TheoryData<string, string> ExpressionPrintResultCases() => new()
    {
        { "20 div 3", "6" }, { "20 mod 3", "2" }, { "6 and 3", "2" }, { "4 or 1", "5" },
    };

    [Theory]
    [MemberData(nameof(ExpressionPrintResultCases))]
    public void Run_IntegerOperator_PrintsResult(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    public static TheoryData<string, string> UnaryMinusCases() => new()
    {
        { "-(2 + 3)", "-5" }, { "-((2 + 3))", "-5" }, { "-(-(2 + 3))", "5" },
    };

    [Theory]
    [MemberData(nameof(UnaryMinusCases))]
    public void Run_Expression_EvaluatesUnaryMinusWithParentheses(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    public static TheoryData<string, string> ComparisonCases() => new()
    {
        { "1 + 2 = 3", "1" },
        { "2 <> 2", "0" },
        { "3 > 2", "1" },
        { "2 >= 3", "0" },
        { "2 < 3", "1" },
        { "2 <= 2", "1" },
    };

    [Theory]
    [MemberData(nameof(ComparisonCases))]
    public void Run_Comparison_PrintsIntegerBoolean(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    public static TheoryData<string, string, string> ValidProgramCases() => new()
    {
        { "program Test; begin end.", "", "" },
        {
            "PROGRAM Test; BEGIN WRITE(MAX(read(), ABS(-3))); WRITELN(min(readln(), 8)); WRITELN(read()); END.",
            "2 9 99\n7\n", "38\n7\n"
        },
        { "program Test; begin writeln(readln()); writeln(read()); end.", "10\n20\n", "10\n20\n" },
        { "program Test; begin writeln(readln()); writeln(read()); end.", "10\r\n20\r\n", "10\n20\n" },
    };

    [Theory]
    [MemberData(nameof(ValidProgramCases))]
    public void Run_Program_UsesProvidedInputAndOutput(string source, string inputText, string expected)
    {
        using StringReader input = new(inputText);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Interpreter.Interpreter.Run(source, input, output, errors);

        Assert.Equal(0, exitCode);
        Assert.Equal(expected, output.ToString());
        Assert.Empty(errors.ToString());
    }

    public static TheoryData<string, string, string> InvalidProgramCases() => new()
    {
        { "program Test; begin write(7); end. extra", "Expected Eof", "" },
        { "program Test; begin write(7);", "Expected End", "" },
        { "program Test; begin var end.", "Expected Write or Writeln", "" },
        { "program Test; begin / end.", "Lexical error", "" },
        { "program Test; begin writeln(7); writeln(1 div 0); writeln(8); end.", "Division by zero", "7\n" },
    };

    [Theory]
    [MemberData(nameof(InvalidProgramCases))]
    public void Run_InvalidProgram_ReportsErrorAndStops(string source, string expectedError, string expectedOutput)
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Interpreter.Interpreter.Run(source, TextReader.Null, output, errors);

        Assert.Equal(1, exitCode);
        Assert.Equal(expectedOutput, output.ToString());
        Assert.Contains(expectedError, errors.ToString());
    }

    private static void AssertExpressionOutput(string expression, string expected)
    {
        string source = "program Test;" +
                        $" begin writeln({expression});" +
                        " end.";
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Interpreter.Interpreter.Run(source, TextReader.Null, output, errors);

        Assert.Equal(0, exitCode);
        Assert.Equal(expected + "\n", output.ToString());
        Assert.Empty(errors.ToString());
    }
}