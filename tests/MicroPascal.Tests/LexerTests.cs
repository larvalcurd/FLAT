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
        Assert.Equal(1, token.Column);
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
        Assert.Throws<Exception>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_ShouldHandleNestedBracesAsContent()
    {
        Lexer lexer = CreateLexer("{ a { b } c } var");
        Assert.Throws<Exception>(() => lexer.NextToken());
    }
    
    [Fact]
    public void NextToken_HandleIntegerLiteral()
    {
        Lexer lexer = CreateLexer("122");
        Token token = lexer.NextToken();
        Assert.Equal(TokenType.IntegerLiteral, token.Type);
    }
    [Fact]
    public void NextToken_ThrowExceptionInvalidIdentifier()
    {
        Lexer lexer = CreateLexer("122a");
        Assert.Throws<Exception>(() => lexer.NextToken());
    }
}