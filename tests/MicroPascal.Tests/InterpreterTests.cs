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

    [Theory]
    [InlineData("20 div 3", "6")]
    [InlineData("20 mod 3", "2")]
    [InlineData("6 and 3", "2")]
    [InlineData("4 or 1", "5")]
    public void Run_IntegerOperator_PrintsResult(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    [Theory]
    [InlineData("-(2 + 3)", "-5")]
    [InlineData("-((2 + 3))", "-5")]
    [InlineData("-(-(2 + 3))", "5")]
    public void Run_Expression_EvaluatesUnaryMinusWithParentheses(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    [Theory]
    [InlineData("1 + 2 = 3", "1")]
    [InlineData("2 <> 2", "0")]
    [InlineData("3 > 2", "1")]
    [InlineData("2 >= 3", "0")]
    [InlineData("2 < 3", "1")]
    [InlineData("2 <= 2", "1")]
    public void Run_Comparison_PrintsIntegerBoolean(string expression, string expected)
    {
        AssertExpressionOutput(expression, expected);
    }

    [Theory]
    [InlineData("program Test; begin end.", "", "")]
    [InlineData(
        "PROGRAM Test; BEGIN WRITE(MAX(read(), ABS(-3))); WRITELN(min(readln(), 8)); WRITELN(read()); END.",
        "2 9 99\n7\n",
        "38\n7\n")]
    [InlineData("program Test; begin writeln(readln()); writeln(read()); end.", "10\n20\n", "10\n20\n")]
    [InlineData("program Test; begin writeln(readln()); writeln(read()); end.", "10\r\n20\r\n", "10\n20\n")]
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

    [Theory]
    [InlineData("program Test; begin write(7); end. extra", "Expected Eof", "")]
    [InlineData("program Test; begin write(7);", "Expected End", "")]
    [InlineData("program Test; begin var end.", "Expected Write or Writeln", "")]
    [InlineData("program Test; begin / end.", "Lexical error", "")]
    [InlineData("program Test; begin writeln(7); writeln(1 div 0); writeln(8); end.", "Division by zero", "7\n")]
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