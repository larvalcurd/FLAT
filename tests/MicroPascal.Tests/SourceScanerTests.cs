using ExampleLib;
using Xunit;

namespace MicroPascal.Tests;

public class SourceScanerTests
{
    [Fact]
    public void Advance_ShouldReturnFirstCharacter()
    {
        SourceScaner scaner = new SourceScaner("Hello");
        char result = scaner.Advance();
        Assert.Equal('H', result);
    }

    [Fact]
    public void Peek_ShouldNotChangePosition()
    {
        SourceScaner scaner = new SourceScaner("AB");
        char first = scaner.Peek(0);
        char second = scaner.Peek(1);
        char firstAgain = scaner.Peek(0);
        Assert.Equal('A', first);
        Assert.Equal('B', second);
        Assert.Equal('A', firstAgain);
        Assert.Equal(0, scaner.Position);
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
        SourceScaner scaner = new SourceScaner(input);
        while (!scaner.IsEof())
        {
            scaner.Advance();
        }
        Assert.Equal(expectedLine, scaner.Line);
        Assert.Equal(expectedCol, scaner.Column);
    }

    [Fact]
    public void EmptyString_ShouldBeEofImmediately()
    {
        SourceScaner scaner = new SourceScaner("");
        Assert.True(scaner.IsEof());
        Assert.Equal('\0', scaner.Peek(0));
    }
}