using ExampleLib;
using Xunit;

namespace MicroPascal.Tests;

public class SourceHandlerTests
{
    [Fact]
    public void Advance_ShouldReturnFirstCharacter()
    {
        SourceHandler handler = new SourceHandler("Hello");
        char result = handler.Advance();
        Assert.Equal('H', result);
    }

    [Fact]
    public void Peek_ShouldNotChangePosition()
    {
        SourceHandler handler = new SourceHandler("AB");
        char first = handler.Peek(0);
        char second = handler.Peek(1);
        char firstAgain = handler.Peek(0);
        Assert.Equal('A', first);
        Assert.Equal('B', second);
        Assert.Equal('A', firstAgain);
        Assert.Equal(0, handler.Position);
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
        SourceHandler handler = new SourceHandler(input);
        while (!handler.IsEof(handler.Position))
        {
            handler.Advance();
        }
        Assert.Equal(expectedLine, handler.Line);
        Assert.Equal(expectedCol, handler.Column);
    }

    [Fact]
    public void EmptyString_ShouldBeEofImmediately()
    {
        SourceHandler handler = new SourceHandler("");
        Assert.True(handler.IsEof(handler.Position));
        Assert.Equal('\0', handler.Peek(0));
    }
}