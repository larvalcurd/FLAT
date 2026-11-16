using Lexer;

using Parsing;

using VM;

namespace Interpreter;

public static class Interpreter
{
    public static int Run(string source, TextReader input, TextWriter output, TextWriter errors)
    {
        try
        {
            Parser parser = new(source);
            List<Instruction> instructions = parser.ParseProgram();
            VirtualMachine vm = new(instructions, input, output);
            vm.Run();
            return 0;
        }
        catch (Exception error) when (error is LexerException or ParserException or VmException or OverflowException)
        {
            errors.WriteLine(error.Message);
            return 1;
        }
    }
}