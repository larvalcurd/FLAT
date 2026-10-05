using Xunit;

namespace Lexer.UnitTests;

public class LexerTests
{
    public static TheoryData<string, int, int> GetSourcesWithInvalidCharacters()
    {
        return new TheoryData<string, int, int>
        {
            { "a ! b", 1, 2 },
            { "a \0 b", 1, 2 },
            { "a ÿ b", 1, 2 },
            { "a\r\n?", 2, 0 },
            { "a/b", 1, 1 },
            { "a } b", 1, 2 },
        };
    }

    [Theory]
    [MemberData(nameof(GetSourcesWithInvalidCharacters))]
    public void NextToken_InvalidCharacter_ThrowsWithCharacterPosition(string sourceText, int expectedLine, int expectedColumn)
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens(sourceText));

        Assert.Equal(new SourcePosition(expectedLine, expectedColumn), ex.Position);
    }

    public static TheoryData<string, string> GetValidIdentifiers()
    {
        return new TheoryData<string, string>
        {
            { "a", "a" },
            { "a1", "a1" },
            { "a_b", "a_b" },
            { "_a", "_a" },
            { "ABC", "abc" },
            { "begin1", "begin1" },
            { "writelnx", "writelnx" },
        };
    }

    [Theory]
    [MemberData(nameof(GetValidIdentifiers))]
    public void NextToken_ValidIdentifier_ReturnsIdentifierInLowerCase(string identifierText, string expectedValue)
    {
        List<Token> tokens = ScanTokens(identifierText);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
        Assert.Equal(TokenType.Eof, tokens[1].Type);
    }

    public static TheoryData<string, int, int> GetInvalidIdentifiers()
    {
        return new TheoryData<string, int, int>
        {
            { "1a", 1, 0 },
            { "абв", 1, 0 },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidIdentifiers))]
    public void NextToken_InvalidIdentifier_ThrowsWithErrorPosition(string sourceText, int expectedLine, int expectedColumn)
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens(sourceText));

        Assert.Equal(new SourcePosition(expectedLine, expectedColumn), ex.Position);
    }

    public static TheoryData<string, TokenType> GetAllKeywords()
    {
        return new TheoryData<string, TokenType>
        {
            { "program", TokenType.Program },
            { "var", TokenType.Var },
            { "const", TokenType.Const },
            { "procedure", TokenType.Procedure },
            { "function", TokenType.Function },
            { "integer", TokenType.Integer },
            { "boolean", TokenType.Boolean },
            { "string", TokenType.String },
            { "record", TokenType.Record },
            { "array", TokenType.Array },
            { "of", TokenType.Of },
            { "begin", TokenType.Begin },
            { "end", TokenType.End },
            { "if", TokenType.If },
            { "then", TokenType.Then },
            { "else", TokenType.Else },
            { "while", TokenType.While },
            { "do", TokenType.Do },
            { "write", TokenType.Write },
            { "writeln", TokenType.Writeln },
            { "read", TokenType.Read },
            { "readln", TokenType.Readln },
            { "true", TokenType.True },
            { "false", TokenType.False },
            { "and", TokenType.And },
            { "or", TokenType.Or },
            { "not", TokenType.Not },
            { "div", TokenType.Div },
            { "mod", TokenType.Mod },
        };
    }

    [Theory]
    [MemberData(nameof(GetAllKeywords))]
    public void NextToken_Keyword_ReturnsKeywordToken(string keywordText, TokenType expectedType)
    {
        List<Token> tokens = ScanTokens(keywordText);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(keywordText, tokens[0].Value);
        Assert.Equal(TokenType.Eof, tokens[1].Type);
    }

    public static TheoryData<string, TokenType> GetKeywordsInDifferentCase()
    {
        return new TheoryData<string, TokenType>
        {
            { "PROGRAM", TokenType.Program },
            { "Begin", TokenType.Begin },
            { "WriteLn", TokenType.Writeln },
            { "TRUE", TokenType.True },
            { "False", TokenType.False },
        };
    }

    [Theory]
    [MemberData(nameof(GetKeywordsInDifferentCase))]
    public void NextToken_KeywordInAnyCase_ReturnsKeywordInLowerCase(string keywordText, TokenType expectedType)
    {
        List<Token> tokens = ScanTokens(keywordText);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(keywordText.ToLowerInvariant(), tokens[0].Value);
        Assert.Equal(TokenType.Eof, tokens[1].Type);
    }

    public static TheoryData<string> GetIntegerLiterals()
    {
        return
        [
            "0",
            "123",
            "2147483648",
        ];
    }

    [Theory]
    [MemberData(nameof(GetIntegerLiterals))]
    public void NextToken_IntegerLiteral_ReturnsLiteralWithSameText(string numberText)
    {
        List<Token> tokens = ScanTokens(numberText);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.IntegerLiteral, tokens[0].Type);
        Assert.Equal(numberText, tokens[0].Value);
        Assert.Equal(TokenType.Eof, tokens[1].Type);
    }

    public static TheoryData<string, TokenType[]> GetIntegerLiteralSequences()
    {
        return new TheoryData<string, TokenType[]>
        {
            { "-1", [TokenType.Minus, TokenType.IntegerLiteral, TokenType.Eof] },
            { "1.2", [TokenType.IntegerLiteral, TokenType.Dot, TokenType.IntegerLiteral, TokenType.Eof] },
        };
    }

    [Theory]
    [MemberData(nameof(GetIntegerLiteralSequences))]
    public void NextToken_IntegerLiteralWithNeighbours_ReturnsSeparateTokens(string sourceText, TokenType[] expectedTokenTypes)
    {
        AssertTokenTypes(sourceText, expectedTokenTypes);
    }

    [Theory]
    [InlineData("01")]
    [InlineData("1_")]
    public void NextToken_InvalidIntegerLiteral_ThrowsWithLiteralStartPosition(string sourceText)
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens(sourceText));

        Assert.Equal(new SourcePosition(1, 0), ex.Position);
    }

    public static TheoryData<string, string> GetStringLiterals()
    {
        return new TheoryData<string, string>
        {
            { "''", string.Empty },
            { "'abc'", "abc" },
            { "'aBc'", "aBc" },
            { "'a\tb'", "a\tb" },
            { "'{}'", "{}" },
            { "'//'", "//" },
            { "'ÿ'", "ÿ" },
            { "'абв'", "абв" },
            { "'a''b'", "a'b" },
            { "''''", "'" },
        };
    }

    [Theory]
    [MemberData(nameof(GetStringLiterals))]
    public void NextToken_StringLiteral_ReturnsContentWithoutQuotes(string sourceText, string expectedValue)
    {
        List<Token> tokens = ScanTokens(sourceText);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
        Assert.Equal(TokenType.Eof, tokens[1].Type);
    }

    public static TheoryData<int> GetForbiddenControlCharacters()
    {
        return
        [
            0x00,
            0x0C,
            0x7F,
        ];
    }

    [Theory]
    [MemberData(nameof(GetForbiddenControlCharacters))]
    public void NextToken_ControlCharacterInStringLiteral_Throws(int code)
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens("'a" + (char)code + "b'"));

        Assert.Equal(new SourcePosition(1, 2), ex.Position);
    }

    [Theory]
    [InlineData("'a\nb'")]
    [InlineData("'a\rb'")]
    [InlineData("'a")]
    [InlineData("'a''")]
    public void NextToken_UnclosedStringLiteral_ThrowsWithOpeningQuotePosition(string sourceText)
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens(sourceText));

        Assert.Equal(new SourcePosition(1, 0), ex.Position);
    }

    public static TheoryData<string, TokenType> GetOperators()
    {
        return new TheoryData<string, TokenType>
        {
            { "+", TokenType.Plus },
            { "-", TokenType.Minus },
            { "*", TokenType.Multiply },
            { "=", TokenType.Equal },
            { "<>", TokenType.NotEqual },
            { "<", TokenType.Less },
            { "<=", TokenType.LessOrEqual },
            { ">", TokenType.Greater },
            { ">=", TokenType.GreaterOrEqual },
            { ":=", TokenType.Assign },
        };
    }

    [Theory]
    [MemberData(nameof(GetOperators))]
    public void NextToken_Operator_ReturnsOperatorToken(string operatorText, TokenType expectedType)
    {
        List<Token> tokens = ScanTokens(operatorText);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(operatorText, tokens[0].Value);
        Assert.Equal(TokenType.Eof, tokens[1].Type);
    }

    public static TheoryData<string, TokenType[]> GetOperatorSequences()
    {
        return new TheoryData<string, TokenType[]>
        {
            { ":==", [TokenType.Assign, TokenType.Equal, TokenType.Eof] },
            { "<>=", [TokenType.NotEqual, TokenType.Equal, TokenType.Eof] },
            { ": =", [TokenType.Colon, TokenType.Equal, TokenType.Eof] },
        };
    }

    [Theory]
    [MemberData(nameof(GetOperatorSequences))]
    public void NextToken_AdjacentOperators_UseLongestMatch(string sourceText, TokenType[] expectedTokenTypes)
    {
        AssertTokenTypes(sourceText, expectedTokenTypes);
    }

    public static TheoryData<string, TokenType> GetSeparators()
    {
        return new TheoryData<string, TokenType>
        {
            { "(", TokenType.OpenParen },
            { ")", TokenType.CloseParen },
            { "[", TokenType.OpenBracket },
            { "]", TokenType.CloseBracket },
            { ",", TokenType.Comma },
            { ":", TokenType.Colon },
            { ";", TokenType.Semicolon },
            { ".", TokenType.Dot },
            { "..", TokenType.DoubleDot },
        };
    }

    [Theory]
    [MemberData(nameof(GetSeparators))]
    public void NextToken_Separator_ReturnsSeparatorToken(string separatorText, TokenType expectedType)
    {
        List<Token> tokens = ScanTokens(separatorText);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(separatorText, tokens[0].Value);
        Assert.Equal(TokenType.Eof, tokens[1].Type);
    }

    public static TheoryData<string, TokenType[]> GetSeparatorSequences()
    {
        return new TheoryData<string, TokenType[]>
        {
            { "...", [TokenType.DoubleDot, TokenType.Dot, TokenType.Eof] },
            {
                "(*a*)",
                [TokenType.OpenParen, TokenType.Multiply, TokenType.Identifier, TokenType.Multiply, TokenType.CloseParen, TokenType.Eof]
            },
            {
                "a:array[1..10]of integer;",
                [
                    TokenType.Identifier, TokenType.Colon, TokenType.Array, TokenType.OpenBracket,
                    TokenType.IntegerLiteral, TokenType.DoubleDot, TokenType.IntegerLiteral, TokenType.CloseBracket,
                    TokenType.Of, TokenType.Integer, TokenType.Semicolon, TokenType.Eof,
                ]
            },
            {
                "const a = 1;",
                [TokenType.Const, TokenType.Identifier, TokenType.Equal, TokenType.IntegerLiteral, TokenType.Semicolon, TokenType.Eof]
            },
            {
                "writeln('a');",
                [TokenType.Writeln, TokenType.OpenParen, TokenType.StringLiteral, TokenType.CloseParen, TokenType.Semicolon, TokenType.Eof]
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetSeparatorSequences))]
    public void NextToken_CodeWithSeparators_ReturnsExpectedTokenTypes(string sourceText, TokenType[] expectedTokenTypes)
    {
        AssertTokenTypes(sourceText, expectedTokenTypes);
    }

    public static TheoryData<string, TokenType[]> GetSourcesWithComments()
    {
        return new TheoryData<string, TokenType[]>
        {
            { "//abc", [TokenType.Eof] },
            { "//abc\na", [TokenType.Identifier, TokenType.Eof] },
            { "//abc\ra", [TokenType.Identifier, TokenType.Eof] },
            { "{\tabc\r\n}a", [TokenType.Identifier, TokenType.Eof] },
            { "{a{b}c", [TokenType.Identifier, TokenType.Eof] },
            { "//'{}абв\na", [TokenType.Identifier, TokenType.Eof] },
            { "{'//абв}a", [TokenType.Identifier, TokenType.Eof] },
            { "a{}b", [TokenType.Identifier, TokenType.Identifier, TokenType.Eof] },
        };
    }

    [Theory]
    [MemberData(nameof(GetSourcesWithComments))]
    public void NextToken_Comment_IsSkipped(string sourceText, TokenType[] expectedTokenTypes)
    {
        AssertTokenTypes(sourceText, expectedTokenTypes);
    }

    [Fact]
    public void NextToken_NestedBlockComment_ThrowsOnExtraClosingBrace()
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens("{a{b}c}"));

        Assert.Equal(new SourcePosition(1, 6), ex.Position);
    }

    [Fact]
    public void NextToken_UnclosedBlockComment_Throws()
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens("a\n{b"));

        Assert.Equal(2, ex.Line);
    }

    [Theory]
    [MemberData(nameof(GetForbiddenControlCharacters))]
    public void NextToken_ControlCharacterInLineComment_Throws(int code)
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens("//a" + (char)code + "b\nc"));

        Assert.Equal(new SourcePosition(1, 3), ex.Position);
    }

    [Theory]
    [MemberData(nameof(GetForbiddenControlCharacters))]
    public void NextToken_ControlCharacterInBlockComment_Throws(int code)
    {
        LexerException ex = Assert.Throws<LexerException>(() => ScanTokens("{a" + (char)code + "b}c"));

        Assert.Equal(1, ex.Line);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\n\r\f\r\n")]
    public void NextToken_EmptyOrWhitespaceOnlySource_ReturnsEof(string sourceText)
    {
        AssertTokenTypes(sourceText, [TokenType.Eof]);
    }

    public static TheoryData<string, int, int> GetSecondTokenPositions()
    {
        return new TheoryData<string, int, int>
        {
            { "a\nb", 2, 0 },
            { "a\rb", 2, 0 },
            { "a\r\nb", 2, 0 },
            { "{\n\n}  a", 3, 3 },
        };
    }

    [Theory]
    [MemberData(nameof(GetSecondTokenPositions))]
    public void NextToken_TokenAfterLineBreak_HasExpectedPosition(string sourceText, int expectedLine, int expectedColumn)
    {
        List<Token> tokens = ScanTokens(sourceText);

        Assert.Equal(new SourcePosition(expectedLine, expectedColumn), tokens[^2].Position);
    }

    [Fact]
    public void NextToken_MultilineSource_ReturnsLineAndColumnForEachToken()
    {
        List<Token> tokens = ScanTokens("program a;\r\n  var b: integer;\r\nbegin end.");

        (TokenType Type, int Line, int Column)[] expected =
        [
            (TokenType.Program, 1, 0),
            (TokenType.Identifier, 1, 8),
            (TokenType.Semicolon, 1, 9),
            (TokenType.Var, 2, 2),
            (TokenType.Identifier, 2, 6),
            (TokenType.Colon, 2, 7),
            (TokenType.Integer, 2, 9),
            (TokenType.Semicolon, 2, 16),
            (TokenType.Begin, 3, 0),
            (TokenType.End, 3, 6),
            (TokenType.Dot, 3, 9),
            (TokenType.Eof, 3, 10),
        ];
        Assert.Equal(expected, tokens.Select(t => (t.Type, t.Line, t.Column)).ToArray());
    }

    [Fact]
    public void NextToken_ErrorInMiddleOfSource_ReturnsPrecedingTokensFirst()
    {
        Lexer lexer = CreateLexer("a b @ c");

        Assert.Equal("a", lexer.NextToken().Value);
        Assert.Equal("b", lexer.NextToken().Value);
        LexerException ex = Assert.Throws<LexerException>(lexer.NextToken);
        Assert.Equal(new SourcePosition(1, 4), ex.Position);
    }

    [Fact]
    public void NextToken_CalledAfterEof_ReturnsEofAgain()
    {
        Lexer lexer = CreateLexer("a");
        lexer.NextToken();

        Assert.Equal(TokenType.Eof, lexer.NextToken().Type);
        Assert.Equal(TokenType.Eof, lexer.NextToken().Type);
    }

    [Fact]
    public void NextToken_ProgramUsingAllConstructs_IsTokenizedWithoutErrors()
    {
        const string sourceText = """
            program a;
            const
              n = 10;
            var
              b: array[1..10] of integer;
              c: record d: string; e: boolean; end;
              i: integer;

            function f(x: integer): integer;
            begin
              f := x * 2 div 1 mod 3;
            end;

            begin
              readln(i);
              while (i <= n) and not (i = 5) do
              begin
                b[i] := -1;
                i := i + 1;
              end;
              c.d := 'a''b';
              c.e := true or false;
              if i <> 0 then writeln(f(i)) else read(i);
              if i >= 1 then write(c.d);
            end.
            """;

        List<Token> tokens = ScanTokens(sourceText);

        Assert.Equal(TokenType.Program, tokens[0].Type);
        Assert.Equal(TokenType.End, tokens[^3].Type);
        Assert.Equal(TokenType.Dot, tokens[^2].Type);
        Assert.Equal(TokenType.Eof, tokens[^1].Type);
        Assert.Contains(tokens, t => t.Type == TokenType.Const);
        Assert.Contains(tokens, t => t.Type == TokenType.Readln);
        Assert.Contains(tokens, t => t.Type == TokenType.Writeln);
    }

    [Fact]
    public void LexerException_Created_ContainsMessageAndPosition()
    {
        LexerException ex = new LexerException("error", new SourcePosition(3, 7));

        Assert.Equal(3, ex.Line);
        Assert.Equal(7, ex.Column);
        Assert.StartsWith("error", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Line = 3", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Column = 7", ex.Message, StringComparison.Ordinal);
    }

    private static List<Token> ScanTokens(string source)
    {
        Lexer lexer = CreateLexer(source);
        List<Token> tokens = [];
        Token token;
        do
        {
            token = lexer.NextToken();
            tokens.Add(token);
        }
        while (token.Type != TokenType.Eof);

        return tokens;
    }

    private static void AssertTokenTypes(string sourceText, TokenType[] expectedTokenTypes)
    {
        List<Token> tokens = ScanTokens(sourceText);

        Assert.Equal(expectedTokenTypes.Length, tokens.Count);
        for (int i = 0; i < expectedTokenTypes.Length; i++)
        {
            Assert.Equal(expectedTokenTypes[i], tokens[i].Type);
        }
    }

    private static Lexer CreateLexer(string source)
    {
        return new Lexer(source);
    }
}