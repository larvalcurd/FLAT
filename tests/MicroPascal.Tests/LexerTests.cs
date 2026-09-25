using ExampleLib;
using Xunit;

namespace MicroPascal.Tests;

public class LexerTests
{
    private static Lexer CreateLexer(string source)
    {
        return new Lexer(source);
    }

    [Fact]
    public void NextToken_ShouldSkipSpacesAndReturnKeyword()
    {
        Lexer lexer = CreateLexer("   begin");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.Begin, token.Type);
        Assert.Equal("begin", token.Value);
    }

    [Fact]
    public void NextToken_ShouldSkipTabAndNewline()
    {
        Lexer lexer = CreateLexer("\t\nvar");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.Var, token.Type);
        Assert.Equal(2, token.Line);
        Assert.Equal(0, token.Column);
    }

    [Fact]
    public void NextToken_ShouldSkipSingleLineComment()
    {
        Lexer lexer = CreateLexer("// comma\nvar");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.Var, token.Type);
        Assert.Equal(2, token.Line);
    }

    [Fact]
    public void NextToken_ShouldSkipBlockComment()
    {
        Lexer lexer = CreateLexer("{ multiline \n comma } end");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.End, token.Type);
    }

    [Fact]
    public void NextToken_ShouldThrowOnUnclosedBlockComment()
    {
        Lexer lexer = CreateLexer("{ unclosed comma");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_ShouldHandleNestedBracesAsContent()
    {
        Lexer lexer = CreateLexer("{ a { b } c } var");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_HandleIntegerLiteral()
    {
        Lexer lexer = CreateLexer("122");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.IntegerLiteral, token.Type);
    }
    [Fact]
    public void NextToken_HandleExceptionNumericLiteralLeadZero()
    {
        Lexer lexer = CreateLexer("0122");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    [Fact]
    public void NextToken_ThrowExceptionInvalidIdentifier()
    {
        Lexer lexer = CreateLexer("122a");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_HandleStringLiteral()
    {
        Lexer lexer = CreateLexer("'123'");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.StringLiteral, token.Type);
    }
    [Fact]
    public void NextToken_ThrowExceptionUnclosedString()
    {
        Lexer lexer = CreateLexer("'123");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    [Fact]
    public void NextToken_ThrowExceptionNewLineInString()
    {
        Lexer lexer = CreateLexer("'\n'");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_ShouldRecognizeAssignmentOperator()
    {
        Lexer lexer = CreateLexer("x := 5");
        lexer.NextToken();
        Token assign = lexer.NextToken();
        Assert.Equal(TokenType.Assign, assign.Type);
    }

    [Fact]
    public void NextToken_ShouldRecognizeComparisonOperators()
    {
        Lexer lexer = CreateLexer("<> <= >=");
        Assert.Equal(TokenType.NotEqual, lexer.NextToken().Type);
        Assert.Equal(TokenType.LessOrEqual, lexer.NextToken().Type);
        Assert.Equal(TokenType.GreaterOrEqual, lexer.NextToken().Type);
    }

    [Fact]
    public void NextToken_ShouldRecognizeDelimiters()
    {
        Lexer lexer = CreateLexer("(arr[i]); ..");
        Assert.Equal(TokenType.OpenParen, lexer.NextToken().Type);
        Assert.Equal(TokenType.OpenBracket, lexer.NextToken().Type);
        Assert.Equal(TokenType.CloseBracket, lexer.NextToken().Type);
        Assert.Equal(TokenType.CloseParen, lexer.NextToken().Type);
        Assert.Equal(TokenType.Semicolon, lexer.NextToken().Type);
        Assert.Equal(TokenType.DoubleDot, lexer.NextToken().Type);
    }
    [Fact]
    public void NextToken_ShouldHandleZeroCorrectly()
    {
        Lexer lexer = CreateLexer("0");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.IntegerLiteral, token.Type);
        Assert.Equal("0", token.Value);
    }

    [Theory]
    [InlineData("00")]
    [InlineData("01")]
    [InlineData("007")]
    public void NextToken_ShouldThrowOnLeadingZeros(string input)
    {
        Lexer lexer = CreateLexer(input);
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    [Fact]
    public void NextToken_ShouldHandleEscapedQuotesInString()
    {
        Lexer lexer = CreateLexer("'It''s ok'");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.StringLiteral, token.Type);
        Assert.Equal("It's ok", token.Value);
    }

    [Fact]
    public void NextToken_ShouldHandleEmptyString()
    {
        Lexer lexer = CreateLexer("''");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.StringLiteral, token.Type);
        Assert.Equal("", token.Value);
    }
    [Fact]
    public void NextToken_ShouldThrowOnInvalidCharacter()
    {
        Lexer lexer = CreateLexer("a / b");
        lexer.NextToken();
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_PositionOfAssignOperator()
    {
        Lexer lexer = CreateLexer("x := 5");
        lexer.NextToken();
        Token assign = lexer.NextToken();
        Assert.Equal(1, assign.Line);
        Assert.Equal(3, assign.Column);
    }
    
    [Fact]
    public void NextToken_ShouldThrowOnDelCharInString()
    {
        Lexer lexer = CreateLexer("'a\u007Fb'");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_ShouldThrowOnUnicodeLetter()
    {
        Lexer lexer = CreateLexer("привет");
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_OnEmptyInput_ReturnsEof()
    {
        Lexer lexer = CreateLexer("");
        Assert.Equal(TokenType.Eof, lexer.NextToken().Type);
    }
    
    [Fact]
    public void Advance_ShouldTreatLoneCrAsNewLine()
    {
        SourceScanner scanner = new SourceScanner("A\rB");
        while (!scanner.IsEof()) scanner.Advance();
        Assert.Equal(2, scanner.Line);
    }
    
    [Fact]
    public void NextToken_ClosingBraceAlone_ShouldThrow()
    {
        Lexer lexer = CreateLexer("a } b");
        lexer.NextToken();
        Assert.Throws<LexerException>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_ShouldParseComplexExpression()
    {
        Lexer lexer = CreateLexer("if (x >= 10) then begin end");
    
        Assert.Equal(TokenType.If, lexer.NextToken().Type);
        Assert.Equal(TokenType.OpenParen, lexer.NextToken().Type);
        Assert.Equal(TokenType.Identifier, lexer.NextToken().Type);
        Assert.Equal(TokenType.GreaterOrEqual, lexer.NextToken().Type);
        Assert.Equal(TokenType.IntegerLiteral, lexer.NextToken().Type);
        Assert.Equal(TokenType.CloseParen, lexer.NextToken().Type);
        Assert.Equal(TokenType.Then, lexer.NextToken().Type);
        Assert.Equal(TokenType.Begin, lexer.NextToken().Type);
        Assert.Equal(TokenType.End, lexer.NextToken().Type);
    }
}