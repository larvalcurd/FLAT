using ExampleLib;
using Xunit;

namespace MicroPascal.Tests;

public class SourceScannerTests
{
    [Fact]
    public void Advance_ShouldReturnFirstCharacter()
    {
        SourceScanner scanner = new SourceScanner("Hello");
        char result = scanner.Advance();
        Assert.Equal('H', result);
    }

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
        TheoryData<string, int, int> data = new TheoryData<string, int, int>();
        data.Add("ABC", 1, 3);
        data.Add("A\nB", 2, 1);
        data.Add("A\r\nB", 2, 1);
        // TODO: посоветоваться, касаемо позиции каретки после переноса с одной строки на другую, то есть наличия пустых строк в конце файла.
        data.Add("\n\n", 3, 0);
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
}