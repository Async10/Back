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
        int start = 0;
        int col = this.FindNonWhiteSpace(line.Substring(start));
        while (col != -1)
        {
            int end = this.FindWhiteSpace(line.Substring(start));
            int length = end != -1 ? end - col : line.Substring(start).Length - col;
            yield return (start + col, line.Substring(start + col, length));
            start += col + length + 1;
            col = this.FindNonWhiteSpace(start < line.Length ? line.Substring(start) : string.Empty);
        }
    }

    private int FindNonWhiteSpace(string text) =>
        this.FindIndex(text, ch => !Char.IsWhiteSpace(ch));

    private int FindWhiteSpace(string text) =>
        this.FindIndex(text, Char.IsWhiteSpace);

    private int FindIndex(string text, Func<char, bool> predicate)
    {
        for (int idx = 0; idx < text.Length; idx++)
        {
            if (predicate(text[idx]))
                return idx;
        }

        return -1;
    }

    private Token CreateToken(string word, Location location)
    {
        if (int.TryParse(word, out var value))
        {
            return new IntToken(value, location);
        }

        return new WordToken(word, location);
    }
}
