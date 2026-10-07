using Xunit;

namespace ExampleLib.UnitTests;

// TODO: посоветоваться, касаемо позиции каретки после переноса с одной строки на другую, то есть наличия пустых строк в конце файла.
public class SourceScannerTests
{
    [Fact]
    public void Peek_AnyOffset_DoesNotMovePosition()
    {
        SourceScanner scanner = new("AB");
        char first = scanner.Peek(0);
        char second = scanner.Peek(1);
        char firstAgain = scanner.Peek(0);
        Assert.Equal('A', first);
        Assert.Equal('B', second);
        Assert.Equal('A', firstAgain);
        Assert.Equal(0, scanner.Position);
    }

    public static TheoryData<string, int, int> LineColumnCases()
    {
        return new TheoryData<string, int, int>
        {
            { "ABC", 1, 3 }, { "A\nB", 2, 1 }, { "A\r\nB", 2, 1 }, { "\r", 2, 0 },
        };
    }

    [Theory]
    [MemberData(nameof(LineColumnCases))]
    public void Advance_ToEnd_UpdatesLineAndColumn(string input, int expectedLine, int expectedCol)
    {
        SourceScanner scanner = new(input);
        while (!scanner.IsEof())
        {
            scanner.Advance();
        }

        Assert.Equal(expectedLine, scanner.Line);
        Assert.Equal(expectedCol, scanner.Column);
    }

    [Fact]
    public void IsEof_EmptySource_ReturnsTrue()
    {
        SourceScanner scanner = new("");

        Assert.True(scanner.IsEof());
        Assert.Equal('\0', scanner.Peek(0));
    }

    [Fact]
    public void Peek_NegativeOffset_Throws()
    {
        SourceScanner scanner = new("ab");

        Assert.Throws<ArgumentOutOfRangeException>(() => scanner.Peek(-1));
    }

    [Fact]
    public void Advance_AtEof_DoesNothing()
    {
        SourceScanner scanner = new("a");
        scanner.Advance();

        scanner.Advance();
        scanner.Advance();

        Assert.True(scanner.IsEof());
        Assert.Equal(1, scanner.Position);
        Assert.Equal(new SourcePosition(1, 1), scanner.CurrentPosition);
    }
}