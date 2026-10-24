namespace VM;

public class InputReader(TextReader input)
{
    public int ReadInt() => ReadIntCore();

    public int ReadlnInt()
    {
        int value = ReadIntCore();
        SkipToEndOfLine();
        return value;
    }

    private int ReadIntCore()
    {
        int ch = GetFirstNotWhiteSpaceSymbol();

        bool isNegative = false;
        if (ch == '-')
        {
            isNegative = ch == '-';
            ch = input.Read();
        }

        if (ch == -1 || !char.IsAsciiDigit((char)ch))
        {
            throw new VmException($"Invalid integer input: expected digit, got '{(char)ch}'");
        }

        long acc = 0;
        while (ch != -1 && char.IsAsciiDigit((char)ch))
        {
            acc = acc * 10 + (ch - '0');

            if (acc > int.MaxValue)
            {
                if (!isNegative || (isNegative && acc - int.MaxValue > 1))
                {
                    throw new VmException("Integer input is out of range");
                }
            }

            ch = input.Read();
        }

        if (ch != -1 && !char.IsWhiteSpace((char)ch))
        {
            throw new VmException($"Invalid integer input: unexpected character '{(char)ch}'");
        }

        return isNegative ? -(int)acc : (int)acc;
    }

    private int GetFirstNotWhiteSpaceSymbol()
    {
        int ch;
        do
        {
            ch = input.Read();
            if (ch == -1)
            {
                throw new VmException("Unexpected end of stream");
            }
        }
        while (char.IsWhiteSpace((char)ch));

        return ch;
    }

    private void SkipToEndOfLine()
    {
        int ch;
        while ((ch = input.Read()) != -1)
        {
            if (ch is '\n' or '\r')
            {
                return;
            }
        }
    }
}