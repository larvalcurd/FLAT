if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: Interpreter <source-file>");
    return 1;
}

try
{
    string source = File.ReadAllText(args[0]);
    return Interpreter.Interpreter.Run(source, Console.In, Console.Out, Console.Error);
}
catch (Exception error) when (error is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}