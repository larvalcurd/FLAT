using Lexer;

using VM;

namespace Parsing;

public class Parser
{
    private readonly Lexer.Lexer _lexer;
    private readonly List<Instruction> _instructions = [];
    private Token _currentToken;

    public Parser(string source)
    {
        _lexer = new Lexer.Lexer(source);
        _currentToken = _lexer.NextToken();
    }

    public List<Instruction> ParseProgram()
    {
        Expect(TokenType.Program);
        Expect(TokenType.Identifier);
        Expect(TokenType.Semicolon);
        ParseBlock();
        Expect(TokenType.Dot);
        Expect(TokenType.Eof);

        return _instructions;
    }

    private void ParseBlock()
    {
        Expect(TokenType.Begin);
        ParseInstructionList();
        Expect(TokenType.End);
    }

    private void ParseInstructionList()
    {
        while (_currentToken.Type != TokenType.End && _currentToken.Type != TokenType.Eof)
        {
            ParseInstruction();
            Expect(TokenType.Semicolon);
        }
    }

    private void ParseInstruction()
    {
        switch (_currentToken.Type)
        {
            case TokenType.Write:
                ParseWrite(TokenType.Write, OpCodes.Write);
                break;
            case TokenType.Writeln:
                ParseWrite(TokenType.Writeln, OpCodes.Writeln);
                break;
            default:
                throw new ParserException(
                    $"Expected Write or Writeln, input is: {_currentToken.Type}", _currentToken.Position);
        }
    }

    private void ParseWrite(TokenType keyword, OpCodes operation)
    {
        Expect(keyword);
        Expect(TokenType.OpenParen);
        ParseExpression();
        Expect(TokenType.CloseParen);
        _instructions.Add(new Instruction(operation));
    }

    private void ParseExpression()
    {
        ParseSimpleExpression();
        OpCodes? operation = _currentToken.Type switch
        {
            TokenType.Equal => OpCodes.Eq,
            TokenType.NotEqual => OpCodes.Ne,
            TokenType.Less => OpCodes.Lt,
            TokenType.LessOrEqual => OpCodes.Le,
            TokenType.Greater => OpCodes.Gt,
            TokenType.GreaterOrEqual => OpCodes.Ge,
            _ => null,
        };

        if (operation != null)
        {
            Advance();
            ParseSimpleExpression();
            _instructions.Add(new Instruction(operation.Value));
        }
    }

    private void ParseSimpleExpression()
    {
        ParseTerm();
        while (true)
        {
            OpCodes? operation = _currentToken.Type switch
            {
                TokenType.Plus => OpCodes.Add,
                TokenType.Minus => OpCodes.Sub,
                TokenType.Or => OpCodes.Or,
                _ => null,
            };

            if (operation == null)
            {
                return;
            }

            Advance();
            ParseTerm();
            _instructions.Add(new Instruction(operation.Value));
        }
    }

    private void ParseTerm()
    {
        ParseFactor();
        while (true)
        {
            OpCodes? operation = _currentToken.Type switch
            {
                TokenType.Multiply => OpCodes.Mul,
                TokenType.Div => OpCodes.Div,
                TokenType.Mod => OpCodes.Mod,
                TokenType.And => OpCodes.And,
                _ => null,
            };

            if (operation == null)
            {
                return;
            }

            Advance();
            ParseFactor();
            _instructions.Add(new Instruction(operation.Value));
        }
    }

    private void ParseFactor()
    {
        OpCodes? operation = _currentToken.Type switch
        {
            TokenType.Minus => OpCodes.Neg,
            TokenType.Not => OpCodes.Not,
            _ => null,
        };

        if (operation == null)
        {
            ParsePrimary();
            return;
        }

        Advance();
        ParseFactor();
        _instructions.Add(new Instruction(operation.Value));
    }

    private void ParsePrimary()
    {
        switch (_currentToken.Type)
        {
            case TokenType.OpenParen:
                Expect(TokenType.OpenParen);
                ParseExpression();
                Expect(TokenType.CloseParen);
                return;
            case TokenType.Read:
                ParseRead(TokenType.Read, OpCodes.Read);
                return;
            case TokenType.Readln:
                ParseRead(TokenType.Readln, OpCodes.Readln);
                return;
            case TokenType.Identifier:
                ParseFunctionCall();
                return;
        }

        Token number = Expect(TokenType.IntegerLiteral);
        if (!int.TryParse(number.Value, out int value))
        {
            throw new ParserException("Integer is out of range", number.Position);
        }

        _instructions.Add(new Instruction(OpCodes.Push, value));
    }

    private void ParseRead(TokenType keyword, OpCodes operation)
    {
        Expect(keyword);
        Expect(TokenType.OpenParen);
        Expect(TokenType.CloseParen);
        _instructions.Add(new Instruction(operation));
    }

    private void ParseFunctionCall()
    {
        Token name = Expect(TokenType.Identifier);
        Expect(TokenType.OpenParen);
        int argumentCount = ParseExpressionList();
        Expect(TokenType.CloseParen);

        OpCodes operation = name.Value switch
        {
            "abs" => OpCodes.Abs,
            "min" => OpCodes.Min,
            "max" => OpCodes.Max,
            _ => throw new ParserException($"Unknown function '{name.Value}'", name.Position),
        };

        int expectedCount = operation == OpCodes.Abs ? 1 : 2;
        if (argumentCount != expectedCount)
        {
            throw new ParserException(
                $"Function '{name.Value}' expects {expectedCount} argument(s), got {argumentCount}", name.Position);
        }

        _instructions.Add(new Instruction(operation));
    }

    private int ParseExpressionList()
    {
        ParseExpression();
        int count = 1;
        while (_currentToken.Type == TokenType.Comma)
        {
            Expect(TokenType.Comma);
            ParseExpression();
            count++;
        }

        return count;
    }

    private Token Expect(TokenType expected)
    {
        return _currentToken.Type != expected
            ? throw new ParserException($"Expected {expected}, found {_currentToken.Type}", _currentToken.Position)
            : Advance();
    }

    private Token Advance()
    {
        Token token = _currentToken;
        _currentToken = _lexer.NextToken();
        return token;
    }
}