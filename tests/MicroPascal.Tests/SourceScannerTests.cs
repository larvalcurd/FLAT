using Xunit;

namespace Lexer.UnitTests;

// TODO: посоветоваться, касаемо позиции каретки после переноса с одной строки на другую, то есть наличия пустых строк в конце файла.
public class SourceScannerTests
{
    [Fact]
    public void Peek_ShouldNotChangePosition()
    {
        SourceScanner scanner = new SourceScanner("AB");
        char first = scanner.Peek(0);
        char second = scanner.Peek(1);
        char firstAgain = scanner.Peek(0);
        Assert.Equal('A', first);
        Assert.Equal('B', second);
        Assert.Equal('A', firstAgain);
        Assert.Equal(0, scanner.Position);
    }

    public static TheoryData<string, int, int> LineColumnTestData()
    {
        TheoryData<string, int, int> data = new TheoryData<string, int, int>
        {
            { "ABC", 1, 3 },
            { "A\nB", 2, 1 },
            { "A\r\nB", 2, 1 },
            { "\r", 2, 0 },
        };
        return data;
    }

    [Theory]
    [MemberData(nameof(LineColumnTestData))]
    public void Advance_ShouldUpdateLineAndColumn(string input, int expectedLine, int expectedCol)
    {
        SourceScanner scanner = new SourceScanner(input);
        while (!scanner.IsEof())
        {
            scanner.Advance();
        }

        Assert.Equal(expectedLine, scanner.Line);
        Assert.Equal(expectedCol, scanner.Column);
    }

    [Fact]
    public void EmptyString_ShouldBeEofImmediately()
    {
        SourceScanner scanner = new SourceScanner("");
        Assert.True(scanner.IsEof());
        Assert.Equal('\0', scanner.Peek(0));
    }

    [Fact]
    public void Peek_NegativeOffset_Throws()
    {
        SourceScanner scanner = new SourceScanner("ab");

        Assert.Throws<ArgumentOutOfRangeException>(() => scanner.Peek(-1));
    }

    [Fact]
    public void Advance_AtEof_DoesNothing()
    {
        SourceScanner scanner = new SourceScanner("a");
        scanner.Advance();

        scanner.Advance();
        scanner.Advance();

        Assert.True(scanner.IsEof());
        Assert.Equal(1, scanner.Position);
        Assert.Equal(new SourcePosition(1, 1), scanner.CurrentPosition);
    }
}