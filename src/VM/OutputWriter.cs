namespace VM;

public class OutputWriter(TextWriter output)
{
    public void Write(int value)
    {
        output.Write(value);
    }

    public void WriteLine(int value)
    {
        output.Write(value);
        output.Write('\n');
    }
}