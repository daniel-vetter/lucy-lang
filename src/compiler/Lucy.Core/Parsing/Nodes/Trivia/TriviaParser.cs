namespace Lucy.Core.Parsing.Nodes.Trivia;

public abstract class TriviaParser
{
    private static int ReadLength(Reader reader)
    {
        var cur = 0;
        var isInSingleLineComment = false;
        var isInMultiLineComment = false;
        while (true)
        {
            var ch = reader.Peek(cur);

            if (ch == '\0')
                break;

            if (ch is ' ' or '\t' or '\r' or '\n')
            {
                cur++;
                continue;
            }

            if (!isInSingleLineComment && !isInMultiLineComment)
            {
                if (ch is '/')
                {
                    if (reader.Peek(cur + 1) is '/')
                    {
                        cur += 2;
                        isInSingleLineComment = true;
                        continue;
                    }

                    if (reader.Peek(cur + 1) is '*')
                    {
                        cur += 2;
                        isInMultiLineComment = true;
                        continue;
                    }
                }
            }

            if (isInSingleLineComment && ch is '\n')
            {
                isInSingleLineComment = false;
                cur++;
                continue;
            }

            if (isInMultiLineComment && ch is '*' && reader.Peek(cur + 1) is '/')
            {
                isInMultiLineComment = false;
                cur += 2;
                continue;
            }

            if (isInMultiLineComment || isInSingleLineComment)
            {
                cur++;
                continue;
            }

            break;
        }

        return cur;
    }

    public static string? Read(Reader reader)
    {
        var len = ReadLength(reader);
        return len == 0 ? null : reader.Internalize(reader.Read(len));
    }
}