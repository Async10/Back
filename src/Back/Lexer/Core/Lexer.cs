using System.Text.RegularExpressions;
namespace Back.Lexer.Core;

using Back.Lexer.Abstractions;
using Back.Shared.Abstractions;

public class Lexer : ILexer
{
    public IEnumerable<Token> LexFile(SourceFile file)
    {
        foreach ((int row, string line) in file.Lines.Enumerate())
        {
            foreach ((int col, string value) in this.LexLine(line))
                yield return this.CreateToken(value, new Location(file.Path, row + 1, col + 1));
        }
    }

    private IEnumerable<(int col, string word)> LexLine(string line)
    {
        int col = this.FindNonWhiteSpace(line);
        while (col != -1)
        {
            int end = this.FindEnd(line.Substring(col));
            if (end == -1)
            {
                yield return (col, line.Substring(col));
                col = end;
            }
            else
            {
                yield return (col, line.Substring(col, end));
                int colAddend = this.FindNonWhiteSpace(line.Substring(col + end));
                col = colAddend != -1 ? col + end + colAddend : colAddend;
            }
        }
    }

    private int FindEnd(string text)
    {
        if (text.StartsWith("'"))
        {
            return this.FindIndex(
                text,
                (ch, idx, text) =>
                {
                    return char.IsWhiteSpace(ch) && text[idx - 1] == '\'' && idx != 1;
                }
            );
        }

        return this.FindWhiteSpace(text);
    }

    private int FindNonWhiteSpace(string text) =>
        this.FindIndex(text, ch => !Char.IsWhiteSpace(ch));

    private int FindWhiteSpace(string text) =>
        this.FindIndex(text, Char.IsWhiteSpace);

    private int FindIndex(string text, Func<char, int, string, bool> predicate)
    {
        for (int idx = 0; idx < text.Length; idx++)
        {
            if (predicate(text[idx], idx, text))
                return idx;
        }

        return -1;
    }

    private int FindIndex(string text, Func<char, bool> predicate) =>
        this.FindIndex(text, (ch, _, _) => predicate(ch));

    private Token CreateToken(string word, Location location)
    {
        if (word.StartsWith("'"))
        {
            var ch = Regex.Unescape(word.Substring(1, word.Length - 2));
            if (char.TryParse(ch, out var c))
            {
                return new IntToken(c, location);
            }
        }

        if (int.TryParse(word, out var i))
        {
            return new IntToken(i, location);
        }

        return new WordToken(word, location);
    }
}
