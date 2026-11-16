using Lexer;

using VM;

using Xunit;

namespace Parsing.UnitTests;

public class ParserTests
{
    // TODO: для тестов, перед сдачей удалить
    [Fact]
    public void ParseProgram_Testing()
    {
        Parser parser = new("program Test; begin write(8 + 3 * 2); end.");

        Instruction[] expected =
        [
            new Instruction(OpCodes.Push, 8),
            new Instruction(OpCodes.Push, 3),
            new Instruction(OpCodes.Push, 2),
            new Instruction(OpCodes.Mul),
            new Instruction(OpCodes.Add),
            new Instruction(OpCodes.Write),
        ];

        Assert.Equal(expected, parser.ParseProgram());
    }

    [Fact]
    public void ParseProgram_EmptyBlock_ReturnsNoInstructions()
    {
        Parser parser = new("program Test; begin end.");

        Assert.Empty(parser.ParseProgram());
    }

    [Fact]
    public void ParseProgram_WriteAndWriteln_ReturnsInstructionsInOrder()
    {
        Parser parser = new("program Test; begin write(3); writeln(((4))); end.");
        Instruction[] expected =
        [
            new Instruction(OpCodes.Push, 3),
            new Instruction(OpCodes.Write),
            new Instruction(OpCodes.Push, 4),
            new Instruction(OpCodes.Writeln),
        ];

        Assert.Equal(expected, parser.ParseProgram());
    }

    [Fact]
    public void ParseProgram_MultiplicationChain_ReturnsInstructionsInOrder()
    {
        Parser parser = new("program Test; begin writeln(2 * 3 * 4); end.");
        Instruction[] expected =
        [
            new Instruction(OpCodes.Push, 2),
            new Instruction(OpCodes.Push, 3),
            new Instruction(OpCodes.Mul),
            new Instruction(OpCodes.Push, 4),
            new Instruction(OpCodes.Mul),
            new Instruction(OpCodes.Writeln),
        ];

        Assert.Equal(expected, parser.ParseProgram());
    }

    [Fact]
    public void ParseProgram_PlusAndMultiplicationChain_ReturnsInstructionsInOrder()
    {
        Parser firstOrderPlusParser = new("program Test; begin write(8 + 3 * 2); end.");
        Parser secondOrderPlusParser = new("program Test; begin write(2 * 3 + 8); end.");

        Instruction[] firstOrderPlusExpected =
        [
            new Instruction(OpCodes.Push, 8),
            new Instruction(OpCodes.Push, 3),
            new Instruction(OpCodes.Push, 2),
            new Instruction(OpCodes.Mul),
            new Instruction(OpCodes.Add),
            new Instruction(OpCodes.Write),
        ];

        Instruction[] secondOrderPlusExpected =
        [
            new Instruction(OpCodes.Push, 2),
            new Instruction(OpCodes.Push, 3),
            new Instruction(OpCodes.Mul),
            new Instruction(OpCodes.Push, 8),
            new Instruction(OpCodes.Add),
            new Instruction(OpCodes.Write),
        ];

        Assert.Equal(firstOrderPlusExpected, firstOrderPlusParser.ParseProgram());
        Assert.Equal(secondOrderPlusExpected, secondOrderPlusParser.ParseProgram());
    }

    [Theory]
    [InlineData("program Demo;\nbegin end", "Expected Dot, found Eof", 2, 9)]
    [InlineData("program Demo;\nbegin end. extra", "Expected Eof, found Identifier", 2, 11)]
    [InlineData("program Demo;\nbegin writeln(2) end.", "Expected Semicolon, found End", 2, 17)]
    [InlineData("program Demo;\nbegin writeln(2; end.", "Expected CloseParen, found Semicolon", 2, 15)]
    [InlineData("program Demo;\nbegin writeln(); end.", "Expected IntegerLiteral, found CloseParen", 2, 14)]
    [InlineData("program Demo;\nbegin writeln(99999999999); end.", "Integer is out of range", 2, 14)]
    [InlineData("program Demo;\nbegin writeln(1 < 2 < 3); end.", "Expected CloseParen, found Less", 2, 20)]
    [InlineData("program Demo;\nbegin writeln(read(1)); end.", "Expected CloseParen, found IntegerLiteral", 2, 19)]
    [InlineData("program Demo;\nbegin writeln(foo(1)); end.", "Unknown function 'foo'", 2, 14)]
    [InlineData("program Demo;\nbegin writeln(abs(1, 2)); end.", "Function 'abs' expects 1 argument(s), got 2", 2, 14)]
    [InlineData("program Demo;\nbegin writeln(max(1)); end.", "Function 'max' expects 2 argument(s), got 1", 2, 14)]
    public void ParseProgram_InvalidProgram_ReportsErrorPosition(string source, string message, int line, int column)
    {
        Parser parser = new(source);

        ParserException error = Assert.Throws<ParserException>(() => parser.ParseProgram());

        Assert.Contains(message, error.Message);
        Assert.Equal(new SourcePosition(line, column), error.Position);
    }

    [Fact]
    public void ParseProgram_UnaryMinusAndMultiplication_ReturnsInstructionsInOrder()
    {
        Parser parser = new("program Test; begin writeln(-2 * 3); end.");
        Instruction[] expected =
        [
            new Instruction(OpCodes.Push, 2),
            new Instruction(OpCodes.Neg),
            new Instruction(OpCodes.Push, 3),
            new Instruction(OpCodes.Mul),
            new Instruction(OpCodes.Writeln),
        ];

        Assert.Equal(expected, parser.ParseProgram());
    }

    [Fact]
    public void ParseProgram_FunctionCalls_EmitArgumentsBeforeCalls()
    {
        Parser parser = new("program Test; begin writeln(max(read(), abs(-3))); end.");
        Instruction[] expected =
        [
            new Instruction(OpCodes.Read),
            new Instruction(OpCodes.Push, 3),
            new Instruction(OpCodes.Neg),
            new Instruction(OpCodes.Abs),
            new Instruction(OpCodes.Max),
            new Instruction(OpCodes.Writeln),
        ];

        Assert.Equal(expected, parser.ParseProgram());
    }
}